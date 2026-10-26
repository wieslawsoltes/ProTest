// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Avalonia.HeadlessTestingFramework.Recording;

/// <summary>
/// Represents an active recording session.
/// </summary>
public class RecordingSession : IDisposable
{
    private readonly Stopwatch _stopwatch = new();
    private readonly List<RecordingEvent> _events = new();
    private RecordingState _state = RecordingState.NotStarted;
    private bool _disposed;

    /// <summary>
    /// Gets the unique identifier for this session.
    /// </summary>
    public Guid SessionId { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets the recording options for this session.
    /// </summary>
    public RecordingOptions Options { get; }

    /// <summary>
    /// Gets the current state of the recording.
    /// </summary>
    public RecordingState State => _state;

    /// <summary>
    /// Gets the elapsed time since recording started.
    /// </summary>
    public TimeSpan Elapsed => _stopwatch.Elapsed;

    /// <summary>
    /// Gets the number of frames captured in this session.
    /// </summary>
    public int FramesCaptured { get; private set; }

    /// <summary>
    /// Gets the number of frames dropped in this session.
    /// </summary>
    public int FramesDropped { get; private set; }

    /// <summary>
    /// Gets the start time of the recording.
    /// </summary>
    public DateTime? StartTime { get; private set; }

    /// <summary>
    /// Gets the end time of the recording.
    /// </summary>
    public DateTime? EndTime { get; private set; }

    /// <summary>
    /// Gets the recorded events (input, scroll, animations, etc.).
    /// </summary>
    public IReadOnlyList<RecordingEvent> Events => _events;

    /// <summary>
    /// Gets the frame buffer for this session.
    /// </summary>
    public FrameBuffer? FrameBuffer { get; private set; }

    /// <summary>
    /// Gets the frame encoder for this session.
    /// </summary>
    public IFrameEncoder? Encoder { get; private set; }

    /// <summary>
    /// Gets the width of the recording in pixels.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// Gets the height of the recording in pixels.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Event raised when the recording state changes.
    /// </summary>
    public event EventHandler<RecordingStateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Event raised when a frame is captured.
    /// </summary>
    public event EventHandler<FrameCapturedEventArgs>? FrameCaptured;

    /// <summary>
    /// Creates a new recording session.
    /// </summary>
    /// <param name="options">Recording options.</param>
    public RecordingSession(RecordingOptions options)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        options.Validate();
    }

    /// <summary>
    /// Initializes the session with the specified dimensions.
    /// </summary>
    /// <param name="width">Frame width.</param>
    /// <param name="height">Frame height.</param>
    /// <param name="outputPath">Output path for the recording.</param>
    public void Initialize(int width, int height, string outputPath)
    {
        if (_state != RecordingState.NotStarted)
            throw new InvalidOperationException("Session has already been initialized.");

        Width = width;
        Height = height;

        FrameBuffer = new FrameBuffer(Options.MaxBufferedFrames);
        Encoder = FrameEncoderFactory.Create(Options.Format);
        Encoder.Initialize(outputPath, width, height, Options);

        SetState(RecordingState.Initialized);
    }

    /// <summary>
    /// Starts the recording.
    /// </summary>
    public void Start()
    {
        if (_state == RecordingState.NotStarted)
            throw new InvalidOperationException("Session must be initialized before starting.");

        if (_state == RecordingState.Recording)
            return;

        if (_state is RecordingState.Stopped or RecordingState.Finalized)
            throw new InvalidOperationException("Cannot restart a stopped session.");

        StartTime = DateTime.UtcNow;
        _stopwatch.Start();
        SetState(RecordingState.Recording);
    }

    /// <summary>
    /// Pauses the recording.
    /// </summary>
    public void Pause()
    {
        if (_state != RecordingState.Recording)
            return;

        _stopwatch.Stop();
        SetState(RecordingState.Paused);
    }

    /// <summary>
    /// Resumes a paused recording.
    /// </summary>
    public void Resume()
    {
        if (_state != RecordingState.Paused)
            return;

        _stopwatch.Start();
        SetState(RecordingState.Recording);
    }

    /// <summary>
    /// Stops the recording.
    /// </summary>
    public void Stop()
    {
        if (_state == RecordingState.Stopped || _state == RecordingState.NotStarted)
            return;

        _stopwatch.Stop();
        EndTime = DateTime.UtcNow;
        SetState(RecordingState.Stopped);
    }

    /// <summary>
    /// Records a captured frame.
    /// </summary>
    /// <param name="frame">The captured frame.</param>
    public void RecordFrame(CapturedFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_state != RecordingState.Recording)
            return;

        if (Options.MaxDuration.HasValue && Elapsed > Options.MaxDuration.Value)
        {
            Stop();
            return;
        }

        var scaled = Options.ScaleFactor == 1.0 ? null : ScaleFrame(frame, Options.ScaleFactor);
        var frameToEncode = scaled ?? frame;
        var buffered = false;
        try
        {
            if (Options.WriteImmediately && Encoder != null)
            {
                Encoder.EncodeFrame(frameToEncode);
            }
            else if (FrameBuffer != null)
            {
                buffered = FrameBuffer.TryAdd(frameToEncode);
                if (!buffered)
                {
                    FramesDropped++;
                    return;
                }
            }
        }
        finally
        {
            if (!buffered)
                scaled?.Dispose();
        }

        FramesCaptured++;
        FrameCaptured?.Invoke(this, new FrameCapturedEventArgs(frame.FrameNumber, frame.Timestamp));
    }

    private static CapturedFrame ScaleFrame(CapturedFrame source, double factor)
    {
        ObjectDisposedException.ThrowIf(source.IsDisposed, source);
        var width = Math.Max(1, checked((int)(source.Width * factor)));
        var height = Math.Max(1, checked((int)(source.Height * factor)));
        var bytesPerPixel = source.PixelFormat == PixelFormat.Rgb888 ? 3 : 4;
        var stride = checked(width * bytesPerPixel);
        var pixels = new byte[checked(stride * height)];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var sourceY = (int)((long)y * source.Height / height);
            var sourceX = (int)((long)x * source.Width / width);
            source.PixelData.Slice(sourceY * source.Stride + sourceX * bytesPerPixel, bytesPerPixel)
                .CopyTo(pixels.AsSpan(y * stride + x * bytesPerPixel, bytesPerPixel));
        }
        return new CapturedFrame(source.FrameNumber, source.Timestamp, width, height, stride, source.PixelFormat, pixels, source.Dpi);
    }

    /// <summary>
    /// Records an event during the recording.
    /// </summary>
    /// <param name="eventType">Type of event.</param>
    /// <param name="description">Event description.</param>
    /// <param name="data">Optional event data.</param>
    public void RecordEvent(RecordingEventType eventType, string description, object? data = null)
    {
        var evt = new RecordingEvent
        {
            Timestamp = Elapsed,
            EventType = eventType,
            Description = description,
            Data = data
        };

        _events.Add(evt);
    }

    /// <summary>
    /// Finalizes the recording and encodes all buffered frames.
    /// </summary>
    public void FinalizeRecording()
    {
        if (_state == RecordingState.Finalized)
            return;
        if (_state != RecordingState.Stopped)
        {
            Stop();
        }

        // Encode any buffered frames
        if (FrameBuffer != null && Encoder != null && !Options.WriteImmediately)
        {
            var frames = FrameBuffer.TakeAll();
            foreach (var frame in frames)
            {
                Encoder.EncodeFrame(frame);
                frame.Dispose();
            }
        }

        Encoder?.FinalizeEncoding();
        SetState(RecordingState.Finalized);
    }

    /// <summary>
    /// Gets recording statistics.
    /// </summary>
    public RecordingStatistics GetStatistics()
    {
        return new RecordingStatistics
        {
            SessionId = SessionId,
            Duration = Elapsed,
            FramesCaptured = FramesCaptured,
            FramesDropped = FramesDropped,
            EventCount = _events.Count,
            AverageFrameRate = Elapsed.TotalSeconds > 0 ? FramesCaptured / Elapsed.TotalSeconds : 0,
            Width = Width,
            Height = Height,
            Format = Options.Format,
            OutputFiles = Encoder?.OutputFiles ?? Array.Empty<string>()
        };
    }

    private void SetState(RecordingState newState)
    {
        var oldState = _state;
        _state = newState;
        StateChanged?.Invoke(this, new RecordingStateChangedEventArgs(oldState, newState));
    }

    /// <summary>
    /// Disposes the recording session.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_state == RecordingState.Recording || _state == RecordingState.Paused)
        {
            Stop();
        }

        FrameBuffer?.Dispose();
        Encoder?.Dispose();

        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// State of a recording session.
/// </summary>
public enum RecordingState
{
    /// <summary>
    /// Recording has not started.
    /// </summary>
    NotStarted,

    /// <summary>
    /// Recording session is initialized.
    /// </summary>
    Initialized,

    /// <summary>
    /// Recording is in progress.
    /// </summary>
    Recording,

    /// <summary>
    /// Recording is paused.
    /// </summary>
    Paused,

    /// <summary>
    /// Recording has stopped.
    /// </summary>
    Stopped,

    /// <summary>
    /// Recording has been finalized.
    /// </summary>
    Finalized
}

/// <summary>
/// Event arguments for recording state changes.
/// </summary>
public class RecordingStateChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the previous state.
    /// </summary>
    public RecordingState OldState { get; }

    /// <summary>
    /// Gets the new state.
    /// </summary>
    public RecordingState NewState { get; }

    /// <summary>
    /// Creates new state changed event arguments.
    /// </summary>
    public RecordingStateChangedEventArgs(RecordingState oldState, RecordingState newState)
    {
        OldState = oldState;
        NewState = newState;
    }
}

/// <summary>
/// Event arguments for frame captured events.
/// </summary>
public class FrameCapturedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the frame number.
    /// </summary>
    public int FrameNumber { get; }

    /// <summary>
    /// Gets the timestamp of the frame.
    /// </summary>
    public TimeSpan Timestamp { get; }

    /// <summary>
    /// Creates new frame captured event arguments.
    /// </summary>
    public FrameCapturedEventArgs(int frameNumber, TimeSpan timestamp)
    {
        FrameNumber = frameNumber;
        Timestamp = timestamp;
    }
}

/// <summary>
/// Represents an event recorded during the session.
/// </summary>
public class RecordingEvent
{
    /// <summary>
    /// Gets or sets the timestamp of the event.
    /// </summary>
    public TimeSpan Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the type of event.
    /// </summary>
    public RecordingEventType EventType { get; set; }

    /// <summary>
    /// Gets or sets the event description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional event data.
    /// </summary>
    public object? Data { get; set; }
}

/// <summary>
/// Types of recording events.
/// </summary>
public enum RecordingEventType
{
    /// <summary>
    /// Generic event.
    /// </summary>
    Generic,

    /// <summary>
    /// Touch/pointer input event.
    /// </summary>
    Input,

    /// <summary>
    /// Scroll event.
    /// </summary>
    Scroll,

    /// <summary>
    /// Animation start/end event.
    /// </summary>
    Animation,

    /// <summary>
    /// Gesture event (pinch, swipe, etc.).
    /// </summary>
    Gesture,

    /// <summary>
    /// UI state change event.
    /// </summary>
    StateChange,

    /// <summary>
    /// Error event.
    /// </summary>
    Error,

    /// <summary>
    /// Custom marker event.
    /// </summary>
    Marker
}

/// <summary>
/// Statistics for a recording session.
/// </summary>
public class RecordingStatistics
{
    /// <summary>
    /// Gets or sets the session ID.
    /// </summary>
    public Guid SessionId { get; set; }

    /// <summary>
    /// Gets or sets the total duration.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the number of frames captured.
    /// </summary>
    public int FramesCaptured { get; set; }

    /// <summary>
    /// Gets or sets the number of frames dropped.
    /// </summary>
    public int FramesDropped { get; set; }

    /// <summary>
    /// Gets or sets the number of recorded events.
    /// </summary>
    public int EventCount { get; set; }

    /// <summary>
    /// Gets or sets the average frame rate.
    /// </summary>
    public double AverageFrameRate { get; set; }

    /// <summary>
    /// Gets or sets the recording width.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the recording height.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Gets or sets the output format.
    /// </summary>
    public RecordingFormat Format { get; set; }

    /// <summary>
    /// Gets or sets the output files.
    /// </summary>
    public IReadOnlyList<string> OutputFiles { get; set; } = Array.Empty<string>();
}

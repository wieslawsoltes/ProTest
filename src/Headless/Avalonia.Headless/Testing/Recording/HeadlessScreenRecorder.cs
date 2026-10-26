// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;

namespace Avalonia.HeadlessTestingFramework.Recording;

/// <summary>
/// A headless screen recorder for capturing frames from Avalonia applications during testing.
/// Supports recording scrolling, input handling, animations, and other UI interactions.
/// </summary>
/// <remarks>
/// This recorder requires Avalonia.Headless to be configured with UseHeadlessDrawing = false
/// and a real rendering backend (e.g., Skia) to capture actual rendered frames.
/// </remarks>
public class HeadlessScreenRecorder : IDisposable
{
    private RecordingSession? _session;
    private TopLevel? _target;
    private CancellationTokenSource? _autoCaptureTokenSource;
    private Task? _autoCaptureTask;
    private int _frameNumber;
    private WriteableBitmap? _previousFrame;
    private bool _disposed;
    private RenderFrameCapture? _renderCapture;
    private bool _useRenderHook;

    /// <summary>
    /// Gets whether a recording is currently in progress.
    /// </summary>
    public bool IsRecording => _session?.State == RecordingState.Recording;

    /// <summary>
    /// Gets whether the recorder is paused.
    /// </summary>
    public bool IsPaused => _session?.State == RecordingState.Paused;

    /// <summary>
    /// Gets the current recording session.
    /// </summary>
    public RecordingSession? CurrentSession => _session;

    /// <summary>
    /// Gets or sets whether to use render hook for automatic frame capture.
    /// When true, frames are captured automatically when the compositor renders.
    /// When false, frames are captured on timer intervals or manually.
    /// </summary>
    public bool UseRenderHook
    {
        get => _useRenderHook;
        set
        {
            if (IsRecording)
                throw new InvalidOperationException("Cannot change UseRenderHook while recording.");
            _useRenderHook = value;
        }
    }

    /// <summary>
    /// Event raised when recording starts.
    /// </summary>
    public event EventHandler? RecordingStarted;

    /// <summary>
    /// Event raised when recording stops.
    /// </summary>
    public event EventHandler? RecordingStopped;

    /// <summary>
    /// Event raised when a frame is captured.
    /// </summary>
    public event EventHandler<FrameCapturedEventArgs>? FrameCaptured;

    /// <summary>
    /// Event raised when an error occurs during recording.
    /// </summary>
    public event EventHandler<RecordingErrorEventArgs>? RecordingError;

    /// <summary>
    /// Creates a new headless screen recorder.
    /// </summary>
    public HeadlessScreenRecorder()
    {
    }

    /// <summary>
    /// Starts recording the specified target.
    /// </summary>
    /// <param name="target">The top-level window or control to record.</param>
    /// <param name="options">Recording options.</param>
    /// <param name="outputPath">Output path for the recording. If null, uses temp directory.</param>
    /// <returns>The recording session.</returns>
    public RecordingSession StartRecording(TopLevel target, RecordingOptions? options = null, string? outputPath = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(HeadlessScreenRecorder));

        if (_session != null)
            throw new InvalidOperationException("A recording session is already active. Stop it before starting another recording.");

        options ??= RecordingOptions.HighQuality;
        options.Validate();

        _target = target ?? throw new ArgumentNullException(nameof(target));
        _frameNumber = 0;
        _previousFrame = null;

        // Determine output path
        outputPath ??= options.OutputDirectory ?? Path.Combine(Path.GetTempPath(), $"recording_{DateTime.Now:yyyyMMdd_HHmmss}");

        // Get target dimensions
        var bounds = target.Bounds;
        var width = (int)(bounds.Width * options.ScaleFactor);
        var height = (int)(bounds.Height * options.ScaleFactor);

        if (width <= 0 || height <= 0)
        {
            // Use default size if bounds not available
            width = 800;
            height = 600;
        }

        // Create and initialize session
        _session = new RecordingSession(options);
        _session.Initialize(width, height, outputPath);
        _session.Start();

        RecordingStarted?.Invoke(this, EventArgs.Empty);

        // Start frame capture
        if (_useRenderHook)
        {
            // Use render hook for automatic capture on each compositor frame
            StartRenderHookCapture(target);
        }
        else if (options.AutoCapture)
        {
            // Use timer-based auto-capture
            StartAutoCapture(options.FrameIntervalMs);
        }

        return _session;
    }

    /// <summary>
    /// Stops the current recording and finalizes output.
    /// </summary>
    /// <returns>Recording statistics.</returns>
    public RecordingStatistics StopRecording()
    {
        if (_session == null)
            throw new InvalidOperationException("No recording in progress.");

        StopAutoCapture();
        StopRenderHookCapture();

        _session.FinalizeRecording();
        var stats = _session.GetStatistics();

        RecordingStopped?.Invoke(this, EventArgs.Empty);

        _session = null;
        _target = null;
        _previousFrame?.Dispose();
        _previousFrame = null;

        return stats;
    }

    /// <summary>
    /// Pauses the current recording.
    /// </summary>
    public void PauseRecording()
    {
        _session?.Pause();
        StopAutoCapture();
        StopRenderHookCapture();
    }

    /// <summary>
    /// Resumes a paused recording.
    /// </summary>
    public void ResumeRecording()
    {
        if (_session?.State == RecordingState.Paused && _target != null)
        {
            _session.Resume();
            if (_useRenderHook)
            {
                StartRenderHookCapture(_target);
            }
            else if (_session.Options.AutoCapture)
            {
                StartAutoCapture(_session.Options.FrameIntervalMs);
            }
        }
    }

    /// <summary>
    /// Debug counter for capture attempts.
    /// </summary>
    public int CaptureAttempts { get; private set; }

    /// <summary>
    /// Debug counter for successful captures.
    /// </summary>
    public int SuccessfulCaptures { get; private set; }

    /// <summary>
    /// Debug last capture failure reason.
    /// </summary>
    public string? LastCaptureFailureReason { get; private set; }

    /// <summary>
    /// Captures a single frame manually.
    /// </summary>
    /// <returns>True if the frame was captured, false otherwise.</returns>
    public bool CaptureFrame()
    {
        CaptureAttempts++;

        if (_session == null)
        {
            LastCaptureFailureReason = "Session is null";
            return false;
        }

        if (_target == null)
        {
            LastCaptureFailureReason = "Target is null";
            return false;
        }

        if (!IsRecording)
        {
            LastCaptureFailureReason = "Not recording";
            return false;
        }

        try
        {
            var result = CaptureFrameInternal();
            if (result)
            {
                SuccessfulCaptures++;
            }
            // Don't override LastCaptureFailureReason - it's set inside CaptureFrameInternal
            return result;
        }
        catch (Exception ex)
        {
            LastCaptureFailureReason = $"Exception: {ex.Message}";
            RecordingError?.Invoke(this, new RecordingErrorEventArgs(ex, "Failed to capture frame"));
            return false;
        }
    }

    /// <summary>
    /// Records an input event.
    /// </summary>
    /// <param name="description">Description of the input event.</param>
    /// <param name="data">Optional event data.</param>
    public void RecordInputEvent(string description, object? data = null)
    {
        _session?.RecordEvent(RecordingEventType.Input, description, data);
    }

    /// <summary>
    /// Records a scroll event.
    /// </summary>
    /// <param name="description">Description of the scroll event.</param>
    /// <param name="delta">Scroll delta.</param>
    public void RecordScrollEvent(string description, Vector delta)
    {
        _session?.RecordEvent(RecordingEventType.Scroll, description, new { Delta = delta });
    }

    /// <summary>
    /// Records an animation event.
    /// </summary>
    /// <param name="description">Description of the animation.</param>
    /// <param name="data">Optional animation data.</param>
    public void RecordAnimationEvent(string description, object? data = null)
    {
        _session?.RecordEvent(RecordingEventType.Animation, description, data);
    }

    /// <summary>
    /// Records a gesture event.
    /// </summary>
    /// <param name="description">Description of the gesture.</param>
    /// <param name="data">Optional gesture data.</param>
    public void RecordGestureEvent(string description, object? data = null)
    {
        _session?.RecordEvent(RecordingEventType.Gesture, description, data);
    }

    /// <summary>
    /// Adds a custom marker event to the recording.
    /// </summary>
    /// <param name="name">Marker name.</param>
    /// <param name="data">Optional marker data.</param>
    public void AddMarker(string name, object? data = null)
    {
        _session?.RecordEvent(RecordingEventType.Marker, name, data);
    }

    /// <summary>
    /// Records a generic event during the recording.
    /// </summary>
    /// <param name="eventType">Type of event.</param>
    /// <param name="description">Event description.</param>
    /// <param name="data">Optional event data.</param>
    public void RecordEvent(RecordingEventType eventType, string description, object? data = null)
    {
        _session?.RecordEvent(eventType, description, data);
    }

    /// <summary>
    /// Captures frames during an action.
    /// </summary>
    /// <param name="action">The action to record.</param>
    /// <param name="frameInterval">Interval between frames in milliseconds.</param>
    public async Task RecordActionAsync(Func<Task> action, int frameInterval = 16)
    {
        if (!IsRecording)
            throw new InvalidOperationException("Recording must be started before recording actions.");

        using var cts = new CancellationTokenSource();

        var captureTask = Task.Run(async () =>
        {
            while (!cts.Token.IsCancellationRequested)
            {
                await Dispatcher.UIThread.InvokeAsync(CaptureFrame);
                await Task.Delay(frameInterval, cts.Token).ConfigureAwait(false);
            }
        }, cts.Token);

        try
        {
            await action();
        }
        finally
        {
            cts.Cancel();
            try
            {
                await captureTask;
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        }
    }

    /// <summary>
    /// Records frames for a specific duration.
    /// </summary>
    /// <param name="duration">Duration to record.</param>
    /// <param name="frameInterval">Interval between frames in milliseconds.</param>
    public async Task RecordDurationAsync(TimeSpan duration, int frameInterval = 16)
    {
        if (!IsRecording)
            throw new InvalidOperationException("Recording must be started before recording duration.");

        var endTime = DateTime.UtcNow + duration;

        while (DateTime.UtcNow < endTime && IsRecording)
        {
            await Dispatcher.UIThread.InvokeAsync(CaptureFrame);
            await Task.Delay(frameInterval).ConfigureAwait(false);
        }
    }

    private bool CaptureFrameInternal()
    {
        if (_target == null || _session == null)
        {
            LastCaptureFailureReason = _target == null ? "Target is null (internal)" : "Session is null (internal)";
            return false;
        }

        // Force layout and render pass
        Dispatcher.UIThread.RunJobs();

        // Ensure layout is up to date
        if (_target is Control control)
        {
            control.UpdateLayout();
        }

        // Run jobs again after layout
        Dispatcher.UIThread.RunJobs();

        // Force a render timer tick (like Avalonia.Headless does)
        ForceRenderTimerTick();

        // Run jobs again after render tick
        Dispatcher.UIThread.RunJobs();

        // Try to capture the rendered frame using Avalonia's headless APIs
        var bitmap = CaptureTargetBitmap();

        if (bitmap == null)
        {
            // LastCaptureFailureReason is already set in CaptureTargetBitmap
            if (string.IsNullOrEmpty(LastCaptureFailureReason))
            {
                LastCaptureFailureReason = "CaptureTargetBitmap returned null (unknown reason)";
            }
            return false;
        }

        // Check for duplicate frame if that option is enabled
        if (_session.Options.CaptureOnChangeOnly && _previousFrame != null)
        {
            if (AreBitmapsEqual(bitmap, _previousFrame))
            {
                bitmap.Dispose();
                LastCaptureFailureReason = "Duplicate frame skipped";
                return false;
            }
        }

        // Create captured frame
        var frame = CreateCapturedFrame(bitmap, _frameNumber, _session.Elapsed);

        _session.RecordFrame(frame);
        FrameCaptured?.Invoke(this, new FrameCapturedEventArgs(_frameNumber, _session.Elapsed));

        _previousFrame?.Dispose();
        _previousFrame = bitmap;
        _frameNumber++;

        return true;
    }

    private WriteableBitmap? CaptureTargetBitmap()
    {
        if (_target == null)
        {
            LastCaptureFailureReason = "Target is null in CaptureTargetBitmap";
            return null;
        }

        try
        {
            // Use Avalonia's headless extension to capture the rendered frame
            // This requires UseHeadlessDrawing = false in AvaloniaHeadlessPlatformOptions
            var frame = HeadlessExtensions.CaptureRenderedFrame(_target);
            if (frame == null)
            {
                LastCaptureFailureReason = "HeadlessExtensions.CaptureRenderedFrame returned null";
            }
            return frame;
        }
        catch (NotSupportedException ex)
        {
            // Headless drawing is enabled - use RenderTargetBitmap fallback
            var result = CaptureUsingRenderTargetBitmap();
            if (result == null)
            {
                LastCaptureFailureReason = $"RenderTargetBitmap fallback failed after NotSupportedException: {ex.Message}";
            }
            return result;
        }
        catch (Exception ex)
        {
            var result = CaptureUsingRenderTargetBitmap();
            if (result == null)
            {
                LastCaptureFailureReason = $"RenderTargetBitmap fallback failed after Exception: {ex.Message}";
            }
            return result;
        }
    }

    private WriteableBitmap? CaptureUsingRenderTargetBitmap()
    {
        if (_target == null)
            return null;

        try
        {
            var bounds = _target.Bounds;
            var width = Math.Max(1, (int)bounds.Width);
            var height = Math.Max(1, (int)bounds.Height);

            var pixelSize = new PixelSize(width, height);
            var dpi = new Vector(96, 96);

            var renderTarget = new RenderTargetBitmap(pixelSize, dpi);
            renderTarget.Render(_target);

            // Convert RenderTargetBitmap to WriteableBitmap by copying pixel data
            var writeable = new WriteableBitmap(pixelSize, dpi, Avalonia.Platform.PixelFormat.Rgba8888, AlphaFormat.Premul);

            // Copy pixels from render target to writeable bitmap
            CopyBitmapData(renderTarget, writeable);

            renderTarget.Dispose();
            return writeable;
        }
        catch
        {
            return null;
        }
    }

    private static void CopyBitmapData(RenderTargetBitmap source, WriteableBitmap dest)
    {
        using var destLock = dest.Lock();
        var destSize = destLock.RowBytes * destLock.Size.Height;

        unsafe
        {
            var destPtr = destLock.Address;
            source.CopyPixels(new PixelRect(0, 0, destLock.Size.Width, destLock.Size.Height),
                destPtr, destSize, destLock.RowBytes);
        }
    }

    private CapturedFrame CreateCapturedFrame(WriteableBitmap bitmap, int frameNumber, TimeSpan timestamp)
    {
        using var lockedBitmap = bitmap.Lock();

        var width = lockedBitmap.Size.Width;
        var height = lockedBitmap.Size.Height;
        var stride = lockedBitmap.RowBytes;
        var dataSize = stride * height;

        var pixelData = new byte[dataSize];

        unsafe
        {
            var srcPtr = (byte*)lockedBitmap.Address.ToPointer();
            for (int i = 0; i < dataSize; i++)
            {
                pixelData[i] = srcPtr[i];
            }
        }

        return new CapturedFrame(
            frameNumber,
            timestamp,
            width,
            height,
            stride,
            ConvertPixelFormat(lockedBitmap.Format),
            pixelData,
            lockedBitmap.Dpi);
    }

    private static PixelFormat ConvertPixelFormat(Avalonia.Platform.PixelFormat format)
    {
        if (format == Avalonia.Platform.PixelFormat.Rgba8888)
            return PixelFormat.Rgba8888;
        if (format == Avalonia.Platform.PixelFormat.Bgra8888)
            return PixelFormat.Bgra8888;
        return PixelFormat.Rgba8888;
    }

    private static bool AreBitmapsEqual(WriteableBitmap a, WriteableBitmap b)
    {
        if (a.PixelSize != b.PixelSize)
            return false;

        using var lockA = a.Lock();
        using var lockB = b.Lock();

        if (lockA.RowBytes != lockB.RowBytes)
            return false;

        var size = lockA.RowBytes * lockA.Size.Height;

        unsafe
        {
            var ptrA = (byte*)lockA.Address.ToPointer();
            var ptrB = (byte*)lockB.Address.ToPointer();

            for (int i = 0; i < size; i++)
            {
                if (ptrA[i] != ptrB[i])
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Forces a render timer tick using the Avalonia headless platform.
    /// </summary>
    private static void ForceRenderTimerTick()
    {
        try
        {
            // Use reflection to call AvaloniaHeadlessPlatform.ForceRenderTimerTick()
            var headlessPlatformType = Type.GetType("Avalonia.Headless.AvaloniaHeadlessPlatform, Avalonia.Headless");
            var forceRenderMethod = headlessPlatformType?.GetMethod("ForceRenderTimerTick",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            forceRenderMethod?.Invoke(null, null);
        }
        catch
        {
            // Platform method not available - this is fine, just means we're not in headless mode
        }
    }

    private void StartAutoCapture(double intervalMs)
    {
        _autoCaptureTokenSource = new CancellationTokenSource();
        var token = _autoCaptureTokenSource.Token;

        _autoCaptureTask = Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Dispatcher.UIThread.InvokeAsync(CaptureFrame);
                    await Task.Delay((int)intervalMs, token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    RecordingError?.Invoke(this, new RecordingErrorEventArgs(ex, "Auto-capture error"));
                }
            }
        }, token);
    }

    private void StopAutoCapture()
    {
        _autoCaptureTokenSource?.Cancel();

        try
        {
            _autoCaptureTask?.Wait(TimeSpan.FromSeconds(1));
        }
        catch (AggregateException)
        {
            // Expected if cancelled
        }

        _autoCaptureTokenSource?.Dispose();
        _autoCaptureTokenSource = null;
        _autoCaptureTask = null;
    }

    private void StartRenderHookCapture(TopLevel target)
    {
        _renderCapture = new RenderFrameCapture();
        _renderCapture.FrameCaptured += OnRenderFrameCaptured;
        _renderCapture.StartCapture(target);
    }

    private void StopRenderHookCapture()
    {
        var capture = _renderCapture;
        _renderCapture = null;
        if (capture == null)
            return;

        capture.FrameCaptured -= OnRenderFrameCaptured;
        try
        {
            capture.StopCapture();
            foreach (var frame in capture.GetCapturedFrames())
                _session?.RecordFrame(frame);
        }
        finally
        {
            capture.Dispose();
        }
    }

    private void OnRenderFrameCaptured(object? sender, FrameCapturedEventArgs e)
    {
        if (_session != null && _renderCapture != null)
        {
            // Get the frame and record it
            if (_renderCapture.TryGetFrame(out var frame) && frame != null)
            {
                _session.RecordFrame(frame);
                _frameNumber++;
                FrameCaptured?.Invoke(this, e);
            }
        }
    }

    /// <summary>
    /// Disposes the recorder.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        var session = _session;
        try
        {
            // Paused and duration-limited sessions still have frames to finalize.
            if (session != null)
                StopRecording();
        }
        catch
        {
            // Preserve disposal behavior if output finalization fails.
        }
        finally
        {
            StopAutoCapture();
            try
            {
                StopRenderHookCapture();
            }
            finally
            {
                _previousFrame?.Dispose();
                _previousFrame = null;
                session?.Dispose();
                _session = null;
                _target = null;
                GC.SuppressFinalize(this);
            }
        }
    }

}

/// <summary>
/// Event arguments for recording errors.
/// </summary>
public class RecordingErrorEventArgs : EventArgs
{
    /// <summary>
    /// Gets the exception that caused the error.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Gets the error message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Creates new recording error event arguments.
    /// </summary>
    public RecordingErrorEventArgs(Exception exception, string message)
    {
        Exception = exception;
        Message = message;
    }
}

/// <summary>
/// Extension methods for headless frame capture.
/// </summary>
public static class HeadlessExtensions
{
    /// <summary>
    /// Captures the currently rendered frame from a TopLevel.
    /// </summary>
    /// <param name="topLevel">The top-level to capture.</param>
    /// <returns>A WriteableBitmap containing the rendered frame, or null if capture failed.</returns>
    /// <remarks>
    /// This method requires:
    /// 1. Avalonia.Headless to be initialized
    /// 2. UseHeadlessDrawing = false in AvaloniaHeadlessPlatformOptions
    /// 3. A real rendering backend (e.g., Skia) via .UseSkia()
    /// </remarks>
    public static WriteableBitmap? CaptureRenderedFrame(this TopLevel topLevel)
    {
        LastRenderDebugInfo = "CaptureRenderedFrame started";

        // Process any pending jobs to ensure rendering is up to date
        Dispatcher.UIThread.RunJobs();

        // Force a render tick if the headless platform supports it
        try
        {
            // Use reflection to access Avalonia.Headless internal methods
            var headlessPlatformType = Type.GetType("Avalonia.Headless.AvaloniaHeadlessPlatform, Avalonia.Headless");
            var forceRenderMethod = headlessPlatformType?.GetMethod("ForceRenderTimerTick",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            forceRenderMethod?.Invoke(null, null);
            LastRenderDebugInfo += ", ForceRenderTick invoked";
        }
        catch (Exception ex)
        {
            // Platform method not available, continue without forcing render
            LastRenderDebugInfo += $", ForceRenderTick failed: {ex.Message}";
        }

        // Try to get the last rendered frame from the headless window implementation
        try
        {
            var platformImpl = topLevel.PlatformImpl;
            if (platformImpl == null)
            {
                LastRenderDebugInfo += ", PlatformImpl is null";
                return RenderToWriteableBitmap(topLevel);
            }

            LastRenderDebugInfo += $", PlatformImpl type: {platformImpl.GetType().Name}";

            // Try to call GetLastRenderedFrame via reflection
            var getFrameMethod = platformImpl.GetType().GetMethod("GetLastRenderedFrame");
            if (getFrameMethod != null)
            {
                var frame = getFrameMethod.Invoke(platformImpl, null) as WriteableBitmap;
                if (frame != null)
                {
                    LastRenderDebugInfo += ", GetLastRenderedFrame succeeded";
                    return frame;
                }
                LastRenderDebugInfo += ", GetLastRenderedFrame returned null";
            }
            else
            {
                LastRenderDebugInfo += ", GetLastRenderedFrame method not found";
            }
        }
        catch (Exception ex)
        {
            // Method not available
            LastRenderDebugInfo += $", GetLastRenderedFrame exception: {ex.Message}";
        }

        // Fallback: render using RenderTargetBitmap
        LastRenderDebugInfo += ", falling back to RenderToWriteableBitmap";
        return RenderToWriteableBitmap(topLevel);
    }

    /// <summary>
    /// Debug info for last render attempt.
    /// </summary>
    public static string? LastRenderDebugInfo { get; private set; }

    /// <summary>
    /// Renders a visual to a WriteableBitmap.
    /// </summary>
    /// <param name="visual">The visual to render.</param>
    /// <returns>A WriteableBitmap containing the rendered visual.</returns>
    public static WriteableBitmap? RenderToWriteableBitmap(Visual visual)
    {
        // Force layout update first
        if (visual is Control control)
        {
            control.UpdateLayout();
        }
        Dispatcher.UIThread.RunJobs();

        var bounds = visual.Bounds;
        var width = Math.Max(1, (int)bounds.Width);
        var height = Math.Max(1, (int)bounds.Height);

        // For TopLevel, use ClientSize which is more reliable
        if (visual is TopLevel topLevel)
        {
            width = Math.Max(1, (int)topLevel.ClientSize.Width);
            height = Math.Max(1, (int)topLevel.ClientSize.Height);
        }

        LastRenderDebugInfo = $"Bounds={bounds.Width}x{bounds.Height}, ClientSize={width}x{height}";

        if (width <= 0 || height <= 0)
        {
            LastRenderDebugInfo += ", INVALID SIZE";
            return null;
        }

        var pixelSize = new PixelSize(width, height);
        var dpi = new Vector(96, 96);

        try
        {
            var renderTarget = new RenderTargetBitmap(pixelSize, dpi);
            renderTarget.Render(visual);

            var writeable = new WriteableBitmap(pixelSize, dpi, Avalonia.Platform.PixelFormat.Rgba8888, AlphaFormat.Premul);

            // Copy pixels using CopyPixels method
            using var destLock = writeable.Lock();
            var destSize = destLock.RowBytes * destLock.Size.Height;

            unsafe
            {
                var destPtr = destLock.Address;
                renderTarget.CopyPixels(new PixelRect(0, 0, destLock.Size.Width, destLock.Size.Height),
                    destPtr, destSize, destLock.RowBytes);
            }

            renderTarget.Dispose();
            LastRenderDebugInfo += ", SUCCESS";
            return writeable;
        }
        catch (Exception ex)
        {
            LastRenderDebugInfo += $", EXCEPTION: {ex.Message}";
            return null;
        }
    }
}

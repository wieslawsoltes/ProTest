// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Avalonia.HeadlessTestingFramework.Recording;

/// <summary>
/// Manages a buffer of captured frames for recording.
/// Thread-safe for concurrent capture and consumption.
/// </summary>
public class FrameBuffer : IDisposable
{
    private readonly ConcurrentQueue<CapturedFrame> _frames = new();
    private readonly int _maxFrames;
    private readonly object _lock = new();
    private int _frameCount;
    private bool _disposed;
    private long _totalBytesBuffered;
    private int _droppedFrameCount;

    /// <summary>
    /// Gets the current number of frames in the buffer.
    /// </summary>
    public int Count => Volatile.Read(ref _frameCount);

    /// <summary>
    /// Gets the maximum number of frames this buffer can hold.
    /// </summary>
    public int MaxFrames => _maxFrames;

    /// <summary>
    /// Gets the total bytes currently buffered.
    /// </summary>
    public long TotalBytesBuffered => Interlocked.Read(ref _totalBytesBuffered);

    /// <summary>
    /// Gets the number of frames that were dropped due to buffer overflow.
    /// </summary>
    public int DroppedFrameCount => Volatile.Read(ref _droppedFrameCount);

    /// <summary>
    /// Gets whether the buffer is full.
    /// </summary>
    public bool IsFull => Count >= _maxFrames;

    /// <summary>
    /// Gets whether the buffer is empty.
    /// </summary>
    public bool IsEmpty => Count == 0;

    /// <summary>
    /// Event raised when the buffer becomes full.
    /// </summary>
    public event EventHandler? BufferFull;

    /// <summary>
    /// Event raised when a frame is dropped.
    /// </summary>
    public event EventHandler<FrameDroppedEventArgs>? FrameDropped;

    /// <summary>
    /// Creates a new frame buffer with the specified maximum capacity.
    /// </summary>
    /// <param name="maxFrames">Maximum number of frames to buffer.</param>
    public FrameBuffer(int maxFrames = 1000)
    {
        if (maxFrames <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxFrames), "Must be greater than 0.");

        _maxFrames = maxFrames;
    }

    /// <summary>
    /// Adds a frame to the buffer.
    /// </summary>
    /// <param name="frame">The frame to add.</param>
    /// <returns>True if the frame was added, false if the buffer is full.</returns>
    public bool TryAdd(CapturedFrame frame)
    {
        if (frame == null)
            throw new ArgumentNullException(nameof(frame));

        lock (_lock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_frameCount >= _maxFrames)
            {
                Interlocked.Increment(ref _droppedFrameCount);
                FrameDropped?.Invoke(this, new FrameDroppedEventArgs(frame.FrameNumber, FrameDropReason.BufferFull));
                return false;
            }

            _frames.Enqueue(frame);
            _frameCount++;
            Interlocked.Add(ref _totalBytesBuffered, frame.PixelData.Length);

            if (_frameCount >= _maxFrames)
            {
                BufferFull?.Invoke(this, EventArgs.Empty);
            }

            return true;
        }
    }

    /// <summary>
    /// Attempts to take a frame from the buffer.
    /// </summary>
    /// <param name="frame">The frame if available.</param>
    /// <returns>True if a frame was retrieved, false if the buffer is empty.</returns>
    public bool TryTake(out CapturedFrame? frame)
    {
        lock (_lock)
        {
            if (_disposed)
            {
                frame = null;
                return false;
            }
            if (_frames.TryDequeue(out frame))
            {
                _frameCount--;
                Interlocked.Add(ref _totalBytesBuffered, -frame.PixelData.Length);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets all frames from the buffer without removing them.
    /// </summary>
    /// <returns>A copy of all buffered frames.</returns>
    public IReadOnlyList<CapturedFrame> GetAllFrames()
    {
        lock (_lock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _frames.ToArray();
        }
    }

    /// <summary>
    /// Takes all frames from the buffer, clearing it.
    /// </summary>
    /// <returns>All frames that were in the buffer.</returns>
    public IReadOnlyList<CapturedFrame> TakeAll()
    {
        lock (_lock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var frames = new List<CapturedFrame>();
            while (_frames.TryDequeue(out var frame))
                frames.Add(frame);
            _frameCount = 0;
            Interlocked.Exchange(ref _totalBytesBuffered, 0);
            return frames;
        }
    }

    /// <summary>
    /// Clears all frames from the buffer and disposes them.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            while (_frames.TryDequeue(out var frame))
                frame.Dispose();
            _frameCount = 0;
            Interlocked.Exchange(ref _totalBytesBuffered, 0);
        }
    }

    /// <summary>
    /// Disposes the frame buffer and all contained frames.
    /// </summary>
    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed)
                return;
            _disposed = true;
            Clear();
        }
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Event arguments for when a frame is dropped.
/// </summary>
public class FrameDroppedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the frame number that was dropped.
    /// </summary>
    public int FrameNumber { get; }

    /// <summary>
    /// Gets the reason the frame was dropped.
    /// </summary>
    public FrameDropReason Reason { get; }

    /// <summary>
    /// Creates new frame dropped event arguments.
    /// </summary>
    public FrameDroppedEventArgs(int frameNumber, FrameDropReason reason)
    {
        FrameNumber = frameNumber;
        Reason = reason;
    }
}

/// <summary>
/// Reasons why a frame might be dropped.
/// </summary>
public enum FrameDropReason
{
    /// <summary>
    /// The frame buffer was full.
    /// </summary>
    BufferFull,

    /// <summary>
    /// The frame was a duplicate of the previous frame.
    /// </summary>
    Duplicate,

    /// <summary>
    /// The frame capture timed out.
    /// </summary>
    Timeout,

    /// <summary>
    /// The frame capture failed for an unknown reason.
    /// </summary>
    CaptureError
}

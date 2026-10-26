// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;

namespace Avalonia.HeadlessTestingFramework.Recording;

/// <summary>
/// Provides render-hooked frame capture by integrating with Avalonia's composition system.
/// Captures frames automatically when the visual tree is actually rendered.
/// </summary>
public class RenderFrameCapture : IDisposable
{
    private readonly ConcurrentQueue<CapturedFrame> _capturedFrames = new();
    private TopLevel? _target;
    private CompositionCustomVisual? _captureVisual;
    private RenderCaptureHandler? _captureHandler;
    private bool _isCapturing;
    private int _frameNumber;
    private bool _disposed;
    private int _captureGeneration;
    private int _maxBufferedFrames = 100;
    private WriteableBitmap? _lastFrame;

    /// <summary>
    /// Gets whether frame capture is currently active.
    /// </summary>
    public bool IsCapturing => _isCapturing;

    /// <summary>
    /// Gets the number of frames captured.
    /// </summary>
    public int FramesCaptured => _frameNumber;

    /// <summary>
    /// Gets or sets the maximum number of frames to buffer.
    /// </summary>
    public int MaxBufferedFrames
    {
        get => _maxBufferedFrames;
        set => _maxBufferedFrames = Math.Max(1, value);
    }

    /// <summary>
    /// Event raised when a frame is captured.
    /// </summary>
    public event EventHandler<FrameCapturedEventArgs>? FrameCaptured;

    /// <summary>
    /// Event raised when the frame buffer is full.
    /// </summary>
    public event EventHandler? BufferFull;

    /// <summary>
    /// Starts capturing frames from the specified target.
    /// </summary>
    /// <param name="target">The top-level control to capture from.</param>
    public void StartCapture(TopLevel target)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RenderFrameCapture));

        if (_isCapturing)
            throw new InvalidOperationException("Capture is already in progress.");

        _target = target;
        _frameNumber = 0;
        _isCapturing = true;

        // Set up the composition hook
        SetupCompositionHook(target, ++_captureGeneration);
    }

    /// <summary>
    /// Stops capturing frames.
    /// </summary>
    public void StopCapture()
    {
        _isCapturing = false;
        _captureGeneration++;

        // Remove the composition hook
        if (_captureVisual != null && _target != null)
        {
            RemoveCompositionHook();
        }

        _target = null;
    }

    /// <summary>
    /// Gets all captured frames and clears the buffer.
    /// </summary>
    /// <returns>Array of captured frames.</returns>
    public CapturedFrame[] GetCapturedFrames()
    {
        var frames = new System.Collections.Generic.List<CapturedFrame>();
        while (_capturedFrames.TryDequeue(out var frame))
        {
            frames.Add(frame);
        }
        return frames.ToArray();
    }

    /// <summary>
    /// Tries to get the next captured frame.
    /// </summary>
    /// <param name="frame">The captured frame if available.</param>
    /// <returns>True if a frame was available.</returns>
    public bool TryGetFrame(out CapturedFrame? frame)
    {
        return _capturedFrames.TryDequeue(out frame);
    }

    /// <summary>
    /// Forces a frame capture immediately using RenderTargetBitmap.
    /// </summary>
    /// <returns>The captured frame or null if capture failed.</returns>
    public CapturedFrame? CaptureNow()
    {
        if (_target == null || !_isCapturing)
            return null;

        return CaptureFrameUsingRenderTarget(_target);
    }

    private void SetupCompositionHook(TopLevel target, int generation)
    {
        // Method 1: Try to use composition visual snapshot approach
        // This hooks into the actual compositor

        Dispatcher.UIThread.Post(() =>
        {
            if (_disposed || !_isCapturing || generation != _captureGeneration)
                return;
            try
            {
                // Create a custom visual handler that gets called on each render frame
                _captureHandler = new RenderCaptureHandler(this, target);

                // Get the element composition and add our capture visual
                var compositor = ElementComposition.GetElementVisual(target)?.Compositor;
                if (compositor != null)
                {
                    _captureVisual = compositor.CreateCustomVisual(_captureHandler);
                    _captureVisual.Size = new Vector(target.ClientSize.Width, target.ClientSize.Height);

                    // Attach to the visual tree
                    var rootVisual = ElementComposition.GetElementVisual(target);
                    if (rootVisual is CompositionContainerVisual container)
                    {
                        container.Children.Add(_captureVisual);
                    }

                    // Request first frame
                    _captureHandler.RegisterForNextAnimationFrameUpdate();
                }
            }
            catch (Exception)
            {
                // Composition hook not available, will fall back to manual capture
            }
        }, DispatcherPriority.Render);
    }

    private void RemoveCompositionHook()
    {
        // Capture the old references before StopCapture clears the target or a new
        // capture starts. The dispatcher callback must not read the replacement state.
        var visual = _captureVisual;
        var target = _target;
        _captureVisual = null;
        _captureHandler = null;
        if (visual == null || target == null)
            return;
        Dispatcher.UIThread.Post(() =>
        {
            try
            {
                if (ElementComposition.GetElementVisual(target) is CompositionContainerVisual container)
                    container.Children.Remove(visual);
            }
            catch
            {
                // The target may already have been closed.
            }
        }, DispatcherPriority.Render);
    }

    internal void OnFrameRendered(TimeSpan compositionTime)
    {
        if (!_isCapturing || _target == null)
            return;

        // Capture the frame
        var frame = CaptureFrameUsingRenderTarget(_target);
        if (frame != null)
        {
            if (_capturedFrames.Count < _maxBufferedFrames)
            {
                _capturedFrames.Enqueue(frame);
                _frameNumber++;
                FrameCaptured?.Invoke(this, new FrameCapturedEventArgs(_frameNumber, compositionTime));
            }
            else
            {
                BufferFull?.Invoke(this, EventArgs.Empty);
            }
        }

        // Request next frame
        _captureHandler?.RegisterForNextAnimationFrameUpdate();
    }

    private CapturedFrame? CaptureFrameUsingRenderTarget(TopLevel target)
    {
        try
        {
            Dispatcher.UIThread.RunJobs();

            var width = Math.Max(1, (int)target.ClientSize.Width);
            var height = Math.Max(1, (int)target.ClientSize.Height);

            if (width <= 0 || height <= 0)
                return null;

            var pixelSize = new PixelSize(width, height);
            var dpi = new Vector(96, 96);

            var renderTarget = new RenderTargetBitmap(pixelSize, dpi);
            renderTarget.Render(target);

            var writeable = new WriteableBitmap(pixelSize, dpi, Avalonia.Platform.PixelFormat.Rgba8888, AlphaFormat.Premul);

            // Copy pixels
            using (var destLock = writeable.Lock())
            {
                var destSize = destLock.RowBytes * destLock.Size.Height;
                unsafe
                {
                    var destPtr = destLock.Address;
                    renderTarget.CopyPixels(new PixelRect(0, 0, destLock.Size.Width, destLock.Size.Height),
                        destPtr, destSize, destLock.RowBytes);
                }
            }

            renderTarget.Dispose();

            // Create the captured frame
            var timestamp = TimeSpan.FromTicks(DateTime.UtcNow.Ticks);
            var frame = CreateCapturedFrame(writeable, _frameNumber, timestamp);

            _lastFrame?.Dispose();
            _lastFrame = writeable;

            return frame;
        }
        catch
        {
            return null;
        }
    }

    private CapturedFrame CreateCapturedFrame(WriteableBitmap bitmap, int frameNumber, TimeSpan timestamp)
    {
        using var lockedBitmap = bitmap.Lock();

        var width = lockedBitmap.Size.Width;
        var height = lockedBitmap.Size.Height;
        var rowBytes = lockedBitmap.RowBytes;
        var totalBytes = rowBytes * height;

        var pixelData = new byte[totalBytes];
        unsafe
        {
            var srcPtr = (byte*)lockedBitmap.Address.ToPointer();
            for (int i = 0; i < totalBytes; i++)
            {
                pixelData[i] = srcPtr[i];
            }
        }

        return new CapturedFrame(
            frameNumber,
            timestamp,
            width,
            height,
            rowBytes,
            PixelFormat.Rgba8888,
            pixelData,
            new Vector(96, 96)
        );
    }

    /// <summary>
    /// Disposes the render frame capture.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        StopCapture();

        while (_capturedFrames.TryDequeue(out var frame))
        {
            frame.Dispose();
        }

        _lastFrame?.Dispose();
        _lastFrame = null;

        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Custom composition visual handler for capturing render frames.
/// </summary>
internal class RenderCaptureHandler : CompositionCustomVisualHandler
{
    private readonly RenderFrameCapture _capture;
    private readonly TopLevel _target;

    public RenderCaptureHandler(RenderFrameCapture capture, TopLevel target)
    {
        _capture = capture;
        _target = target;
    }

    /// <summary>
    /// Called on each animation frame update - this is our hook into the render loop.
    /// </summary>
    public override void OnAnimationFrameUpdate()
    {
        // Notify the capture system that a frame has been rendered
        _capture.OnFrameRendered(CompositionNow);
    }

    /// <summary>
    /// Called when rendering - we don't actually draw anything, just observe.
    /// </summary>
    public override void OnRender(ImmediateDrawingContext drawingContext)
    {
        // We don't render anything - this is just an observer
    }

    /// <summary>
    /// Requests callback on the next animation frame.
    /// </summary>
    public new void RegisterForNextAnimationFrameUpdate()
    {
        base.RegisterForNextAnimationFrameUpdate();
    }
}

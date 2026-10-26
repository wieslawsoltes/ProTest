// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System;

namespace Avalonia.HeadlessTestingFramework.Recording;

/// <summary>
/// Represents a single captured frame.
/// </summary>
public class CapturedFrame : IDisposable
{
    private byte[]? _pixelData;
    private bool _disposed;

    /// <summary>
    /// Gets the frame number (0-based index).
    /// </summary>
    public int FrameNumber { get; }

    /// <summary>
    /// Gets the timestamp when this frame was captured.
    /// </summary>
    public TimeSpan Timestamp { get; }

    /// <summary>
    /// Gets the width of the frame in pixels.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the height of the frame in pixels.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets the stride (bytes per row) of the frame.
    /// </summary>
    public int Stride { get; }

    /// <summary>
    /// Gets the pixel format of the frame.
    /// </summary>
    public PixelFormat PixelFormat { get; }

    /// <summary>
    /// Gets the raw pixel data.
    /// </summary>
    public ReadOnlySpan<byte> PixelData => _pixelData ?? ReadOnlySpan<byte>.Empty;

    /// <summary>
    /// Gets whether this frame has been disposed.
    /// </summary>
    public bool IsDisposed => _disposed;

    /// <summary>
    /// Gets the DPI vector of the captured frame.
    /// </summary>
    public Vector Dpi { get; }

    /// <summary>
    /// Creates a new captured frame.
    /// </summary>
    /// <param name="frameNumber">The frame number.</param>
    /// <param name="timestamp">The capture timestamp.</param>
    /// <param name="width">Frame width in pixels.</param>
    /// <param name="height">Frame height in pixels.</param>
    /// <param name="stride">Bytes per row.</param>
    /// <param name="pixelFormat">The pixel format.</param>
    /// <param name="pixelData">The raw pixel data.</param>
    /// <param name="dpi">The DPI vector.</param>
    public CapturedFrame(
        int frameNumber,
        TimeSpan timestamp,
        int width,
        int height,
        int stride,
        PixelFormat pixelFormat,
        byte[] pixelData,
        Vector dpi)
    {
        ArgumentNullException.ThrowIfNull(pixelData);
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height));
        var bytesPerPixel = pixelFormat switch
        {
            PixelFormat.Rgb888 => 3,
            PixelFormat.Rgba8888 or PixelFormat.Bgra8888 => 4,
            _ => throw new ArgumentOutOfRangeException(nameof(pixelFormat))
        };
        if (stride < checked(width * bytesPerPixel))
            throw new ArgumentOutOfRangeException(nameof(stride), "Stride must contain a complete pixel row.");
        if (pixelData.Length < checked(stride * height))
            throw new ArgumentException("Pixel data must contain every frame row, including stride padding.", nameof(pixelData));

        FrameNumber = frameNumber;
        Timestamp = timestamp;
        Width = width;
        Height = height;
        Stride = stride;
        PixelFormat = pixelFormat;
        _pixelData = pixelData;
        Dpi = dpi;
    }

    /// <summary>
    /// Gets a copy of the pixel data.
    /// </summary>
    public byte[] GetPixelDataCopy()
    {
        if (_disposed || _pixelData == null)
            throw new ObjectDisposedException(nameof(CapturedFrame));

        var copy = new byte[_pixelData.Length];
        Array.Copy(_pixelData, copy, _pixelData.Length);
        return copy;
    }

    /// <summary>
    /// Releases the frame resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _pixelData = null;
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Pixel format for captured frames.
/// </summary>
public enum PixelFormat
{
    /// <summary>
    /// 32-bit RGBA format (8 bits per channel).
    /// </summary>
    Rgba8888,

    /// <summary>
    /// 32-bit BGRA format (8 bits per channel).
    /// </summary>
    Bgra8888,

    /// <summary>
    /// 24-bit RGB format (8 bits per channel).
    /// </summary>
    Rgb888
}

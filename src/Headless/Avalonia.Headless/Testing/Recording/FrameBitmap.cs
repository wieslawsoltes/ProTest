// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Avalonia.HeadlessTestingFramework.Recording;

internal static class FrameBitmap
{
    public static WriteableBitmap Create(CapturedFrame frame, AlphaFormat alphaFormat)
    {
        ObjectDisposedException.ThrowIf(frame.IsDisposed, frame);
        var format = frame.PixelFormat == PixelFormat.Bgra8888
            ? Platform.PixelFormat.Bgra8888
            : Platform.PixelFormat.Rgba8888;
        var bitmap = new WriteableBitmap(new PixelSize(frame.Width, frame.Height), frame.Dpi, format, alphaFormat);
        try
        {
            using var locked = bitmap.Lock();
            unsafe
            {
                for (var y = 0; y < frame.Height; y++)
                {
                    var destination = new Span<byte>((byte*)locked.Address + y * locked.RowBytes, frame.Width * 4);
                    var source = frame.PixelData.Slice(y * frame.Stride);
                    if (frame.PixelFormat == PixelFormat.Rgb888)
                    {
                        for (var x = 0; x < frame.Width; x++)
                        {
                            source.Slice(x * 3, 3).CopyTo(destination.Slice(x * 4, 3));
                            destination[x * 4 + 3] = 255;
                        }
                    }
                    else
                    {
                        source.Slice(0, destination.Length).CopyTo(destination);
                    }
                }
            }
            return bitmap;
        }
        catch
        {
            bitmap.Dispose();
            throw;
        }
    }
}

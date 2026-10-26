// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Avalonia.HeadlessTestingFramework.Recording;

/// <summary>
/// Writes GIF89a with a fixed 256-color RGB palette and an infinite animation loop.
/// Premultiplied alpha is flattened onto black. No external encoder is required.
/// </summary>
internal static class GifAnimationWriter
{
    public static void Write(string outputPath, IReadOnlyList<CapturedFrame> frames, int frameRate)
    {
        if (frameRate <= 0)
            throw new ArgumentOutOfRangeException(nameof(frameRate));
        var width = frames[0].Width;
        var height = frames[0].Height;
        if (width > ushort.MaxValue || height > ushort.MaxValue)
            throw new ArgumentException("GIF dimensions must fit in 16 bits.", nameof(frames));
        foreach (var frame in frames)
        {
            ObjectDisposedException.ThrowIf(frame.IsDisposed, frame);
            if (frame.Width != width || frame.Height != height)
                throw new ArgumentException("Every GIF frame must have the same dimensions.", nameof(frames));
        }

        using var writer = new BinaryWriter(File.Create(outputPath));
        writer.Write(Encoding.ASCII.GetBytes("GIF89a"));
        writer.Write((ushort)width);
        writer.Write((ushort)height);
        writer.Write((byte)0xf7); // Global 256-entry color table, eight-bit color resolution.
        writer.Write((byte)0);
        writer.Write((byte)0);
        for (var color = 0; color < 256; color++)
        {
            writer.Write((byte)(((color >> 5) & 7) * 255 / 7));
            writer.Write((byte)(((color >> 2) & 7) * 255 / 7));
            writer.Write((byte)((color & 3) * 255 / 3));
        }
        writer.Write(new byte[] { 0x21, 0xff, 11 });
        writer.Write(Encoding.ASCII.GetBytes("NETSCAPE2.0"));
        writer.Write(new byte[] { 3, 1, 0, 0, 0 });

        var delay = (ushort)Math.Clamp((int)Math.Round(100.0 / frameRate), 1, ushort.MaxValue);
        foreach (var frame in frames)
        {
            writer.Write(new byte[] { 0x21, 0xf9, 4, 4 }); // Keep the full opaque frame.
            writer.Write(delay);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((byte)0x2c);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write((ushort)width);
            writer.Write((ushort)height);
            writer.Write((byte)0);
            writer.Write((byte)8); // LZW minimum code size.

            using var encoded = new MemoryStream();
            var bits = 0;
            var bitCount = 0;
            void WriteCode(int code)
            {
                bits |= code << bitCount;
                bitCount += 9;
                while (bitCount >= 8)
                {
                    encoded.WriteByte((byte)bits);
                    bits >>= 8;
                    bitCount -= 8;
                }
            }

            // Reset before the dictionary reaches 512 entries to keep codes nine bits wide.
            // Literal codes favor predictable memory use over compression complexity.
            WriteCode(256);
            var literals = 0;
            var bytesPerPixel = frame.PixelFormat == PixelFormat.Rgb888 ? 3 : 4;
            var pixels = frame.PixelData;
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                if (literals == 250)
                {
                    WriteCode(256);
                    literals = 0;
                }
                var offset = y * frame.Stride + x * bytesPerPixel;
                var red = pixels[offset + (frame.PixelFormat == PixelFormat.Bgra8888 ? 2 : 0)];
                var green = pixels[offset + 1];
                var blue = pixels[offset + (frame.PixelFormat == PixelFormat.Bgra8888 ? 0 : 2)];
                WriteCode((red & 0xe0) | ((green >> 3) & 0x1c) | (blue >> 6));
                literals++;
            }
            WriteCode(257);
            if (bitCount > 0)
                encoded.WriteByte((byte)bits);
            var data = encoded.GetBuffer();
            for (var offset = 0; offset < encoded.Length; offset += 255)
            {
                var length = (int)Math.Min(255, encoded.Length - offset);
                writer.Write((byte)length);
                writer.Write(data, offset, length);
            }
            writer.Write((byte)0);
        }
        writer.Write((byte)0x3b);
    }
}

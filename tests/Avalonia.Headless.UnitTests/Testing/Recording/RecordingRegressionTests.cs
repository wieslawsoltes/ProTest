// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.HeadlessTestingFramework.Recording;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using SkiaSharp;
using Xunit;
using FramePixelFormat = Avalonia.HeadlessTestingFramework.Recording.PixelFormat;

namespace Avalonia.Headless.UnitTests.Testing.Recording;

public class RecordingRegressionTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"ProTestRecordingRegression_{Guid.NewGuid():N}");

    public RecordingRegressionTests() => Directory.CreateDirectory(_directory);

    public void Dispose() => Directory.Delete(_directory, true);

    [AvaloniaFact]
    public void Png_Encoder_Converts_Rgb_And_Respects_Source_Stride()
    {
        using var frame = new CapturedFrame(0, TimeSpan.Zero, 2, 2, 8, FramePixelFormat.Rgb888,
            new byte[] { 255, 0, 0, 0, 255, 0, 99, 99, 0, 0, 255, 255, 255, 255, 99, 99 }, new Vector(96, 96));
        using var encoder = new PngSequenceEncoder();
        encoder.Initialize(_directory, 2, 2, new RecordingOptions());
        encoder.EncodeFrame(frame);
        using var decoded = SKBitmap.Decode(Assert.Single(encoder.OutputFiles));
        Assert.NotNull(decoded);
        Assert.Equal(SKColors.Red, decoded.GetPixel(0, 0));
        Assert.Equal(SKColors.Lime, decoded.GetPixel(1, 0));
        Assert.Equal(SKColors.Blue, decoded.GetPixel(0, 1));
        Assert.Equal(SKColors.White, decoded.GetPixel(1, 1));
    }

    [AvaloniaFact]
    public void Png_Encoder_Respects_Bgra_Row_Padding()
    {
        using var frame = new CapturedFrame(0, TimeSpan.Zero, 1, 2, 8, FramePixelFormat.Bgra8888,
            new byte[] { 0, 0, 255, 255, 99, 99, 99, 99, 255, 0, 0, 255, 99, 99, 99, 99 }, new Vector(96, 96));
        using var encoder = new PngSequenceEncoder();
        encoder.Initialize(_directory, 1, 2, new RecordingOptions());
        encoder.EncodeFrame(frame);
        using var decoded = SKBitmap.Decode(Assert.Single(encoder.OutputFiles));
        Assert.Equal(SKColors.Red, decoded.GetPixel(0, 0));
        Assert.Equal(SKColors.Blue, decoded.GetPixel(0, 1));
    }

    [AvaloniaFact]
    public async Task Gif_FinalizeAsync_Writes_A_Decodable_Animation()
    {
        using var encoder = new GifEncoder();
        encoder.Initialize(_directory, 40, 30, RecordingOptions.AnimatedGif);
        using var red = SolidFrame(0, 40, 30, 255, 0, 0);
        using var green = SolidFrame(1, 40, 30, 0, 255, 0);
        encoder.EncodeFrame(red);
        encoder.EncodeFrame(green);
        await encoder.FinalizeAsync(TestContext.Current.CancellationToken);

        var output = Assert.Single(encoder.OutputFiles);
        Assert.True(File.Exists(output));
        using var codec = SKCodec.Create(output);
        Assert.NotNull(codec);
        Assert.Equal(2, codec.FrameCount);
        using var decoded = new SKBitmap(new SKImageInfo(40, 30, SKColorType.Rgba8888, SKAlphaType.Premul));
        Assert.Equal(SKCodecResult.Success, codec.GetPixels(decoded.Info, decoded.GetPixels(), new SKCodecOptions(0)));
        Assert.Equal(SKColors.Red, decoded.GetPixel(39, 29));
        Assert.Equal(SKCodecResult.Success, codec.GetPixels(decoded.Info, decoded.GetPixels(), new SKCodecOptions(1)));
        Assert.Equal(SKColors.Lime, decoded.GetPixel(39, 29));
        encoder.FinalizeEncoding();
        Assert.Single(encoder.OutputFiles);
    }

    [Fact]
    public void CapturedFrame_Rejects_Incomplete_Rows()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CapturedFrame(0, TimeSpan.Zero, 2, 1, 7,
            FramePixelFormat.Rgba8888, new byte[8], new Vector(96, 96)));
        Assert.Throws<ArgumentException>(() => new CapturedFrame(0, TimeSpan.Zero, 2, 2, 8,
            FramePixelFormat.Rgba8888, new byte[15], new Vector(96, 96)));
    }

    [Fact]
    public async Task Buffer_Draining_And_Concurrent_Additions_Keep_Counts_Consistent()
    {
        using var buffer = new FrameBuffer(10000);
        var added = 0;
        var removed = 0;
        var complete = 0;
        var cancellationToken = TestContext.Current.CancellationToken;
        var writer = Task.Run(() =>
        {
            try
            {
                for (var i = 0; i < 10000; i++)
                {
                    var frame = SolidFrame(i, 1, 1, 255, 0, 0);
                    if (buffer.TryAdd(frame))
                        added++;
                    else
                        frame.Dispose();
                }
            }
            finally { Volatile.Write(ref complete, 1); }
        }, cancellationToken);
        var reader = Task.Run(() =>
        {
            while (Volatile.Read(ref complete) == 0)
            {
                foreach (var frame in buffer.TakeAll())
                {
                    frame.Dispose();
                    removed++;
                }
            }
        }, cancellationToken);
        await Task.WhenAll(writer, reader);
        Assert.Equal(added, removed + buffer.Count);
        Assert.Equal(buffer.Count * 4L, buffer.TotalBytesBuffered);
        Assert.Equal(buffer.Count, buffer.GetAllFrames().Count);
    }

    [Fact]
    public void Session_Scales_Encoded_Frames_And_Does_Not_Restart_After_Finalizing()
    {
        using var session = new RecordingSession(new RecordingOptions { Format = RecordingFormat.RawFrames, ScaleFactor = 0.5 });
        session.Initialize(2, 1, _directory);
        session.Start();
        using var frame = SolidFrame(0, 4, 2, 255, 0, 0);
        session.RecordFrame(frame);
        session.FinalizeRecording();
        session.FinalizeRecording();
        var encoded = Assert.Single(Assert.IsType<RawFrameEncoder>(session.Encoder).Frames);
        Assert.Equal(2, encoded.Width);
        Assert.Equal(1, encoded.Height);
        Assert.Equal(8, encoded.PixelData.Length);
        Assert.False(encoded.IsDisposed);
        Assert.Throws<InvalidOperationException>(() => session.Start());
    }

    [Fact]
    public void Unsupported_Overlays_And_Nonfinite_Scale_Are_Rejected()
    {
        Assert.Throws<NotSupportedException>(() => new RecordingOptions { IncludeTimestamp = true }.Validate());
        Assert.Throws<NotSupportedException>(() => new RecordingOptions { IncludeFrameNumber = true }.Validate());
        Assert.Throws<ArgumentException>(() => new RecordingOptions { ScaleFactor = double.NaN }.Validate());
    }

    [AvaloniaFact]
    public void Disposing_A_Paused_Recorder_Finalizes_Buffered_Gif_Frames()
    {
        var window = new Window
        {
            Width = 40,
            Height = 30,
            Content = new Border { Background = Brushes.Red }
        };
        try
        {
            window.Show();
            using var recorder = new HeadlessScreenRecorder();
            recorder.StartRecording(window, new RecordingOptions
            {
                Format = RecordingFormat.Gif,
                AutoCapture = false,
                WriteImmediately = false
            }, _directory);
            Assert.True(recorder.CaptureFrame(), recorder.LastCaptureFailureReason);
            recorder.PauseRecording();
            Assert.Throws<InvalidOperationException>(() => recorder.StartRecording(window));
            recorder.Dispose();
            using var codec = SKCodec.Create(Path.Combine(_directory, "frame.gif"));
            Assert.NotNull(codec);
            Assert.Equal(1, codec.FrameCount);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Stopping_Render_Capture_Detaches_The_Visual_And_Cancels_Queued_Setup()
    {
        var window = new Window { Width = 40, Height = 30 };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var root = Assert.IsAssignableFrom<CompositionContainerVisual>(ElementComposition.GetElementVisual(window));
            var initialCount = root.Children.Count;
            using var capture = new RenderFrameCapture();
            capture.StartCapture(window);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(initialCount + 1, root.Children.Count);
            capture.StopCapture();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(initialCount, root.Children.Count);

            capture.StartCapture(window);
            capture.StopCapture();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(initialCount, root.Children.Count);
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData(VideoFormat.Mp4)]
    [InlineData(VideoFormat.Gif)]
    public void Video_Conversion_Scaling_Is_Culture_Independent_And_Preserved_For_Gif(VideoFormat format)
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("pl-PL");
            var buildArguments = typeof(VideoConverter).GetMethod("BuildFfmpegArguments", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var arguments = Assert.IsType<string>(buildArguments.Invoke(new VideoConverter(), new object[]
            {
                _directory,
                Path.Combine(_directory, "output.video"),
                new VideoConversionOptions { Format = format, Scale = 0.5 },
                new List<string> { Path.Combine(_directory, "frame_000000.png") }
            }));
            Assert.Contains("scale=iw*0.5:ih*0.5", arguments);
            Assert.DoesNotContain("0,5", arguments);
            if (format == VideoFormat.Gif)
                Assert.Contains("scale=iw*0.5:ih*0.5,split[s0][s1]", arguments);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    private static CapturedFrame SolidFrame(int number, int width, int height, byte red, byte green, byte blue)
    {
        var pixels = Enumerable.Range(0, width * height).SelectMany(_ => new byte[] { red, green, blue, 255 }).ToArray();
        return new CapturedFrame(number, TimeSpan.FromMilliseconds(number * 20), width, height,
            width * 4, FramePixelFormat.Rgba8888, pixels, new Vector(96, 96));
    }
}

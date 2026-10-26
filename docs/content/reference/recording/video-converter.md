---
title: "Video conversion"
description: "Examples and API guidance for video conversion."
sidebar_position: 3
doc-type: reference
---

Use `Avalonia.HeadlessTestingFramework.Recording` and configure [Skia rendering](../../guides/rendering.md) before recording. Run capture operations on the Avalonia UI thread.

The `VideoConverter` converts PNG sequences to video files using FFmpeg.

## Basic Usage

```csharp
var converter = new VideoConverter();

// Check FFmpeg availability
if (converter.IsFfmpegAvailable())
{
    Console.WriteLine($"FFmpeg version: {converter.GetFfmpegVersion()}");

    // Convert PNG sequence to video
    var result = converter.ConvertPngSequenceToVideo(
        inputDirectory: "/path/to/frames",
        outputPath: "/path/to/output.mp4",
        options: new VideoConversionOptions
        {
            Format = VideoFormat.Mp4,
            FrameRate = 30,
            Quality = 23
        });

    if (result.Success)
    {
        Console.WriteLine($"Video created: {result.OutputPath}");
        Console.WriteLine($"Frames processed: {result.FrameCount}");
        Console.WriteLine($"Duration: {result.Duration}");
    }
    else
    {
        Console.WriteLine($"Error: {result.ErrorMessage}");
    }
}
```

## Video Formats

| Format | Codec | Description |
|--------|-------|-------------|
| `Mp4` | H.264 | Most compatible, good quality |
| `WebM` | VP9 | Web-optimized, open format |
| `Gif` | GIF | Animated GIF (limited colors) |
| `Avi` | MJPEG | Legacy format |

## Conversion Options

```csharp
var options = new VideoConversionOptions
{
    Format = VideoFormat.Mp4,
    FrameRate = 30,              // Output frame rate
    Quality = 23,                // H.264 CRF (0-51, lower = better)
    Scale = 1.0,                 // Output scale (0.5 = half size)
    OverwriteExisting = true,
    DeleteSourceFiles = false,   // Keep PNG files after conversion
    Timeout = TimeSpan.FromMinutes(5),
    LoopCount = 0,               // GIF loop count (0 = infinite)
    FfmpegPath = null,           // Custom FFmpeg path (null = auto-detect)
    CustomArguments = null       // Additional FFmpeg arguments
};
```

## Async Conversion

```csharp
var result = await converter.ConvertPngSequenceToVideoAsync(
    inputDirectory: "/path/to/frames",
    options: options,
    cancellationToken: cts.Token);
```

## Convert Multiple Directories

```csharp
var results = await converter.ConvertMultipleAsync(
    directories: new[] { "/dir1", "/dir2", "/dir3" },
    options: options);

foreach (var (dir, result) in results)
{
    Console.WriteLine($"{dir}: {(result.Success ? "Success" : result.ErrorMessage)}");
}
```

## Extension Methods

```csharp
// Convert a recording session directly
var session = recorder.CurrentSession;
var result = await session.ToVideoAsync(
    outputPath: "/output.mp4",
    options: new VideoConversionOptions { Format = VideoFormat.Mp4 });

// Synchronous version
var result = session.ToVideo();
```

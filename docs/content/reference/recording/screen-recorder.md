---
title: "Screen recording"
description: "Examples and API guidance for screen recording."
sidebar_position: 1
doc-type: reference
---

Use `Avalonia.HeadlessTestingFramework.Recording` and configure [Skia rendering](../../guides/rendering.md) before recording. Run capture operations on the Avalonia UI thread.

The `HeadlessScreenRecorder` captures frames during UI interactions for visual testing.

## Basic Recording

```csharp
using var recorder = new HeadlessScreenRecorder();

// Start recording
var session = recorder.StartRecording(
    window,
    options: new RecordingOptions
    {
        FrameRate = 60,
        Format = RecordingFormat.PngSequence,
        Quality = 90
    },
    outputPath: "/path/to/output"
);

// Manual frame capture
recorder.CaptureFrame();

// Stop recording
RecordingStatistics stats = recorder.StopRecording();

Console.WriteLine($"Frames captured: {stats.FramesCaptured}");
Console.WriteLine($"Duration: {stats.Duration}");
```

## Recording Options

```csharp
// High quality preset
var options = RecordingOptions.HighQuality;

// Performance preset (lower quality, smaller files)
var options = RecordingOptions.Performance;

// Custom options
var options = new RecordingOptions
{
    FrameRate = 30,
    Format = RecordingFormat.PngSequence,
    Quality = 100,
    ScaleFactor = 1.0,
    AutoCapture = false,           // Manual capture mode
    WriteImmediately = true,       // Write frames to disk immediately
    MaxBufferedFrames = 1000,
    CaptureOnChangeOnly = false,   // Capture every frame
    IncludeTimestamp = false,
    IncludeFrameNumber = false,
    MaxDuration = TimeSpan.FromMinutes(5),
    OutputDirectory = "/custom/path"
};
```

## Recording Formats

| Format | Description |
|--------|-------------|
| `PngSequence` | Sequence of PNG images (lossless) |
| `JpegSequence` | Sequence of JPEG images (lossy, smaller) |
| `Gif` | Animated GIF (limited colors) |
| `RawFrames` | Raw frame data for custom processing |


## Recording behavior

`ScaleFactor` resizes captured pixels with nearest-neighbor sampling. `MaxDuration` stops a session when the next submitted frame exceeds that duration; it is not an independent timer.

`IncludeTimestamp` and `IncludeFrameNumber` must remain `false`: overlay rendering is not implemented and enabling either option throws `NotSupportedException`. Use recorded timeline events and markers for those diagnostics.

The built-in GIF encoder writes an animated GIF with a fixed 256-color palette and an infinite loop. It uses opaque output, flattens premultiplied alpha against black, and has a minimum frame delay of 10 milliseconds. Use PNG sequences for lossless frame storage or FFmpeg conversion when another encoding policy is required.

## Events

```csharp
recorder.RecordingStarted += (s, e) => Console.WriteLine("Recording started");
recorder.RecordingStopped += (s, e) => Console.WriteLine("Recording stopped");
recorder.FrameCaptured += (s, e) => Console.WriteLine($"Frame {e.FrameNumber} captured");
recorder.RecordingError += (s, e) => Console.WriteLine($"Error: {e.Exception.Message}");
```

## Async Recording

```csharp
// Record an async action
await recorder.RecordActionAsync(async () =>
{
    await someAnimationTask;
}, frameInterval: 16);

// Record for a duration
await recorder.RecordDurationAsync(TimeSpan.FromSeconds(2), frameInterval: 16);
```

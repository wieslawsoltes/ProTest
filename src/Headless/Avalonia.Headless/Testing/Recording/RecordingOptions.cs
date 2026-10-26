// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System;

namespace Avalonia.HeadlessTestingFramework.Recording;

/// <summary>
/// Configuration options for headless screen recording.
/// </summary>
public class RecordingOptions
{
    /// <summary>
    /// Gets or sets the target frame rate for recording.
    /// Default is 60 FPS.
    /// </summary>
    public int FrameRate { get; set; } = 60;

    /// <summary>
    /// Gets or sets the output format for the recording.
    /// Default is PNG sequence.
    /// </summary>
    public RecordingFormat Format { get; set; } = RecordingFormat.PngSequence;

    /// <summary>
    /// Gets or sets the output directory for recorded frames or video.
    /// If null, a temporary directory will be used.
    /// </summary>
    public string? OutputDirectory { get; set; }

    /// <summary>
    /// Gets or sets the base filename for output files.
    /// Default is "frame".
    /// </summary>
    public string BaseFileName { get; set; } = "frame";

    /// <summary>
    /// Gets or sets whether to capture frames automatically at the specified frame rate.
    /// If false, frames must be captured manually.
    /// Default is true.
    /// </summary>
    public bool AutoCapture { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of frames to buffer in memory.
    /// Default is 1000 frames.
    /// </summary>
    public int MaxBufferedFrames { get; set; } = 1000;

    /// <summary>
    /// Gets or sets whether to write frames to disk immediately or buffer them.
    /// Default is true (write immediately).
    /// </summary>
    public bool WriteImmediately { get; set; } = true;

    /// <summary>
    /// Gets or sets the quality for lossy formats (0-100).
    /// Default is 90.
    /// </summary>
    public int Quality { get; set; } = 90;

    /// <summary>
    /// Gets or sets the scale factor for recorded frames.
    /// 1.0 = original size, 0.5 = half size, etc.
    /// Default is 1.0.
    /// </summary>
    public double ScaleFactor { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets whether to include timestamp overlays on frames.
    /// Overlays are not supported; setting this to true causes validation to fail.
    /// </summary>
    public bool IncludeTimestamp { get; set; }

    /// <summary>
    /// Gets or sets whether to include frame number overlays on frames.
    /// Overlays are not supported; setting this to true causes validation to fail.
    /// </summary>
    public bool IncludeFrameNumber { get; set; }

    /// <summary>
    /// Gets or sets the maximum recording duration.
    /// Null means no limit.
    /// Default is null.
    /// </summary>
    public TimeSpan? MaxDuration { get; set; }

    /// <summary>
    /// Gets or sets whether to capture only when content changes.
    /// This can reduce file size for static content.
    /// Default is false.
    /// </summary>
    public bool CaptureOnChangeOnly { get; set; }

    /// <summary>
    /// Gets the frame interval in milliseconds based on the frame rate.
    /// </summary>
    public double FrameIntervalMs => 1000.0 / FrameRate;

    /// <summary>
    /// Validates the recording options.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when options are invalid.</exception>
    public void Validate()
    {
        if (FrameRate <= 0 || FrameRate > 120)
            throw new ArgumentException("FrameRate must be between 1 and 120.", nameof(FrameRate));

        if (MaxBufferedFrames <= 0)
            throw new ArgumentException("MaxBufferedFrames must be greater than 0.", nameof(MaxBufferedFrames));

        if (Quality < 0 || Quality > 100)
            throw new ArgumentException("Quality must be between 0 and 100.", nameof(Quality));

        if (!double.IsFinite(ScaleFactor) || ScaleFactor <= 0 || ScaleFactor > 4)
            throw new ArgumentException("ScaleFactor must be between 0 (exclusive) and 4.", nameof(ScaleFactor));

        if (IncludeTimestamp || IncludeFrameNumber)
            throw new NotSupportedException("Timestamp and frame-number overlays are not supported. Record markers with RecordingSession.RecordEvent instead.");

        if (MaxDuration is { } duration && duration <= TimeSpan.Zero)
            throw new ArgumentException("MaxDuration must be positive.", nameof(MaxDuration));
    }

    /// <summary>
    /// Creates default options for high-quality recording.
    /// </summary>
    public static RecordingOptions HighQuality => new()
    {
        FrameRate = 60,
        Quality = 100,
        ScaleFactor = 1.0,
        Format = RecordingFormat.PngSequence
    };

    /// <summary>
    /// Creates default options for performance-oriented recording.
    /// </summary>
    public static RecordingOptions Performance => new()
    {
        FrameRate = 30,
        Quality = 80,
        ScaleFactor = 0.5,
        Format = RecordingFormat.JpegSequence
    };

    /// <summary>
    /// Creates default options for GIF output.
    /// </summary>
    public static RecordingOptions AnimatedGif => new()
    {
        FrameRate = 15,
        Quality = 80,
        ScaleFactor = 1.0,
        Format = RecordingFormat.Gif
    };
}

/// <summary>
/// Output formats for screen recording.
/// </summary>
public enum RecordingFormat
{
    /// <summary>
    /// Output as a sequence of PNG images.
    /// </summary>
    PngSequence,

    /// <summary>
    /// Output as a sequence of JPEG images.
    /// </summary>
    JpegSequence,

    /// <summary>
    /// Output as an animated GIF.
    /// </summary>
    Gif,

    /// <summary>
    /// Output as raw frame data (for custom processing).
    /// </summary>
    RawFrames
}

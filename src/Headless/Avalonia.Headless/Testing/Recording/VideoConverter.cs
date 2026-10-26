// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Avalonia.HeadlessTestingFramework.Recording;

/// <summary>
/// Video output formats supported by the converter.
/// </summary>
public enum VideoFormat
{
    /// <summary>
    /// MP4 format with H.264 codec.
    /// </summary>
    Mp4,

    /// <summary>
    /// WebM format with VP9 codec.
    /// </summary>
    WebM,

    /// <summary>
    /// Animated GIF format.
    /// </summary>
    Gif,

    /// <summary>
    /// AVI format with MJPEG codec.
    /// </summary>
    Avi
}

/// <summary>
/// Options for video conversion.
/// </summary>
public class VideoConversionOptions
{
    /// <summary>
    /// Gets or sets the output video format.
    /// </summary>
    public VideoFormat Format { get; set; } = VideoFormat.Mp4;

    /// <summary>
    /// Gets or sets the frame rate for the output video.
    /// Default is 30 FPS.
    /// </summary>
    public int FrameRate { get; set; } = 30;

    /// <summary>
    /// Gets or sets the video quality (0-51 for H.264, lower is better).
    /// Default is 23 (good quality).
    /// </summary>
    public int Quality { get; set; } = 23;

    /// <summary>
    /// Gets or sets whether to delete source PNG files after conversion.
    /// </summary>
    public bool DeleteSourceFiles { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to overwrite existing output files.
    /// </summary>
    public bool OverwriteExisting { get; set; } = true;

    /// <summary>
    /// Gets or sets custom FFmpeg arguments to append.
    /// </summary>
    public string? CustomArguments { get; set; }

    /// <summary>
    /// Gets or sets the path to FFmpeg executable.
    /// If null, assumes FFmpeg is in PATH.
    /// </summary>
    public string? FfmpegPath { get; set; }

    /// <summary>
    /// Gets or sets the timeout for video conversion.
    /// Default is 5 minutes.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets whether to loop the video (for GIF output).
    /// Default is 0 (infinite loop).
    /// </summary>
    public int LoopCount { get; set; } = 0;

    /// <summary>
    /// Gets or sets the scale factor for the output video.
    /// 1.0 = original size, 0.5 = half size.
    /// </summary>
    public double Scale { get; set; } = 1.0;
}

/// <summary>
/// Result of a video conversion operation.
/// </summary>
public class VideoConversionResult
{
    /// <summary>
    /// Gets whether the conversion was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets the output file path.
    /// </summary>
    public string? OutputPath { get; set; }

    /// <summary>
    /// Gets the error message if conversion failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets the duration of the conversion.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets the FFmpeg output/log.
    /// </summary>
    public string? FfmpegOutput { get; set; }

    /// <summary>
    /// Gets the number of source frames processed.
    /// </summary>
    public int FrameCount { get; set; }
}

/// <summary>
/// Converts PNG image sequences to video files using FFmpeg.
/// </summary>
public class VideoConverter
{
    private readonly string _ffmpegPath;

    /// <summary>
    /// Creates a new video converter.
    /// </summary>
    /// <param name="ffmpegPath">Path to FFmpeg executable. If null, assumes FFmpeg is in PATH.</param>
    public VideoConverter(string? ffmpegPath = null)
    {
        _ffmpegPath = ffmpegPath ?? GetDefaultFfmpegPath();
    }

    /// <summary>
    /// Checks if FFmpeg is available.
    /// </summary>
    /// <returns>True if FFmpeg is available, false otherwise.</returns>
    public bool IsFfmpegAvailable() => ProbeFfmpegVersion() != null;

    /// <summary>
    /// Gets the FFmpeg version string, or null when FFmpeg is unavailable.
    /// </summary>
    public string? GetFfmpegVersion() => ProbeFfmpegVersion();

    private string? ProbeFfmpegVersion()
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = "-version",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });
            if (process == null)
                return null;
            var output = process.StandardOutput.ReadToEndAsync();
            var error = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit(5000))
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit();
                return null;
            }
            Task.WhenAll(output, error).GetAwaiter().GetResult();
            return process.ExitCode == 0 ? output.Result.Split('\n')[0].TrimEnd('\r') : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Converts a directory of PNG frames to a video file.
    /// </summary>
    /// <param name="inputDirectory">Directory containing PNG frames.</param>
    /// <param name="outputPath">Output video file path. If null, uses input directory with appropriate extension.</param>
    /// <param name="options">Conversion options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The conversion result.</returns>
    public async Task<VideoConversionResult> ConvertPngSequenceToVideoAsync(
        string inputDirectory,
        string? outputPath = null,
        VideoConversionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new VideoConversionOptions();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Find PNG files
            var pngFiles = Directory.GetFiles(inputDirectory, "*.png")
                .OrderBy(f => f)
                .ToList();

            if (pngFiles.Count == 0)
            {
                return new VideoConversionResult
                {
                    Success = false,
                    ErrorMessage = "No PNG files found in the input directory.",
                    Duration = stopwatch.Elapsed,
                    FrameCount = 0
                };
            }

            // Determine output path
            outputPath ??= Path.Combine(inputDirectory, $"output{GetExtension(options.Format)}");

            if (File.Exists(outputPath))
            {
                if (options.OverwriteExisting)
                {
                    File.Delete(outputPath);
                }
                else
                {
                    return new VideoConversionResult
                    {
                        Success = false,
                        ErrorMessage = $"Output file already exists: {outputPath}",
                        Duration = stopwatch.Elapsed,
                        FrameCount = pngFiles.Count
                    };
                }
            }

            // Build FFmpeg arguments
            var arguments = BuildFfmpegArguments(inputDirectory, outputPath, options, pngFiles);

            // Run FFmpeg
            var (exitCode, output) = await RunFfmpegAsync(arguments, options.Timeout, cancellationToken).ConfigureAwait(false);

            if (exitCode != 0)
            {
                return new VideoConversionResult
                {
                    Success = false,
                    ErrorMessage = $"FFmpeg exited with code {exitCode}",
                    Duration = stopwatch.Elapsed,
                    FfmpegOutput = output,
                    FrameCount = pngFiles.Count
                };
            }

            // Delete source files if requested
            if (options.DeleteSourceFiles && File.Exists(outputPath))
            {
                foreach (var file in pngFiles)
                {
                    try { File.Delete(file); } catch { /* ignore */ }
                }
            }

            return new VideoConversionResult
            {
                Success = true,
                OutputPath = outputPath,
                Duration = stopwatch.Elapsed,
                FfmpegOutput = output,
                FrameCount = pngFiles.Count
            };
        }
        catch (Exception ex)
        {
            return new VideoConversionResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Duration = stopwatch.Elapsed
            };
        }
    }

    /// <summary>
    /// Converts a directory of PNG frames to a video file synchronously.
    /// </summary>
    public VideoConversionResult ConvertPngSequenceToVideo(
        string inputDirectory,
        string? outputPath = null,
        VideoConversionOptions? options = null)
    {
        return ConvertPngSequenceToVideoAsync(inputDirectory, outputPath, options).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Converts a recording session's output to a video file.
    /// </summary>
    /// <param name="session">The recording session.</param>
    /// <param name="outputPath">Output video file path.</param>
    /// <param name="options">Conversion options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The conversion result.</returns>
    public async Task<VideoConversionResult> ConvertSessionToVideoAsync(
        RecordingSession session,
        string? outputPath = null,
        VideoConversionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (session.Encoder == null)
        {
            return new VideoConversionResult
            {
                Success = false,
                ErrorMessage = "Session has no encoder."
            };
        }

        var outputFiles = session.Encoder.OutputFiles;
        if (outputFiles.Count == 0)
        {
            return new VideoConversionResult
            {
                Success = false,
                ErrorMessage = "Session has no output files."
            };
        }

        // Find the directory containing PNG files
        var firstFile = outputFiles[0];
        var inputDirectory = Path.GetDirectoryName(firstFile);

        if (string.IsNullOrEmpty(inputDirectory))
        {
            return new VideoConversionResult
            {
                Success = false,
                ErrorMessage = "Could not determine input directory from session."
            };
        }

        return await ConvertPngSequenceToVideoAsync(inputDirectory, outputPath, options, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Converts multiple recording directories to videos.
    /// </summary>
    /// <param name="directories">Directories containing PNG sequences.</param>
    /// <param name="options">Conversion options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Results for each conversion.</returns>
    public async Task<Dictionary<string, VideoConversionResult>> ConvertMultipleAsync(
        IEnumerable<string> directories,
        VideoConversionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, VideoConversionResult>();

        foreach (var dir in directories)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var result = await ConvertPngSequenceToVideoAsync(dir, null, options, cancellationToken).ConfigureAwait(false);
            results[dir] = result;
        }

        return results;
    }

    private string BuildFfmpegArguments(string inputDirectory, string outputPath, VideoConversionOptions options, List<string> pngFiles)
    {
        var args = new List<string>();

        // Prevent FFmpeg from reading stdin (important for non-interactive execution)
        args.Add("-nostdin");

        // Determine the input pattern
        var firstFile = Path.GetFileName(pngFiles[0]);
        var inputPattern = GetInputPattern(firstFile, inputDirectory);

        // Frame rate
        args.Add($"-framerate {options.FrameRate}");

        // Input pattern
        args.Add($"-i \"{inputPattern}\"");

        var scale = options.Scale.ToString(CultureInfo.InvariantCulture);
        var scaleFilter = Math.Abs(options.Scale - 1.0) > 0.001
            ? $"scale=iw*{scale}:ih*{scale}"
            : null;
        if (scaleFilter != null && options.Format != VideoFormat.Gif)
            args.Add($"-vf \"{scaleFilter}\"");

        // Format-specific encoding options
        switch (options.Format)
        {
            case VideoFormat.Mp4:
                args.Add("-c:v libx264");
                args.Add("-pix_fmt yuv420p");
                args.Add($"-crf {options.Quality}");
                args.Add("-preset medium");
                break;

            case VideoFormat.WebM:
                args.Add("-c:v libvpx-vp9");
                args.Add($"-crf {options.Quality}");
                args.Add("-b:v 0");
                break;

            case VideoFormat.Gif:
                var gifFilter = "split[s0][s1];[s0]palettegen[p];[s1][p]paletteuse";
                if (scaleFilter != null)
                    gifFilter = scaleFilter + "," + gifFilter;
                args.Add($"-vf \"{gifFilter}\"");
                args.Add($"-loop {options.LoopCount}");
                break;

            case VideoFormat.Avi:
                args.Add("-c:v mjpeg");
                args.Add($"-q:v {Math.Max(1, Math.Min(31, options.Quality / 3))}");
                break;
        }

        // Custom arguments
        if (!string.IsNullOrEmpty(options.CustomArguments))
        {
            args.Add(options.CustomArguments!);
        }

        // Overwrite flag
        if (options.OverwriteExisting)
        {
            args.Add("-y");
        }

        // Output path
        args.Add($"\"{outputPath}\"");

        return string.Join(" ", args);
    }

    private string GetInputPattern(string firstFileName, string inputDirectory)
    {
        // Try to detect the naming pattern (e.g., frame_000001.png)
        // Common patterns: frame_%06d.png, frame_000001.png, etc.

        // Find the numeric part
        var name = Path.GetFileNameWithoutExtension(firstFileName);
        var lastUnderscore = name.LastIndexOf('_');

        if (lastUnderscore >= 0 && lastUnderscore < name.Length - 1)
        {
            var numericPart = name.Substring(lastUnderscore + 1);
            if (int.TryParse(numericPart, out _))
            {
                var prefix = name.Substring(0, lastUnderscore + 1);
                var digits = numericPart.Length;
                return Path.Combine(inputDirectory, $"{prefix}%0{digits}d.png");
            }
        }

        // Fallback: glob pattern for any PNG
        return Path.Combine(inputDirectory, "*.png");
    }

    private async Task<(int exitCode, string output)> RunFfmpegAsync(
        string arguments,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _ffmpegPath,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        var outputBuilder = new System.Text.StringBuilder();
        var errorBuilder = new System.Text.StringBuilder();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
                outputBuilder.AppendLine(e.Data);
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
                errorBuilder.AppendLine(e.Data);
        };

        process.Start();

        // Close stdin immediately to prevent FFmpeg from waiting for input
        process.StandardInput.Close();

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        try
        {
            await process.WaitForExitAsync(cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            try
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch (InvalidOperationException) { /* Process already exited. */ }
            throw;
        }

        var combinedOutput = outputBuilder.ToString() + Environment.NewLine + errorBuilder.ToString();
        return (process.ExitCode, combinedOutput);
    }

    private static string GetExtension(VideoFormat format)
    {
        return format switch
        {
            VideoFormat.Mp4 => ".mp4",
            VideoFormat.WebM => ".webm",
            VideoFormat.Gif => ".gif",
            VideoFormat.Avi => ".avi",
            _ => ".mp4"
        };
    }

    private static string GetDefaultFfmpegPath()
    {
        // Check if ffmpeg is in PATH
        var ffmpegName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffmpeg.exe" : "ffmpeg";

        // Try common installation paths
        var commonPaths = new[]
        {
            ffmpegName, // PATH
            "/usr/bin/ffmpeg",
            "/usr/local/bin/ffmpeg",
            "/opt/homebrew/bin/ffmpeg", // macOS Homebrew ARM
            @"C:\Program Files\ffmpeg\bin\ffmpeg.exe",
            @"C:\ffmpeg\bin\ffmpeg.exe"
        };

        foreach (var path in commonPaths)
        {
            if (path == ffmpegName)
                return ffmpegName; // Rely on PATH

            if (File.Exists(path))
                return path;
        }

        return ffmpegName; // Default to PATH lookup
    }
}

/// <summary>
/// Extension methods for recording sessions to convert to video.
/// </summary>
public static class RecordingSessionVideoExtensions
{
    /// <summary>
    /// Converts the recording session's PNG output to a video file.
    /// </summary>
    /// <param name="session">The recording session.</param>
    /// <param name="outputPath">Output video file path.</param>
    /// <param name="options">Conversion options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The conversion result.</returns>
    public static async Task<VideoConversionResult> ToVideoAsync(
        this RecordingSession session,
        string? outputPath = null,
        VideoConversionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var converter = new VideoConverter(options?.FfmpegPath);
        return await converter.ConvertSessionToVideoAsync(session, outputPath, options, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Converts the recording session's PNG output to a video file.
    /// </summary>
    public static VideoConversionResult ToVideo(
        this RecordingSession session,
        string? outputPath = null,
        VideoConversionOptions? options = null)
    {
        return session.ToVideoAsync(outputPath, options).GetAwaiter().GetResult();
    }
}

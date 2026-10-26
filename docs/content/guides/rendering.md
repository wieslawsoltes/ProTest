---
title: "Rendering and visual tests"
description: "Enable Skia rendering, capture bitmaps, and choose stable visual baselines."
sidebar_position: 5
doc-type: how-to
---

The default headless drawing backend supports behavior tests without producing pixels. Add official `Avalonia.Skia` version `12.1.4` when tests need screenshots, recording, or pixel comparisons. Add the same-version theme and fonts used by the application.

```csharp
public static AppBuilder BuildAvaloniaApp() =>
    AppBuilder.Configure<TestApplication>()
        .UseSkia()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions
        {
            UseHeadlessDrawing = false
        });
```

## Capture a frame

```csharp
var window = new Window
{
    Width = 160,
    Height = 90,
    Content = new Border { Background = Brushes.CornflowerBlue }
};
window.Show();
try
{
    using var bitmap = HeadlessWindowExtensions.CaptureRenderedFrame(window);
    Assert.NotNull(bitmap);
    bitmap.Save("actual.png");
}
finally
{
    window.Close();
}
```

The explicit static call selects the platform's capture method even if you also import the recording namespace, which contains a helper with the same extension-method name. The bitmap is owned by the caller and must be disposed.

## Compare output

Store a reviewed baseline with the test suite, capture an image at the same dimensions and DPI, and compare pixel buffers or use an image comparison library. Record the maximum tolerated difference in the test. Save the actual image and a difference image as CI artifacts when an assertion fails.

Use fixed fonts, theme, control dimensions, scaling, and operating-system images for stable baselines. A structural [tree comparison](../reference/trees/comparison.md) is often easier to maintain when the requirement concerns hierarchy or properties rather than pixels.

## Headless platform options

| Property | Default | Effect |
| --- | --- | --- |
| `UseHeadlessDrawing` | `true` | Uses the fake drawing backend; set false with Skia to render pixels |
| `FrameBufferFormat` | `Rgba8888` | Chooses the headless framebuffer pixel layout |
| `Fps` | `60` | Sets the render timer frequency |
| `ShouldRenderOnUIThread` | `true` | Processes rendering on the dispatcher thread |
| `OverlayPopups` | `true` | Hosts popups in the parent top-level overlay |
| `UseSharedMouseDevice` | `null` | Set true for shared mouse capture and click state across windows |
| `SupportsWindowPositioning` | `true` | Set false to model a compositor that owns top-level positions |
| `UseLogicalDesktopCoordinates` | `false` | Set true for logical desktop coordinates independent of render scaling |

## Record interactions

[Screen recording](../reference/recording/screen-recorder.md) captures PNG/JPEG sequences, raw frames, or GIF output. [Recorded input](../reference/recording/recorded-input.md) associates gesture steps and timeline markers with frames. [Video conversion](../reference/recording/video-converter.md) uses an optional FFmpeg installation to convert PNG sequences.

Topic reference: [Avalonia visual regression guidance](https://docs.avaloniaui.net/docs/testing/setting-up-the-headless-platform#visual-regression-testing).

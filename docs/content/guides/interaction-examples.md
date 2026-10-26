---
title: "Interaction recipes"
description: "Complete tests for text input, touch dragging, and frame recording."
sidebar_position: 7
doc-type: how-to
---

These examples use standard Avalonia controls and work with either ProTest xUnit adapter. Start with the [xUnit setup](../getting-started/xunit.md) and load your application's theme for template-dependent controls such as `TextBox`. Each test closes its window during cleanup.

## Find a control and enter text

Use the driver when a test should describe a sequence of user interactions. Tree queries can then verify that a locator selected the intended control.

```csharp
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.HeadlessTestingFramework;
using Avalonia.HeadlessTestingFramework.Appium;
using Xunit;

public class EditorInteractionTests
{
    [AvaloniaFact]
    public void Typing_updates_the_named_editor()
    {
        var editor = new TextBox { Name = "editor" };
        var window = new Window { Content = editor, Width = 240, Height = 80 };
        try
        {
            window.Show();
            using var driver = new AvaloniaDriver(window);
            driver.FindElement(By.Id("editor")).SendKeys("ProTest helpers");

            Assert.Equal("ProTest helpers", editor.Text);
            Assert.Same(editor, window.Find<TextBox>().FindFirst<TextBox>());
            Assert.Same(editor,
                new TreeXPath(window)
                    .Select<TextBox>("//TextBox[@Name='editor']")
                    .Single());
        }
        finally
        {
            window.Close();
        }
    }
}
```

See [driver and elements](../reference/automation/driver.md) for additional locators and actions, or [control finder](../reference/trees/control-finder.md) for fluent tree filters.

## Verify a complete touch drag

Observe the pointer events delivered to a control, including the final position and release of capture. This helps test controls that implement their own dragging behavior.

```csharp
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.HeadlessTestingFramework;
using Avalonia.Media;
using Xunit;

public class TouchInteractionTests
{
    [AvaloniaFact]
    public void Drag_delivers_press_moves_and_release()
    {
        var target = new Border { Background = Brushes.White };
        var window = new Window { Content = target, Width = 200, Height = 100 };
        var pressed = 0;
        var positions = new List<Point>();
        var released = 0;

        target.PointerPressed += (_, _) => pressed++;
        target.PointerMoved += (_, e) => positions.Add(e.GetPosition(target));
        target.PointerReleased += (_, _) => released++;

        try
        {
            window.Show();
            var touch = new GestureRecognizerTestHelper();
            touch.Drag(target, new Point(10, 10), new Point(90, 50), steps: 4);

            Assert.Equal(1, pressed);
            Assert.Equal(4, positions.Count);
            Assert.Equal(new Point(90, 50), positions[^1]);
            Assert.Equal(1, released);
            Assert.Null(touch.Captured);
        }
        finally
        {
            window.Close();
        }
    }
}
```

For pinch, scroll, or other recognizer-driven gestures, [register gesture handlers](gesture-registration.md) before showing the window. Use [multi-touch helpers](../reference/input/multi-touch.md) when the test needs several independent pointers.

## Record and inspect a rendered frame

Configure [Skia rendering](rendering.md) before running this test. It records a PNG sequence and decodes the resulting image to check its dimensions. PNG capture does not require FFmpeg.

```csharp
using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.HeadlessTestingFramework.Recording;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Xunit;

public class RecordingInteractionTests
{
    [AvaloniaFact]
    public void Recording_writes_a_rendered_image()
    {
        var output = Path.Combine(
            Path.GetTempPath(), "ProTest-recording-" + Guid.NewGuid().ToString("N"));
        var window = new Window
        {
            Width = 120,
            Height = 80,
            Content = new Border { Background = Brushes.CornflowerBlue }
        };

        try
        {
            window.Show();
            using var recorder = new HeadlessScreenRecorder();
            recorder.StartRecording(window,
                new RecordingOptions { Format = RecordingFormat.PngSequence }, output);

            Assert.True(recorder.CaptureFrame(), recorder.LastCaptureFailureReason);
            var statistics = recorder.StopRecording();

            Assert.True(statistics.FramesCaptured > 0);
            var frames = Directory.GetFiles(output, "*.png");
            Assert.NotEmpty(frames);
            using var image = new Bitmap(frames[0]);
            Assert.Equal(new PixelSize(120, 80), image.PixelSize);
        }
        finally
        {
            window.Close();
            if (Directory.Exists(output))
                Directory.Delete(output, recursive: true);
        }
    }
}
```

For diagnostic recordings in CI, retain the output directory as a build artifact instead of deleting it. [Recorded input](../reference/recording/recorded-input.md) combines capture with scripted interactions; [video conversion](../reference/recording/video-converter.md) explains the optional FFmpeg workflow.

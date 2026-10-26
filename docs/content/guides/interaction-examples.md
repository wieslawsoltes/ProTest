---
title: "Interaction recipes"
description: "Examples and API guidance for interaction recipes."
sidebar_position: 7
doc-type: reference
---

These extended recipes originated in PanAndZoom. `ZoomBorder` and `ButtonName` belong to the separate [PanAndZoom library](https://github.com/wieslawsoltes/PanAndZoom), not ProTest. Add a compatible PanAndZoom package or substitute the control under test. Recording recipes require [Skia rendering](rendering.md); video conversion also needs FFmpeg.

## Example 1: Testing Pinch-to-Zoom

```csharp
[AvaloniaFact]
public void ZoomBorder_PinchZoom_ChangesZoomLevel()
{
    // Setup
    var zoomBorder = new ZoomBorder { PanButton = ButtonName.Left };
    Gestures.AddPinchHandler(zoomBorder, (s, e) => { });

    var window = new Window
    {
        Width = 400,
        Height = 300,
        Content = zoomBorder
    };
    window.Show();

    try
    {
        var simulator = new TouchInputSimulator();
        var center = new Point(200, 150);

        // Capture initial zoom
        double initialZoom = zoomBorder.ZoomX;

        // Perform pinch zoom (zoom in)
        simulator.SimulatePinchZoom(zoomBorder, center,
            startDistance: 50, endDistance: 150, steps: 10);

        // Verify zoom increased
        Assert.True(zoomBorder.ZoomX > initialZoom);
    }
    finally
    {
        window.Close();
    }
}
```

## Example 2: Recording Gesture Test

```csharp
[AvaloniaFact]
public void Recording_PinchZoom_CreatesVideoFile()
{
    using var simulator = new RecordedTouchSimulator();
    simulator.FramesPerStep = 2;

    var zoomBorder = new ZoomBorder { PanButton = ButtonName.Left };
    var window = new Window { Width = 400, Height = 300, Content = zoomBorder };
    window.Show();

    try
    {
        // Start recording
        simulator.StartRecording(window, outputPath: "/test/pinch_zoom");

        // Perform recorded gestures
        simulator.AddMarker("ZoomIn");
        simulator.RecordedPinchZoom(zoomBorder, new Point(200, 150), 50, 150);

        simulator.AddMarker("Pan");
        simulator.RecordedDrag(zoomBorder, new Point(200, 150), new Point(100, 100));

        // Stop and convert to video
        var (stats, videoResult) = simulator.StopRecordingAndConvertToVideo(
            new VideoConversionOptions { Format = VideoFormat.Mp4 });

        Assert.True(stats.FramesCaptured > 0);
        Assert.True(videoResult.Success);
    }
    finally
    {
        window.Close();
    }
}
```

## Example 3: Using GestureRecognizerTestHelper for Scroll

```csharp
[AvaloniaFact]
public void ZoomBorder_Scroll_PansContent()
{
    var zoomBorder = new ZoomBorder { PanButton = ButtonName.Left };
    var scrollRaised = false;

    Gestures.AddScrollGestureHandler(zoomBorder, (s, e) => scrollRaised = true);

    var window = new Window { Content = zoomBorder };
    window.Show();

    try
    {
        var helper = new GestureRecognizerTestHelper();
        var startPos = new Point(200, 150);

        // Perform drag gesture
        helper.Down(zoomBorder, startPos);
        for (int i = 1; i <= 10; i++)
        {
            helper.Move(zoomBorder, new Point(startPos.X - i * 5, startPos.Y));
        }
        helper.Up(zoomBorder, new Point(150, 150));

        // Verify scroll occurred
        Assert.True(scrollRaised);
    }
    finally
    {
        window.Close();
    }
}
```

## Example 4: Complex Multi-Gesture Recording

```csharp
[AvaloniaFact]
public async Task Recording_CompleteWorkflow_WithVideo()
{
    using var simulator = new RecordedTouchSimulator();

    var control = CreateTestControl();
    var window = new Window { Content = control };
    window.Show();

    try
    {
        var session = simulator.StartRecording(window,
            options: RecordingOptions.HighQuality,
            outputPath: "/recordings/workflow");

        // Complex interaction sequence
        simulator.AddMarker("Initial");
        simulator.CaptureFrame();

        simulator.AddMarker("ZoomIn");
        simulator.RecordedPinchZoom(control, new Point(200, 150), 50, 150, steps: 15);

        simulator.AddMarker("Pan");
        simulator.RecordedTwoFingerPan(control,
            new Point(200, 150), new Point(100, 100), steps: 10);

        simulator.AddMarker("Rotate");
        simulator.RecordedRotation(control,
            new Point(200, 150), 50, 0, 90, steps: 20);

        simulator.AddMarker("ZoomOut");
        simulator.RecordedPinchZoom(control, new Point(200, 150), 150, 50, steps: 15);

        // Record animation settling
        await simulator.RecordAnimationAsync(TimeSpan.FromMilliseconds(500));

        var stats = simulator.StopRecording();

        // Convert to multiple formats
        var converter = new VideoConverter();

        var mp4Result = await converter.ConvertPngSequenceToVideoAsync(
            session.Encoder!.OutputFiles[0].Directory!.FullName,
            options: new VideoConversionOptions { Format = VideoFormat.Mp4 });

        var gifResult = await converter.ConvertPngSequenceToVideoAsync(
            session.Encoder!.OutputFiles[0].Directory!.FullName,
            options: new VideoConversionOptions { Format = VideoFormat.Gif, FrameRate = 15 });

        // Assertions
        Assert.True(stats.FramesCaptured > 50);
        Assert.Contains(session.Events, e => e.Description == "ZoomIn");
        Assert.True(mp4Result.Success);
        Assert.True(gifResult.Success);
    }
    finally
    {
        window.Close();
    }
}
```

## Example 5: Keyboard and Mouse Input Recording

```csharp
[AvaloniaFact]
public void Recording_KeyboardAndMouseWorkflow_WithVideo()
{
    using var simulator = new RecordedTouchSimulator();
    simulator.FramesPerStep = 1;

    var textBox = new TextBox { Width = 300, Height = 30 };
    var button = new Button { Content = "Submit", Width = 100 };

    var window = new Window
    {
        Width = 400, Height = 200,
        Content = new StackPanel
        {
            Spacing = 20,
            Children = { textBox, button }
        }
    };
    window.Show();

    try
    {
        var session = simulator.StartRecording(window, outputPath: "/recordings/input_test");

        // Click text box to focus
        simulator.AddMarker("FocusTextBox");
        simulator.RecordedMouseClick(textBox, new Point(150, 15));

        // Type some text
        simulator.AddMarker("TypeText");
        simulator.RecordedKeyPress(textBox, Key.H);
        simulator.RecordedKeyPress(textBox, Key.E);
        simulator.RecordedKeyPress(textBox, Key.L);
        simulator.RecordedKeyPress(textBox, Key.L);
        simulator.RecordedKeyPress(textBox, Key.O);

        // Select all (Ctrl+A)
        simulator.AddMarker("SelectAll");
        simulator.RecordedKeyboardShortcut(textBox, Key.A, KeyModifiers.Control);

        // Tab to button
        simulator.AddMarker("TabToButton");
        simulator.RecordedTabNavigation(textBox, 1);

        // Click button
        simulator.AddMarker("ClickSubmit");
        simulator.RecordedMouseClick(button, new Point(50, 15));

        var stats = simulator.StopRecording();

        // Convert to video
        var converter = new VideoConverter();
        if (converter.IsFfmpegAvailable())
        {
            var result = converter.ConvertPngSequenceToVideo(
                session.Encoder!.OutputFiles[0].Directory!.FullName,
                options: new VideoConversionOptions { Format = VideoFormat.Mp4 });
            Assert.True(result.Success);
        }

        Assert.True(stats.FramesCaptured > 0);
        Assert.Contains(session.Events, e => e.Description == "TypeText");
    }
    finally
    {
        window.Close();
    }
}
```

## Example 6: Mouse Drag and Scroll Testing

```csharp
[AvaloniaFact]
public void Recording_MouseDragAndScroll_CreatesFrames()
{
    using var simulator = new RecordedTouchSimulator();
    simulator.FramesPerStep = 2;

    var canvas = new Canvas
    {
        Width = 400, Height = 300,
        Background = Brushes.LightGray
    };

    var items = new StackPanel();
    items.Children.AddRange(Enumerable.Range(1, 20)
        .Select(i => new TextBlock { Text = $"Item {i}", Height = 40 }));
    var scrollViewer = new ScrollViewer { Width = 400, Height = 200, Content = items };

    var window = new Window
    {
        Width = 400, Height = 500,
        Content = new StackPanel { Children = { canvas, scrollViewer } }
    };
    window.Show();

    try
    {
        var session = simulator.StartRecording(window, outputPath: "/recordings/drag_scroll");

        // Draw a path on canvas
        simulator.AddMarker("DrawPath");
        var drawPath = new[]
        {
            new Point(50, 50),
            new Point(150, 100),
            new Point(250, 50),
            new Point(350, 100)
        };
        simulator.RecordedMouseMovePath(canvas, drawPath);

        // Drag operation
        simulator.AddMarker("Drag");
        simulator.RecordedMouseDrag(canvas,
            new Point(50, 200), new Point(350, 200), steps: 20);

        // Scroll down in scroll viewer
        simulator.AddMarker("ScrollDown");
        simulator.RecordedMouseWheel(scrollViewer,
            new Vector(0, -5), new Point(200, 400), steps: 10);

        // Scroll back up
        simulator.AddMarker("ScrollUp");
        simulator.RecordedMouseWheel(scrollViewer,
            new Vector(0, 5), new Point(200, 400), steps: 10);

        var stats = simulator.StopRecording();

        Assert.True(stats.FramesCaptured > 20);
    }
    finally
    {
        window.Close();
    }
}
```

---

Adapted from the [PanAndZoom headless testing guide](https://github.com/wieslawsoltes/PanAndZoom/blob/0c938aa771970ad8e2387f669f910c8b2a2d4d30/docs/HeadlessTestingAPI.md), copyright © Wiesław Šoltés. See [source attribution and licenses](../attribution/sources.md).

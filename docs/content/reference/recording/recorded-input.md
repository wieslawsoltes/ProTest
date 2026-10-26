---
title: "Recorded input"
description: "Examples and API guidance for recorded input."
sidebar_position: 2
doc-type: reference
---

Use `Avalonia.HeadlessTestingFramework.Recording` and configure [Skia rendering](../../guides/rendering.md) before recording. Run capture operations on the Avalonia UI thread.

The `RecordedTouchSimulator` combines touch simulation with screen recording for comprehensive visual testing.

## Basic Usage

```csharp
using var simulator = new RecordedTouchSimulator();
simulator.FramesPerStep = 2;        // Frames to capture per gesture step
simulator.CaptureBeforeGesture = true;
simulator.CaptureAfterGesture = true;

// Start recording
var session = simulator.StartRecording(window, outputPath: "/path/to/output");

// Perform gestures with automatic frame capture
simulator.RecordedTap(control, new Point(100, 100));
simulator.RecordedDoubleTap(control, new Point(100, 100));
simulator.RecordedDrag(control, start, end, steps: 10);
simulator.RecordedPinchZoom(control, center, startDistance: 50, endDistance: 150, steps: 10);

// Stop recording
RecordingStatistics stats = simulator.StopRecording();
```

## Recorded Gestures

```csharp
// Tap
simulator.RecordedTap(control, new Point(100, 100), holdTime: 50);

// Double tap
simulator.RecordedDoubleTap(control, new Point(100, 100), tapInterval: 100);

// Drag
simulator.RecordedDrag(control,
    startPoint: new Point(100, 100),
    endPoint: new Point(200, 200),
    steps: 10);

// Pinch zoom
simulator.RecordedPinchZoom(control,
    centerPoint: new Point(200, 150),
    startDistance: 50,
    endDistance: 150,
    steps: 10);

// Scroll
simulator.RecordedScroll(control, new Vector(0, -50));

// Two-finger pan
simulator.RecordedTwoFingerPan(control,
    startPoint: new Point(200, 150),
    endPoint: new Point(100, 100),
    fingerSpacing: 50,
    steps: 10);

// Rotation
simulator.RecordedRotation(control,
    centerPoint: new Point(200, 150),
    radius: 50,
    startAngle: 0,
    endAngle: 45,
    steps: 10);

// Swipe
simulator.RecordedSwipe(control,
    startPoint: new Point(200, 150),
    SwipeDirection.Left,
    distance: 100);

// Touchpad magnify
simulator.RecordedTouchpadMagnify(control, new Vector(0.1, 0), new Point(200, 150));
```

## Recorded Keyboard Input

```csharp
// Key press with recording
simulator.RecordedKeyPress(textBox, Key.A);

// Keyboard shortcut (e.g., Ctrl+A)
simulator.RecordedKeyboardShortcut(textBox, Key.A, KeyModifiers.Control);

// Tab navigation through controls
simulator.RecordedTabNavigation(startControl, tabCount: 5);
simulator.RecordedTabNavigation(startControl, tabCount: 3, reverse: true);  // Shift+Tab

// Arrow key navigation (e.g., for list selection)
var keys = new[] { Key.Down, Key.Down, Key.Down, Key.Up };
simulator.RecordedArrowNavigation(listBox, keys);

// Access underlying keyboard simulator
simulator.KeyboardSimulator.TypeText(textBox, "Hello World!");
```

## Recorded Mouse Input

```csharp
// Mouse click with recording
simulator.RecordedMouseClick(control, new Point(100, 100));

// Double click
simulator.RecordedMouseDoubleClick(control, new Point(100, 100));

// Right click (context menu)
simulator.RecordedMouseRightClick(control, new Point(100, 100));

// Drag operation
simulator.RecordedMouseDrag(canvas,
    startPoint: new Point(50, 50),
    endPoint: new Point(200, 200),
    steps: 15);

// Mouse wheel scroll
simulator.RecordedMouseWheel(scrollViewer,
    delta: new Vector(0, -3),
    position: new Point(200, 150),
    steps: 5);

// Hover over element
simulator.RecordedMouseHover(button, new Point(50, 20), duration: 300);

// Move through a path
var path = new[]
{
    new Point(50, 50),
    new Point(100, 150),
    new Point(200, 50),
    new Point(300, 150)
};
simulator.RecordedMouseMovePath(canvas, path);

// Access underlying mouse simulator for modifier combinations
simulator.MouseSimulator.Modifiers = KeyModifiers.Control;
simulator.RecordedMouseClick(control, new Point(100, 100));  // Ctrl+Click
simulator.MouseSimulator.Modifiers = KeyModifiers.None;
```

## Markers and Events

```csharp
// Add markers for timeline navigation
simulator.AddMarker("ZoomStart");
simulator.RecordedPinchZoom(control, center, 50, 150);
simulator.AddMarker("ZoomEnd");

// Access recorded events
foreach (var evt in session.Events)
{
    Console.WriteLine($"{evt.Timestamp}: {evt.Type} - {evt.Description}");
}
```

## Video Conversion

```csharp
// Convert after stopping
var stats = simulator.StopRecording();
var videoResult = simulator.ConvertToVideo(options: new VideoConversionOptions
{
    Format = VideoFormat.Mp4,
    FrameRate = 30,
    Quality = 23
});

// Or stop and convert in one call
var (stats, videoResult) = simulator.StopRecordingAndConvertToVideo(
    new VideoConversionOptions { Format = VideoFormat.Mp4 });
```

---

Adapted from the [PanAndZoom headless testing guide](https://github.com/wieslawsoltes/PanAndZoom/blob/0c938aa771970ad8e2387f669f910c8b2a2d4d30/docs/HeadlessTestingAPI.md), copyright © Wiesław Šoltés, [MIT licensed](../../attribution/panandzoom-license.md).

---
title: "Multi-touch gestures"
description: "Examples and API guidance for multi-touch gestures."
sidebar_position: 6
doc-type: reference
---

The `MultiTouchTestHelperFactory` provides factory methods for creating paired gesture recognizer test helpers for multi-touch scenarios like pinch gestures.

## Creating Touch Helper Pairs

```csharp
// Create a pair for two-finger gestures
var (first, second) = MultiTouchTestHelperFactory.CreatePair();

// Create multiple helpers for multi-finger gestures
var helpers = MultiTouchTestHelperFactory.Create(count: 3);
```

## Pinch Gesture Simulation

```csharp
// Pinch zoom in (fingers moving apart)
MultiTouchTestHelperFactory.SimulatePinchZoomIn(
    target: zoomBorder,
    center: new Point(200, 150),
    startDistance: 50,
    endDistance: 150,
    steps: 10
);

// Pinch zoom out (fingers moving together)
MultiTouchTestHelperFactory.SimulatePinchZoomOut(
    target: zoomBorder,
    center: new Point(200, 150),
    startDistance: 150,
    endDistance: 50,
    steps: 10
);

// Custom pinch with specific start/end positions
MultiTouchTestHelperFactory.SimulatePinch(
    target: zoomBorder,
    firstStart: new Point(150, 150),
    secondStart: new Point(250, 150),
    firstEnd: new Point(100, 150),
    secondEnd: new Point(300, 150),
    steps: 10
);
```

## Rotation Gesture Simulation

```csharp
// Rotate clockwise
MultiTouchTestHelperFactory.SimulateRotation(
    target: control,
    center: new Point(200, 150),
    radius: 50,
    startAngle: 0,
    endAngle: 90,  // degrees
    steps: 20
);

// Counter-clockwise rotation
MultiTouchTestHelperFactory.SimulateRotation(
    target: control,
    center: new Point(200, 150),
    radius: 50,
    startAngle: 0,
    endAngle: -45,
    steps: 10
);
```

## Two-Finger Pan

```csharp
// Two-finger pan/scroll
MultiTouchTestHelperFactory.SimulateTwoFingerPan(
    target: scrollViewer,
    startPoint: new Point(200, 200),
    endPoint: new Point(200, 100),
    fingerSpacing: 40,
    steps: 10
);
```

## Manual Multi-Touch Control

```csharp
// Full control over multi-touch sequence
var (first, second) = MultiTouchTestHelperFactory.CreatePair();

// Start both fingers
first.Down(control, new Point(150, 150));
second.Down(control, new Point(250, 150));

// Move independently
for (int i = 0; i < 10; i++)
{
    first.Move(control, new Point(150 - i * 5, 150));
    second.Move(control, new Point(250 + i * 5, 150));
}

// Release both
first.Up(control, new Point(100, 150));
second.Up(control, new Point(300, 150));
```

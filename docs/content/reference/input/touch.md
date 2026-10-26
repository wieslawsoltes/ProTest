---
title: "Touch input"
description: "Examples and API guidance for touch input."
sidebar_position: 1
doc-type: reference
---

The `TouchInputSimulator` class provides low-level touch input simulation for Avalonia controls.

## Basic Touch Events

```csharp
var simulator = new TouchInputSimulator();

// Touch down (returns touch ID for tracking)
int touchId = simulator.TouchDown(control, new Point(100, 100));

// Move the touch point
simulator.TouchMove(control, touchId, new Point(150, 150));

// Release the touch
simulator.TouchUp(control, touchId);
```

## Convenience Methods

```csharp
// Simple tap
simulator.Tap(control, new Point(100, 100), holdTime: 50);

// Double tap
simulator.DoubleTap(control, new Point(100, 100), tapInterval: 100);

// Swipe in a direction
simulator.Swipe(control, new Point(200, 200), SwipeDirection.Left, distance: 100);
```

## Gesture Events

```csharp
// Pinch gesture (scale > 1 = zoom in, < 1 = zoom out)
simulator.PinchGesture(control, scale: 1.5, scaleOrigin: new Point(200, 150));
simulator.PinchGestureEnded(control);

// Scroll gesture
simulator.ScrollGesture(control, new Vector(0, -50), gestureId: 1);
simulator.ScrollGestureEnded(control, gestureId: 1);

// Touchpad magnify (macOS trackpad)
simulator.TouchpadMagnify(control, new Vector(0.1, 0), new Point(200, 150));

// Touchpad rotate
simulator.TouchpadRotate(control, delta: 15.0, position: new Point(200, 150));
```

## Multi-Touch Scenarios

```csharp
// Pinch zoom with two fingers
simulator.SimulatePinchZoom(
    control,
    centerPoint: new Point(200, 150),
    startDistance: 50,
    endDistance: 150,
    steps: 10
);

// Two-finger pan
simulator.SimulateTwoFingerPan(
    control,
    startPoint: new Point(200, 150),
    endPoint: new Point(100, 100),
    fingerSpacing: 50,
    steps: 10
);

// Rotation gesture
simulator.SimulateRotation(
    control,
    centerPoint: new Point(200, 150),
    radius: 50,
    startAngle: 0,
    endAngle: 45,
    steps: 10
);

// Single finger drag
simulator.SimulateDrag(
    control,
    startPoint: new Point(100, 100),
    endPoint: new Point(200, 200),
    steps: 10
);
```

## Time Management

```csharp
// Advance simulated time
simulator.AdvanceTime(milliseconds: 16); // ~60fps frame

// Reset simulator state
simulator.Reset();

// Access current timestamp
ulong timestamp = simulator.Timestamp;

// Access active touch points
var touchPoints = simulator.ActiveTouchPoints;
```

---
title: "Gesture events"
description: "Examples and API guidance for gesture events."
sidebar_position: 4
doc-type: reference
---

`GestureSimulator` raises processed gesture events such as tapped, pinch, and scroll events directly. Use it to test gesture handlers. `TouchInputSimulator` raises targeted pointer events; use `GestureRecognizerTestHelper` when the recognizer itself is under test.

## When to Use Which Simulator

| Simulator | Use Case |
|-----------|----------|
| `TouchInputSimulator` | Low-level touch input testing, gesture recognizer testing |
| `GestureSimulator` | Direct gesture event testing, simulating processed gestures |
| `GestureRecognizerTestHelper` | Testing gesture recognizer behavior with reflection |

## Basic Usage

```csharp
var simulator = new GestureSimulator();

// Simulate a tap gesture
simulator.Tap(control, new Point(100, 100));

// Simulate a double tap
simulator.DoubleTap(control, new Point(100, 100));

// Simulate a right tap (context menu)
simulator.RightTap(control, new Point(100, 100));
```

## Tap Gestures

```csharp
// Standard tap with touch pointer
simulator.Tap(control, new Point(100, 100));

// Tap with specific pointer type
simulator.Tap(control, new Point(100, 100), PointerType.Mouse);
simulator.Tap(control, new Point(100, 100), PointerType.Pen);

// Tap with key modifiers (Ctrl, Shift, Alt)
simulator.Tap(control, new Point(100, 100), PointerType.Touch, KeyModifiers.Control);

// Double tap
simulator.DoubleTap(control, new Point(100, 100));

// Right tap (triggers context menu)
simulator.RightTap(control, new Point(100, 100));
```

## Holding Gesture (Press and Hold)

```csharp
// Individual holding states
simulator.HoldingStarted(control, new Point(100, 100));
// ... simulate user holding for a duration ...
simulator.HoldingCompleted(control, new Point(100, 100));

// Or cancel the hold
simulator.HoldingCancelled(control, new Point(100, 100));

// Complete hold sequence (started + wait + completed)
simulator.Hold(control, new Point(100, 100), holdDuration: 500);
```

## Pinch Gestures (Zoom/Rotate)

```csharp
// Single pinch event
simulator.Pinch(control, scale: 1.5, scaleOrigin: new Point(200, 150));

// Pinch with rotation
simulator.Pinch(control, scale: 1.5, scaleOrigin: new Point(200, 150), angleDelta: 0.1);

// End pinch gesture
simulator.PinchEnded(control);

// Complete pinch zoom sequence (multiple steps)
simulator.PinchZoom(
    control,
    origin: new Point(200, 150),
    startScale: 1.0,
    endScale: 2.0,
    steps: 10
);

// Pinch rotation
simulator.PinchRotate(
    control,
    origin: new Point(200, 150),
    startAngle: 0,
    endAngle: 90,  // degrees
    steps: 10
);
```

## Scroll Gestures

```csharp
// Single scroll event
var gestureId = simulator.GetNextGestureId();
simulator.Scroll(control, new Vector(0, 100), gestureId);
simulator.ScrollEnded(control, gestureId);

// Scroll with inertia (uses reflection for internal constructor)
simulator.ScrollInertiaStarting(control, gestureId, new Vector(0, 500));

// Complete scroll sequence
simulator.ScrollSequence(
    control,
    totalDelta: new Vector(0, 200),
    steps: 10,
    withInertia: false
);

// Scroll with inertia
simulator.ScrollSequence(
    control,
    totalDelta: new Vector(0, 200),
    steps: 10,
    withInertia: true,
    inertiaVelocity: new Vector(0, 500)
);
```

## Pull Gestures (Pull-to-Refresh)

```csharp
// Single pull event
var gestureId = simulator.GetNextGestureId();
simulator.Pull(control, new Vector(0, 50), PullDirection.TopToBottom, gestureId);
simulator.PullEnded(control, gestureId, PullDirection.TopToBottom);

// Complete pull-to-refresh sequence
simulator.PullToRefresh(
    control,
    direction: PullDirection.TopToBottom,
    distance: 100,
    steps: 10
);
```

Pull directions:
- `PullDirection.TopToBottom` - Pull down to refresh
- `PullDirection.BottomToTop` - Pull up
- `PullDirection.LeftToRight` - Pull right
- `PullDirection.RightToLeft` - Pull left

## Touchpad Gestures (macOS Trackpad)

```csharp
// Touchpad magnify (pinch-to-zoom on trackpad)
simulator.TouchpadMagnify(control, new Vector(0.5, 0), new Point(200, 150));

// Touchpad rotate
simulator.TouchpadRotate(control, angleDelta: 45, position: new Point(200, 150));

// Touchpad swipe
simulator.TouchpadSwipe(control, new Vector(100, 0), new Point(200, 150));

// Complete magnify sequence
simulator.TouchpadMagnifySequence(
    control,
    position: new Point(200, 150),
    totalMagnification: 1.0,
    steps: 10
);

// Complete rotation sequence
simulator.TouchpadRotateSequence(
    control,
    position: new Point(200, 150),
    totalAngle: 90,  // degrees
    steps: 10
);

// Swipe sequence
simulator.TouchpadSwipeSequence(
    control,
    position: new Point(200, 150),
    direction: SwipeDirection.Left,
    distance: 100,
    steps: 10
);
```

## Compound Gestures

```csharp
// Tap and hold (tap followed by hold)
simulator.TapAndHold(control, new Point(100, 100), holdDuration: 500);

// Double-tap-and-zoom (common iOS/Android pattern)
simulator.DoubleTapZoom(
    control,
    position: new Point(200, 150),
    zoomScale: 2.0,
    steps: 10
);

// Flick/fling (fast swipe with inertia)
simulator.Flick(
    control,
    direction: SwipeDirection.Up,
    distance: 50,
    velocity: 500
);

// Three-finger swipe (navigation)
simulator.ThreeFingerSwipe(
    control,
    position: new Point(200, 150),
    direction: SwipeDirection.Left,
    distance: 100
);
```

## Time Management

```csharp
// Advance internal timestamp
simulator.AdvanceTime(milliseconds: 16);

// Get current timestamp
ulong currentTime = simulator.Timestamp;

// Reset simulator to initial state
simulator.Reset();

// Generate unique gesture IDs
int gestureId = simulator.GetNextGestureId();
```

## Complete Example: Testing Pinch-to-Zoom

```csharp
[AvaloniaFact]
public void PinchZoom_ShouldScaleContent()
{
    // Arrange
    var zoomBorder = new Border { Width = 400, Height = 300 };
    var content = new Border { Background = Brushes.Blue };
    zoomBorder.Child = content;

    var window = new Window { Content = zoomBorder };
    window.Show();

    var simulator = new GestureSimulator();
    double? finalScale = null;

    zoomBorder.AddHandler(Gestures.PinchEvent, (sender, args) =>
    {
        if (args is PinchEventArgs pinch)
        {
            finalScale = pinch.Scale;
        }
    });

    // Act - Simulate pinch zoom from 1x to 2x
    simulator.PinchZoom(zoomBorder, new Point(200, 150), 1.0, 2.0, steps: 5);

    // Assert
    Assert.NotNull(finalScale);
    Assert.Equal(2.0, finalScale.Value, 2);
    window.Close();
}
```

---

Adapted from the [PanAndZoom headless testing guide](https://github.com/wieslawsoltes/PanAndZoom/blob/0c938aa771970ad8e2387f669f910c8b2a2d4d30/docs/HeadlessTestingAPI.md), copyright © Wiesław Šoltés, [MIT licensed](../../attribution/panandzoom-license.md).

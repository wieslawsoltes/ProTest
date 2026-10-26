---
title: "Register gesture handlers"
description: "Examples and API guidance for register gesture handlers."
sidebar_position: 6
doc-type: reference
---

> ⚠️ **This is the most common source of bugs when testing gestures.**

When using `GestureRecognizerTestHelper` or `MultiTouchTestHelperFactory` to test gesture events like Pinch, Scroll, or Pull-to-Refresh, **handlers must be registered BEFORE the window is shown**.

## Register before showing the window

```csharp
var control = new MyControl();
var window = new Window { Content = control };
window.Show();  // ❌ Show FIRST

// Too late! Gesture recognizers are already initialized
Gestures.AddPinchHandler(control, (s, e) => { /* never called */ });
```

## Recommended setup

```csharp
var control = new MyControl();

// ✅ Register handlers BEFORE Show()
Gestures.AddPinchHandler(control, (s, e) => {
    Console.WriteLine($"Pinch: Scale={e.Scale}");
});

var window = new Window { Content = control };
window.Show();  // ✅ Show AFTER

// Now gestures will work
var (first, second) = MultiTouchTestHelperFactory.CreatePair();
first.Down(control, new Point(100, 150));
second.Down(control, new Point(200, 150));
// ... gesture events will fire correctly
```

## Why Does This Happen?

Avalonia's gesture recognizers are attached to controls when they enter the visual tree (during `Show()`). If handlers aren't registered at that point, the gesture recognizer doesn't know to track those events.

## Which APIs Are Affected?

| API | Affected? | Notes |
|-----|-----------|-------|
| `GestureRecognizerTestHelper` | ✅ Yes | Uses actual gesture recognizers |
| `MultiTouchTestHelperFactory` | ✅ Yes | Wraps GestureRecognizerTestHelper |
| `TouchInputSimulator.PinchGesture()` | ❌ No | Raises events directly via RaiseEvent |
| `GestureSimulator.Pinch()` | ❌ No | Raises events directly via RaiseEvent |
| `AvaloniaDriver` touch actions | ✅ Yes | Uses gesture recognizer path |

## Debugging Tip

If your gesture handlers aren't being called:
1. Check that handlers are registered before `window.Show()`
2. Verify the control is actually in the visual tree
3. Use `GestureSimulator` if you just need to raise events without actual recognizers

---

Adapted from the [PanAndZoom headless testing guide](https://github.com/wieslawsoltes/PanAndZoom/blob/0c938aa771970ad8e2387f669f910c8b2a2d4d30/docs/HeadlessTestingAPI.md), copyright © Wiesław Šoltés. See [source attribution and licenses](../attribution/sources.md).

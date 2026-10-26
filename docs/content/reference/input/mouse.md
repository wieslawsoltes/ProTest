---
title: "Mouse input"
description: "Examples and API guidance for mouse input."
sidebar_position: 3
doc-type: reference
---

The `MouseInputSimulator` class provides mouse input simulation for Avalonia controls in headless testing.

## Basic Mouse Events

```csharp
var simulator = new MouseInputSimulator();

// Move mouse to position
simulator.MoveTo(control, new Point(100, 100));

// Press mouse button
simulator.ButtonDown(control, MouseButton.Left, new Point(100, 100));

// Release mouse button
simulator.ButtonUp(control, MouseButton.Left, new Point(100, 100));
```

## Click Operations

```csharp
// Left click
simulator.Click(control, new Point(100, 100));

// Right click (context menu)
simulator.RightClick(control, new Point(100, 100));

// Middle click
simulator.MiddleClick(control, new Point(100, 100));

// Double click
simulator.DoubleClick(control, new Point(100, 100));

// Click with custom button
simulator.Click(control, new Point(100, 100), MouseButton.Middle);
```

## Drag Operations

```csharp
// Simple drag
simulator.Drag(control,
    startPoint: new Point(100, 100),
    endPoint: new Point(200, 200),
    steps: 10);

// Drag with specific button
simulator.Drag(control,
    startPoint: new Point(100, 100),
    endPoint: new Point(200, 200),
    steps: 10,
    button: MouseButton.Left);

// Move through a path
var path = new[]
{
    new Point(50, 50),
    new Point(100, 100),
    new Point(150, 50),
    new Point(200, 100)
};
simulator.HoverPath(control, path);
```

## Mouse Wheel

```csharp
// Scroll up
simulator.Wheel(control, new Vector(0, 1), new Point(200, 150));

// Scroll down
simulator.Wheel(control, new Vector(0, -1), new Point(200, 150));

// Scroll horizontally
simulator.Wheel(control, new Vector(1, 0), new Point(200, 150));

// Smooth scroll (multiple steps)
simulator.SmoothScroll(control,
    totalDelta: new Vector(0, -5),
    position: new Point(200, 150),
    steps: 10);
```

## Hover

```csharp
// Hover over a position for specified duration
simulator.Hover(control, new Point(100, 100), duration: 500);

// Move to position and wait
simulator.MoveTo(control, new Point(100, 100));
// ... control will process pointer enter events
```

## Mouse State

```csharp
// Check current position
Point currentPos = simulator.Position;

// Check if button is pressed
bool isLeftDown = simulator.PressedButton == MouseButton.Left;

// Check current modifiers (for Ctrl+Click, etc.)
simulator.Modifiers = KeyModifiers.Control;
simulator.Click(control, new Point(100, 100));  // Ctrl+Click
simulator.Modifiers = KeyModifiers.None;

// Release tracked mouse state between independent interactions.
simulator.Reset();
```

---

Adapted from the [PanAndZoom headless testing guide](https://github.com/wieslawsoltes/PanAndZoom/blob/0c938aa771970ad8e2387f669f910c8b2a2d4d30/docs/HeadlessTestingAPI.md), copyright © Wiesław Šoltés, [MIT licensed](../../attribution/panandzoom-license.md).

---
title: "Gesture recognizers"
description: "Examples and API guidance for gesture recognizers."
sidebar_position: 5
doc-type: reference
---

The `GestureRecognizerTestHelper` provides proper gesture recognizer integration using reflection to access Avalonia's internal APIs.

Register gesture handlers before calling `window.Show()`. See [gesture registration](../../guides/gesture-registration.md) for the setup pattern.

## Correct Pattern

```csharp
// CORRECT: Register handlers first
var zoomBorder = new Border();
Gestures.AddPinchHandler(zoomBorder, OnPinch);
Gestures.AddPinchEndedHandler(zoomBorder, OnPinchEnded);

var window = new Window { Content = zoomBorder };
window.Show(); // Show AFTER registering handlers
```

## Basic Usage

```csharp
var helper = new GestureRecognizerTestHelper();

// Pointer down
helper.Down(control, new Point(100, 100));

// Pointer move (handles gesture recognizer capture automatically)
helper.Move(control, new Point(150, 150));

// Pointer up
helper.Up(control, new Point(150, 150));

// Simple tap
helper.Tap(control, new Point(100, 100));

// Drag gesture
helper.Drag(control, start: new Point(100, 100), end: new Point(200, 200), steps: 10);

// Cancel current gesture
helper.Cancel();
```

## Pointer Type

```csharp
// Default: Touch pointer
var touchHelper = new GestureRecognizerTestHelper();

// Specify pointer type
var mouseHelper = new GestureRecognizerTestHelper(PointerType.Mouse);
var penHelper = new GestureRecognizerTestHelper(PointerType.Pen);
```

## Accessing Captured State

```csharp
// Check what's captured
IInputElement? captured = helper.Captured;
GestureRecognizer? capturedRecognizer = helper.CapturedGestureRecognizer;
Pointer pointer = helper.Pointer;
```

---
title: "Keyboard input"
description: "Examples and API guidance for keyboard input."
sidebar_position: 2
doc-type: reference
---

The `KeyboardInputSimulator` class provides keyboard input simulation for Avalonia controls in headless testing.

## Basic Key Events

```csharp
var simulator = new KeyboardInputSimulator();

// Key down
simulator.KeyDown(control, Key.A);

// Key up
simulator.KeyUp(control, Key.A);

// Key press (down + up)
simulator.KeyPress(control, Key.A);

// Key press with modifiers
simulator.KeyPress(control, Key.A, modifiers: KeyModifiers.Control | KeyModifiers.Shift);
```

## Modifier Keys

```csharp
// Track modifier state
simulator.CtrlDown(control);    // Sets CurrentModifiers to Control
simulator.ShiftDown(control);   // Adds Shift to CurrentModifiers
simulator.AltDown(control);     // Adds Alt to CurrentModifiers

// Release modifiers
simulator.ShiftUp(control);
simulator.CtrlUp(control);
simulator.AltUp(control);

// Check current modifiers
KeyModifiers modifiers = simulator.CurrentModifiers;

// Reset all modifiers
simulator.CtrlUp(control);
simulator.ShiftUp(control);
simulator.AltUp(control);
simulator.MetaUp(control);
```

## Common Shortcuts

```csharp
// Built-in shortcuts
simulator.CtrlKey(control, Key.A);  // Ctrl+A (Select All)
simulator.CtrlKey(control, Key.C);  // Ctrl+C (Copy)
simulator.CtrlKey(control, Key.V);  // Ctrl+V (Paste)
simulator.CtrlKey(control, Key.X);  // Ctrl+X (Cut)
simulator.CtrlKey(control, Key.Z);  // Ctrl+Z (Undo)
simulator.CtrlKey(control, Key.Y);  // Ctrl+Y (Redo)

// Alt shortcuts
simulator.AltKey(control, Key.F4);  // Alt+F4 (Close)

// Shift shortcuts
simulator.ShiftTab(control);  // Shift+Tab (Reverse Tab)

// Combined modifiers
simulator.CtrlShiftKey(control, Key.S);  // Ctrl+Shift+S
simulator.KeyPress(control, Key.Delete, modifiers: KeyModifiers.Control | KeyModifiers.Alt);  // Ctrl+Alt+Delete
```

## Navigation Keys

```csharp
// Arrow keys
simulator.ArrowUp(control);
simulator.ArrowDown(control);
simulator.ArrowLeft(control);
simulator.ArrowRight(control);

// Special keys
simulator.Tab(control);
simulator.Enter(control);
simulator.Escape(control);
simulator.Space(control);
simulator.Backspace(control);
simulator.Delete(control);

// Page navigation
simulator.PageUp(control);
simulator.PageDown(control);
simulator.Home(control);
simulator.End(control);
```

## Function Keys

```csharp
// F1-F12
simulator.FunctionKey(control, 1);   // F1
simulator.FunctionKey(control, 5);   // F5 (Refresh)
simulator.FunctionKey(control, 12);  // F12 (Dev tools)
```

## Type Text

```csharp
// Type a sequence of characters
simulator.TypeText(control, "Hello World!");

// Type with delay between keys
simulator.TypeText(control, "Hello", keyInterval: 50);
```

## Custom Key Sequences

```csharp
// Execute a sequence of keys
foreach (var key in new[] { Key.H, Key.E, Key.L, Key.L, Key.O })
    simulator.KeyPress(control, key);

// Keys with modifiers
foreach (var key in new[] { Key.A, Key.B })
    simulator.KeyPress(control, key, modifiers: KeyModifiers.Control);
```

---

Adapted from the [PanAndZoom headless testing guide](https://github.com/wieslawsoltes/PanAndZoom/blob/0c938aa771970ad8e2387f669f910c8b2a2d4d30/docs/HeadlessTestingAPI.md), copyright © Wiesław Šoltés, [MIT licensed](../../attribution/panandzoom-license.md).

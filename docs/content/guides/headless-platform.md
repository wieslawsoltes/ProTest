---
title: "Headless platform"
description: "Simulate input, flush dispatcher work, and run manual sessions."
sidebar_position: 2
doc-type: how-to
---

The headless backend replaces native windows with an in-memory implementation. Avalonia still creates controls, evaluates bindings and styles, performs layout, and processes input. Install an [adapter](../getting-started/xunit.md) for test-managed setup, or use `ProTest.Avalonia.Headless` for a manual session.

## Simulate platform input

Import `Avalonia.Headless` to use extension methods on a headless `Window` or other `TopLevel`.

| Operation | Methods |
| --- | --- |
| Text composition | `KeyTextInput(string)` |
| Keyboard keys | `KeyPress`, `KeyRelease`, `KeyPressQwerty`, `KeyReleaseQwerty` |
| Mouse | `MouseDown`, `MouseUp`, `MouseMove`, `MouseWheel` |
| Touch | `TouchBegin`, `TouchMove`, `TouchEnd` |
| Drag/drop | `DragDrop`, using Avalonia 12's `IDataTransfer` |
| DPI | `SetRenderScaling` |

Keyboard key events and text composition are separate. Focus the target control before sending text:

```csharp
editor.Focus();
window.KeyTextInput("ProTest");
window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);
window.KeyReleaseQwerty(PhysicalKey.Enter, RawInputModifiers.None);
```

Mouse coordinates are relative to the top level. For a 100 × 100 window containing a stretched button:

```csharp
window.MouseMove(new Point(50, 50));
window.MouseDown(new Point(50, 50), MouseButton.Left);
window.MouseUp(new Point(50, 50), MouseButton.Left);
```

These methods exercise input routing and hit testing. Calling `RaiseEvent` on a button is useful for testing an event subscriber, but does not substitute for the button's input and command behavior.

The richer [input helpers](../getting-started/helpers.md) target individual controls and build multi-step interactions.

## Bindings, focus, and real views

Instantiate your actual view and attach its view model before showing the window. Keep references to controls, use a namescope lookup for XAML names, or query the [visual and logical trees](../reference/trees/visual-tree.md).

```csharp
var editor = new TextBox();
var window = new Window { Content = editor };
window.Show();
try
{
    editor.Focus();
    Assert.True(editor.IsFocused);
    window.KeyTextInput("Changed");
    Assert.Equal("Changed", editor.Text);
}
finally
{
    window.Close();
}
```

For binding assertions, use the application's actual binding mode and property-change notifications. A `TwoWay` binding must write back through a writable source property; changing an unbound control proves only the control behavior.

## Dispatcher and render work

After a state change that schedules work, flush the dispatcher before reading the outcome:

```csharp
window.Width = 320;
window.Height = 200;
Dispatcher.UIThread.RunJobs();
Assert.Equal(new Size(320, 200), window.ClientSize);
```

`AvaloniaHeadlessPlatform.ForceRenderTimerTick()` advances the headless render timer. Window input helpers and frame capture perform their own internal queue processing. Prefer an explicit state condition over arbitrary delays.

## Manual sessions

Outside the xUnit adapter, use a session to own the dispatcher and application lifetime. `TestAppBuilder` is the same builder shown in the [quick start](../getting-started/xunit.md).

```csharp
await using var session = HeadlessUnitTestSession.StartNew(typeof(TestAppBuilder));
await session.Dispatch(() =>
{
    var editor = new TextBox();
    var window = new Window { Content = editor };
    window.Show();
    try
    {
        editor.Focus();
        window.KeyTextInput("Manual session");
        if (editor.Text != "Manual session")
            throw new InvalidOperationException("Text input did not reach the editor.");
    }
    finally
    {
        window.Close();
    }
}, CancellationToken.None);
```

Create, mutate, and inspect controls within dispatched work. Do not combine manual Avalonia initialization with the adapter-managed lifecycle in the same test.

See [rendering](rendering.md) for pixel capture and all platform options. Topic reference: [Avalonia headless platform guide](https://docs.avaloniaui.net/docs/testing/setting-up-the-headless-platform).

---
title: "Choose a testing helper"
description: "Find the input, query, automation, or recording API for your test."
sidebar_position: 3
doc-type: how-to
---

The testing helpers are included in `ProTest.Avalonia.Headless`, and therefore in both xUnit adapter installations. Use them inside an Avalonia test or a manual headless session.

| Goal | API | Reference |
| --- | --- | --- |
| Exercise platform keyboard/mouse processing | `HeadlessWindowExtensions` | [Platform input](../guides/headless-platform.md) |
| Raise pointer events on a selected control | `TouchInputSimulator`, `MouseInputSimulator` | [Touch](../reference/input/touch.md), [mouse](../reference/input/mouse.md) |
| Compose keyboard shortcuts and text input | `KeyboardInputSimulator` | [Keyboard](../reference/input/keyboard.md) |
| Raise processed gesture events | `GestureSimulator` | [Gestures](../reference/input/gestures.md) |
| Feed pointer events through actual recognizers | `GestureRecognizerTestHelper`, `MultiTouchTestHelperFactory` | [Recognizers](../reference/input/gesture-recognizers.md), [multi-touch](../reference/input/multi-touch.md) |
| Find descendants and inspect ancestry | `VisualTreeTestHelper`, `LogicalTreeTestHelper` | [Visual](../reference/trees/visual-tree.md), [logical](../reference/trees/logical-tree.md) |
| Combine type, state, name, and property filters | `ControlFinder` | [Control finder](../reference/trees/control-finder.md) |
| Query with path expressions | `TreeXPath` | [XPath](../reference/trees/xpath.md) |
| Validate or compare a tree | `TreeValidator`, `TreeComparer`, `TemplateComparer` | [Validation](../reference/trees/validation.md), [comparison](../reference/trees/comparison.md), [templates](../reference/trees/templates.md) |
| Use Appium-style locators and action chains | `AvaloniaDriver`, `By`, `Actions`, `TouchAction` | [Automation](../reference/automation/driver.md) |
| Wait for changing UI state | `DriverWait`, `ExpectedConditions`, `WaitHelper` | [Waits](../reference/automation/waits.md) |
| Capture a sequence of frames | `HeadlessScreenRecorder`, `RecordedTouchSimulator` | [Recording](../reference/recording/screen-recorder.md) |
| Convert recorded PNGs into a video | `VideoConverter` | [Video conversion](../reference/recording/video-converter.md) |

## Namespaces

```csharp
using Avalonia.HeadlessTestingFramework;
using Avalonia.HeadlessTestingFramework.Appium;
using Avalonia.HeadlessTestingFramework.Recording;
```

The helpers operate in process. They do not launch a native window or connect to a WebDriver server. Pointer-event helpers target the supplied control; the platform's `Window.MouseDown` / `MouseUp` methods exercise hit testing from window coordinates.

## Query a control tree

```csharp
var panel = new StackPanel
{
    Children =
    {
        new Button { Name = "Save", Content = "Save" },
        new TextBox { Name = "Title", Text = "Draft" }
    }
};
var window = new Window { Content = panel };
window.Show();
try
{
    var save = window.GetByName<Button>("Save");
    Assert.True(ControlFinder.From(window).OfType<TextBox>().Any());
    Assert.Equal("Save", save.Content);
}
finally
{
    window.Close();
}
```

For recognizer-based gestures, configure handlers and recognizers before attaching the control to a window. See [gesture registration](../guides/gesture-registration.md).

The [API catalog](../reference/api-catalog.md) lists every imported source component and its public signatures.

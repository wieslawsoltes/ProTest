---
title: "Control finder"
description: "Examples and API guidance for control finder."
sidebar_position: 3
doc-type: reference
---

Fluent API for building complex control queries.

```csharp
using Avalonia.HeadlessTestingFramework;

// Basic usage
var buttons = ControlFinder.From(window)
    .OfType<Button>()
    .FindAll();

// Complex query
var results = ControlFinder.From(window)
    .InVisualTree()           // or .InLogicalTree()
    .IncludeSelf()
    .MaxDepth(5)
    .OfType<Button>()
    .WithNameStartingWith("btn_")
    .Where<Button>(b => b.IsEnabled)
    .Skip(2)
    .Take(5)
    .FindAll();

// Find single element
Button? button = ControlFinder.From(window)
    .OfType<Button>()
    .WithName("submit")
    .FindFirst<Button>();

// Filter by property
var visibleButtons = ControlFinder.From(window)
    .OfType<Button>()
    .WithProperty(Visual.IsVisibleProperty, true)
    .FindAll();

// Filter by class/style
var primaryButtons = ControlFinder.From(window)
    .OfType<Button>()
    .WithClass("primary")
    .FindAll();

// Filter by automation ID
var loginBtn = ControlFinder.From(window)
    .Where<Control>(c => AutomationProperties.GetAutomationId(c) == "login_button")
    .FindFirst();

// Check existence
bool hasErrors = ControlFinder.From(form)
    .OfType<TextBlock>()
    .WithClass("error")
    .Any();

// Count matching
int buttonCount = ControlFinder.From(toolbar)
    .OfType<Button>()
    .Count();
```

---

Adapted from the [PanAndZoom headless testing guide](https://github.com/wieslawsoltes/PanAndZoom/blob/0c938aa771970ad8e2387f669f910c8b2a2d4d30/docs/HeadlessTestingAPI.md), copyright © Wiesław Šoltés, [MIT licensed](../../attribution/panandzoom-license.md).

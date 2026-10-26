---
title: "Visual tree helpers"
description: "Examples and API guidance for visual tree helpers."
sidebar_position: 1
doc-type: reference
---

Extension methods for traversing and querying the visual tree.

```csharp
using Avalonia.HeadlessTestingFramework;

// Find first descendant of type
Button? button = window.FindFirst<Button>();

// Find all descendants of type
IEnumerable<TextBox> textBoxes = window.FindAll<TextBox>();

// Find with predicate
var enabledButtons = window.FindAll<Button>(b => b.IsEnabled);

// Find by name
Control? submitBtn = window.FindByName("submitButton");
Button? typedBtn = window.FindByName<Button>("submitButton");

// Include self in search
var allPanels = panel.FindAll<Panel>(includeSelf: true);

// Find parent of type
Window? parentWindow = button.FindAncestor<Window>();

// Find at specific depth
var directChildren = window.GetChildren<Control>();

// Get path to control
string path = button.GetPathString();
// Returns: "Window/Grid/StackPanel/Button"

// Check if descendant
bool isChild = button.GetAncestors<Visual>().Contains(window);

// Get visual bounds relative to ancestor
Rect? bounds = button.GetBoundsRelativeTo(window);
```

---

Adapted from the [PanAndZoom headless testing guide](https://github.com/wieslawsoltes/PanAndZoom/blob/0c938aa771970ad8e2387f669f910c8b2a2d4d30/docs/HeadlessTestingAPI.md), copyright © Wiesław Šoltés, [MIT licensed](../../attribution/panandzoom-license.md).

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

---
title: "Logical tree helpers"
description: "Examples and API guidance for logical tree helpers."
sidebar_position: 2
doc-type: reference
---

Extension methods for the logical tree (conceptual parent-child as defined in XAML).

```csharp
using Avalonia.HeadlessTestingFramework;

// Find in logical tree
Button? button = root.FindFirstLogical<Button>();
IEnumerable<TextBox> textBoxes = root.FindAllLogical<TextBox>();

// Find by name in logical tree
Control? ctrl = root.FindLogicalByName("myControl");
Button? btn = root.FindLogicalByName<Button>("submitBtn");

// Find logical parent
StackPanel? parent = button.FindLogicalAncestor<StackPanel>();

// Get logical path
string path = control.GetLogicalPathString();

// Check logical ancestry
bool isLogicalChild = control.GetLogicalAncestors<Control>().Contains(parent);

// Get logical children count
int childCount = panel.GetLogicalChildCount();

// Iterate logical children
foreach (var child in panel.GetLogicalChildren<Control>())
{
    // Process child
}
```

---

Adapted from the [PanAndZoom headless testing guide](https://github.com/wieslawsoltes/PanAndZoom/blob/0c938aa771970ad8e2387f669f910c8b2a2d4d30/docs/HeadlessTestingAPI.md), copyright © Wiesław Šoltés, [MIT licensed](../../attribution/panandzoom-license.md).

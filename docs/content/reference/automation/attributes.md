---
title: "Element attributes"
description: "Examples and API guidance for element attributes."
sidebar_position: 3
doc-type: reference
---

The `ElementAttributes` class provides standardized attribute access.

```csharp
using Avalonia.HeadlessTestingFramework.Appium;

var control = element.Control;

// Get standard attributes
string? id = ElementAttributes.GetAttribute(control, "id");
string? name = ElementAttributes.GetAttribute(control, "name");
string? automationId = ElementAttributes.GetAttribute(control, "automationid");
string? className = ElementAttributes.GetAttribute(control, "class");
string? text = ElementAttributes.GetAttribute(control, "text");
string? value = ElementAttributes.GetAttribute(control, "value");
string? enabled = ElementAttributes.GetAttribute(control, "enabled");
string? visible = ElementAttributes.GetAttribute(control, "visible");
string? focused = ElementAttributes.GetAttribute(control, "focused");

// Geometry attributes
string? x = ElementAttributes.GetAttribute(control, "x");
string? y = ElementAttributes.GetAttribute(control, "y");
string? width = ElementAttributes.GetAttribute(control, "width");
string? height = ElementAttributes.GetAttribute(control, "height");
string? rect = ElementAttributes.GetAttribute(control, "rect"); // "x,y,width,height"

// Visual attributes
string? opacity = ElementAttributes.GetAttribute(control, "opacity");
string? background = ElementAttributes.GetAttribute(control, "background");
string? foreground = ElementAttributes.GetAttribute(control, "foreground");

// Accessibility attributes
string? accessibleName = ElementAttributes.GetAttribute(control, "accessiblename");
string? helpText = ElementAttributes.GetAttribute(control, "helptext");

// Get multiple attributes
var attrs = ElementAttributes.GetAttributes(control, "id", "text", "enabled");

// Get all attributes
var allAttrs = ElementAttributes.GetAllAttributes(control);

// Check attribute existence
bool hasId = ElementAttributes.HasAttribute(control, "id");

// Check CSS class
bool hasPrimary = ElementAttributes.HasClass(control, "primary");

// Get CSS-like values
string? display = ElementAttributes.GetCssValue(control, "display");
string? visibility = ElementAttributes.GetCssValue(control, "visibility");
string? bgColor = ElementAttributes.GetCssValue(control, "background-color");

// Accessibility helpers
string name2 = ElementAttributes.GetAccessibleName(control);
string role = ElementAttributes.GetRole(control); // "button", "textbox", etc.
```

---

Adapted from the [PanAndZoom headless testing guide](https://github.com/wieslawsoltes/PanAndZoom/blob/0c938aa771970ad8e2387f669f910c8b2a2d4d30/docs/HeadlessTestingAPI.md), copyright © Wiesław Šoltés, [MIT licensed](../../attribution/panandzoom-license.md).

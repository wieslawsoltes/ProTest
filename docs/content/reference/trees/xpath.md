---
title: "XPath queries"
description: "Examples and API guidance for xpath queries."
sidebar_position: 4
doc-type: reference
---

XPath-like query engine for visual and logical trees.

```csharp
using Avalonia.HeadlessTestingFramework;

var xpath = new TreeXPath(window);

// Select by type
var buttons = xpath.Select<Button>("//Button");

// Select by path
var nestedButton = xpath.SelectFirst<Button>("/Window/Grid/StackPanel/Button");

// Select with attributes
var submitBtn = xpath.SelectFirst<Button>("//Button[@Name='submit']");

// Select by index
var secondButton = xpath.SelectFirst<Button>("//StackPanel/Button[2]");

// Select descendants
var allTextBlocks = xpath.Select<TextBlock>("//TextBlock");

// Complex queries
var enabledPrimaryBtns = xpath.Select<Button>(
    "//Button[@IsEnabled='true' and contains(@Classes, 'primary')]");

// Check existence
bool hasErrors = xpath.Exists("//TextBlock[@Classes='error']");

// Count matches
int buttonCount = xpath.Count("//Button");

// Select with predicates
var largeButtons = xpath.Select<Button>("//Button[Width > 100]");

// Parent axis
var buttonParent = xpath.SelectFirst("//Button[@Name='submit']/..");

// Sibling axis
var nextSibling = xpath.SelectFirst("//Button[@Name='first']/following-sibling::Button");
```

### Supported XPath Syntax

| Expression | Description |
|------------|-------------|
| `//Type` | Select all descendants of type |
| `/Path/To/Type` | Select by absolute path |
| `Type[@Name='value']` | Filter by property |
| `Type[index]` | Select by position (1-based) |
| `Type[@Prop='val' and @Prop2='val2']` | Multiple conditions |
| `Type[contains(@Prop, 'text')]` | Contains function |
| `Type[starts-with(@Name, 'btn')]` | Starts-with function |
| `..` | Parent node |
| `following-sibling::Type` | Following siblings |
| `preceding-sibling::Type` | Preceding siblings |
| `ancestor::Type` | Ancestor nodes |
| `descendant::Type` | Descendant nodes |

---

Adapted from the [PanAndZoom headless testing guide](https://github.com/wieslawsoltes/PanAndZoom/blob/0c938aa771970ad8e2387f669f910c8b2a2d4d30/docs/HeadlessTestingAPI.md), copyright © Wiesław Šoltés, [MIT licensed](../../attribution/panandzoom-license.md).

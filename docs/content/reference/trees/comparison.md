---
title: "Tree comparison"
description: "Examples and API guidance for tree comparison."
sidebar_position: 6
doc-type: reference
---

Compare two visual or logical trees to find differences.

```csharp
using Avalonia.HeadlessTestingFramework;

// Compare two trees
var result = TreeComparer.CompareVisualTrees(expectedWindow, actualWindow);

if (!result.AreEqual)
{
    Console.WriteLine($"Trees differ: {result.Differences.Count} differences");
    Console.WriteLine($"Match percentage: {result.MatchPercentage:F1}%");

    foreach (var diff in result.Differences)
    {
        Console.WriteLine($"  {diff}");
    }
}

// Compare with options
var options = new TreeComparisonOptions
{
    CompareTypes = true,
    CompareNames = true,
    CompareOrder = true,
    CompareChildCount = true,
    PropertiesToCompare = new() { Button.IsEnabledProperty, Visual.IsVisibleProperty },
    IgnoreTypes = new() { typeof(Border), typeof(ContentPresenter) },
    MaxDepth = 10
};

var result = TreeComparer.CompareVisualTrees(expected, actual, options);

// Compare logical trees
var logicalResult = TreeComparer.CompareLogicalTrees(expectedRoot, actualRoot);

// Summary and detailed differences
Console.WriteLine(result);
foreach (var difference in result.Differences)
    Console.WriteLine(difference);
```

### Difference Types

| Type | Description |
|------|-------------|
| `MissingNode` | Node in expected but not actual |
| `ExtraNode` | Node in actual but not expected |
| `TypeMismatch` | Node types differ |
| `PropertyMismatch` | Property values differ |
| `ChildCountMismatch` | Number of children differs |
| `OrderMismatch` | Child order differs |
| `NameMismatch` | Control names differ |

---

Adapted from the [PanAndZoom headless testing guide](https://github.com/wieslawsoltes/PanAndZoom/blob/0c938aa771970ad8e2387f669f910c8b2a2d4d30/docs/HeadlessTestingAPI.md), copyright © Wiesław Šoltés, [MIT licensed](../../attribution/panandzoom-license.md).

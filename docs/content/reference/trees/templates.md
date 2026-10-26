---
title: "Template inspection"
description: "Examples and API guidance for template inspection."
sidebar_position: 7
doc-type: reference
---

Compare control templates for testing theme/style consistency.

```csharp
using Avalonia.HeadlessTestingFramework;

// Compare templates of two controls
var result = TemplateComparer.CompareTemplates(expectedButton, actualButton);

if (!result.AreEqual)
{
    foreach (var diff in result.Differences)
    {
        Console.WriteLine($"Template difference: {diff}");
    }
}

// Get template info
var info = TemplateComparer.GetTemplateInfo(button);
Console.WriteLine($"Template target type: {info.TargetType}");
Console.WriteLine($"Part count: {info.PartCount}");
Console.WriteLine($"Visual count: {info.VisualCount}");
Console.WriteLine($"Tree depth: {info.TreeDepth}");

foreach (var part in info.Parts)
{
    Console.WriteLine($"  Part: {part.Name} ({part.Type?.Name})");
}

// Compare with custom options
var options = new TemplateComparisonOptions
{
    CompareParts = true,
    CompareStructure = true,
    IgnoreParts = new() { "PART_Decorator" }
};

var result = TemplateComparer.CompareTemplates(expected, actual, options);

// Validate template has required parts
bool hasParts = TemplateComparer.HasTemplateParts(button,
    "PART_ContentPresenter", "PART_Border");

// Missing parts declared as required by the control
var missing = TemplateComparer.GetMissingRequiredParts(button);
Console.WriteLine($"Has requested parts: {hasParts}");
Console.WriteLine($"Missing required parts: {string.Join(", ", missing)}");
```

---

Adapted from the [PanAndZoom headless testing guide](https://github.com/wieslawsoltes/PanAndZoom/blob/0c938aa771970ad8e2387f669f910c8b2a2d4d30/docs/HeadlessTestingAPI.md), copyright © Wiesław Šoltés, [MIT licensed](../../attribution/panandzoom-license.md).

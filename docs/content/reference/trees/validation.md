---
title: "Tree validation"
description: "Examples and API guidance for tree validation."
sidebar_position: 5
doc-type: reference
---

`TreeValidator` accumulates rules and applies them to a visual or logical tree. Validation returns the validator, whose `IsValid`, `Failures`, and `Results` properties expose the outcome.

```csharp
using Avalonia.Controls;
using Avalonia.HeadlessTestingFramework;
using System.Linq;

var validator = TreeValidator.Create()
    .RequireName("submitButton")
    .RequireNames("username", "password", "email")
    .RequireType<Button>()
    .RequireExactCount<TextBox>(3)
    .RequireMinCount<Label>(2)
    .RequireMaxCount<ComboBox>(1)
    .RequirePattern("//StackPanel/Button")
    .RequireProperty("submitButton", Button.IsEnabledProperty, true)
    .RequireVisible("submitButton")
    .ForbidType<ProgressBar>()
    .ForbidPattern("//*[@Name='deprecatedControl']")
    .Custom("Buttons should be at least 80 pixels wide",
        root => root.FindAll<Button>().All(button => button.Width >= 80));

validator.ValidateVisualTree(window);
foreach (var failure in validator.Failures)
    Console.WriteLine($"{failure.RuleDescription}: {failure.Message}");

validator.AssertValid();
```

Use `ValidateLogicalTree(root)` for the logical hierarchy. `AssertValid()` throws `TreeValidationException` when a rule fails; `GetSummary()` provides a readable result for diagnostic output.

Additional rules include `RequireNameOfType<T>`, `RequireEnabled`, `RequireChildOf`, and `RequireClass`. Rules that query a tree need the relevant controls to have been attached and their templates created. Validate the logical tree when the requirement concerns application content rather than generated template visuals.

---

Adapted from the [PanAndZoom headless testing guide](https://github.com/wieslawsoltes/PanAndZoom/blob/0c938aa771970ad8e2387f669f910c8b2a2d4d30/docs/HeadlessTestingAPI.md), copyright © Wiesław Šoltés, [MIT licensed](../../attribution/panandzoom-license.md).

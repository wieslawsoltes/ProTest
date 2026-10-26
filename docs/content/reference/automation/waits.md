---
title: "Asynchronous waits"
description: "Examples and API guidance for asynchronous waits."
sidebar_position: 5
doc-type: reference
---

The `WaitHelper` class provides advanced waiting utilities for asynchronous operations.

```csharp
using Avalonia.HeadlessTestingFramework.Appium;

var wait = new WaitHelper(TimeSpan.FromSeconds(10));

// Here element is an Avalonia Control, not an AvaloniaElement wrapper.
// Use wrappedElement.Control when starting from a driver element.

// Wait for condition
await wait.UntilAsync(() => element.IsVisible);
await wait.UntilAsync(async () => await IsDataLoadedAsync());

// Wait on UI thread
await wait.UntilOnUIThreadAsync(() => control.IsVisible);

// Wait for non-null result
var element = await wait.UntilNotNullAsync(() => FindElement("button"));

// Element-specific waits
await wait.ForVisibleAsync(element);
await wait.ForHiddenAsync(element);
await wait.ForEnabledAsync(element);
await wait.ForDisabledAsync(element);
await wait.ForFocusedAsync(element);

// Property value wait
await wait.ForPropertyValueAsync(element, Control.OpacityProperty, 1.0);

// Text waits
await wait.ForTextAsync(textBlock, "Expected Text");
await wait.ForTextContainsAsync(textBlock, "substring");
await wait.ForTextNotEmptyAsync(textBlock);

// Collection waits
await wait.ForCountAsync(() => GetItems(), expectedCount: 5);
await wait.ForMinimumCountAsync(() => GetItems(), minimumCount: 3);

// Non-throwing variants
bool success = await wait.TryUntilAsync(() => element.IsVisible);
var element = await wait.TryForElementAsync(() => FindElement("optional"));

// Fluent builder
await WaitHelper.With(TimeSpan.FromSeconds(5))
    .WithPollingInterval(TimeSpan.FromMilliseconds(200))
    .WithMessage("Element did not become visible")
    .IgnoreException<InvalidOperationException>()
    .UntilAsync(() => element.IsVisible);
```

---

Adapted from the [PanAndZoom headless testing guide](https://github.com/wieslawsoltes/PanAndZoom/blob/0c938aa771970ad8e2387f669f910c8b2a2d4d30/docs/HeadlessTestingAPI.md), copyright © Wiesław Šoltés, [MIT licensed](../../attribution/panandzoom-license.md).

---
title: "Action sequences"
description: "Examples and API guidance for action sequences."
sidebar_position: 2
doc-type: reference
---

The `Actions` class provides Selenium 4-style Actions API for building complex interaction sequences.

```csharp
using Avalonia.HeadlessTestingFramework.Appium;

// Basic mouse actions
new Actions(driver)
    .MoveToElement(element)
    .Click()
    .Perform();

// Complex sequence with keyboard and mouse
new Actions(driver)
    .MoveToElement(inputField)
    .Click()
    .KeyDown(Key.LeftShift)
    .SendKeys("HELLO")
    .KeyUp(Key.LeftShift)
    .Perform();

// Drag and drop
new Actions(driver)
    .DragAndDrop(sourceElement, targetElement)
    .Perform();

// Drag by offset
new Actions(driver)
    .DragAndDropBy(element, 100, 50)
    .Perform();

// Double click
new Actions(driver)
    .DoubleClick(element)
    .Perform();

// Context click (right-click)
new Actions(driver)
    .ContextClick(element)
    .Perform();

// Click and hold
new Actions(driver)
    .MoveToElement(element)
    .ClickAndHold()
    .MoveByOffset(100, 0)
    .Release()
    .Perform();

// Scroll
new Actions(driver)
    .Scroll(0, -100)
    .Perform();

// Scroll to element
new Actions(driver)
    .ScrollToElement(element)
    .Perform();

// With pauses
new Actions(driver)
    .MoveToElement(element)
    .Pause(500)
    .Click()
    .Pause(TimeSpan.FromMilliseconds(200))
    .SendKeys("text")
    .Perform();

// Async execution
await new Actions(driver)
    .MoveToElement(element)
    .Click()
    .PerformAsync();

// Reset and reuse
var actions = new Actions(driver);
actions.Click(element1).Perform();
actions.Reset();
actions.Click(element2).Perform();
```

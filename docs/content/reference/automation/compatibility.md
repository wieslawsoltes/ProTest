---
title: "WebDriver API mapping"
description: "Examples and API guidance for webdriver api mapping."
sidebar_position: 6
doc-type: reference
---

This table maps familiar Selenium/Appium concepts to local Avalonia helpers. A matching name does not imply WebDriver protocol compliance or equivalent native accessibility behavior. Use [real Appium testing](../../guides/appium.md) when those are required.

This section provides a comprehensive mapping between standard Selenium/Appium APIs and their HeadlessTestingFramework equivalents.

## Element Locators (By Class)

| Selenium/Appium | HeadlessTestingFramework | Notes |
|-----------------|--------------------------|-------|
| `By.Id("id")` | `By.Id("id")` | Corresponding local operation |
| `By.Name("name")` | `By.Name("name")` | Corresponding local operation |
| `By.ClassName("class")` | `By.ClassName("class")` | Maps to Avalonia control type name |
| `By.TagName("tag")` | `By.TagName("tag")` | Maps to control type name |
| `By.CssSelector(".class")` | `By.CssSelector(".class")` | CSS class selector support |
| `By.CssSelector("#id")` | `By.CssSelector("#id")` | ID selector support |
| `By.LinkText("text")` | `By.LinkText("text")` | Maps to exact text match |
| `By.PartialLinkText("text")` | `By.PartialLinkText("text")` | Maps to partial text match |
| `By.XPath("//path")` | `By.XPath("//path")` | XPath-like expressions |
| - | `By.AutomationId("id")` | Avalonia-specific, uses AutomationProperties.AutomationId |
| - | `By.Text("text")` | Avalonia-specific, exact text match |
| - | `By.ContainsText("text")` | Avalonia-specific, partial text match |
| - | `By.CssClass("class")` | Avalonia-specific, matches Avalonia CSS classes |
| - | `By.Property("prop", value)` | Avalonia-specific, property value matching |
| - | `By.Type<T>()` | Avalonia-specific, generic type matching |
| - | `By.Type(Type)` | Avalonia-specific, type matching (non-generic) |
| - | `By.FullClassName("ns.Class")` | Avalonia-specific, full type name with namespace |
| - | `By.NameRegex("pattern")` | Avalonia-specific, regex pattern matching |
| - | `By.AccessibilityName("name")` | Avalonia-specific, AutomationProperties.Name |
| - | `By.AccessibilityLabel("label")` | Avalonia-specific, accessibility label |
| - | `By.Predicate(desc, key)` | Avalonia-specific, custom predicate matching |
| - | `By.Focused()` | Avalonia-specific, find currently focused element |

## Composite Locators

| Selenium/Appium | HeadlessTestingFramework | Notes |
|-----------------|--------------------------|-------|
| Multiple `findElement` calls | `By.Chained(By..., By...)` | Chain locators for nested search |
| - | `By.All(By..., By...)` | All conditions must match (AND) |
| - | `By.Any(By..., By...)` | Any condition can match (OR) |

## WebElement Methods (AvaloniaElement)

| Selenium/Appium | HeadlessTestingFramework | Notes |
|-----------------|--------------------------|-------|
| `element.click()` | `element.Click()` | Corresponding local operation |
| - | `element.DoubleClick()` | Extended: double-click action |
| - | `element.RightClick()` | Extended: right-click/context menu |
| `element.sendKeys("text")` | `element.SendKeys("text")` | Corresponding local operation |
| `element.clear()` | `element.Clear()` | Corresponding local operation |
| `element.submit()` | `element.Submit()` | Presses Enter or clicks button |
| `element.getText()` | `element.Text` | Property instead of method |
| `element.getTagName()` | `element.TagName` | Property instead of method |
| - | `element.FullTagName` | Extended: full type name with namespace |
| `element.getAttribute("attr")` | `element.GetAttribute("attr")` | Corresponding local operation |
| `element.isDisplayed()` | `element.Displayed` | Property instead of method |
| `element.isEnabled()` | `element.Enabled` | Property instead of method |
| `element.isSelected()` | `element.Selected` | Property instead of method |
| `element.getLocation()` | `element.Location` | Property instead of method |
| `element.getSize()` | `element.Size` | Property instead of method |
| `element.findElement(By)` | `element.FindElement(By)` | Corresponding local operation |
| `element.findElements(By)` | `element.FindElements(By)` | Corresponding local operation |
| `element.getScreenshotAs()` | `element.Screenshot()` | Returns bitmap |
| - | `element.Focused` | Avalonia-specific |
| - | `element.Focus()` | Avalonia-specific |
| - | `element.Hover()` | Avalonia-specific |
| - | `element.ScrollIntoView()` | Avalonia-specific |
| - | `element.DragTo(target)` | Avalonia-specific drag and drop |
| - | `element.Parent` | Avalonia-specific navigation |
| - | `element.Children` | Avalonia-specific navigation |
| - | `element.TryFindElement(By)` | Returns null instead of throwing |
| - | `element.Rect` | Bounding rectangle |
| - | `element.Center` | Center point of element |
| - | `element.AbsoluteCenter` | Absolute center in screen coordinates |
| - | `element.GetProperty<T>(name)` | Get typed Avalonia property value |
| - | `element.SetProperty(name, value)` | Set Avalonia property value |
| - | `element.GetClasses()` | Get CSS/style classes |
| - | `element.HasClass(name)` | Check for CSS class |
| - | `element.WaitUntil(condition)` | Wait for custom condition |
| - | `element.WaitUntilVisible()` | Wait until element is visible |
| - | `element.WaitUntilEnabled()` | Wait until element is enabled |
| - | `element.WaitUntilClickable()` | Wait until element is clickable |

## Touch Element Methods (Appium)

| Appium | HeadlessTestingFramework | Notes |
|--------|--------------------------|-------|
| `element.tap()` (MobileElement) | `element.Tap()` | Touch tap |
| - | `element.DoubleTap()` | Extended: touch double-tap |
| `element.longPress()` | `element.LongPress(durationMs)` | Long press/hold |
| `element.swipe()` | `element.Swipe(direction, distance)` | Swipe gesture |
| - | `element.Pinch(scale)` | Extended: pinch zoom |
| - | `element.Scroll(deltaX, deltaY)` | Extended: scroll gesture |
| - | `element.PressKey(key, modifiers)` | Extended: keyboard input |

## Touch Actions (TouchAction Class)

| Appium TouchAction | HeadlessTestingFramework | Notes |
|--------------------|--------------------------|-------|
| `touchAction.press(element)` | `touchAction.Press(element)` | Corresponding local operation |
| `touchAction.press(x, y)` | `touchAction.Press(x, y)` | Corresponding local operation |
| `touchAction.release()` | `touchAction.Release()` | Corresponding local operation |
| `touchAction.moveTo(element)` | `touchAction.MoveTo(element)` | Corresponding local operation |
| `touchAction.moveTo(x, y)` | `touchAction.MoveTo(x, y)` | Corresponding local operation |
| `touchAction.tap(element)` | `touchAction.Tap(element)` | Corresponding local operation |
| `touchAction.tap(x, y)` | `touchAction.Tap(x, y)` | Corresponding local operation |
| `touchAction.longPress(element)` | `touchAction.LongPress(element)` | Corresponding local operation |
| `touchAction.longPress(x, y, duration)` | `touchAction.LongPress(x, y, duration)` | Corresponding local operation |
| `touchAction.wait(ms)` | `touchAction.Wait(ms)` | Corresponding local operation |
| `touchAction.perform()` | `touchAction.Perform()` | Corresponding local operation |
| - | `touchAction.Swipe(direction, distance)` | Extended API |
| - | `touchAction.DoubleTap(element)` | Extended API |
| - | `touchAction.Cancel()` | Extended API |

## Multi-Touch Actions

| Appium MultiTouchAction | HeadlessTestingFramework | Notes |
|-------------------------|--------------------------|-------|
| `multiTouchAction.add(touchAction)` | `multiTouchAction.Add(touchAction)` | Corresponding local operation |
| `multiTouchAction.perform()` | `multiTouchAction.Perform()` | Corresponding local operation |
| - | `MultiTouchAction.Pinch(driver, element, scale)` | Convenience method |
| - | `MultiTouchAction.Scroll(driver, element, x, y)` | Convenience method |

## Actions API (Selenium 4)

| Selenium Actions | HeadlessTestingFramework | Notes |
|------------------|--------------------------|-------|
| `actions.moveToElement(element)` | `actions.MoveToElement(element)` | Corresponding local operation |
| `actions.moveByOffset(x, y)` | `actions.MoveByOffset(x, y)` | Corresponding local operation |
| `actions.moveToLocation(x, y)` | `actions.MoveToLocation(x, y)` | Corresponding local operation |
| `actions.click()` | `actions.Click()` | Corresponding local operation |
| `actions.click(element)` | `actions.Click(element)` | Corresponding local operation |
| `actions.doubleClick()` | `actions.DoubleClick()` | Corresponding local operation |
| `actions.doubleClick(element)` | `actions.DoubleClick(element)` | Corresponding local operation |
| `actions.contextClick()` | `actions.ContextClick()` | Corresponding local operation |
| `actions.contextClick(element)` | `actions.ContextClick(element)` | Corresponding local operation |
| `actions.clickAndHold()` | `actions.ClickAndHold()` | Corresponding local operation |
| `actions.clickAndHold(element)` | `actions.ClickAndHold(element)` | Corresponding local operation |
| `actions.release()` | `actions.Release()` | Corresponding local operation |
| `actions.release(element)` | `actions.MoveToElement(element).Release()` | Corresponding local operation |
| `actions.dragAndDrop(source, target)` | `actions.DragAndDrop(source, target)` | Corresponding local operation |
| `actions.dragAndDropBy(source, x, y)` | `actions.DragAndDropBy(source, x, y)` | Corresponding local operation |
| `actions.keyDown(key)` | `actions.KeyDown(key)` | Corresponding local operation |
| `actions.keyUp(key)` | `actions.KeyUp(key)` | Corresponding local operation |
| `actions.sendKeys(keys)` | `actions.SendKeys(keys)` | Corresponding local operation |
| `actions.pause(duration)` | `actions.Pause(duration)` | Corresponding local operation |
| `actions.scroll(x, y)` | `actions.Scroll(x, y)` | Corresponding local operation |
| `actions.scrollToElement(element)` | `actions.ScrollToElement(element)` | Corresponding local operation |
| `actions.perform()` | `actions.Perform()` | Corresponding local operation |
| - | `actions.PerformAsync()` | Extended async API |
| - | `actions.Reset()` | Extended API |

## WebDriverWait / Expected Conditions

| Selenium ExpectedConditions | HeadlessTestingFramework | Notes |
|-----------------------------|--------------------------|-------|
| `presenceOfElementLocated(By)` | `ElementExists(By)` | Corresponding local operation |
| `visibilityOfElementLocated(By)` | `ElementIsVisible(By)` | Corresponding local operation |
| `elementToBeClickable(By)` | `ElementToBeClickable(By)` | Corresponding local operation |
| `invisibilityOfElementLocated(By)` | `InvisibilityOfElement(By)` | Corresponding local operation |
| `textToBe(By, "text")` | `TextToBe(By, "text")` | Corresponding local operation |
| `textToBePresentInElement(By, "text")` | `TextToBePresentInElement(By, "text")` | Corresponding local operation |
| `attributeToBe(By, "attr", "value")` | `AttributeToBe(By, "attr", "value")` | Corresponding local operation |
| `attributeContains(By, "attr", "value")` | `AttributeContains(By, "attr", "value")` | Corresponding local operation |
| `numberOfElementsToBe(By, count)` | `NumberOfElementsToBe(By, count)` | Corresponding local operation |
| `numberOfElementsToBeMoreThan(By, count)` | `NumberOfElementsToBeMoreThan(By, count)` | Corresponding local operation |
| `titleIs("title")` | `TitleIs("title")` | Corresponding local operation |
| `titleContains("text")` | `TitleContains("text")` | Corresponding local operation |
| `and(condition1, condition2)` | `And(condition1, condition2)` | Corresponding local operation |
| `or(condition1, condition2)` | `Or(condition1, condition2)` | Corresponding local operation |
| `not(condition)` | `Not(condition)` | Corresponding local operation |
| - | `ElementToBeEnabled(By)` | Extended API |
| - | `ElementToBeFocused(By)` | Extended API |
| - | `TextToMatch(By, regex)` | Extended API |
| - | `CustomCondition(predicate)` | Check a control property with a custom predicate |

## WebDriver Methods (AvaloniaDriver)

| Selenium WebDriver | HeadlessTestingFramework | Notes |
|--------------------|--------------------------|-------|
| `driver.findElement(By)` | `driver.FindElement(By)` | Corresponding local operation |
| `driver.findElements(By)` | `driver.FindElements(By)` | Corresponding local operation |
| `driver.getPageSource()` | `driver.PageSource` | Returns XML tree |
| `driver.getTitle()` | `driver.Title` | Window title |
| `driver.close()` | `driver.Close()` | Corresponding local operation |
| `driver.quit()` | `driver.Dispose()` | IDisposable pattern |
| `driver.getScreenshotAs()` | `driver.Screenshot()` | Returns bitmap |
| `driver.manage().window().setSize()` | `driver.SetWindowSize(w, h)` | Corresponding local operation |
| `driver.manage().window().getSize()` | `driver.GetWindowSize()` | Corresponding local operation |
| `driver.manage().window().setPosition()` | `driver.SetWindowPosition(x, y)` | Corresponding local operation |
| `driver.manage().window().getPosition()` | `driver.GetWindowPosition()` | Corresponding local operation |
| `driver.manage().window().maximize()` | `driver.Maximize()` | Corresponding local operation |
| `driver.manage().window().minimize()` | `driver.Minimize()` | Corresponding local operation |
| `driver.manage().window().fullscreen()` | `driver.Fullscreen()` | Corresponding local operation |
| `driver.manage().timeouts().implicitlyWait()` | `driver.ImplicitWait` | Property |
| `new WebDriverWait(driver, timeout)` | `driver.Wait` | Pre-configured |
| - | `driver.CreateTouchAction()` | Touch action factory |
| - | `new Actions(driver)` | Actions API factory |
| - | `driver.ElementExists(By)` | Convenience method |
| - | `driver.ElementCount(By)` | Count matching elements |
| - | `driver.TryFindElement(By)` | Returns null instead of throwing |
| - | `driver.ActiveElement` | Get currently focused element |
| - | `driver.Restore()` | Restore window from maximized/minimized |
| - | `driver.Refresh()` | Force layout update |
| - | `driver.Back()` | Navigate back (Alt+Back) |
| - | `driver.Forward()` | Navigate forward (Alt+Right) |
| - | `driver.NavigateTo<T>()` | Navigate to view by type |
| - | `driver.RegisterPredicate(key, func)` | Register custom predicate for By.Predicate |
| - | `AvaloniaDriver.FromApplication()` | Create driver from current app's main window |

## Screenshot Methods

| Selenium/Appium | HeadlessTestingFramework | Notes |
|-----------------|--------------------------|-------|
| `driver.getScreenshotAs(OutputType.FILE)` | `driver.SaveScreenshot("path.png")` | Saves to file |
| `driver.getScreenshotAs(OutputType.BASE64)` | `driver.ScreenshotAsBase64()` | Base64 string |
| `driver.getScreenshotAs(OutputType.BYTES)` | `driver.Screenshot()` | Returns bitmap |
| `element.getScreenshotAs(...)` | `element.SaveScreenshot("path.png")` | Element screenshot |

## Mapping notes

| Note | Meaning |
|--------|---------|
| | Full compatibility - API matches Selenium/Appium standard |
| Maps to | Functionality available but with Avalonia-specific naming |
| Extended API | Additional functionality not in standard Selenium/Appium |
| - | Not applicable / Avalonia-specific feature |

## Migration Guide: Selenium to HeadlessTestingFramework

```csharp
// Selenium
IWebDriver driver = new ChromeDriver();
IWebElement button = driver.FindElement(By.Id("submit"));
button.Click();
button.SendKeys("text");
driver.Quit();

// HeadlessTestingFramework - nearly identical!
using var driver = new AvaloniaDriver(window);
var button = driver.FindElement(By.Id("submit"));
button.Click();
button.SendKeys("text");
// driver.Dispose() called automatically
```

## Migration Guide: Appium to HeadlessTestingFramework

```csharp
// Appium
AndroidDriver driver = new AndroidDriver(serverUrl, capabilities);
var element = driver.FindElement(By.AccessibilityId("myElement"));
new TouchAction(driver)
    .Press(element)
    .Wait(500)
    .MoveTo(100, 200)
    .Release()
    .Perform();

// HeadlessTestingFramework - similar pattern!
using var driver = new AvaloniaDriver(window);
var element = driver.FindElement(By.AutomationId("myElement"));
driver.CreateTouchAction()
    .Press(element)
    .Wait(500)
    .MoveTo(100, 200)
    .Release()
    .Perform();
```

---

Adapted from the [PanAndZoom headless testing guide](https://github.com/wieslawsoltes/PanAndZoom/blob/0c938aa771970ad8e2387f669f910c8b2a2d4d30/docs/HeadlessTestingAPI.md), copyright © Wiesław Šoltés, [MIT licensed](../../attribution/panandzoom-license.md).

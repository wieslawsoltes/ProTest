---
title: "Driver and elements"
description: "Examples and API guidance for driver and elements."
sidebar_position: 1
doc-type: reference
---

The in-process `AvaloniaDriver` uses familiar Appium and Selenium names. It drives Avalonia controls directly; it is not an Appium server, a WebDriver protocol implementation, or a drop-in `OpenQA.Selenium` driver.

## Namespaces

```csharp
using Avalonia.HeadlessTestingFramework.Appium;
```

## AvaloniaDriver

The main entry point for Appium-style testing.

```csharp
// Create driver from a window
using var driver = new AvaloniaDriver(window);

// Or from a control
using var driver = new AvaloniaDriver(rootControl);
```

## Finding Elements

Use `By` locators to find elements, similar to Selenium/Appium:

```csharp
// By ID (Name property)
var button = driver.FindElement(By.Id("submitButton"));

// By AutomationId
var input = driver.FindElement(By.AutomationId("username_field"));

// By class name (type name)
var buttons = driver.FindElements(By.ClassName("Button"));

// By text content
var element = driver.FindElement(By.Text("Click Me"));

// By partial text
var element = driver.FindElement(By.ContainsText("Click"));

// By CSS selector (standard Selenium method)
var primary = driver.FindElement(By.CssSelector(".primary"));

// By CSS class (Avalonia-specific)
var styled = driver.FindElement(By.CssClass("primary"));

// By link text (standard Selenium)
var link = driver.FindElement(By.LinkText("Click here"));

// By partial link text (standard Selenium)
var partialLink = driver.FindElement(By.PartialLinkText("Click"));

// By XPath-like expression
var nested = driver.FindElement(By.XPath("//StackPanel/Button[@Name='submit']"));

// By tag name (standard Selenium)
var buttons = driver.FindElements(By.TagName("Button"));

// By property value
var enabled = driver.FindElement(By.Property("IsEnabled", true));

// Generic type
var button = driver.FindElement(By.Type<Button>());

// Regex pattern
var elements = driver.FindElements(By.NameRegex("btn_.*"));
```

## Composite Locators

Combine multiple locators:

```csharp
// All conditions must match (AND)
var element = driver.FindElement(By.All(
    By.ClassName("Button"),
    By.Text("Submit")));

// Any condition can match (OR)
var element = driver.FindElement(By.Any(
    By.Id("submitBtn"),
    By.Id("okBtn")));

// Chained search (nested)
var element = driver.FindElement(By.Chained(
    By.Id("container"),
    By.ClassName("Button")));
```

## AvaloniaElement

Wrapper around Avalonia controls with Appium-like methods:

```csharp
var element = driver.FindElement(By.Id("myButton"));

// Properties
string id = element.Id;
string tagName = element.TagName;
string text = element.Text;
bool displayed = element.Displayed;
bool enabled = element.Enabled;
bool selected = element.Selected;
bool focused = element.Focused;
Point location = element.Location;
Size size = element.Size;

// Actions (chainable) - Standard Selenium methods
element
    .Click()
    .SendKeys("Hello")
    .Clear()
    .Submit()  // Standard Selenium form submission
    .Focus()
    .Hover()
    .ScrollIntoView();

// Touch actions (Appium-compatible)
element.Tap();
element.DoubleTap();
element.LongPress(1000);
element.Swipe(SwipeDirection.Up, 200);
element.Pinch(1.5); // Zoom in
element.Scroll(0, -100);

// Keyboard
element.PressKey(Key.Enter);

// Attributes (standard Selenium getAttribute)
string value = element.GetAttribute("enabled");
var isEnabled = element.GetProperty<bool>("IsEnabled");
element.SetProperty("IsEnabled", false);

// CSS classes
var classes = element.GetClasses();
bool hasClass = element.HasClass("primary");

// Find child elements
var child = element.FindElement(By.ClassName("TextBlock"));
var children = element.FindElements(By.ClassName("Button"));

// Navigation
var parent = element.Parent;
var kids = element.Children;

// Screenshots
element.SaveScreenshot("element.png");
using var bitmap = element.Screenshot();
```

## TouchAction

Build complex gesture sequences:

```csharp
// Press and drag
driver.CreateTouchAction()
    .Press(element)
    .Wait(100)
    .MoveTo(200, 300)
    .Release()
    .Perform();

// Long press
driver.CreateTouchAction()
    .LongPress(element, 1000)
    .Release()
    .Perform();

// Swipe
driver.CreateTouchAction()
    .Press(100, 100)
    .Swipe(SwipeDirection.Left, 200)
    .Release()
    .Perform();

// Complex gesture
driver.CreateTouchAction()
    .Tap(element)
    .Wait(500)
    .Tap(100, 200, count: 2) // Double tap at position
    .Perform();

// Multi-touch (pinch)
MultiTouchAction.Pinch(driver, element, 0.5, durationMs: 300);
MultiTouchAction.Scroll(driver, element, 0, -100);
```

## Waiting

WebDriverWait-style explicit waits:

```csharp
// Configure default timeout
driver.Wait.Timeout = TimeSpan.FromSeconds(10);
driver.Wait.PollingInterval = TimeSpan.FromMilliseconds(100);

// Wait for element
var element = driver.Wait.ForElement(By.Id("loading"));

// Wait for visible
var visible = driver.Wait.ForElementVisible(By.Id("content"));

// Wait for clickable
var clickable = driver.Wait.ForElementClickable(By.Id("submit"));

// Wait for element to disappear
driver.Wait.ForElementNotPresent(By.Id("spinner"));

// Wait for text
driver.Wait.ForTextPresent(By.Id("result"), "Success");

// Custom condition
driver.Wait.Until(d => d.FindElement(By.Id("count")).Text == "5");

// Fixed delay
driver.Wait.ForMilliseconds(500);
```

## ExpectedConditions

Pre-built conditions for common scenarios:

```csharp
using static Avalonia.HeadlessTestingFramework.Appium.ExpectedConditions;

// Element conditions
var el = driver.Wait.Until(ElementExists(By.Id("btn")));
var visible = driver.Wait.Until(ElementIsVisible(By.Id("btn")));
var clickable = driver.Wait.Until(ElementToBeClickable(By.Id("btn")));

// Invisibility
driver.Wait.Until(InvisibilityOfElement(By.Id("loading")));

// Text conditions
driver.Wait.Until(TextToBe(By.Id("status"), "Complete"));
driver.Wait.Until(TextToBePresentInElement(By.Id("log"), "Success"));
driver.Wait.Until(TextToMatch(By.Id("code"), @"\d{6}"));

// Attribute conditions
driver.Wait.Until(AttributeToBe(By.Id("input"), "value", "test"));
driver.Wait.Until(AttributeContains(By.Id("class"), "class", "active"));

// Count conditions
var elements = driver.Wait.Until(NumberOfElementsToBe(By.ClassName("Item"), 5));
var moreElements = driver.Wait.Until(NumberOfElementsToBeMoreThan(By.ClassName("Item"), 3));

// Logical combinations
driver.Wait.Until(And(
    ElementToBeEnabled(By.Id("submit")),
    d => d.ElementExists(By.Id("form"))));

driver.Wait.Until(Or(
    ElementExists(By.Id("success")),
    ElementExists(By.Id("error"))));

driver.Wait.Until(Not(ElementExists(By.Id("loading"))));

// Window conditions
driver.Wait.Until(TitleIs("Dashboard"));
driver.Wait.Until(TitleContains("Settings"));
```

## Screenshots

```csharp
// Full window screenshot
driver.SaveScreenshot("window.png");
var bitmap = driver.Screenshot();
var base64 = driver.ScreenshotAsBase64();

// Element screenshot
element.SaveScreenshot("element.png");
```

## Window Management

```csharp
// Size
driver.SetWindowSize(1280, 720);
var size = driver.GetWindowSize();

// Position
driver.SetWindowPosition(100, 100);
var pos = driver.GetWindowPosition();

// States
driver.Maximize();
driver.Minimize();
driver.Restore();
driver.Fullscreen();
driver.Close();

// Title
string title = driver.Title;
```

## Page Source

Get XML representation of the visual tree:

```csharp
string source = driver.PageSource;
// Returns XML like:
// <Window name="MainWindow">
//   <StackPanel name="Container">
//     <Button name="submit" text="Click Me" />
//   </StackPanel>
// </Window>
```

## DataContext Interaction

Execute actions on the ViewModel:

```csharp
// Execute action
driver.ExecuteScript<MyViewModel>(vm => vm.LoadData());

// Get value
var count = driver.ExecuteScript<MyViewModel, int>(vm => vm.ItemCount);
```

## Custom Predicates

Register custom element finders:

```csharp
driver.RegisterPredicate("largeButton", c =>
    c is Button btn && btn.Bounds.Width > 100);

var largeButtons = driver.FindElements(By.Predicate("Large button", "largeButton"));
```

## Complete Example

```csharp
[AvaloniaFact]
public void Login_WithValidCredentials_ShowsDashboard()
{
    var window = new MainWindow();
    window.Show();

    using var driver = new AvaloniaDriver(window);
    driver.Wait.Timeout = TimeSpan.FromSeconds(5);

    // Enter credentials
    driver.FindElement(By.AutomationId("username"))
        .SendKeys("testuser");

    driver.FindElement(By.AutomationId("password"))
        .SendKeys("password123");

    // Click login
    driver.FindElement(By.Id("loginButton")).Click();

    // Wait for dashboard
    var dashboard = driver.Wait.Until(
        ExpectedConditions.ElementIsVisible(By.Id("dashboard")));

    Assert.NotNull(dashboard);
    Assert.Contains("Welcome", dashboard.Text);

    // Take screenshot
    driver.SaveScreenshot("dashboard.png");
}
```

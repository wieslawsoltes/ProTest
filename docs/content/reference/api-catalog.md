---
title: "Public API catalog"
description: "Source-derived public entry points for all imported testing helper components."
sidebar_position: 5
doc-type: reference
---

This catalog lists the public types and callable entry points from the imported helpers and ProTest recording additions. It complements the task examples in the input, tree, automation, and recording reference sections. Types retain their original namespaces; source links include property declarations, enums, XML comments, and implementation details.

The declarations below are a compact signature index, not a compilable C# file. All entries are included in `ProTest.Avalonia.Headless`.

## Automation

### Actions

[Source](https://github.com/wieslawsoltes/ProTest/blob/main/src/Headless/Avalonia.Headless/Testing/Appium/Actions.cs)

<details>
<summary>Public signatures</summary>

```csharp
public class Actions
public Actions(AvaloniaDriver driver);
public Actions MoveToElement(AvaloniaElement element);
public Actions MoveToElement(AvaloniaElement element, int offsetX, int offsetY);
public Actions MoveByOffset(int offsetX, int offsetY);
public Actions MoveToLocation(int x, int y);
public Actions Click();
public Actions Click(AvaloniaElement element);
public Actions DoubleClick();
public Actions DoubleClick(AvaloniaElement element);
public Actions ContextClick();
public Actions ContextClick(AvaloniaElement element);
public Actions ClickAndHold(MouseButton button = MouseButton.Left);
public Actions Release(MouseButton button = MouseButton.Left);
public Actions DragAndDrop(AvaloniaElement source, AvaloniaElement target);
public Actions DragAndDropBy(AvaloniaElement source, int offsetX, int offsetY);
public Actions Scroll(int deltaX, int deltaY);
public Actions ScrollToElement(AvaloniaElement element);
public Actions KeyDown(Key key);
public Actions KeyDown(AvaloniaElement element, Key key);
public Actions KeyUp(Key key);
public Actions KeyUp(AvaloniaElement element, Key key);
public Actions SendKeys(string text);
public Actions SendKeys(AvaloniaElement element, string text);
public Actions Pause(int milliseconds);
public Actions Pause(TimeSpan duration);
public void Perform();
public async Task PerformAsync();
public Actions Reset();
public interface IAction
public class ActionSequence
public ActionSequence(ActionSourceType sourceType, string id);
public void AddAction(IAction action);
public void Clear();
public enum ActionSourceType
public enum PointerOrigin
public class PointerMoveAction : IAction
public class PointerDownAction : IAction
public class PointerUpAction : IAction
public class ScrollAction : IAction
public class KeyDownAction : IAction
public class KeyUpAction : IAction
public class SendKeysAction : IAction
public class PauseAction : IAction
```

</details>

### AvaloniaDriver

[Source](https://github.com/wieslawsoltes/ProTest/blob/main/src/Headless/Avalonia.Headless/Testing/Appium/AvaloniaDriver.cs)

<details>
<summary>Public signatures</summary>

```csharp
public class AvaloniaDriver : IDisposable
public AvaloniaDriver(Window window) : this((Control)window);
public AvaloniaDriver(Control root);
public static AvaloniaDriver FromApplication();
public AvaloniaElement FindElement(By by);
public IReadOnlyList<AvaloniaElement> FindElements(By by);
public AvaloniaElement? TryFindElement(By by);
public bool ElementExists(By by);
public int ElementCount(By by);
public void RegisterPredicate(string key, Func<Control, bool> predicate);
public void NavigateTo(Type viewType);
public void NavigateTo<T>() where T : Control, new();
public void Back();
public void Forward();
public void Refresh();
public RenderTargetBitmap? Screenshot();
public void SaveScreenshot(string path);
public string? ScreenshotAsBase64();
public void Maximize();
public void Minimize();
public void Restore();
public void Fullscreen();
public void SetWindowSize(double width, double height);
public void SetWindowPosition(int x, int y);
public Size GetWindowSize();
public PixelPoint GetWindowPosition();
public void Close();
public void Click(double x, double y);
public void Tap(double x, double y);
public void SendKeys(string keys);
public void PressKey(Key key, KeyModifiers modifiers = KeyModifiers.None);
public TouchAction CreateTouchAction();
public void ExecuteScript<TViewModel>(Action<TViewModel> action) where TViewModel : class;
public TResult? ExecuteScript<TViewModel, TResult>(Func<TViewModel, TResult> selector) where TViewModel : class;
public void Dispose();
public class DriverWait
public T Until<T>(Func<AvaloniaDriver, T> condition, TimeSpan? timeout = null);
public AvaloniaElement ForElement(By by, TimeSpan? timeout = null);
public AvaloniaElement ForElementVisible(By by, TimeSpan? timeout = null);
public AvaloniaElement ForElementClickable(By by, TimeSpan? timeout = null);
public void ForElementNotPresent(By by, TimeSpan? timeout = null);
public bool ForTextPresent(By by, string text, TimeSpan? timeout = null);
public void For(TimeSpan duration);
public void ForMilliseconds(int milliseconds);
```

</details>

### AvaloniaElement

[Source](https://github.com/wieslawsoltes/ProTest/blob/main/src/Headless/Avalonia.Headless/Testing/Appium/AvaloniaElement.cs)

<details>
<summary>Public signatures</summary>

```csharp
public class AvaloniaElement
public AvaloniaElement Click();
public AvaloniaElement DoubleClick();
public AvaloniaElement RightClick();
public AvaloniaElement Tap();
public AvaloniaElement DoubleTap();
public AvaloniaElement LongPress(int durationMs = 1000);
public AvaloniaElement SendKeys(string text);
public AvaloniaElement Clear();
public AvaloniaElement Submit();
public AvaloniaElement Focus();
public AvaloniaElement ScrollIntoView();
public AvaloniaElement Hover();
public AvaloniaElement DragTo(AvaloniaElement target);
public AvaloniaElement Swipe(SwipeDirection direction, double distance = 100);
public AvaloniaElement Pinch(double scale);
public AvaloniaElement Scroll(double deltaX, double deltaY);
public AvaloniaElement PressKey(Key key, KeyModifiers modifiers = KeyModifiers.None);
public string? GetAttribute(string name);
public T? GetProperty<T>(string propertyName);
public AvaloniaElement SetProperty(string propertyName, object? value);
public IReadOnlyList<string> GetClasses();
public bool HasClass(string className);
public AvaloniaElement FindElement(By by);
public IReadOnlyList<AvaloniaElement> FindElements(By by);
public AvaloniaElement? TryFindElement(By by);
public RenderTargetBitmap? Screenshot();
public void SaveScreenshot(string path);
public AvaloniaElement WaitUntil(Func<AvaloniaElement, bool> condition, TimeSpan? timeout = null, TimeSpan? pollInterval = null);
public AvaloniaElement WaitUntilVisible(TimeSpan? timeout = null);
public AvaloniaElement WaitUntilEnabled(TimeSpan? timeout = null);
public AvaloniaElement WaitUntilClickable(TimeSpan? timeout = null);
public override string ToString();
public class NoSuchElementException : Exception
public NoSuchElementException(string message, By? locator = null) : base(message);
public NoSuchElementException(string message, Exception innerException) : base(message, innerException);
public class StaleElementReferenceException : Exception
public StaleElementReferenceException(string message) : base(message);
public class ElementNotInteractableException : Exception
public ElementNotInteractableException(string message) : base(message);
```

</details>

### By

[Source](https://github.com/wieslawsoltes/ProTest/blob/main/src/Headless/Avalonia.Headless/Testing/Appium/By.cs)

<details>
<summary>Public signatures</summary>

```csharp
public class By
public static By Id(string id);
public static By Name(string name);
public static By AutomationId(string automationId);
public static By ClassName(string className);
public static By FullClassName(string fullClassName);
public static By Type<T>();
public static By Type(Type type);
public static By XPath(string xpath);
public static By Text(string text, bool exact = true);
public static By ContainsText(string text);
public static By CssSelector(string cssSelector);
public static By CssClass(string cssClass);
public static By TagName(string tagName);
public static By LinkText(string linkText);
public static By PartialLinkText(string partialLinkText);
public static By Property(string propertyName, object? value);
public static By AccessibilityName(string accessibilityName);
public static By AccessibilityLabel(string accessibilityLabel);
public static By Predicate(string description, string predicateKey);
public static By NameRegex(string pattern);
public static By Focused();
public static By All(params By[] locators);
public static By Any(params By[] locators);
public static By Chained(params By[] locators);
public override string ToString();
public enum LocatorStrategy
public class LocatorOptions
public enum CompositeMode
```

</details>

### DriverSession

[Source](https://github.com/wieslawsoltes/ProTest/blob/main/src/Headless/Avalonia.Headless/Testing/Appium/DriverSession.cs)

<details>
<summary>Public signatures</summary>

```csharp
public class DriverSession : IDisposable
public DriverSession(AvaloniaDriver driver, SessionCapabilities? capabilities = null);
public void SetData(string key, object value);
public T? GetData<T>(string key);
public T GetData<T>(string key, T defaultValue);
public bool RemoveData(string key);
public bool HasData(string key);
public void ClearData();
public void RegisterCleanup(Action cleanup);
public void RunCleanup();
public void Reset();
public Dictionary<string, object> GetSessionInfo();
public void Dispose();
public class SessionCapabilities
public class SessionLogger
public void Log(LogLevel level, string message, object? data = null);
public void Info(string message);
public void Debug(string message);
public void Warning(string message);
public void Error(string message, Exception? exception = null);
public IEnumerable<LogEntry> GetEntries(LogLevel minLevel = LogLevel.Debug);
public void Clear();
public class LogEntry
public override string ToString();
public enum LogLevel
```

</details>

### ElementAttributes

[Source](https://github.com/wieslawsoltes/ProTest/blob/main/src/Headless/Avalonia.Headless/Testing/Appium/ElementAttributes.cs)

<details>
<summary>Public signatures</summary>

```csharp
public static class ElementAttributes
public static string? GetAttribute(Control element, string attributeName);
public static Dictionary<string, string?> GetAttributes(Control element, params string[] attributeNames);
public static Dictionary<string, string?> GetAllAttributes(Control element);
public static string? GetCssValue(Control element, string propertyName);
public static bool HasAttribute(Control element, string attributeName);
public static bool HasClass(Control element, string className);
public static string GetAccessibleName(Control element);
public static string GetRole(Control element);
```

</details>

### ExpectedConditions

[Source](https://github.com/wieslawsoltes/ProTest/blob/main/src/Headless/Avalonia.Headless/Testing/Appium/ExpectedConditions.cs)

<details>
<summary>Public signatures</summary>

```csharp
public static class ExpectedConditions
public static Func<AvaloniaDriver, AvaloniaElement> ElementExists(By locator);
public static Func<AvaloniaDriver, AvaloniaElement> ElementIsVisible(By locator);
public static Func<AvaloniaDriver, AvaloniaElement> ElementToBeClickable(By locator);
public static Func<AvaloniaDriver, bool> InvisibilityOfElement(By locator);
public static Func<AvaloniaDriver, bool> InvisibilityOf(AvaloniaElement element);
public static Func<AvaloniaDriver, IReadOnlyList<AvaloniaElement>> VisibilityOfAllElements(By locator);
public static Func<AvaloniaDriver, IReadOnlyList<AvaloniaElement>> PresenceOfAllElements(By locator);
public static Func<AvaloniaDriver, bool> ElementToBeEnabled(By locator);
public static Func<AvaloniaDriver, bool> ElementToBeDisabled(By locator);
public static Func<AvaloniaDriver, bool> ElementToBeSelected(By locator);
public static Func<AvaloniaDriver, bool> ElementSelectionStateToBe(By locator, bool selected);
public static Func<AvaloniaDriver, bool> ElementToBeFocused(By locator);
public static Func<AvaloniaDriver, bool> TextToBe(By locator, string text);
public static Func<AvaloniaDriver, bool> TextToBePresentInElement(By locator, string text);
public static Func<AvaloniaDriver, bool> TextToMatch(By locator, string pattern);
public static Func<AvaloniaDriver, bool> TextToBeNotEmpty(By locator);
public static Func<AvaloniaDriver, bool> AttributeToBe(By locator, string attribute, string value);
public static Func<AvaloniaDriver, bool> AttributeContains(By locator, string attribute, string value);
public static Func<AvaloniaDriver, bool> AttributeToBeNotEmpty(By locator, string attribute);
public static Func<AvaloniaDriver, IReadOnlyList<AvaloniaElement>> NumberOfElementsToBe(By locator, int count);
public static Func<AvaloniaDriver, IReadOnlyList<AvaloniaElement>> NumberOfElementsToBeMoreThan(By locator, int count);
public static Func<AvaloniaDriver, IReadOnlyList<AvaloniaElement>> NumberOfElementsToBeLessThan(By locator, int count);
public static Func<AvaloniaDriver, bool> TitleIs(string title);
public static Func<AvaloniaDriver, bool> TitleContains(string text);
public static Func<AvaloniaDriver, bool> And(params Func<AvaloniaDriver, bool>[] conditions);
public static Func<AvaloniaDriver, bool> Or(params Func<AvaloniaDriver, bool>[] conditions);
public static Func<AvaloniaDriver, bool> Not(Func<AvaloniaDriver, bool> condition);
public static Func<AvaloniaDriver, bool> CustomCondition(Func<AvaloniaDriver, bool> condition, string description = "Custom condition");
public static Func<AvaloniaDriver, AvaloniaElement> CustomElementCondition(Func<AvaloniaDriver, AvaloniaElement?> condition);
public static Func<AvaloniaDriver, bool> StalenessOf(AvaloniaElement element);
public static Func<AvaloniaDriver, AvaloniaElement> RefreshedElement(By locator, AvaloniaElement oldElement);
```

</details>

### TouchAction

[Source](https://github.com/wieslawsoltes/ProTest/blob/main/src/Headless/Avalonia.Headless/Testing/Appium/TouchAction.cs)

<details>
<summary>Public signatures</summary>

```csharp
public class TouchAction
public TouchAction(AvaloniaDriver driver);
public TouchAction Press(AvaloniaElement element);
public TouchAction Press(AvaloniaElement element, double x, double y);
public TouchAction Press(double x, double y);
public TouchAction Press(Control control, Point position);
public TouchAction LongPress(AvaloniaElement element, int durationMs = 1000);
public TouchAction LongPress(double x, double y, int durationMs = 1000);
public TouchAction LongPress(Control control, Point position, int durationMs = 1000);
public TouchAction MoveTo(AvaloniaElement element);
public TouchAction MoveTo(AvaloniaElement element, double x, double y);
public TouchAction MoveTo(double x, double y);
public TouchAction MoveBy(double offsetX, double offsetY);
public TouchAction MoveTo(Control control, Point position);
public TouchAction Release();
public TouchAction Tap(AvaloniaElement element, int count = 1);
public TouchAction Tap(double x, double y, int count = 1);
public TouchAction Tap(Control control, Point position, int count = 1);
public TouchAction DoubleTap(AvaloniaElement element);
public TouchAction Wait(int milliseconds);
public TouchAction Wait(TimeSpan duration);
public TouchAction Swipe(SwipeDirection direction, double distance = 100, int durationMs = 300);
public TouchAction Swipe(double startX, double startY, double endX, double endY, int durationMs = 300);
public void Perform();
public async Task PerformAsync();
public void Cancel();
public static class MultiTouchAction
public static void Perform(params TouchAction[] actions);
public static void Pinch(AvaloniaDriver driver, AvaloniaElement element, double scale, int durationMs = 300);
public static void Scroll(AvaloniaDriver driver, AvaloniaElement element, double deltaX, double deltaY);
public static void Rotate(AvaloniaDriver driver, AvaloniaElement element, double degrees, int durationMs = 300);
```

</details>

### WaitHelper

[Source](https://github.com/wieslawsoltes/ProTest/blob/main/src/Headless/Avalonia.Headless/Testing/Appium/WaitHelper.cs)

<details>
<summary>Public signatures</summary>

```csharp
public class WaitHelper
public WaitHelper() : this(TimeSpan.FromSeconds(30));
public WaitHelper(TimeSpan timeout, TimeSpan? pollingInterval = null);
public async Task<bool> UntilAsync(Func<bool> condition, string? message = null, Action<TimeSpan>? onProgress = null);
public async Task<bool> UntilAsync(Func<Task<bool>> condition, string? message = null);
public async Task<bool> UntilOnUIThreadAsync(Func<bool> condition, string? message = null);
public async Task<T> UntilNotNullAsync<T>(Func<T?> func, string? message = null) where T : class;
public async Task<T> UntilNotNullAsync<T>(Func<Task<T?>> func, string? message = null) where T : class;
public async Task ForVisibleAsync(Control element, string? message = null);
public async Task ForHiddenAsync(Control element, string? message = null);
public async Task ForEnabledAsync(Control element, string? message = null);
public async Task ForDisabledAsync(Control element, string? message = null);
public async Task ForFocusedAsync(Control element, string? message = null);
public async Task ForPropertyValueAsync<T>(AvaloniaObject element, AvaloniaProperty<T> property, T expectedValue, string? message = null);
public async Task<Control> ForElementAsync(Func<Control?> findFunc, string? message = null);
public async Task ForTextAsync(Control element, string expectedText, string? message = null);
public async Task ForTextContainsAsync(Control element, string substring, string? message = null);
public async Task ForTextNotEmptyAsync(Control element, string? message = null);
public async Task ForCountAsync<T>(Func<IEnumerable<T>> getCollection, int expectedCount, string? message = null);
public async Task ForMinimumCountAsync<T>(Func<IEnumerable<T>> getCollection, int minimumCount, string? message = null);
public async Task<bool> TryUntilAsync(Func<bool> condition);
public async Task<Control?> TryForElementAsync(Func<Control?> findFunc);
public static WaitBuilder With(TimeSpan timeout);
public class WaitBuilder
public WaitBuilder WithTimeout(TimeSpan timeout);
public WaitBuilder WithPollingInterval(TimeSpan interval);
public WaitBuilder WithMessage(string message);
public WaitBuilder IgnoreException<T>() where T : Exception;
public async Task<bool> UntilAsync(Func<bool> condition);
public async Task<bool> UntilAsync(Func<Task<bool>> condition);
public async Task<T> UntilNotNullAsync<T>(Func<T?> func) where T : class;
```

</details>

## Gesture recognizers

### GestureRecognizerTestHelper

[Source](https://github.com/wieslawsoltes/ProTest/blob/main/src/Headless/Avalonia.Headless/Testing/Gestures/GestureRecognizerTestHelper.cs)

<details>
<summary>Public signatures</summary>

```csharp
public class GestureRecognizerTestHelper
public GestureRecognizerTestHelper();
public GestureRecognizerTestHelper(PointerType pointerType);
public void Down(Interactive target, Point position = default, KeyModifiers modifiers = default);
public void Down(Interactive target, Interactive source, Point position = default, KeyModifiers modifiers = default);
public void Move(Interactive target, Point position, KeyModifiers modifiers = default);
public void Move(Interactive target, Interactive source, Point position, KeyModifiers modifiers = default);
public void Up(Interactive target, Point position = default, KeyModifiers modifiers = default);
public void Up(Interactive target, Interactive source, Point position = default, KeyModifiers modifiers = default);
public void Tap(Interactive target, Point position = default, KeyModifiers modifiers = default);
public void Tap(Interactive target, Interactive source, Point position = default, KeyModifiers modifiers = default);
public void Cancel();
public void Drag(Interactive target, Point start, Point end, int steps = 10);
```

</details>

### GestureSimulator

[Source](https://github.com/wieslawsoltes/ProTest/blob/main/src/Headless/Avalonia.Headless/Testing/Gestures/GestureSimulator.cs)

<details>
<summary>Public signatures</summary>

```csharp
public class GestureSimulator
public GestureSimulator();
public void AdvanceTime(int milliseconds);
public void Reset();
public int GetNextGestureId();
public void Tap(Interactive target, Point position, PointerType pointerType = PointerType.Touch, KeyModifiers modifiers = KeyModifiers.None);
public void DoubleTap(Interactive target, Point position, PointerType pointerType = PointerType.Touch, KeyModifiers modifiers = KeyModifiers.None);
public void RightTap(Interactive target, Point position, PointerType pointerType = PointerType.Touch, KeyModifiers modifiers = KeyModifiers.None);
public void HoldingStarted(Interactive target, Point position, PointerType pointerType = PointerType.Touch);
public void HoldingCompleted(Interactive target, Point position, PointerType pointerType = PointerType.Touch);
public void HoldingCancelled(Interactive target, Point position, PointerType pointerType = PointerType.Touch);
public void Hold(Interactive target, Point position, int holdDuration = 500, PointerType pointerType = PointerType.Touch);
public void Pinch(Interactive target, double scale, Point scaleOrigin, double angleDelta = 0.0, double distance = 0.0);
public void PinchEnded(Interactive target);
public void PinchZoom(Interactive target, Point origin, double startScale, double endScale, int steps = 10, double startAngle = 0, double endAngle = 0);
public void PinchRotate(Interactive target, Point origin, double startAngle, double endAngle, int steps = 10);
public void Scroll(Interactive target, Vector delta, int? gestureId = null);
public void ScrollEnded(Interactive target, int gestureId);
public bool ScrollInertiaStarting(Interactive target, int gestureId, Vector inertia);
public void ScrollSequence(Interactive target, Vector totalDelta, int steps = 10, bool withInertia = false, Vector? inertiaVelocity = null);
public void Pull(Interactive target, Vector delta, PullDirection direction, int? gestureId = null);
public void PullEnded(Interactive target, int gestureId, PullDirection direction);
public void PullToRefresh(Interactive target, PullDirection direction, double distance = 100, int steps = 10);
public void TouchpadMagnify(Interactive target, Vector delta, Point position, KeyModifiers modifiers = KeyModifiers.None);
public void TouchpadRotate(Interactive target, double angleDelta, Point position, KeyModifiers modifiers = KeyModifiers.None);
public void TouchpadSwipe(Interactive target, Vector delta, Point position, KeyModifiers modifiers = KeyModifiers.None);
public void TouchpadMagnifySequence(Interactive target, Point position, double totalMagnification, int steps = 10, KeyModifiers modifiers = KeyModifiers.None);
public void TouchpadRotateSequence(Interactive target, Point position, double totalAngle, int steps = 10, KeyModifiers modifiers = KeyModifiers.None);
public void TouchpadSwipeSequence(Interactive target, Point position, SwipeDirection direction, double distance = 100, int steps = 10, KeyModifiers modifiers = KeyModifiers.None);
public void TapAndHold(Interactive target, Point position, int holdDuration = 500, PointerType pointerType = PointerType.Touch);
public void DoubleTapZoom(Interactive target, Point position, double zoomScale = 2.0, int steps = 10);
public void Flick(Interactive target, SwipeDirection direction, double distance = 50, double velocity = 500);
public void ThreeFingerSwipe(Interactive target, Point position, SwipeDirection direction, double distance = 100);
```

</details>

### MultiTouchTestHelperFactory

[Source](https://github.com/wieslawsoltes/ProTest/blob/main/src/Headless/Avalonia.Headless/Testing/Gestures/MultiTouchTestHelperFactory.cs)

<details>
<summary>Public signatures</summary>

```csharp
public static class MultiTouchTestHelperFactory
public static (GestureRecognizerTestHelper First, GestureRecognizerTestHelper Second) CreatePair();
public static GestureRecognizerTestHelper[] Create(int count);
public static void SimulatePinch( Interactive target, Point firstStart, Point secondStart, Point firstEnd, Point secondEnd, int steps = 10);
public static void SimulatePinchZoomIn( Interactive target, Point center, double startDistance, double endDistance, int steps = 10);
public static void SimulatePinchZoomOut( Interactive target, Point center, double startDistance, double endDistance, int steps = 10);
public static void SimulateTwoFingerPan( Interactive target, Point start, Point end, double fingerSpacing = 50, int steps = 10);
public static void SimulateRotation( Interactive target, Point center, double radius, double startAngleDegrees, double endAngleDegrees, int steps = 10);
```

</details>

## Input simulators

### KeyboardInputSimulator

[Source](https://github.com/wieslawsoltes/ProTest/blob/main/src/Headless/Avalonia.Headless/Testing/Input/KeyboardInputSimulator.cs)

<details>
<summary>Public signatures</summary>

```csharp
public class KeyboardInputSimulator
public KeyboardInputSimulator();
public void AdvanceTime(int milliseconds);
public void Reset();
public void KeyDown(Interactive target, Key key, KeyModifiers? modifiers = null);
public void KeyUp(Interactive target, Key key, KeyModifiers? modifiers = null);
public void KeyPress(Interactive target, Key key, int holdTime = 50, KeyModifiers? modifiers = null);
public void TypeText(Interactive target, string text, int keyInterval = 50);
public void CtrlDown(Interactive target);
public void CtrlUp(Interactive target);
public void ShiftDown(Interactive target);
public void ShiftUp(Interactive target);
public void AltDown(Interactive target);
public void AltUp(Interactive target);
public void MetaDown(Interactive target);
public void MetaUp(Interactive target);
public void CtrlKey(Interactive target, Key key);
public void CtrlShiftKey(Interactive target, Key key);
public void AltKey(Interactive target, Key key);
public void Copy(Interactive target);
public void Paste(Interactive target);
public void Cut(Interactive target);
public void Undo(Interactive target);
public void Redo(Interactive target);
public void SelectAll(Interactive target);
public void Save(Interactive target);
public void ArrowUp(Interactive target);
public void ArrowDown(Interactive target);
public void ArrowLeft(Interactive target);
public void ArrowRight(Interactive target);
public void Tab(Interactive target);
public void ShiftTab(Interactive target);
public void Enter(Interactive target);
public void Escape(Interactive target);
public void Space(Interactive target);
public void Backspace(Interactive target);
public void Delete(Interactive target);
public void Home(Interactive target);
public void End(Interactive target);
public void PageUp(Interactive target);
public void PageDown(Interactive target);
public void FunctionKey(Interactive target, int number);
```

</details>

### MouseInputSimulator

[Source](https://github.com/wieslawsoltes/ProTest/blob/main/src/Headless/Avalonia.Headless/Testing/Input/MouseInputSimulator.cs)

<details>
<summary>Public signatures</summary>

```csharp
public class MouseInputSimulator
public MouseInputSimulator();
public void AdvanceTime(int milliseconds);
public void Reset();
public void MoveTo(Interactive target, Point position);
public void MoveBy(Interactive target, double deltaX, double deltaY);
public void Enter(Interactive target, Point position);
public void Leave(Interactive target);
public void ButtonDown(Interactive target, MouseButton button = MouseButton.Left, Point? position = null);
public void ButtonUp(Interactive target, MouseButton button = MouseButton.Left, Point? position = null);
public void Click(Interactive target, Point position, MouseButton button = MouseButton.Left, int holdTime = 50);
public void DoubleClick(Interactive target, Point position, MouseButton button = MouseButton.Left, int clickInterval = 100);
public void RightClick(Interactive target, Point position, int holdTime = 50);
public void MiddleClick(Interactive target, Point position, int holdTime = 50);
public void Drag(Interactive target, Point startPoint, Point endPoint, MouseButton button = MouseButton.Left, int steps = 10);
public void DragPath(Interactive target, Point[] path, MouseButton button = MouseButton.Left, int stepInterval = 16);
public void Wheel(Interactive target, Vector delta, Point? position = null);
public void ScrollVertical(Interactive target, double lines, Point? position = null);
public void ScrollHorizontal(Interactive target, double lines, Point? position = null);
public void SmoothScroll(Interactive target, Vector totalDelta, int steps = 10, Point? position = null);
public void Hover(Interactive target, Point position, int duration = 500);
public void HoverPath(Interactive target, Point[] positions, int pauseDuration = 100);
```

</details>

### TouchInputSimulator

[Source](https://github.com/wieslawsoltes/ProTest/blob/main/src/Headless/Avalonia.Headless/Testing/Input/TouchInputSimulator.cs)

<details>
<summary>Public signatures</summary>

```csharp
public class TouchInputSimulator
public class TouchPoint
public TouchInputSimulator();
public void AdvanceTime(int milliseconds);
public void Reset();
public int TouchDown(Interactive target, Point position, float pressure = 1.0f);
public void TouchMove(Interactive target, int touchId, Point newPosition);
public void TouchUp(Interactive target, int touchId);
public void Tap(Interactive target, Point position, int holdTime = 50);
public void DoubleTap(Interactive target, Point position, int tapInterval = 100);
public void PinchGesture(Interactive target, double scale, Point scaleOrigin, double angleDelta = 0.0, double distance = 0.0);
public void PinchGestureEnded(Interactive target);
public void ScrollGesture(Interactive target, Vector delta, int gestureId = 1);
public void ScrollGestureEnded(Interactive target, int gestureId = 1);
public void TouchpadMagnify(Interactive target, Vector delta, Point position, KeyModifiers modifiers = KeyModifiers.None);
public void TouchpadSwipe(Interactive target, Vector delta, Point position, KeyModifiers modifiers = KeyModifiers.None);
public void TouchpadRotate(Interactive target, double delta, Point position, KeyModifiers modifiers = KeyModifiers.None);
public void SimulatePinchZoom(Interactive target, Point centerPoint, double startDistance, double endDistance, int steps = 10);
public void SimulateTwoFingerPan(Interactive target, Point startPoint, Point endPoint, double fingerSpacing = 50, int steps = 10);
public void SimulateRotation(Interactive target, Point centerPoint, double radius, double startAngle, double endAngle, int steps = 10);
public void SimulateDrag(Interactive target, Point startPoint, Point endPoint, int steps = 10);
public void Swipe(Interactive target, Point startPoint, SwipeDirection direction, double distance = 100, int duration = 200);
public enum SwipeDirection
```

</details>

## Recording and encoding

### CapturedFrame

[Source](https://github.com/wieslawsoltes/ProTest/blob/main/src/Headless/Avalonia.Headless/Testing/Recording/CapturedFrame.cs)

<details>
<summary>Public signatures</summary>

```csharp
public class CapturedFrame : IDisposable
public CapturedFrame( int frameNumber, TimeSpan timestamp, int width, int height, int stride, PixelFormat pixelFormat, byte[] pixelData, Vector dpi);
public byte[] GetPixelDataCopy();
public void Dispose();
public enum PixelFormat
```

</details>

### FrameBuffer

[Source](https://github.com/wieslawsoltes/ProTest/blob/main/src/Headless/Avalonia.Headless/Testing/Recording/FrameBuffer.cs)

<details>
<summary>Public signatures</summary>

```csharp
public class FrameBuffer : IDisposable
public FrameBuffer(int maxFrames = 1000);
public bool TryAdd(CapturedFrame frame);
public bool TryTake(out CapturedFrame? frame);
public IReadOnlyList<CapturedFrame> GetAllFrames();
public IReadOnlyList<CapturedFrame> TakeAll();
public void Clear();
public void Dispose();
public class FrameDroppedEventArgs : EventArgs
public FrameDroppedEventArgs(int frameNumber, FrameDropReason reason);
public enum FrameDropReason
```

</details>

### FrameEncoder

[Source](https://github.com/wieslawsoltes/ProTest/blob/main/src/Headless/Avalonia.Headless/Testing/Recording/FrameEncoder.cs)

<details>
<summary>Public signatures</summary>

```csharp
public interface IFrameEncoder : IDisposable
public class PngSequenceEncoder : IFrameEncoder
public void Initialize(string outputPath, int width, int height, RecordingOptions options);
public void EncodeFrame(CapturedFrame frame);
public async Task EncodeFrameAsync(CapturedFrame frame, CancellationToken cancellationToken = default);
public void FinalizeEncoding();
public Task FinalizeAsync(CancellationToken cancellationToken = default);
public void Dispose();
public class JpegSequenceEncoder : IFrameEncoder
public void Initialize(string outputPath, int width, int height, RecordingOptions options);
public void EncodeFrame(CapturedFrame frame);
public async Task EncodeFrameAsync(CapturedFrame frame, CancellationToken cancellationToken = default);
public void FinalizeEncoding();
public Task FinalizeAsync(CancellationToken cancellationToken = default);
public void Dispose();
public class RawFrameEncoder : IFrameEncoder
public void Initialize(string outputPath, int width, int height, RecordingOptions options);
public void EncodeFrame(CapturedFrame frame);
public async Task EncodeFrameAsync(CapturedFrame frame, CancellationToken cancellationToken = default);
public void FinalizeEncoding();
public Task FinalizeAsync(CancellationToken cancellationToken = default);
public void Dispose();
public static class FrameEncoderFactory
public static IFrameEncoder Create(RecordingFormat format);
public class GifEncoder : IFrameEncoder
public void Initialize(string outputPath, int width, int height, RecordingOptions options);
public void EncodeFrame(CapturedFrame frame);
public async Task EncodeFrameAsync(CapturedFrame frame, CancellationToken cancellationToken = default);
public void FinalizeEncoding();
public async Task FinalizeAsync(CancellationToken cancellationToken = default);
public void Dispose();
```

</details>

### HeadlessScreenRecorder

[Source](https://github.com/wieslawsoltes/ProTest/blob/main/src/Headless/Avalonia.Headless/Testing/Recording/HeadlessScreenRecorder.cs)

<details>
<summary>Public signatures</summary>

```csharp
public class HeadlessScreenRecorder : IDisposable
public HeadlessScreenRecorder();
public RecordingSession StartRecording(TopLevel target, RecordingOptions? options = null, string? outputPath = null);
public RecordingStatistics StopRecording();
public void PauseRecording();
public void ResumeRecording();
public bool CaptureFrame();
public void RecordInputEvent(string description, object? data = null);
public void RecordScrollEvent(string description, Vector delta);
public void RecordAnimationEvent(string description, object? data = null);
public void RecordGestureEvent(string description, object? data = null);
public void AddMarker(string name, object? data = null);
public void RecordEvent(RecordingEventType eventType, string description, object? data = null);
public async Task RecordActionAsync(Func<Task> action, int frameInterval = 16);
public async Task RecordDurationAsync(TimeSpan duration, int frameInterval = 16);
public void Dispose();
public class RecordingErrorEventArgs : EventArgs
public RecordingErrorEventArgs(Exception exception, string message);
public static class HeadlessExtensions
public static WriteableBitmap? CaptureRenderedFrame(this TopLevel topLevel);
public static WriteableBitmap? RenderToWriteableBitmap(Visual visual);
```

</details>

### RecordedTouchSimulator

[Source](https://github.com/wieslawsoltes/ProTest/blob/main/src/Headless/Avalonia.Headless/Testing/Recording/RecordedTouchSimulator.cs)

<details>
<summary>Public signatures</summary>

```csharp
public class RecordedTouchSimulator : IDisposable
public RecordedTouchSimulator();
public RecordingSession StartRecording(TopLevel target, RecordingOptions? options = null, string? outputPath = null);
public RecordingStatistics StopRecording();
public void CaptureFrame();
public void RecordedTap(Interactive target, Point position, int holdTime = 50);
public void RecordedDoubleTap(Interactive target, Point position, int tapInterval = 100);
public void RecordedDrag(Interactive target, Point startPoint, Point endPoint, int steps = 10);
public void RecordedPinchZoom(Interactive target, Point centerPoint, double startDistance, double endDistance, int steps = 10);
public void RecordedScroll(Interactive target, Vector delta, int gestureId = 1);
public void RecordedTwoFingerPan(Interactive target, Point startPoint, Point endPoint, double fingerSpacing = 50, int steps = 10);
public void RecordedRotation(Interactive target, Point centerPoint, double radius, double startAngle, double endAngle, int steps = 10);
public void RecordedSwipe(Interactive target, Point startPoint, SwipeDirection direction, double distance = 100, int duration = 200);
public void RecordedTouchpadMagnify(Interactive target, Vector delta, Point position, KeyModifiers modifiers = KeyModifiers.None);
public async Task RecordActionAsync(Func<Task> action, int frameInterval = 16);
public async Task RecordAnimationAsync(TimeSpan duration, int frameInterval = 16);
public void AddMarker(string name, object? data = null);
public VideoConversionResult ConvertToVideo(string? outputPath = null, VideoConversionOptions? options = null);
public async Task<VideoConversionResult> ConvertToVideoAsync( string? outputPath = null, VideoConversionOptions? options = null, CancellationToken cancellationToken = default);
public (RecordingStatistics Stats, VideoConversionResult VideoResult) StopRecordingAndConvertToVideo( VideoConversionOptions? videoOptions = null);
public void RecordedKeyPress(Interactive target, Key key, int holdTime = 50, KeyModifiers? modifiers = null);
public void RecordedTypeText(Interactive target, string text, int keyInterval = 50);
public void RecordedKeyboardShortcut(Interactive target, Key key, KeyModifiers modifiers);
public void RecordedArrowNavigation(Interactive target, Key[] keys, int keyInterval = 100);
public void RecordedTabNavigation(Interactive target, int tabCount, bool reverse = false, int tabInterval = 100);
public void RecordedMouseClick(Interactive target, Point position, MouseButton button = MouseButton.Left, int holdTime = 50);
public void RecordedMouseDoubleClick(Interactive target, Point position, MouseButton button = MouseButton.Left, int clickInterval = 100);
public void RecordedMouseRightClick(Interactive target, Point position, int holdTime = 50);
public void RecordedMouseDrag(Interactive target, Point startPoint, Point endPoint, MouseButton button = MouseButton.Left, int steps = 10);
public void RecordedMouseWheel(Interactive target, Vector delta, Point? position = null, int steps = 5);
public void RecordedMouseHover(Interactive target, Point position, int duration = 500);
public void RecordedMouseMovePath(Interactive target, Point[] path, int stepInterval = 50);
public void Dispose();
```

</details>

### RecordingOptions

[Source](https://github.com/wieslawsoltes/ProTest/blob/main/src/Headless/Avalonia.Headless/Testing/Recording/RecordingOptions.cs)

<details>
<summary>Public signatures</summary>

```csharp
public class RecordingOptions
public void Validate();
public enum RecordingFormat
```

</details>

### RecordingSession

[Source](https://github.com/wieslawsoltes/ProTest/blob/main/src/Headless/Avalonia.Headless/Testing/Recording/RecordingSession.cs)

<details>
<summary>Public signatures</summary>

```csharp
public class RecordingSession : IDisposable
public RecordingSession(RecordingOptions options);
public void Initialize(int width, int height, string outputPath);
public void Start();
public void Pause();
public void Resume();
public void Stop();
public void RecordFrame(CapturedFrame frame);
public void RecordEvent(RecordingEventType eventType, string description, object? data = null);
public void FinalizeRecording();
public RecordingStatistics GetStatistics();
public void Dispose();
public enum RecordingState
public class RecordingStateChangedEventArgs : EventArgs
public RecordingStateChangedEventArgs(RecordingState oldState, RecordingState newState);
public class FrameCapturedEventArgs : EventArgs
public FrameCapturedEventArgs(int frameNumber, TimeSpan timestamp);
public class RecordingEvent
public enum RecordingEventType
public class RecordingStatistics
```

</details>

### RenderFrameCapture

[Source](https://github.com/wieslawsoltes/ProTest/blob/main/src/Headless/Avalonia.Headless/Testing/Recording/RenderFrameCapture.cs)

<details>
<summary>Public signatures</summary>

```csharp
public class RenderFrameCapture : IDisposable
public void StartCapture(TopLevel target);
public void StopCapture();
public CapturedFrame[] GetCapturedFrames();
public bool TryGetFrame(out CapturedFrame? frame);
public CapturedFrame? CaptureNow();
public void Dispose();
```

</details>

### VideoConverter

[Source](https://github.com/wieslawsoltes/ProTest/blob/main/src/Headless/Avalonia.Headless/Testing/Recording/VideoConverter.cs)

<details>
<summary>Public signatures</summary>

```csharp
public enum VideoFormat
public class VideoConversionOptions
public class VideoConversionResult
public class VideoConverter
public VideoConverter(string? ffmpegPath = null);
public bool IsFfmpegAvailable();
public string? GetFfmpegVersion();
public async Task<VideoConversionResult> ConvertPngSequenceToVideoAsync( string inputDirectory, string? outputPath = null, VideoConversionOptions? options = null, CancellationToken cancellationToken = default);
public VideoConversionResult ConvertPngSequenceToVideo( string inputDirectory, string? outputPath = null, VideoConversionOptions? options = null);
public async Task<VideoConversionResult> ConvertSessionToVideoAsync( RecordingSession session, string? outputPath = null, VideoConversionOptions? options = null, CancellationToken cancellationToken = default);
public async Task<Dictionary<string, VideoConversionResult>> ConvertMultipleAsync( IEnumerable<string> directories, VideoConversionOptions? options = null, CancellationToken cancellationToken = default);
public static class RecordingSessionVideoExtensions
public static async Task<VideoConversionResult> ToVideoAsync( this RecordingSession session, string? outputPath = null, VideoConversionOptions? options = null, CancellationToken cancellationToken = default);
public static VideoConversionResult ToVideo( this RecordingSession session, string? outputPath = null, VideoConversionOptions? options = null);
```

</details>

## Control trees

### ControlFinder

[Source](https://github.com/wieslawsoltes/ProTest/blob/main/src/Headless/Avalonia.Headless/Testing/Trees/ControlFinder.cs)

<details>
<summary>Public signatures</summary>

```csharp
public class ControlFinder
public ControlFinder(Visual root);
public static ControlFinder From(Visual root);
public ControlFinder InVisualTree();
public ControlFinder InLogicalTree();
public ControlFinder IncludeSelf();
public ControlFinder MaxDepth(int depth);
public ControlFinder OfType<T>() where T : class;
public ControlFinder ExactType<T>() where T : class;
public ControlFinder AssignableFrom(Type type);
public ControlFinder WithName(string name);
public ControlFinder WithNameStartingWith(string prefix);
public ControlFinder WithNameEndingWith(string suffix);
public ControlFinder WithNameContaining(string substring);
public ControlFinder WithNameMatching(string pattern);
public ControlFinder WithProperty<TProperty>(AvaloniaProperty<TProperty> property, TProperty value);
public ControlFinder WithProperty<TProperty>(AvaloniaProperty<TProperty> property, Func<TProperty, bool> predicate);
public ControlFinder WithTag(object tag);
public ControlFinder WithDataContext<T>() where T : class;
public ControlFinder WithDataContext(object dataContext);
public ControlFinder WithDataContextProperty<TContext, TProperty>( Expression<Func<TContext, TProperty>> selector, TProperty value) where TContext : class;
public ControlFinder WithClass(string className);
public ControlFinder WithClasses(params string[] classNames);
public ControlFinder WithAnyClass(params string[] classNames);
public ControlFinder WithoutClass(string className);
public ControlFinder WithPseudoClass(string pseudoClass);
public ControlFinder Enabled();
public ControlFinder Disabled();
public ControlFinder Visible();
public ControlFinder Hidden();
public ControlFinder Focusable();
public ControlFinder Focused();
public ControlFinder HitTestable();
public ControlFinder EffectivelyVisible();
public ControlFinder EffectivelyEnabled();
public ControlFinder WithMinWidth(double minWidth);
public ControlFinder WithMinHeight(double minHeight);
public ControlFinder ContainingPoint(Point point);
public ControlFinder IntersectingRect(Rect rect);
public ControlFinder WithText(string text);
public ControlFinder WithTextContaining(string substring);
public ControlFinder WithContent<T>() where T : class;
public ControlFinder WithItemCount(int count);
public ControlFinder WithMinItemCount(int minCount);
public ControlFinder Where(Func<object, bool> predicate);
public ControlFinder Where<T>(Func<T, bool> predicate) where T : class;
public ControlFinder Except(Func<object, bool> predicate);
public ControlFinder Except<T>() where T : class;
public ControlFinder Skip(int count);
public ControlFinder Take(int count);
public IEnumerable<object> FindAll();
public IEnumerable<T> FindAll<T>() where T : class;
public object? FindFirst();
public T? FindFirst<T>() where T : class;
public object GetFirst();
public T GetFirst<T>() where T : class;
public object Single();
public T Single<T>() where T : class;
public int Count();
public bool Any();
public bool None();
public bool All();
public static class ControlFinderExtensions
public static ControlFinder Find(this Visual root);
public static ControlFinder Find<T>(this Visual root) where T : class;
```

</details>

### LogicalTreeTestHelper

[Source](https://github.com/wieslawsoltes/ProTest/blob/main/src/Headless/Avalonia.Headless/Testing/Trees/LogicalTreeTestHelper.cs)

<details>
<summary>Public signatures</summary>

```csharp
public static class LogicalTreeTestHelper
public static T? FindFirstLogical<T>(this ILogical root, bool includeSelf = false) where T : class;
public static IEnumerable<T> FindAllLogical<T>(this ILogical root, bool includeSelf = false) where T : class;
public static IEnumerable<T> FindAllLogical<T>(this ILogical root, Func<T, bool> predicate, bool includeSelf = false) where T : class;
public static T? FindFirstLogical<T>(this ILogical root, Func<T, bool> predicate, bool includeSelf = false) where T : class;
public static Control? FindLogicalByName(this ILogical root, string name);
public static T? FindLogicalByName<T>(this ILogical root, string name) where T : Control;
public static T GetLogicalByName<T>(this ILogical root, string name) where T : Control;
public static IEnumerable<T> FindLogicalByProperty<T, TProperty>( this ILogical root, AvaloniaProperty<TProperty> property, TProperty value) where T : AvaloniaObject;
public static IEnumerable<Control> FindLogicalByTag(this ILogical root, object tag);
public static IEnumerable<T> FindLogicalByClass<T>(this ILogical root, string className) where T : StyledElement;
public static IEnumerable<T> FindLogicalEnabled<T>(this ILogical root) where T : InputElement;
public static IEnumerable<T> FindLogicalDisabled<T>(this ILogical root) where T : InputElement;
public static IEnumerable<T> FindLogicalVisible<T>(this ILogical root) where T : Visual;
public static T? FindLogicalAncestor<T>(this ILogical logical, bool includeSelf = false) where T : class;
public static T? FindLogicalAncestor<T>(this ILogical logical, Func<T, bool> predicate, bool includeSelf = false) where T : class;
public static IEnumerable<T> GetLogicalAncestors<T>(this ILogical logical) where T : class;
public static bool HasLogicalAncestor<T>(this ILogical logical) where T : class;
public static IEnumerable<T> GetLogicalChildren<T>(this ILogical logical) where T : class;
public static int GetLogicalChildCount(this ILogical logical);
public static int GetLogicalDescendantCount(this ILogical logical);
public static ILogical GetLogicalChildAt(this ILogical logical, int index);
public static T GetLogicalChildAt<T>(this ILogical logical, int index) where T : ILogical;
public static T? GetFirstLogicalChild<T>(this ILogical logical) where T : class;
public static T? GetLastLogicalChild<T>(this ILogical logical) where T : class;
public static IEnumerable<T> GetLogicalSiblings<T>(this ILogical logical) where T : class;
public static T? GetNextLogicalSibling<T>(this ILogical logical) where T : class;
public static T? GetPreviousLogicalSibling<T>(this ILogical logical) where T : class;
public static int GetLogicalSiblingIndex(this ILogical logical);
public static bool IsFirstLogicalChild(this ILogical logical);
public static bool IsLastLogicalChild(this ILogical logical);
public static bool IsOnlyLogicalChild(this ILogical logical);
public static IReadOnlyList<ILogical> GetLogicalPathFromRoot(this ILogical logical);
public static string GetLogicalPathString(this ILogical logical);
public static string GetDetailedLogicalPathString(this ILogical logical);
public static bool ContainsLogical<T>(this ILogical root) where T : class;
public static bool ContainsLogicalNamed(this ILogical root, string name);
public static int CountLogical<T>(this ILogical root) where T : class;
public static bool MatchesLogicalStructure(this ILogical logical, params Type[] expectedPath);
public static T? GetContent<T>(this ContentControl contentControl) where T : class;
public static IEnumerable<T> GetItems<T>(this ItemsControl itemsControl) where T : class;
public static T? GetItemAt<T>(this ItemsControl itemsControl, int index) where T : class;
public static int GetItemCount(this ItemsControl itemsControl);
public static string PrintLogicalTree(this ILogical root, int maxDepth = int.MaxValue);
public static Dictionary<string, int> GetLogicalTypeSummary(this ILogical root);
public static Dictionary<string, Control> GetLogicalNamedControls(this ILogical root);
public static (int LogicalDepth, int VisualDepth) CompareTreeDepths(this Control control);
```

</details>

### TemplateComparer

[Source](https://github.com/wieslawsoltes/ProTest/blob/main/src/Headless/Avalonia.Headless/Testing/Trees/TemplateComparer.cs)

<details>
<summary>Public signatures</summary>

```csharp
public class TemplateComparisonResult
public override string ToString();
public class TemplateInfo
public class TemplatePartInfo
public class TemplateDifference
public override string ToString();
public enum TemplateDifferenceType
public class TemplateComparisonOptions
public static class TemplateComparer
public static TemplateComparisonResult CompareTemplates( TemplatedControl expected, TemplatedControl actual, TemplateComparisonOptions? options = null);
public static TemplateInfo GetTemplateInfo(TemplatedControl control);
public static List<TemplatePartInfo> GetTemplateParts(TemplatedControl control);
public static List<string> GetMissingRequiredParts(TemplatedControl control);
public static bool HasTemplateParts(TemplatedControl control, params string[] partNames);
public static T? GetTemplatePart<T>(TemplatedControl control, string partName) where T : class;
public static bool IsTemplatePartOfType<T>(TemplatedControl control, string partName) where T : class;
public static TemplateSnapshot CreateSnapshot(TemplatedControl control);
public static TemplateComparisonResult CompareToSnapshot( TemplatedControl control, TemplateSnapshot snapshot, TemplateComparisonOptions? options = null);
public static TemplateComparisonResult CompareTemplateTo( this TemplatedControl expected, TemplatedControl actual, TemplateComparisonOptions? options = null);
public static TemplateSnapshot SnapshotTemplate(this TemplatedControl control);
public static bool ValidateTemplateParts(this TemplatedControl control);
public class TemplateSnapshot
public class TemplateSnapshotNode
```

</details>

### TreeComparer

[Source](https://github.com/wieslawsoltes/ProTest/blob/main/src/Headless/Avalonia.Headless/Testing/Trees/TreeComparer.cs)

<details>
<summary>Public signatures</summary>

```csharp
public class TreeComparisonResult
public override string ToString();
public class TreeDifference
public override string ToString();
public enum TreeDifferenceType
public class TreeComparisonOptions
public static class TreeComparer
public static TreeComparisonResult CompareVisualTrees( Visual expected, Visual actual, TreeComparisonOptions? options = null);
public static TreeComparisonResult CompareLogicalTrees( ILogical expected, ILogical actual, TreeComparisonOptions? options = null);
public static TreeSnapshot CreateVisualSnapshot(Visual root, TreeComparisonOptions? options = null);
public static TreeSnapshot CreateLogicalSnapshot(ILogical root, TreeComparisonOptions? options = null);
public static TreeComparisonResult CompareToSnapshot( Visual actual, TreeSnapshot snapshot, TreeComparisonOptions? options = null);
public static TreeComparisonResult CompareVisualTreeTo( this Visual expected, Visual actual, TreeComparisonOptions? options = null);
public static TreeComparisonResult CompareLogicalTreeTo( this ILogical expected, ILogical actual, TreeComparisonOptions? options = null);
public static TreeSnapshot SnapshotVisualTree( this Visual root, TreeComparisonOptions? options = null);
public static TreeSnapshot SnapshotLogicalTree( this ILogical root, TreeComparisonOptions? options = null);
public static TreeComparisonResult VerifyAgainstSnapshot( this Visual actual, TreeSnapshot snapshot, TreeComparisonOptions? options = null);
public class TreeSnapshot
public enum TreeSnapshotType
public class TreeSnapshotNode
```

</details>

### TreeValidator

[Source](https://github.com/wieslawsoltes/ProTest/blob/main/src/Headless/Avalonia.Headless/Testing/Trees/TreeValidator.cs)

<details>
<summary>Public signatures</summary>

```csharp
public class TreeValidator
public static TreeValidator Create();
public TreeValidator RequireName(string name);
public TreeValidator RequireNames(params string[] names);
public TreeValidator RequireType<T>() where T : class;
public TreeValidator RequireExactCount<T>(int count) where T : class;
public TreeValidator RequireMinCount<T>(int minCount) where T : class;
public TreeValidator RequireMaxCount<T>(int maxCount) where T : class;
public TreeValidator RequirePattern(string pattern);
public TreeValidator RequireNameOfType<T>(string name) where T : class;
public TreeValidator RequireEnabled(string name);
public TreeValidator RequireVisible(string name);
public TreeValidator RequireProperty<TValue>(string name, AvaloniaProperty<TValue> property, TValue expectedValue);
public TreeValidator RequireChildOf(string childName, string parentName);
public TreeValidator RequireClass(string name, string className);
public TreeValidator ForbidPattern(string pattern);
public TreeValidator ForbidType<T>() where T : class;
public TreeValidator Custom(string description, Func<Visual, bool> validator);
public TreeValidator ValidateVisualTree(Visual root);
public TreeValidator ValidateLogicalTree(ILogical root);
public void AssertValid();
public string GetSummary();
public class TreeValidationResult
public class TreeValidationException : Exception
public TreeValidationException(string message, IEnumerable<TreeValidationResult> failures) : base(message);
public static class TreeValidatorExtensions
public static TreeValidator Validate(this Visual root);
public static bool HasControl(this Visual root, string name);
public static bool HasControlOfType<T>(this Visual root) where T : class;
public static bool IsControlEnabled(this Visual root, string name);
public static bool IsControlVisible(this Visual root, string name);
```

</details>

### TreeXPath

[Source](https://github.com/wieslawsoltes/ProTest/blob/main/src/Headless/Avalonia.Headless/Testing/Trees/TreeXPath.cs)

<details>
<summary>Public signatures</summary>

```csharp
public class TreeXPath
public TreeXPath(Visual root);
public TreeXPath(ILogical root, bool useVisualTree = false);
public IEnumerable<object> Select(string xpath);
public IEnumerable<T> Select<T>(string xpath) where T : class;
public object? SelectFirst(string xpath);
public T? SelectFirst<T>(string xpath) where T : class;
public bool Exists(string xpath);
public int Count(string xpath);
public static class TreeXPathExtensions
public static TreeXPath XPath(this Visual root);
public static TreeXPath XPath(this ILogical root, bool useVisualTree = false);
public static IEnumerable<object> SelectXPath(this Visual root, string xpath);
public static IEnumerable<T> SelectXPath<T>(this Visual root, string xpath) where T : class;
public static T? SelectFirstXPath<T>(this Visual root, string xpath) where T : class;
public static bool ExistsXPath(this Visual root, string xpath);
public static int CountXPath(this Visual root, string xpath);
```

</details>

### VisualTreeTestHelper

[Source](https://github.com/wieslawsoltes/ProTest/blob/main/src/Headless/Avalonia.Headless/Testing/Trees/VisualTreeTestHelper.cs)

<details>
<summary>Public signatures</summary>

```csharp
public static class VisualTreeTestHelper
public static T? FindFirst<T>(this Visual root, bool includeSelf = false) where T : class;
public static IEnumerable<T> FindAll<T>(this Visual root, bool includeSelf = false) where T : class;
public static IEnumerable<T> FindAll<T>(this Visual root, Func<T, bool> predicate, bool includeSelf = false) where T : class;
public static T? FindFirst<T>(this Visual root, Func<T, bool> predicate, bool includeSelf = false) where T : class;
public static Control? FindByName(this Visual root, string name);
public static T? FindByName<T>(this Visual root, string name) where T : Control;
public static T GetByName<T>(this Visual root, string name) where T : Control;
public static IEnumerable<T> FindByProperty<T, TProperty>( this Visual root, AvaloniaProperty<TProperty> property, TProperty value) where T : AvaloniaObject;
public static T? FindFirstByProperty<T, TProperty>( this Visual root, AvaloniaProperty<TProperty> property, TProperty value) where T : AvaloniaObject;
public static IEnumerable<Control> FindByTag(this Visual root, object tag);
public static IEnumerable<T> FindByClass<T>(this Visual root, string className) where T : StyledElement;
public static IEnumerable<T> FindByClasses<T>(this Visual root, params string[] classNames) where T : StyledElement;
public static IEnumerable<T> FindEnabled<T>(this Visual root) where T : InputElement;
public static IEnumerable<T> FindDisabled<T>(this Visual root) where T : InputElement;
public static IEnumerable<T> FindVisible<T>(this Visual root) where T : Visual;
public static IEnumerable<T> FindHidden<T>(this Visual root) where T : Visual;
public static IEnumerable<T> FindFocused<T>(this Visual root) where T : InputElement;
public static IEnumerable<T> FindFocusable<T>(this Visual root) where T : InputElement;
public static IEnumerable<T> FindByPseudoClass<T>(this Visual root, string pseudoClass) where T : StyledElement;
public static T? FindAncestor<T>(this Visual visual, bool includeSelf = false) where T : class;
public static T? FindAncestor<T>(this Visual visual, Func<T, bool> predicate, bool includeSelf = false) where T : class;
public static IEnumerable<T> GetAncestors<T>(this Visual visual) where T : class;
public static bool HasAncestor<T>(this Visual visual) where T : class;
public static IEnumerable<T> GetChildren<T>(this Visual visual) where T : class;
public static int GetChildCount(this Visual visual);
public static int GetDescendantCount(this Visual visual);
public static Visual GetChildAt(this Visual visual, int index);
public static T GetChildAt<T>(this Visual visual, int index) where T : Visual;
public static T? GetFirstChild<T>(this Visual visual) where T : class;
public static T? GetLastChild<T>(this Visual visual) where T : class;
public static IReadOnlyList<Visual> GetPathFromRoot(this Visual visual);
public static string GetPathString(this Visual visual);
public static string GetDetailedPathString(this Visual visual);
public static bool Contains<T>(this Visual root) where T : class;
public static bool ContainsNamed(this Visual root, string name);
public static int Count<T>(this Visual root) where T : class;
public static bool MatchesStructure(this Visual visual, params Type[] expectedPath);
public static Visual? HitTest(this Visual root, Point point);
public static Control? HitTestControl(this Visual root, Point point);
public static IEnumerable<Visual> HitTestAll(this Visual root, Point point);
public static IEnumerable<Control> HitTestAllControls(this Visual root, Point point);
public static Rect? GetBoundsRelativeTo(this Visual visual, Visual relativeTo);
public static Point GetCenter(this Visual visual);
public static Point? GetCenterRelativeTo(this Visual visual, Visual relativeTo);
public static bool IsVisibleWithin(this Visual visual, Visual ancestor);
public static string PrintTree(this Visual root, int maxDepth = int.MaxValue);
public static Dictionary<string, int> GetTypeSummary(this Visual root);
public static Dictionary<string, Control> GetNamedControls(this Visual root);
```

</details>

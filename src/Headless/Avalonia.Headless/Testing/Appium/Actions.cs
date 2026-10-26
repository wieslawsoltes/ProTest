// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;

namespace Avalonia.HeadlessTestingFramework.Appium;

/// <summary>
/// Provides Selenium 4-style Actions API for complex interaction sequences.
/// Supports mouse, keyboard, touch, and pen input with parallel execution.
/// </summary>
/// <example>
/// <code>
/// new Actions(driver)
///     .MoveToElement(element)
///     .Click()
///     .KeyDown(Key.LeftShift)
///     .SendKeys("HELLO")
///     .KeyUp(Key.LeftShift)
///     .Perform();
/// </code>
/// </example>
public class Actions
{
    private readonly AvaloniaDriver _driver;
    private readonly List<ActionSequence> _sequences = new();
    private ActionSequence _currentPointerSequence;
    private ActionSequence _currentKeySequence;
    private AvaloniaElement? _currentElement;
    private Point _currentPosition;

    /// <summary>
    /// Creates a new Actions builder.
    /// </summary>
    /// <param name="driver">The driver instance.</param>
    public Actions(AvaloniaDriver driver)
    {
        _driver = driver ?? throw new ArgumentNullException(nameof(driver));
        _currentPointerSequence = new ActionSequence(ActionSourceType.Pointer, "mouse");
        _currentKeySequence = new ActionSequence(ActionSourceType.Key, "keyboard");
        _sequences.Add(_currentPointerSequence);
        _sequences.Add(_currentKeySequence);
    }

    #region Mouse Actions

    /// <summary>
    /// Moves the mouse to the center of the element.
    /// </summary>
    /// <param name="element">The target element.</param>
    /// <returns>This Actions instance for chaining.</returns>
    public Actions MoveToElement(AvaloniaElement element)
    {
        _currentElement = element;
        _currentPosition = element.AbsoluteCenter;
        _currentPointerSequence.AddAction(new PointerMoveAction
        {
            Origin = PointerOrigin.Element,
            Element = element,
            X = 0,
            Y = 0
        });
        return this;
    }

    /// <summary>
    /// Moves the mouse to an offset from the element's center.
    /// </summary>
    /// <param name="element">The target element.</param>
    /// <param name="offsetX">X offset from center.</param>
    /// <param name="offsetY">Y offset from center.</param>
    /// <returns>This Actions instance for chaining.</returns>
    public Actions MoveToElement(AvaloniaElement element, int offsetX, int offsetY)
    {
        _currentElement = element;
        _currentPosition = new Point(element.AbsoluteCenter.X + offsetX, element.AbsoluteCenter.Y + offsetY);
        _currentPointerSequence.AddAction(new PointerMoveAction
        {
            Origin = PointerOrigin.Element,
            Element = element,
            X = offsetX,
            Y = offsetY
        });
        return this;
    }

    /// <summary>
    /// Moves the mouse by an offset from its current position.
    /// </summary>
    /// <param name="offsetX">X offset.</param>
    /// <param name="offsetY">Y offset.</param>
    /// <returns>This Actions instance for chaining.</returns>
    public Actions MoveByOffset(int offsetX, int offsetY)
    {
        _currentPosition = new Point(_currentPosition.X + offsetX, _currentPosition.Y + offsetY);
        _currentPointerSequence.AddAction(new PointerMoveAction
        {
            Origin = PointerOrigin.Pointer,
            X = offsetX,
            Y = offsetY
        });
        return this;
    }

    /// <summary>
    /// Moves the mouse to absolute coordinates.
    /// </summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <returns>This Actions instance for chaining.</returns>
    public Actions MoveToLocation(int x, int y)
    {
        _currentPosition = new Point(x, y);
        _currentPointerSequence.AddAction(new PointerMoveAction
        {
            Origin = PointerOrigin.Viewport,
            X = x,
            Y = y
        });
        return this;
    }

    /// <summary>
    /// Clicks at the current mouse position.
    /// </summary>
    /// <returns>This Actions instance for chaining.</returns>
    public Actions Click()
    {
        _currentPointerSequence.AddAction(new PointerDownAction { Button = MouseButton.Left });
        _currentPointerSequence.AddAction(new PointerUpAction { Button = MouseButton.Left });
        return this;
    }

    /// <summary>
    /// Clicks on an element.
    /// </summary>
    /// <param name="element">The element to click.</param>
    /// <returns>This Actions instance for chaining.</returns>
    public Actions Click(AvaloniaElement element)
    {
        return MoveToElement(element).Click();
    }

    /// <summary>
    /// Double-clicks at the current mouse position.
    /// </summary>
    /// <returns>This Actions instance for chaining.</returns>
    public Actions DoubleClick()
    {
        // Add a proper double-click sequence: down, up, pause, down, up
        // The pause between clicks should be short (typical double-click timing)
        _currentPointerSequence.AddAction(new PointerDownAction { Button = MouseButton.Left, ClickCount = 1 });
        _currentPointerSequence.AddAction(new PointerUpAction { Button = MouseButton.Left });
        _currentPointerSequence.AddAction(new PauseAction { Duration = TimeSpan.FromMilliseconds(50) });
        _currentPointerSequence.AddAction(new PointerDownAction { Button = MouseButton.Left, ClickCount = 2 });
        _currentPointerSequence.AddAction(new PointerUpAction { Button = MouseButton.Left });
        return this;
    }

    /// <summary>
    /// Double-clicks on an element.
    /// </summary>
    /// <param name="element">The element to double-click.</param>
    /// <returns>This Actions instance for chaining.</returns>
    public Actions DoubleClick(AvaloniaElement element)
    {
        return MoveToElement(element).DoubleClick();
    }

    /// <summary>
    /// Right-clicks at the current mouse position.
    /// </summary>
    /// <returns>This Actions instance for chaining.</returns>
    public Actions ContextClick()
    {
        _currentPointerSequence.AddAction(new PointerDownAction { Button = MouseButton.Right });
        _currentPointerSequence.AddAction(new PointerUpAction { Button = MouseButton.Right });
        return this;
    }

    /// <summary>
    /// Right-clicks on an element.
    /// </summary>
    /// <param name="element">The element to right-click.</param>
    /// <returns>This Actions instance for chaining.</returns>
    public Actions ContextClick(AvaloniaElement element)
    {
        return MoveToElement(element).ContextClick();
    }

    /// <summary>
    /// Presses the mouse button without releasing.
    /// </summary>
    /// <param name="button">The button to press.</param>
    /// <returns>This Actions instance for chaining.</returns>
    public Actions ClickAndHold(MouseButton button = MouseButton.Left)
    {
        _currentPointerSequence.AddAction(new PointerDownAction { Button = button });
        return this;
    }

    /// <summary>
    /// Releases the mouse button.
    /// </summary>
    /// <param name="button">The button to release.</param>
    /// <returns>This Actions instance for chaining.</returns>
    public Actions Release(MouseButton button = MouseButton.Left)
    {
        _currentPointerSequence.AddAction(new PointerUpAction { Button = button });
        return this;
    }

    /// <summary>
    /// Drags an element to another element.
    /// </summary>
    /// <param name="source">The source element.</param>
    /// <param name="target">The target element.</param>
    /// <returns>This Actions instance for chaining.</returns>
    public Actions DragAndDrop(AvaloniaElement source, AvaloniaElement target)
    {
        return MoveToElement(source)
            .ClickAndHold()
            .MoveToElement(target)
            .Release();
    }

    /// <summary>
    /// Drags an element by an offset.
    /// </summary>
    /// <param name="source">The source element.</param>
    /// <param name="offsetX">X offset.</param>
    /// <param name="offsetY">Y offset.</param>
    /// <returns>This Actions instance for chaining.</returns>
    public Actions DragAndDropBy(AvaloniaElement source, int offsetX, int offsetY)
    {
        return MoveToElement(source)
            .ClickAndHold()
            .MoveByOffset(offsetX, offsetY)
            .Release();
    }

    /// <summary>
    /// Scrolls by the specified amount.
    /// </summary>
    /// <param name="deltaX">Horizontal scroll amount.</param>
    /// <param name="deltaY">Vertical scroll amount.</param>
    /// <returns>This Actions instance for chaining.</returns>
    public Actions Scroll(int deltaX, int deltaY)
    {
        _currentPointerSequence.AddAction(new ScrollAction
        {
            DeltaX = deltaX,
            DeltaY = deltaY,
            Origin = PointerOrigin.Viewport
        });
        return this;
    }

    /// <summary>
    /// Scrolls to an element.
    /// </summary>
    /// <param name="element">The element to scroll to.</param>
    /// <returns>This Actions instance for chaining.</returns>
    public Actions ScrollToElement(AvaloniaElement element)
    {
        _currentPointerSequence.AddAction(new ScrollAction
        {
            Origin = PointerOrigin.Element,
            Element = element
        });
        return this;
    }

    #endregion

    #region Keyboard Actions

    /// <summary>
    /// Presses a key down without releasing.
    /// </summary>
    /// <param name="key">The key to press.</param>
    /// <returns>This Actions instance for chaining.</returns>
    public Actions KeyDown(Key key)
    {
        _currentKeySequence.AddAction(new KeyDownAction { Key = key });
        return this;
    }

    /// <summary>
    /// Presses a key down on an element.
    /// </summary>
    /// <param name="element">The element to receive the key.</param>
    /// <param name="key">The key to press.</param>
    /// <returns>This Actions instance for chaining.</returns>
    public Actions KeyDown(AvaloniaElement element, Key key)
    {
        Click(element);
        return KeyDown(key);
    }

    /// <summary>
    /// Releases a key.
    /// </summary>
    /// <param name="key">The key to release.</param>
    /// <returns>This Actions instance for chaining.</returns>
    public Actions KeyUp(Key key)
    {
        _currentKeySequence.AddAction(new KeyUpAction { Key = key });
        return this;
    }

    /// <summary>
    /// Releases a key on an element.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <param name="key">The key to release.</param>
    /// <returns>This Actions instance for chaining.</returns>
    public Actions KeyUp(AvaloniaElement element, Key key)
    {
        Click(element);
        return KeyUp(key);
    }

    /// <summary>
    /// Types the specified text.
    /// </summary>
    /// <param name="text">The text to type.</param>
    /// <returns>This Actions instance for chaining.</returns>
    public Actions SendKeys(string text)
    {
        foreach (var c in text)
        {
            _currentKeySequence.AddAction(new SendKeysAction { Text = c.ToString() });
        }
        return this;
    }

    /// <summary>
    /// Types text on an element.
    /// </summary>
    /// <param name="element">The element to receive the text.</param>
    /// <param name="text">The text to type.</param>
    /// <returns>This Actions instance for chaining.</returns>
    public Actions SendKeys(AvaloniaElement element, string text)
    {
        Click(element);
        return SendKeys(text);
    }

    #endregion

    #region Timing

    /// <summary>
    /// Pauses for the specified duration.
    /// </summary>
    /// <param name="milliseconds">Duration in milliseconds.</param>
    /// <returns>This Actions instance for chaining.</returns>
    public Actions Pause(int milliseconds)
    {
        _currentPointerSequence.AddAction(new PauseAction { Duration = TimeSpan.FromMilliseconds(milliseconds) });
        _currentKeySequence.AddAction(new PauseAction { Duration = TimeSpan.FromMilliseconds(milliseconds) });
        return this;
    }

    /// <summary>
    /// Pauses for the specified duration.
    /// </summary>
    /// <param name="duration">The duration.</param>
    /// <returns>This Actions instance for chaining.</returns>
    public Actions Pause(TimeSpan duration)
    {
        _currentPointerSequence.AddAction(new PauseAction { Duration = duration });
        _currentKeySequence.AddAction(new PauseAction { Duration = duration });
        return this;
    }

    #endregion

    #region Execution

    /// <summary>
    /// Performs all queued actions.
    /// </summary>
    public void Perform()
    {
        foreach (var sequence in _sequences)
        {
            ExecuteSequence(sequence);
        }
        Dispatcher.UIThread.RunJobs();
    }

    /// <summary>
    /// Performs all queued actions asynchronously.
    /// </summary>
    public async Task PerformAsync()
    {
        foreach (var sequence in _sequences)
        {
            await ExecuteSequenceAsync(sequence);
        }
        Dispatcher.UIThread.RunJobs();
    }

    /// <summary>
    /// Clears all queued actions.
    /// </summary>
    /// <returns>This Actions instance for chaining.</returns>
    public Actions Reset()
    {
        foreach (var sequence in _sequences)
        {
            sequence.Clear();
        }
        _currentElement = null;
        _currentPosition = default;
        return this;
    }

    private void ExecuteSequence(ActionSequence sequence)
    {
        foreach (var action in sequence.Actions)
        {
            ExecuteAction(action);
        }
    }

    private async Task ExecuteSequenceAsync(ActionSequence sequence)
    {
        foreach (var action in sequence.Actions)
        {
            await ExecuteActionAsync(action);
        }
    }

    private void ExecuteAction(IAction action)
    {
        switch (action)
        {
            case PointerMoveAction move:
                ExecutePointerMove(move);
                break;
            case PointerDownAction down:
                _driver.MouseSimulator.ButtonDown(_currentElement?.Control ?? _driver.Root, down.Button, _currentPosition);
                break;
            case PointerUpAction up:
                _driver.MouseSimulator.ButtonUp(_currentElement?.Control ?? _driver.Root, up.Button, _currentPosition);
                break;
            case ScrollAction scroll:
                _driver.MouseSimulator.Wheel(_currentElement?.Control ?? _driver.Root, new Vector(scroll.DeltaX, scroll.DeltaY), _currentPosition);
                break;
            case KeyDownAction keyDown:
                _driver.KeyboardSimulator.KeyDown(_currentElement?.Control ?? _driver.Root, keyDown.Key);
                break;
            case KeyUpAction keyUp:
                _driver.KeyboardSimulator.KeyUp(_currentElement?.Control ?? _driver.Root, keyUp.Key);
                break;
            case SendKeysAction sendKeys:
                _driver.KeyboardSimulator.TypeText(_currentElement?.Control ?? _driver.Root, sendKeys.Text);
                break;
            case PauseAction pause:
                Task.Delay(pause.Duration).Wait();
                break;
        }
        Dispatcher.UIThread.RunJobs();
    }

    private async Task ExecuteActionAsync(IAction action)
    {
        switch (action)
        {
            case PauseAction pause:
                await Task.Delay(pause.Duration);
                break;
            default:
                ExecuteAction(action);
                break;
        }
    }

    private void ExecutePointerMove(PointerMoveAction move)
    {
        Point targetPosition;
        Control targetControl;

        switch (move.Origin)
        {
            case PointerOrigin.Element when move.Element != null:
                targetControl = move.Element.Control;
                targetPosition = new Point(move.Element.AbsoluteCenter.X + move.X, move.Element.AbsoluteCenter.Y + move.Y);
                _currentElement = move.Element;
                break;
            case PointerOrigin.Pointer:
                targetControl = _currentElement?.Control ?? _driver.Root;
                targetPosition = new Point(_currentPosition.X + move.X, _currentPosition.Y + move.Y);
                break;
            default:
                targetControl = _driver.Root;
                targetPosition = new Point(move.X, move.Y);
                break;
        }

        _currentPosition = targetPosition;
        _driver.MouseSimulator.MoveTo(targetControl, targetPosition);
    }

    #endregion
}

#region Action Types

/// <summary>
/// Base interface for all action types.
/// </summary>
public interface IAction
{
    /// <summary>
    /// Gets the action type.
    /// </summary>
    string Type { get; }
}

/// <summary>
/// Represents an action sequence for a single input source.
/// </summary>
public class ActionSequence
{
    private readonly List<IAction> _actions = new();

    /// <summary>
    /// Gets the source type.
    /// </summary>
    public ActionSourceType SourceType { get; }

    /// <summary>
    /// Gets the source ID.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the actions in this sequence.
    /// </summary>
    public IReadOnlyList<IAction> Actions => _actions;

    /// <summary>
    /// Creates a new action sequence.
    /// </summary>
    /// <param name="sourceType">The source type.</param>
    /// <param name="id">The source ID.</param>
    public ActionSequence(ActionSourceType sourceType, string id)
    {
        SourceType = sourceType;
        Id = id;
    }

    /// <summary>
    /// Adds an action to the sequence.
    /// </summary>
    /// <param name="action">The action to add.</param>
    public void AddAction(IAction action) => _actions.Add(action);

    /// <summary>
    /// Clears all actions.
    /// </summary>
    public void Clear() => _actions.Clear();
}

/// <summary>
/// Action source types.
/// </summary>
public enum ActionSourceType
{
    /// <summary>No source.</summary>
    None,
    /// <summary>Pointer input (mouse, touch, pen).</summary>
    Pointer,
    /// <summary>Keyboard input.</summary>
    Key,
    /// <summary>Wheel input.</summary>
    Wheel
}

/// <summary>
/// Pointer origin types.
/// </summary>
public enum PointerOrigin
{
    /// <summary>Viewport coordinates.</summary>
    Viewport,
    /// <summary>Relative to current pointer position.</summary>
    Pointer,
    /// <summary>Relative to an element.</summary>
    Element
}

/// <summary>
/// Pointer move action.
/// </summary>
public class PointerMoveAction : IAction
{
    /// <inheritdoc />
    public string Type => "pointerMove";
    /// <summary>Origin for the move.</summary>
    public PointerOrigin Origin { get; set; }
    /// <summary>Target element if origin is Element.</summary>
    public AvaloniaElement? Element { get; set; }
    /// <summary>X coordinate or offset.</summary>
    public int X { get; set; }
    /// <summary>Y coordinate or offset.</summary>
    public int Y { get; set; }
    /// <summary>Duration of the move.</summary>
    public TimeSpan Duration { get; set; } = TimeSpan.FromMilliseconds(100);
}

/// <summary>
/// Pointer down action.
/// </summary>
public class PointerDownAction : IAction
{
    /// <inheritdoc />
    public string Type => "pointerDown";
    /// <summary>The button to press.</summary>
    public MouseButton Button { get; set; } = MouseButton.Left;
    /// <summary>The click count (1 for single click, 2 for double click).</summary>
    public int ClickCount { get; set; } = 1;
}

/// <summary>
/// Pointer up action.
/// </summary>
public class PointerUpAction : IAction
{
    /// <inheritdoc />
    public string Type => "pointerUp";
    /// <summary>The button to release.</summary>
    public MouseButton Button { get; set; } = MouseButton.Left;
}

/// <summary>
/// Scroll action.
/// </summary>
public class ScrollAction : IAction
{
    /// <inheritdoc />
    public string Type => "scroll";
    /// <summary>Horizontal scroll amount.</summary>
    public int DeltaX { get; set; }
    /// <summary>Vertical scroll amount.</summary>
    public int DeltaY { get; set; }
    /// <summary>Origin for the scroll.</summary>
    public PointerOrigin Origin { get; set; }
    /// <summary>Target element if origin is Element.</summary>
    public AvaloniaElement? Element { get; set; }
}

/// <summary>
/// Key down action.
/// </summary>
public class KeyDownAction : IAction
{
    /// <inheritdoc />
    public string Type => "keyDown";
    /// <summary>The key to press.</summary>
    public Key Key { get; set; }
}

/// <summary>
/// Key up action.
/// </summary>
public class KeyUpAction : IAction
{
    /// <inheritdoc />
    public string Type => "keyUp";
    /// <summary>The key to release.</summary>
    public Key Key { get; set; }
}

/// <summary>
/// Send keys action.
/// </summary>
public class SendKeysAction : IAction
{
    /// <inheritdoc />
    public string Type => "sendKeys";
    /// <summary>The text to send.</summary>
    public string Text { get; set; } = string.Empty;
}

/// <summary>
/// Pause action.
/// </summary>
public class PauseAction : IAction
{
    /// <inheritdoc />
    public string Type => "pause";
    /// <summary>Duration of the pause.</summary>
    public TimeSpan Duration { get; set; }
}

#endregion

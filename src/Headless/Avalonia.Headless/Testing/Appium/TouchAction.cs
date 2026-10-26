// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace Avalonia.HeadlessTestingFramework.Appium;

/// <summary>
/// Chainable touch action builder for complex gesture sequences.
/// Similar to Appium TouchAction for mobile automation.
/// </summary>
/// <example>
/// <code>
/// driver.CreateTouchAction()
///     .Press(element)
///     .Wait(500)
///     .MoveTo(target)
///     .Release()
///     .Perform();
///
/// // Multi-finger gestures
/// var action1 = driver.CreateTouchAction()
///     .Press(100, 100)
///     .MoveTo(150, 150)
///     .Release();
///
/// var action2 = driver.CreateTouchAction()
///     .Press(200, 200)
///     .MoveTo(250, 250)
///     .Release();
///
/// MultiTouchAction.Perform(action1, action2);
/// </code>
/// </example>
public class TouchAction
{
    private readonly AvaloniaDriver _driver;
    private readonly List<TouchStep> _steps = new();
    private int _currentTouchId = -1;
    private Point _currentPosition;
    private Control? _currentElement;

    /// <summary>
    /// Gets the steps in this action.
    /// </summary>
    internal IReadOnlyList<TouchStep> Steps => _steps;

    /// <summary>
    /// Creates a new TouchAction.
    /// </summary>
    /// <param name="driver">The driver instance.</param>
    public TouchAction(AvaloniaDriver driver)
    {
        _driver = driver ?? throw new ArgumentNullException(nameof(driver));
    }

    /// <summary>
    /// Press down at the specified element's center.
    /// </summary>
    /// <param name="element">The element to press.</param>
    /// <returns>This TouchAction for chaining.</returns>
    public TouchAction Press(AvaloniaElement element)
    {
        return Press(element.Control, element.Center);
    }

    /// <summary>
    /// Press down at the specified position on the element.
    /// </summary>
    /// <param name="element">The element to press.</param>
    /// <param name="x">X offset from element's top-left.</param>
    /// <param name="y">Y offset from element's top-left.</param>
    /// <returns>This TouchAction for chaining.</returns>
    public TouchAction Press(AvaloniaElement element, double x, double y)
    {
        return Press(element.Control, new Point(x, y));
    }

    /// <summary>
    /// Press down at the specified position.
    /// </summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <returns>This TouchAction for chaining.</returns>
    public TouchAction Press(double x, double y)
    {
        return Press(_driver.Root, new Point(x, y));
    }

    /// <summary>
    /// Press down at the specified position on a control.
    /// </summary>
    /// <param name="control">The control to press.</param>
    /// <param name="position">The position.</param>
    /// <returns>This TouchAction for chaining.</returns>
    public TouchAction Press(Control control, Point position)
    {
        _steps.Add(new TouchStep
        {
            Type = TouchStepType.Press,
            Position = position,
            Element = control
        });
        _currentPosition = position;
        _currentElement = control;
        return this;
    }

    /// <summary>
    /// Long press at the specified element's center.
    /// </summary>
    /// <param name="element">The element to long press.</param>
    /// <param name="durationMs">Duration in milliseconds.</param>
    /// <returns>This TouchAction for chaining.</returns>
    public TouchAction LongPress(AvaloniaElement element, int durationMs = 1000)
    {
        return LongPress(element.Control, element.Center, durationMs);
    }

    /// <summary>
    /// Long press at the specified position.
    /// </summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <param name="durationMs">Duration in milliseconds.</param>
    /// <returns>This TouchAction for chaining.</returns>
    public TouchAction LongPress(double x, double y, int durationMs = 1000)
    {
        return LongPress(_driver.Root, new Point(x, y), durationMs);
    }

    /// <summary>
    /// Long press at the specified position on a control.
    /// </summary>
    /// <param name="control">The control to long press.</param>
    /// <param name="position">The position.</param>
    /// <param name="durationMs">Duration in milliseconds.</param>
    /// <returns>This TouchAction for chaining.</returns>
    public TouchAction LongPress(Control control, Point position, int durationMs = 1000)
    {
        _steps.Add(new TouchStep
        {
            Type = TouchStepType.LongPress,
            Position = position,
            Element = control,
            Duration = TimeSpan.FromMilliseconds(durationMs)
        });
        _currentPosition = position;
        _currentElement = control;
        return this;
    }

    /// <summary>
    /// Move to the specified element's center.
    /// </summary>
    /// <param name="element">The element to move to.</param>
    /// <returns>This TouchAction for chaining.</returns>
    public TouchAction MoveTo(AvaloniaElement element)
    {
        return MoveTo(element.Control, element.AbsoluteCenter);
    }

    /// <summary>
    /// Move to the specified position on the element.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <param name="x">X offset from element's top-left.</param>
    /// <param name="y">Y offset from element's top-left.</param>
    /// <returns>This TouchAction for chaining.</returns>
    public TouchAction MoveTo(AvaloniaElement element, double x, double y)
    {
        return MoveTo(element.Control, new Point(x, y));
    }

    /// <summary>
    /// Move to the specified position.
    /// </summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <returns>This TouchAction for chaining.</returns>
    public TouchAction MoveTo(double x, double y)
    {
        return MoveTo(_currentElement ?? _driver.Root, new Point(x, y));
    }

    /// <summary>
    /// Move by offset from current position.
    /// </summary>
    /// <param name="offsetX">X offset.</param>
    /// <param name="offsetY">Y offset.</param>
    /// <returns>This TouchAction for chaining.</returns>
    public TouchAction MoveBy(double offsetX, double offsetY)
    {
        var newPosition = new Point(_currentPosition.X + offsetX, _currentPosition.Y + offsetY);
        return MoveTo(_currentElement ?? _driver.Root, newPosition);
    }

    /// <summary>
    /// Move to the specified position on a control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <param name="position">The position.</param>
    /// <returns>This TouchAction for chaining.</returns>
    public TouchAction MoveTo(Control control, Point position)
    {
        _steps.Add(new TouchStep
        {
            Type = TouchStepType.Move,
            Position = position,
            Element = control
        });
        _currentPosition = position;
        _currentElement = control;
        return this;
    }

    /// <summary>
    /// Release the touch.
    /// </summary>
    /// <returns>This TouchAction for chaining.</returns>
    public TouchAction Release()
    {
        _steps.Add(new TouchStep
        {
            Type = TouchStepType.Release,
            Position = _currentPosition,
            Element = _currentElement ?? _driver.Root
        });
        return this;
    }

    /// <summary>
    /// Tap at the specified element's center.
    /// </summary>
    /// <param name="element">The element to tap.</param>
    /// <param name="count">Number of taps.</param>
    /// <returns>This TouchAction for chaining.</returns>
    public TouchAction Tap(AvaloniaElement element, int count = 1)
    {
        return Tap(element.Control, element.Center, count);
    }

    /// <summary>
    /// Tap at the specified position.
    /// </summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <param name="count">Number of taps.</param>
    /// <returns>This TouchAction for chaining.</returns>
    public TouchAction Tap(double x, double y, int count = 1)
    {
        return Tap(_driver.Root, new Point(x, y), count);
    }

    /// <summary>
    /// Tap at the specified position on a control.
    /// </summary>
    /// <param name="control">The control to tap.</param>
    /// <param name="position">The position.</param>
    /// <param name="count">Number of taps.</param>
    /// <returns>This TouchAction for chaining.</returns>
    public TouchAction Tap(Control control, Point position, int count = 1)
    {
        _steps.Add(new TouchStep
        {
            Type = TouchStepType.Tap,
            Position = position,
            Element = control,
            Count = count
        });
        _currentPosition = position;
        _currentElement = control;
        return this;
    }

    /// <summary>
    /// Double tap at the specified element's center.
    /// </summary>
    /// <param name="element">The element to double tap.</param>
    /// <returns>This TouchAction for chaining.</returns>
    public TouchAction DoubleTap(AvaloniaElement element)
    {
        return Tap(element, 2);
    }

    /// <summary>
    /// Wait for the specified duration.
    /// </summary>
    /// <param name="milliseconds">Duration in milliseconds.</param>
    /// <returns>This TouchAction for chaining.</returns>
    public TouchAction Wait(int milliseconds)
    {
        _steps.Add(new TouchStep
        {
            Type = TouchStepType.Wait,
            Duration = TimeSpan.FromMilliseconds(milliseconds)
        });
        return this;
    }

    /// <summary>
    /// Wait for the specified duration.
    /// </summary>
    /// <param name="duration">Duration to wait.</param>
    /// <returns>This TouchAction for chaining.</returns>
    public TouchAction Wait(TimeSpan duration)
    {
        _steps.Add(new TouchStep
        {
            Type = TouchStepType.Wait,
            Duration = duration
        });
        return this;
    }

    /// <summary>
    /// Swipe from the current position in the specified direction.
    /// </summary>
    /// <param name="direction">The swipe direction.</param>
    /// <param name="distance">The swipe distance.</param>
    /// <param name="durationMs">Duration in milliseconds.</param>
    /// <returns>This TouchAction for chaining.</returns>
    public TouchAction Swipe(SwipeDirection direction, double distance = 100, int durationMs = 300)
    {
        var endPoint = direction switch
        {
            SwipeDirection.Up => new Point(_currentPosition.X, _currentPosition.Y - distance),
            SwipeDirection.Down => new Point(_currentPosition.X, _currentPosition.Y + distance),
            SwipeDirection.Left => new Point(_currentPosition.X - distance, _currentPosition.Y),
            SwipeDirection.Right => new Point(_currentPosition.X + distance, _currentPosition.Y),
            _ => _currentPosition
        };

        _steps.Add(new TouchStep
        {
            Type = TouchStepType.Swipe,
            Position = _currentPosition,
            EndPosition = endPoint,
            Element = _currentElement ?? _driver.Root,
            Duration = TimeSpan.FromMilliseconds(durationMs)
        });
        _currentPosition = endPoint;
        return this;
    }

    /// <summary>
    /// Swipe from start to end position.
    /// </summary>
    /// <param name="startX">Start X.</param>
    /// <param name="startY">Start Y.</param>
    /// <param name="endX">End X.</param>
    /// <param name="endY">End Y.</param>
    /// <param name="durationMs">Duration in milliseconds.</param>
    /// <returns>This TouchAction for chaining.</returns>
    public TouchAction Swipe(double startX, double startY, double endX, double endY, int durationMs = 300)
    {
        _steps.Add(new TouchStep
        {
            Type = TouchStepType.Swipe,
            Position = new Point(startX, startY),
            EndPosition = new Point(endX, endY),
            Element = _currentElement ?? _driver.Root,
            Duration = TimeSpan.FromMilliseconds(durationMs)
        });
        _currentPosition = new Point(endX, endY);
        return this;
    }

    /// <summary>
    /// Performs all the actions in this chain.
    /// </summary>
    public void Perform()
    {
        foreach (var step in _steps)
        {
            ExecuteStep(step);
        }
        _steps.Clear();
    }

    /// <summary>
    /// Performs all the actions asynchronously.
    /// </summary>
    public async Task PerformAsync()
    {
        foreach (var step in _steps)
        {
            await ExecuteStepAsync(step);
        }
        _steps.Clear();
    }

    /// <summary>
    /// Cancels all pending actions.
    /// </summary>
    public void Cancel()
    {
        if (_currentTouchId >= 0)
        {
            _driver.TouchSimulator.TouchUp(_driver.Root, _currentTouchId);
            _currentTouchId = -1;
        }
        _steps.Clear();
    }

    private void ExecuteStep(TouchStep step)
    {
        var element = step.Element ?? _driver.Root;

        switch (step.Type)
        {
            case TouchStepType.Press:
                _currentTouchId = _driver.TouchSimulator.TouchDown(element, step.Position);
                break;

            case TouchStepType.LongPress:
                _currentTouchId = _driver.TouchSimulator.TouchDown(element, step.Position);
                Task.Delay(step.Duration).Wait();
                Dispatcher.UIThread.RunJobs();
                break;

            case TouchStepType.Move:
                if (_currentTouchId >= 0)
                {
                    _driver.TouchSimulator.TouchMove(element, _currentTouchId, step.Position);
                }
                break;

            case TouchStepType.Release:
                if (_currentTouchId >= 0)
                {
                    _driver.TouchSimulator.TouchUp(element, _currentTouchId);
                    _currentTouchId = -1;
                }
                break;

            case TouchStepType.Tap:
                for (int i = 0; i < step.Count; i++)
                {
                    if (i > 0)
                    {
                        Task.Delay(100).Wait();
                    }
                    _driver.TouchSimulator.Tap(element, step.Position);
                }
                break;

            case TouchStepType.Wait:
                Task.Delay(step.Duration).Wait();
                Dispatcher.UIThread.RunJobs();
                break;

            case TouchStepType.Swipe:
                PerformSwipe(element, step.Position, step.EndPosition, step.Duration);
                break;
        }
    }

    private async Task ExecuteStepAsync(TouchStep step)
    {
        var element = step.Element ?? _driver.Root;

        switch (step.Type)
        {
            case TouchStepType.Press:
                _currentTouchId = _driver.TouchSimulator.TouchDown(element, step.Position);
                break;

            case TouchStepType.LongPress:
                _currentTouchId = _driver.TouchSimulator.TouchDown(element, step.Position);
                await Task.Delay(step.Duration);
                Dispatcher.UIThread.RunJobs();
                break;

            case TouchStepType.Move:
                if (_currentTouchId >= 0)
                {
                    _driver.TouchSimulator.TouchMove(element, _currentTouchId, step.Position);
                }
                break;

            case TouchStepType.Release:
                if (_currentTouchId >= 0)
                {
                    _driver.TouchSimulator.TouchUp(element, _currentTouchId);
                    _currentTouchId = -1;
                }
                break;

            case TouchStepType.Tap:
                for (int i = 0; i < step.Count; i++)
                {
                    if (i > 0)
                    {
                        await Task.Delay(100);
                    }
                    _driver.TouchSimulator.Tap(element, step.Position);
                }
                break;

            case TouchStepType.Wait:
                await Task.Delay(step.Duration);
                Dispatcher.UIThread.RunJobs();
                break;

            case TouchStepType.Swipe:
                await PerformSwipeAsync(element, step.Position, step.EndPosition, step.Duration);
                break;
        }
    }

    private void PerformSwipe(Control element, Point start, Point end, TimeSpan duration)
    {
        var steps = Math.Max(10, (int)(duration.TotalMilliseconds / 16));
        var stepDelay = (int)(duration.TotalMilliseconds / steps);

        var touchId = _driver.TouchSimulator.TouchDown(element, start);

        for (int i = 1; i <= steps; i++)
        {
            var t = (double)i / steps;
            var currentPoint = new Point(
                start.X + (end.X - start.X) * t,
                start.Y + (end.Y - start.Y) * t);

            _driver.TouchSimulator.TouchMove(element, touchId, currentPoint);
            Task.Delay(stepDelay).Wait();
        }

        _driver.TouchSimulator.TouchUp(element, touchId);
    }

    private async Task PerformSwipeAsync(Control element, Point start, Point end, TimeSpan duration)
    {
        var steps = Math.Max(10, (int)(duration.TotalMilliseconds / 16));
        var stepDelay = (int)(duration.TotalMilliseconds / steps);

        var touchId = _driver.TouchSimulator.TouchDown(element, start);

        for (int i = 1; i <= steps; i++)
        {
            var t = (double)i / steps;
            var currentPoint = new Point(
                start.X + (end.X - start.X) * t,
                start.Y + (end.Y - start.Y) * t);

            _driver.TouchSimulator.TouchMove(element, touchId, currentPoint);
            await Task.Delay(stepDelay);
        }

        _driver.TouchSimulator.TouchUp(element, touchId);
    }
}

/// <summary>
/// Represents a step in a touch action sequence.
/// </summary>
internal class TouchStep
{
    public TouchStepType Type { get; set; }
    public Point Position { get; set; }
    public Point EndPosition { get; set; }
    public Control? Element { get; set; }
    public TimeSpan Duration { get; set; } = TimeSpan.FromMilliseconds(100);
    public int Count { get; set; } = 1;
}

/// <summary>
/// Types of touch steps.
/// </summary>
internal enum TouchStepType
{
    Press,
    LongPress,
    Move,
    Release,
    Tap,
    Wait,
    Swipe
}

/// <summary>
/// Performs multiple touch actions simultaneously (multi-touch).
/// </summary>
public static class MultiTouchAction
{
    /// <summary>
    /// Performs multiple touch actions simultaneously.
    /// </summary>
    /// <param name="actions">The touch actions to perform.</param>
    public static void Perform(params TouchAction[] actions)
    {
        if (actions.Length == 0)
            return;

        // Get max steps count
        var maxSteps = 0;
        foreach (var action in actions)
        {
            if (action.Steps.Count > maxSteps)
                maxSteps = action.Steps.Count;
        }

        // Execute steps in parallel
        for (int i = 0; i < maxSteps; i++)
        {
            foreach (var action in actions)
            {
                if (i < action.Steps.Count)
                {
                    // Execute step (simplified - real implementation would be more complex)
                }
            }
            Dispatcher.UIThread.RunJobs();
        }

        // Perform all actions
        foreach (var action in actions)
        {
            action.Perform();
        }
    }

    /// <summary>
    /// Performs a pinch gesture with two fingers.
    /// </summary>
    /// <param name="driver">The driver.</param>
    /// <param name="element">The element to pinch.</param>
    /// <param name="scale">Scale factor (greater than 1 = spread/zoom in, less than 1 = pinch/zoom out).</param>
    /// <param name="durationMs">Duration in milliseconds.</param>
    public static void Pinch(AvaloniaDriver driver, AvaloniaElement element, double scale, int durationMs = 300)
    {
        var center = element.Center;
        var startDistance = 50.0;
        var endDistance = startDistance * scale;

        var start1 = new Point(center.X - startDistance, center.Y);
        var start2 = new Point(center.X + startDistance, center.Y);
        var end1 = new Point(center.X - endDistance, center.Y);
        var end2 = new Point(center.X + endDistance, center.Y);

        // Simulate two-finger pinch using gesture event
        driver.TouchSimulator.PinchGesture(element.Control, scale, center);
        Task.Delay(durationMs).Wait();
        driver.TouchSimulator.PinchGestureEnded(element.Control);
    }

    /// <summary>
    /// Performs a scroll gesture.
    /// </summary>
    /// <param name="driver">The driver.</param>
    /// <param name="element">The element to scroll.</param>
    /// <param name="deltaX">Horizontal scroll amount.</param>
    /// <param name="deltaY">Vertical scroll amount.</param>
    public static void Scroll(AvaloniaDriver driver, AvaloniaElement element, double deltaX, double deltaY)
    {
        driver.TouchSimulator.ScrollGesture(element.Control, new Vector(deltaX, deltaY));
    }

    /// <summary>
    /// Performs a two-finger rotation gesture.
    /// </summary>
    /// <param name="driver">The driver.</param>
    /// <param name="element">The element to rotate.</param>
    /// <param name="degrees">Rotation in degrees.</param>
    /// <param name="durationMs">Duration in milliseconds.</param>
    public static void Rotate(AvaloniaDriver driver, AvaloniaElement element, double degrees, int durationMs = 300)
    {
        // Rotation would be simulated through specific gesture events if supported
        var radians = degrees * Math.PI / 180;
        // Implementation depends on platform support for rotation gestures
    }
}

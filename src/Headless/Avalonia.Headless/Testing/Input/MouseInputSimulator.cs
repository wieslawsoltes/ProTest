// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;

namespace Avalonia.HeadlessTestingFramework;

/// <summary>
/// A mouse input simulator for headless testing of Avalonia controls.
/// Provides methods to simulate mouse events including clicks, movement, drag, and wheel scrolling.
/// </summary>
public class MouseInputSimulator
{
    private Point _currentPosition;
    private MouseButton _pressedButton = MouseButton.None;
    private ulong _timestamp;
    private int _clickCount;
    private DateTime _lastClickTime = DateTime.MinValue;
    private Point _lastClickPosition;
    private readonly TimeSpan _doubleClickInterval = TimeSpan.FromMilliseconds(500);
    private readonly double _doubleClickDistance = 4;

    /// <summary>
    /// Gets the current mouse position.
    /// </summary>
    public Point Position => _currentPosition;

    /// <summary>
    /// Gets the currently pressed mouse button.
    /// </summary>
    public MouseButton PressedButton => _pressedButton;

    /// <summary>
    /// Gets the current timestamp.
    /// </summary>
    public ulong Timestamp => _timestamp;

    /// <summary>
    /// Gets or sets the current key modifiers for mouse events.
    /// </summary>
    public KeyModifiers Modifiers { get; set; } = KeyModifiers.None;

    /// <summary>
    /// Creates a new mouse input simulator.
    /// </summary>
    public MouseInputSimulator()
    {
        _timestamp = 0;
        _currentPosition = new Point(0, 0);
    }

    /// <summary>
    /// Advances the internal timestamp by the specified number of milliseconds.
    /// </summary>
    public void AdvanceTime(int milliseconds)
    {
        _timestamp += (ulong)milliseconds;
    }

    /// <summary>
    /// Resets the simulator to its initial state.
    /// </summary>
    public void Reset()
    {
        _currentPosition = new Point(0, 0);
        _pressedButton = MouseButton.None;
        _timestamp = 0;
        _clickCount = 0;
        Modifiers = KeyModifiers.None;
    }

    #region Mouse Movement

    /// <summary>
    /// Moves the mouse to the specified position.
    /// </summary>
    /// <param name="target">The target control to receive the event.</param>
    /// <param name="position">The new mouse position.</param>
    public void MoveTo(Interactive target, Point position)
    {
        _currentPosition = position;

        var pointer = CreateMousePointer();
        var modifiers = GetRawModifiers();

        var args = new PointerEventArgs(
            InputElement.PointerMovedEvent,
            target,
            pointer,
            (Visual)target,
            position,
            _timestamp,
            new PointerPointProperties(modifiers, PointerUpdateKind.Other),
            Modifiers);

        target.RaiseEvent(args);
    }

    /// <summary>
    /// Moves the mouse by the specified delta.
    /// </summary>
    /// <param name="target">The target control to receive the event.</param>
    /// <param name="deltaX">Horizontal movement.</param>
    /// <param name="deltaY">Vertical movement.</param>
    public void MoveBy(Interactive target, double deltaX, double deltaY)
    {
        MoveTo(target, new Point(_currentPosition.X + deltaX, _currentPosition.Y + deltaY));
    }

    /// <summary>
    /// Simulates mouse entering a control.
    /// </summary>
    public void Enter(Interactive target, Point position)
    {
        _currentPosition = position;

        var pointer = CreateMousePointer();
        var args = new PointerEventArgs(
            InputElement.PointerEnteredEvent,
            target,
            pointer,
            (Visual)target,
            position,
            _timestamp,
            new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.Other),
            Modifiers);

        target.RaiseEvent(args);
    }

    /// <summary>
    /// Simulates mouse leaving a control.
    /// </summary>
    public void Leave(Interactive target)
    {
        var pointer = CreateMousePointer();
        var args = new PointerEventArgs(
            InputElement.PointerExitedEvent,
            target,
            pointer,
            (Visual)target,
            _currentPosition,
            _timestamp,
            new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.Other),
            Modifiers);

        target.RaiseEvent(args);
    }

    #endregion

    #region Mouse Buttons

    /// <summary>
    /// Simulates a mouse button press.
    /// </summary>
    /// <param name="target">The target control to receive the event.</param>
    /// <param name="button">The button to press.</param>
    /// <param name="position">Optional position (uses current position if null).</param>
    public void ButtonDown(Interactive target, MouseButton button = MouseButton.Left, Point? position = null)
    {
        if (position.HasValue)
            _currentPosition = position.Value;

        _pressedButton = button;
        UpdateClickCount();

        var pointer = CreateMousePointer();
        var (modifiers, updateKind) = GetButtonDownInfo(button);

        var args = new PointerPressedEventArgs(
            target,
            pointer,
            (Visual)target,
            _currentPosition,
            _timestamp,
            new PointerPointProperties(modifiers, updateKind),
            Modifiers,
            _clickCount)
        {
            RoutedEvent = InputElement.PointerPressedEvent
        };

        target.RaiseEvent(args);
    }

    /// <summary>
    /// Simulates a mouse button release.
    /// </summary>
    /// <param name="target">The target control to receive the event.</param>
    /// <param name="button">The button to release.</param>
    /// <param name="position">Optional position (uses current position if null).</param>
    public void ButtonUp(Interactive target, MouseButton button = MouseButton.Left, Point? position = null)
    {
        if (position.HasValue)
            _currentPosition = position.Value;

        _pressedButton = MouseButton.None;

        var pointer = CreateMousePointer();
        var (_, updateKind) = GetButtonUpInfo(button);

        var args = new PointerReleasedEventArgs(
            target,
            pointer,
            (Visual)target,
            _currentPosition,
            _timestamp,
            new PointerPointProperties(RawInputModifiers.None, updateKind),
            Modifiers,
            button)
        {
            RoutedEvent = InputElement.PointerReleasedEvent
        };

        target.RaiseEvent(args);
    }

    /// <summary>
    /// Simulates a complete click (press and release).
    /// </summary>
    /// <param name="target">The target control.</param>
    /// <param name="position">Click position.</param>
    /// <param name="button">Mouse button to click.</param>
    /// <param name="holdTime">Time to hold the button in milliseconds.</param>
    public void Click(Interactive target, Point position, MouseButton button = MouseButton.Left, int holdTime = 50)
    {
        ButtonDown(target, button, position);
        AdvanceTime(holdTime);
        ButtonUp(target, button, position);
    }

    /// <summary>
    /// Simulates a double click.
    /// </summary>
    /// <param name="target">The target control.</param>
    /// <param name="position">Click position.</param>
    /// <param name="button">Mouse button to click.</param>
    /// <param name="clickInterval">Time between clicks in milliseconds.</param>
    public void DoubleClick(Interactive target, Point position, MouseButton button = MouseButton.Left, int clickInterval = 100)
    {
        Click(target, position, button);
        AdvanceTime(clickInterval);
        Click(target, position, button);
    }

    /// <summary>
    /// Simulates a right-click (context menu).
    /// </summary>
    public void RightClick(Interactive target, Point position, int holdTime = 50)
    {
        Click(target, position, MouseButton.Right, holdTime);
    }

    /// <summary>
    /// Simulates a middle-click.
    /// </summary>
    public void MiddleClick(Interactive target, Point position, int holdTime = 50)
    {
        Click(target, position, MouseButton.Middle, holdTime);
    }

    #endregion

    #region Drag Operations

    /// <summary>
    /// Simulates a mouse drag operation.
    /// </summary>
    /// <param name="target">The target control.</param>
    /// <param name="startPoint">Starting position.</param>
    /// <param name="endPoint">Ending position.</param>
    /// <param name="button">Mouse button to use for dragging.</param>
    /// <param name="steps">Number of intermediate steps.</param>
    public void Drag(Interactive target, Point startPoint, Point endPoint, MouseButton button = MouseButton.Left, int steps = 10)
    {
        ButtonDown(target, button, startPoint);

        var deltaX = (endPoint.X - startPoint.X) / steps;
        var deltaY = (endPoint.Y - startPoint.Y) / steps;

        for (int i = 1; i <= steps; i++)
        {
            var currentPoint = new Point(
                startPoint.X + (deltaX * i),
                startPoint.Y + (deltaY * i)
            );
            MoveTo(target, currentPoint);
            AdvanceTime(16); // ~60fps
        }

        ButtonUp(target, button, endPoint);
    }

    /// <summary>
    /// Simulates a drag with custom path.
    /// </summary>
    /// <param name="target">The target control.</param>
    /// <param name="path">Array of points defining the drag path.</param>
    /// <param name="button">Mouse button to use for dragging.</param>
    /// <param name="stepInterval">Time between steps in milliseconds.</param>
    public void DragPath(Interactive target, Point[] path, MouseButton button = MouseButton.Left, int stepInterval = 16)
    {
        if (path.Length < 2)
            throw new ArgumentException("Path must contain at least 2 points.", nameof(path));

        ButtonDown(target, button, path[0]);

        for (int i = 1; i < path.Length; i++)
        {
            MoveTo(target, path[i]);
            AdvanceTime(stepInterval);
        }

        ButtonUp(target, button, path[path.Length - 1]);
    }

    #endregion

    #region Mouse Wheel

    /// <summary>
    /// Simulates mouse wheel scrolling.
    /// </summary>
    /// <param name="target">The target control.</param>
    /// <param name="delta">Scroll delta (positive = up/left, negative = down/right).</param>
    /// <param name="position">Optional position (uses current position if null).</param>
    public void Wheel(Interactive target, Vector delta, Point? position = null)
    {
        if (position.HasValue)
            _currentPosition = position.Value;

        var pointer = CreateMousePointer();
        var args = new PointerWheelEventArgs(
            target,
            pointer,
            (Visual)target,
            _currentPosition,
            _timestamp,
            new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.Other),
            Modifiers,
            delta)
        {
            RoutedEvent = InputElement.PointerWheelChangedEvent
        };

        target.RaiseEvent(args);
    }

    /// <summary>
    /// Simulates vertical wheel scrolling.
    /// </summary>
    /// <param name="target">The target control.</param>
    /// <param name="lines">Number of lines to scroll (positive = up, negative = down).</param>
    /// <param name="position">Optional position.</param>
    public void ScrollVertical(Interactive target, double lines, Point? position = null)
    {
        Wheel(target, new Vector(0, lines), position);
    }

    /// <summary>
    /// Simulates horizontal wheel scrolling.
    /// </summary>
    /// <param name="target">The target control.</param>
    /// <param name="lines">Number of columns to scroll (positive = left, negative = right).</param>
    /// <param name="position">Optional position.</param>
    public void ScrollHorizontal(Interactive target, double lines, Point? position = null)
    {
        Wheel(target, new Vector(lines, 0), position);
    }

    /// <summary>
    /// Simulates smooth scrolling over time.
    /// </summary>
    /// <param name="target">The target control.</param>
    /// <param name="totalDelta">Total scroll amount.</param>
    /// <param name="steps">Number of scroll steps.</param>
    /// <param name="position">Optional position.</param>
    public void SmoothScroll(Interactive target, Vector totalDelta, int steps = 10, Point? position = null)
    {
        var stepDelta = new Vector(totalDelta.X / steps, totalDelta.Y / steps);

        for (int i = 0; i < steps; i++)
        {
            Wheel(target, stepDelta, position);
            AdvanceTime(16);
        }
    }

    #endregion

    #region Hover Effects

    /// <summary>
    /// Simulates hovering over a control for a duration.
    /// </summary>
    /// <param name="target">The target control.</param>
    /// <param name="position">Hover position.</param>
    /// <param name="duration">Duration in milliseconds.</param>
    public void Hover(Interactive target, Point position, int duration = 500)
    {
        Enter(target, position);
        AdvanceTime(duration);
    }

    /// <summary>
    /// Simulates moving through a series of positions (e.g., for testing hover states).
    /// </summary>
    /// <param name="target">The target control.</param>
    /// <param name="positions">Positions to move through.</param>
    /// <param name="pauseDuration">Time to pause at each position in milliseconds.</param>
    public void HoverPath(Interactive target, Point[] positions, int pauseDuration = 100)
    {
        Enter(target, positions[0]);
        AdvanceTime(pauseDuration);

        for (int i = 1; i < positions.Length; i++)
        {
            MoveTo(target, positions[i]);
            AdvanceTime(pauseDuration);
        }
    }

    #endregion

    #region Helper Methods

    private Pointer CreateMousePointer()
    {
        return new Pointer(0, PointerType.Mouse, true);
    }

    private RawInputModifiers GetRawModifiers()
    {
        var modifiers = RawInputModifiers.None;

        if (_pressedButton == MouseButton.Left)
            modifiers |= RawInputModifiers.LeftMouseButton;
        else if (_pressedButton == MouseButton.Right)
            modifiers |= RawInputModifiers.RightMouseButton;
        else if (_pressedButton == MouseButton.Middle)
            modifiers |= RawInputModifiers.MiddleMouseButton;

        if (Modifiers.HasFlag(KeyModifiers.Control))
            modifiers |= RawInputModifiers.Control;
        if (Modifiers.HasFlag(KeyModifiers.Shift))
            modifiers |= RawInputModifiers.Shift;
        if (Modifiers.HasFlag(KeyModifiers.Alt))
            modifiers |= RawInputModifiers.Alt;
        if (Modifiers.HasFlag(KeyModifiers.Meta))
            modifiers |= RawInputModifiers.Meta;

        return modifiers;
    }

    private (RawInputModifiers modifiers, PointerUpdateKind updateKind) GetButtonDownInfo(MouseButton button)
    {
        return button switch
        {
            MouseButton.Left => (RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed),
            MouseButton.Right => (RawInputModifiers.RightMouseButton, PointerUpdateKind.RightButtonPressed),
            MouseButton.Middle => (RawInputModifiers.MiddleMouseButton, PointerUpdateKind.MiddleButtonPressed),
            MouseButton.XButton1 => (RawInputModifiers.XButton1MouseButton, PointerUpdateKind.XButton1Pressed),
            MouseButton.XButton2 => (RawInputModifiers.XButton2MouseButton, PointerUpdateKind.XButton2Pressed),
            _ => (RawInputModifiers.None, PointerUpdateKind.Other)
        };
    }

    private (RawInputModifiers modifiers, PointerUpdateKind updateKind) GetButtonUpInfo(MouseButton button)
    {
        return button switch
        {
            MouseButton.Left => (RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased),
            MouseButton.Right => (RawInputModifiers.None, PointerUpdateKind.RightButtonReleased),
            MouseButton.Middle => (RawInputModifiers.None, PointerUpdateKind.MiddleButtonReleased),
            MouseButton.XButton1 => (RawInputModifiers.None, PointerUpdateKind.XButton1Released),
            MouseButton.XButton2 => (RawInputModifiers.None, PointerUpdateKind.XButton2Released),
            _ => (RawInputModifiers.None, PointerUpdateKind.Other)
        };
    }

    private void UpdateClickCount()
    {
        var now = DateTime.Now;
        var timeSinceLastClick = now - _lastClickTime;
        var distanceFromLastClick = Math.Sqrt(
            Math.Pow(_currentPosition.X - _lastClickPosition.X, 2) +
            Math.Pow(_currentPosition.Y - _lastClickPosition.Y, 2));

        if (timeSinceLastClick < _doubleClickInterval && distanceFromLastClick < _doubleClickDistance)
        {
            _clickCount++;
        }
        else
        {
            _clickCount = 1;
        }

        _lastClickTime = now;
        _lastClickPosition = _currentPosition;
    }

    #endregion
}

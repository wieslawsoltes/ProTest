// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System;
using System.Collections.Generic;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Avalonia.HeadlessTestingFramework;

/// <summary>
/// A keyboard input simulator for headless testing of Avalonia controls.
/// Provides methods to simulate keyboard events including key presses, releases, and text input.
/// </summary>
public class KeyboardInputSimulator
{
    private readonly HashSet<Key> _pressedKeys = new();
    private ulong _timestamp;
    private KeyModifiers _currentModifiers = KeyModifiers.None;

    /// <summary>
    /// Gets the current timestamp.
    /// </summary>
    public ulong Timestamp => _timestamp;

    /// <summary>
    /// Gets the currently pressed keys.
    /// </summary>
    public IReadOnlyCollection<Key> PressedKeys => _pressedKeys;

    /// <summary>
    /// Gets the current key modifiers state.
    /// </summary>
    public KeyModifiers CurrentModifiers => _currentModifiers;

    /// <summary>
    /// Creates a new keyboard input simulator.
    /// </summary>
    public KeyboardInputSimulator()
    {
        _timestamp = 0;
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
        _pressedKeys.Clear();
        _currentModifiers = KeyModifiers.None;
        _timestamp = 0;
    }

    #region Key Events

    /// <summary>
    /// Simulates a key down event.
    /// </summary>
    /// <param name="target">The target control to receive the event.</param>
    /// <param name="key">The key to press.</param>
    /// <param name="modifiers">Optional key modifiers (defaults to current state).</param>
    public void KeyDown(Interactive target, Key key, KeyModifiers? modifiers = null)
    {
        _pressedKeys.Add(key);
        UpdateModifiersForKey(key, true);

        var actualModifiers = modifiers ?? _currentModifiers;
        var args = new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key = key,
            KeyModifiers = actualModifiers,
            Source = target
        };

        target.RaiseEvent(args);
    }

    /// <summary>
    /// Simulates a key up event.
    /// </summary>
    /// <param name="target">The target control to receive the event.</param>
    /// <param name="key">The key to release.</param>
    /// <param name="modifiers">Optional key modifiers (defaults to current state).</param>
    public void KeyUp(Interactive target, Key key, KeyModifiers? modifiers = null)
    {
        _pressedKeys.Remove(key);
        UpdateModifiersForKey(key, false);

        var actualModifiers = modifiers ?? _currentModifiers;
        var args = new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyUpEvent,
            Key = key,
            KeyModifiers = actualModifiers,
            Source = target
        };

        target.RaiseEvent(args);
    }

    /// <summary>
    /// Simulates a complete key press (down and up).
    /// </summary>
    /// <param name="target">The target control to receive the event.</param>
    /// <param name="key">The key to press.</param>
    /// <param name="holdTime">Time in milliseconds to hold the key.</param>
    /// <param name="modifiers">Optional key modifiers.</param>
    public void KeyPress(Interactive target, Key key, int holdTime = 50, KeyModifiers? modifiers = null)
    {
        KeyDown(target, key, modifiers);
        AdvanceTime(holdTime);
        KeyUp(target, key, modifiers);
    }

    /// <summary>
    /// Simulates typing a string of text.
    /// </summary>
    /// <param name="target">The target control to receive the events.</param>
    /// <param name="text">The text to type.</param>
    /// <param name="keyInterval">Time between key presses in milliseconds.</param>
    public void TypeText(Interactive target, string text, int keyInterval = 50)
    {
        foreach (char c in text)
        {
            var key = CharToKey(c);
            var needsShift = char.IsUpper(c) || IsShiftedChar(c);

            if (needsShift)
            {
                KeyDown(target, Key.LeftShift);
            }

            KeyPress(target, key, keyInterval / 2);

            if (needsShift)
            {
                KeyUp(target, Key.LeftShift);
            }

            // Also send text input event
            var textArgs = new TextInputEventArgs
            {
                RoutedEvent = InputElement.TextInputEvent,
                Text = c.ToString(),
                Source = target
            };
            target.RaiseEvent(textArgs);

            AdvanceTime(keyInterval);
        }
    }

    #endregion

    #region Modifier Keys

    /// <summary>
    /// Presses the Control key.
    /// </summary>
    public void CtrlDown(Interactive target)
    {
        KeyDown(target, Key.LeftCtrl);
    }

    /// <summary>
    /// Releases the Control key.
    /// </summary>
    public void CtrlUp(Interactive target)
    {
        KeyUp(target, Key.LeftCtrl);
    }

    /// <summary>
    /// Presses the Shift key.
    /// </summary>
    public void ShiftDown(Interactive target)
    {
        KeyDown(target, Key.LeftShift);
    }

    /// <summary>
    /// Releases the Shift key.
    /// </summary>
    public void ShiftUp(Interactive target)
    {
        KeyUp(target, Key.LeftShift);
    }

    /// <summary>
    /// Presses the Alt key.
    /// </summary>
    public void AltDown(Interactive target)
    {
        KeyDown(target, Key.LeftAlt);
    }

    /// <summary>
    /// Releases the Alt key.
    /// </summary>
    public void AltUp(Interactive target)
    {
        KeyUp(target, Key.LeftAlt);
    }

    /// <summary>
    /// Presses the Meta/Command key.
    /// </summary>
    public void MetaDown(Interactive target)
    {
        KeyDown(target, Key.LWin);
    }

    /// <summary>
    /// Releases the Meta/Command key.
    /// </summary>
    public void MetaUp(Interactive target)
    {
        KeyUp(target, Key.LWin);
    }

    #endregion

    #region Keyboard Shortcuts

    /// <summary>
    /// Simulates Ctrl+Key combination.
    /// </summary>
    public void CtrlKey(Interactive target, Key key)
    {
        CtrlDown(target);
        KeyPress(target, key, modifiers: KeyModifiers.Control);
        CtrlUp(target);
    }

    /// <summary>
    /// Simulates Ctrl+Shift+Key combination.
    /// </summary>
    public void CtrlShiftKey(Interactive target, Key key)
    {
        CtrlDown(target);
        ShiftDown(target);
        KeyPress(target, key, modifiers: KeyModifiers.Control | KeyModifiers.Shift);
        ShiftUp(target);
        CtrlUp(target);
    }

    /// <summary>
    /// Simulates Alt+Key combination.
    /// </summary>
    public void AltKey(Interactive target, Key key)
    {
        AltDown(target);
        KeyPress(target, key, modifiers: KeyModifiers.Alt);
        AltUp(target);
    }

    /// <summary>Simulates Ctrl+C (Copy).</summary>
    public void Copy(Interactive target) => CtrlKey(target, Key.C);

    /// <summary>Simulates Ctrl+V (Paste).</summary>
    public void Paste(Interactive target) => CtrlKey(target, Key.V);

    /// <summary>Simulates Ctrl+X (Cut).</summary>
    public void Cut(Interactive target) => CtrlKey(target, Key.X);

    /// <summary>Simulates Ctrl+Z (Undo).</summary>
    public void Undo(Interactive target) => CtrlKey(target, Key.Z);

    /// <summary>Simulates Ctrl+Shift+Z (Redo).</summary>
    public void Redo(Interactive target) => CtrlShiftKey(target, Key.Z);

    /// <summary>Simulates Ctrl+A (Select All).</summary>
    public void SelectAll(Interactive target) => CtrlKey(target, Key.A);

    /// <summary>Simulates Ctrl+S (Save).</summary>
    public void Save(Interactive target) => CtrlKey(target, Key.S);

    #endregion

    #region Navigation Keys

    /// <summary>Simulates Arrow Up key.</summary>
    public void ArrowUp(Interactive target) => KeyPress(target, Key.Up);

    /// <summary>Simulates Arrow Down key.</summary>
    public void ArrowDown(Interactive target) => KeyPress(target, Key.Down);

    /// <summary>Simulates Arrow Left key.</summary>
    public void ArrowLeft(Interactive target) => KeyPress(target, Key.Left);

    /// <summary>Simulates Arrow Right key.</summary>
    public void ArrowRight(Interactive target) => KeyPress(target, Key.Right);

    /// <summary>
    /// Simulates Tab key.
    /// </summary>
    public void Tab(Interactive target) => KeyPress(target, Key.Tab);

    /// <summary>
    /// Simulates Shift+Tab (reverse tab).
    /// </summary>
    public void ShiftTab(Interactive target)
    {
        ShiftDown(target);
        KeyPress(target, Key.Tab, modifiers: KeyModifiers.Shift);
        ShiftUp(target);
    }

    /// <summary>
    /// Simulates Enter key.
    /// </summary>
    public void Enter(Interactive target) => KeyPress(target, Key.Enter);

    /// <summary>
    /// Simulates Escape key.
    /// </summary>
    public void Escape(Interactive target) => KeyPress(target, Key.Escape);

    /// <summary>
    /// Simulates Space key.
    /// </summary>
    public void Space(Interactive target) => KeyPress(target, Key.Space);

    /// <summary>
    /// Simulates Backspace key.
    /// </summary>
    public void Backspace(Interactive target) => KeyPress(target, Key.Back);

    /// <summary>
    /// Simulates Delete key.
    /// </summary>
    public void Delete(Interactive target) => KeyPress(target, Key.Delete);

    /// <summary>
    /// Simulates Home key.
    /// </summary>
    public void Home(Interactive target) => KeyPress(target, Key.Home);

    /// <summary>
    /// Simulates End key.
    /// </summary>
    public void End(Interactive target) => KeyPress(target, Key.End);

    /// <summary>
    /// Simulates Page Up key.
    /// </summary>
    public void PageUp(Interactive target) => KeyPress(target, Key.PageUp);

    /// <summary>
    /// Simulates Page Down key.
    /// </summary>
    public void PageDown(Interactive target) => KeyPress(target, Key.PageDown);

    #endregion

    #region Function Keys

    /// <summary>
    /// Simulates a function key press (F1-F12).
    /// </summary>
    public void FunctionKey(Interactive target, int number)
    {
        if (number < 1 || number > 12)
            throw new ArgumentOutOfRangeException(nameof(number), "Function key number must be between 1 and 12.");

        var key = (Key)((int)Key.F1 + number - 1);
        KeyPress(target, key);
    }

    #endregion

    #region Helper Methods

    private void UpdateModifiersForKey(Key key, bool isPressed)
    {
        KeyModifiers modifier = key switch
        {
            Key.LeftCtrl or Key.RightCtrl => KeyModifiers.Control,
            Key.LeftShift or Key.RightShift => KeyModifiers.Shift,
            Key.LeftAlt or Key.RightAlt => KeyModifiers.Alt,
            Key.LWin or Key.RWin => KeyModifiers.Meta,
            _ => KeyModifiers.None
        };

        if (modifier != KeyModifiers.None)
        {
            if (isPressed)
                _currentModifiers |= modifier;
            else
                _currentModifiers &= ~modifier;
        }
    }

    private static Key CharToKey(char c)
    {
        // Handle letters
        if (char.IsLetter(c))
        {
            var upper = char.ToUpperInvariant(c);
            return (Key)((int)Key.A + (upper - 'A'));
        }

        // Handle digits
        if (char.IsDigit(c))
        {
            return (Key)((int)Key.D0 + (c - '0'));
        }

        // Handle special characters
        return c switch
        {
            ' ' => Key.Space,
            '\t' => Key.Tab,
            '\n' or '\r' => Key.Enter,
            '.' => Key.OemPeriod,
            ',' => Key.OemComma,
            ';' => Key.OemSemicolon,
            ':' => Key.OemSemicolon, // Shift+;
            '\'' => Key.OemQuotes,
            '"' => Key.OemQuotes, // Shift+'
            '/' => Key.OemQuestion,
            '?' => Key.OemQuestion, // Shift+/
            '\\' => Key.OemBackslash,
            '|' => Key.OemBackslash, // Shift+\
            '[' => Key.OemOpenBrackets,
            ']' => Key.OemCloseBrackets,
            '{' => Key.OemOpenBrackets, // Shift+[
            '}' => Key.OemCloseBrackets, // Shift+]
            '-' => Key.OemMinus,
            '_' => Key.OemMinus, // Shift+-
            '=' => Key.OemPlus,
            '+' => Key.OemPlus, // Shift+=
            '`' => Key.OemTilde,
            '~' => Key.OemTilde, // Shift+`
            '!' => Key.D1, // Shift+1
            '@' => Key.D2, // Shift+2
            '#' => Key.D3, // Shift+3
            '$' => Key.D4, // Shift+4
            '%' => Key.D5, // Shift+5
            '^' => Key.D6, // Shift+6
            '&' => Key.D7, // Shift+7
            '*' => Key.D8, // Shift+8
            '(' => Key.D9, // Shift+9
            ')' => Key.D0, // Shift+0
            _ => Key.None
        };
    }

    private static bool IsShiftedChar(char c)
    {
        return c switch
        {
            '!' or '@' or '#' or '$' or '%' or '^' or '&' or '*' or '(' or ')' => true,
            ':' or '"' or '?' or '|' or '{' or '}' or '_' or '+' or '~' or '<' or '>' => true,
            _ => false
        };
    }

    #endregion
}

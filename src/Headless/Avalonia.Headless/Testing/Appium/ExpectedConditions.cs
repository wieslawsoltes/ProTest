// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace Avalonia.HeadlessTestingFramework.Appium;

/// <summary>
/// Provides expected conditions for use with DriverWait.Until().
/// Similar to Selenium/Appium ExpectedConditions.
/// </summary>
/// <example>
/// <code>
/// // Wait for element to be visible
/// driver.Wait.Until(ExpectedConditions.ElementIsVisible(By.Id("loading")));
///
/// // Wait for element to be clickable
/// var button = driver.Wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("submit")));
///
/// // Wait for text to appear
/// driver.Wait.Until(ExpectedConditions.TextToBePresentInElement(By.Id("result"), "Success"));
/// </code>
/// </example>
public static class ExpectedConditions
{
    #region Element Presence

    /// <summary>
    /// An expectation for checking that an element is present in the visual tree.
    /// </summary>
    /// <param name="locator">The locator to find the element.</param>
    /// <returns>The element when found.</returns>
    public static Func<AvaloniaDriver, AvaloniaElement> ElementExists(By locator)
    {
        return driver =>
        {
            try
            {
                return driver.FindElement(locator);
            }
            catch (NoSuchElementException)
            {
                return null!;
            }
        };
    }

    /// <summary>
    /// An expectation for checking that an element is present and visible.
    /// </summary>
    /// <param name="locator">The locator to find the element.</param>
    /// <returns>The element when visible.</returns>
    public static Func<AvaloniaDriver, AvaloniaElement> ElementIsVisible(By locator)
    {
        return driver =>
        {
            try
            {
                var element = driver.FindElement(locator);
                return element.Displayed ? element : null!;
            }
            catch (NoSuchElementException)
            {
                return null!;
            }
        };
    }

    /// <summary>
    /// An expectation for checking that an element is present, visible, and enabled.
    /// </summary>
    /// <param name="locator">The locator to find the element.</param>
    /// <returns>The element when clickable.</returns>
    public static Func<AvaloniaDriver, AvaloniaElement> ElementToBeClickable(By locator)
    {
        return driver =>
        {
            try
            {
                var element = driver.FindElement(locator);
                return element.Displayed && element.Enabled ? element : null!;
            }
            catch (NoSuchElementException)
            {
                return null!;
            }
        };
    }

    /// <summary>
    /// An expectation for checking that an element is invisible or not present.
    /// </summary>
    /// <param name="locator">The locator to find the element.</param>
    /// <returns>True when the element is invisible or not present.</returns>
    public static Func<AvaloniaDriver, bool> InvisibilityOfElement(By locator)
    {
        return driver =>
        {
            try
            {
                var element = driver.TryFindElement(locator);
                return element == null || !element.Displayed;
            }
            catch (NoSuchElementException)
            {
                return true;
            }
        };
    }

    /// <summary>
    /// An expectation for checking that an element is invisible or not present.
    /// </summary>
    /// <param name="element">The element to check.</param>
    /// <returns>True when the element is invisible.</returns>
    public static Func<AvaloniaDriver, bool> InvisibilityOf(AvaloniaElement element)
    {
        return driver => !element.Displayed;
    }

    /// <summary>
    /// An expectation for checking that all elements matching the locator are visible.
    /// </summary>
    /// <param name="locator">The locator to find elements.</param>
    /// <returns>The list of visible elements.</returns>
    public static Func<AvaloniaDriver, IReadOnlyList<AvaloniaElement>> VisibilityOfAllElements(By locator)
    {
        return driver =>
        {
            var elements = driver.FindElements(locator);
            if (elements.Count == 0)
                return null!;

            return elements.All(e => e.Displayed) ? elements : null!;
        };
    }

    /// <summary>
    /// An expectation for checking that at least one element is present.
    /// </summary>
    /// <param name="locator">The locator to find elements.</param>
    /// <returns>The list of elements when at least one is found.</returns>
    public static Func<AvaloniaDriver, IReadOnlyList<AvaloniaElement>> PresenceOfAllElements(By locator)
    {
        return driver =>
        {
            var elements = driver.FindElements(locator);
            return elements.Count > 0 ? elements : null!;
        };
    }

    #endregion

    #region Element State

    /// <summary>
    /// An expectation for checking that an element is enabled.
    /// </summary>
    /// <param name="locator">The locator to find the element.</param>
    /// <returns>True when the element is enabled.</returns>
    public static Func<AvaloniaDriver, bool> ElementToBeEnabled(By locator)
    {
        return driver =>
        {
            try
            {
                var element = driver.FindElement(locator);
                return element.Enabled;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        };
    }

    /// <summary>
    /// An expectation for checking that an element is disabled.
    /// </summary>
    /// <param name="locator">The locator to find the element.</param>
    /// <returns>True when the element is disabled.</returns>
    public static Func<AvaloniaDriver, bool> ElementToBeDisabled(By locator)
    {
        return driver =>
        {
            try
            {
                var element = driver.FindElement(locator);
                return !element.Enabled;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        };
    }

    /// <summary>
    /// An expectation for checking that an element is selected.
    /// </summary>
    /// <param name="locator">The locator to find the element.</param>
    /// <returns>True when the element is selected.</returns>
    public static Func<AvaloniaDriver, bool> ElementToBeSelected(By locator)
    {
        return driver =>
        {
            try
            {
                var element = driver.FindElement(locator);
                return element.Selected;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        };
    }

    /// <summary>
    /// An expectation for checking that an element's selection state matches the expected state.
    /// </summary>
    /// <param name="locator">The locator to find the element.</param>
    /// <param name="selected">Expected selection state.</param>
    /// <returns>True when the element's selection matches.</returns>
    public static Func<AvaloniaDriver, bool> ElementSelectionStateToBe(By locator, bool selected)
    {
        return driver =>
        {
            try
            {
                var element = driver.FindElement(locator);
                return element.Selected == selected;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        };
    }

    /// <summary>
    /// An expectation for checking that an element has focus.
    /// </summary>
    /// <param name="locator">The locator to find the element.</param>
    /// <returns>True when the element is focused.</returns>
    public static Func<AvaloniaDriver, bool> ElementToBeFocused(By locator)
    {
        return driver =>
        {
            try
            {
                var element = driver.FindElement(locator);
                return element.Focused;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        };
    }

    #endregion

    #region Text Conditions

    /// <summary>
    /// An expectation for checking that an element's text matches exactly.
    /// </summary>
    /// <param name="locator">The locator to find the element.</param>
    /// <param name="text">The expected text.</param>
    /// <returns>True when the text matches.</returns>
    public static Func<AvaloniaDriver, bool> TextToBe(By locator, string text)
    {
        return driver =>
        {
            try
            {
                var element = driver.FindElement(locator);
                return element.Text == text;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        };
    }

    /// <summary>
    /// An expectation for checking that an element's text contains the specified text.
    /// </summary>
    /// <param name="locator">The locator to find the element.</param>
    /// <param name="text">The text to search for.</param>
    /// <returns>True when the text is present.</returns>
    public static Func<AvaloniaDriver, bool> TextToBePresentInElement(By locator, string text)
    {
        return driver =>
        {
            try
            {
                var element = driver.FindElement(locator);
                return element.Text?.Contains(text) == true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        };
    }

    /// <summary>
    /// An expectation for checking that an element's text matches a regex pattern.
    /// </summary>
    /// <param name="locator">The locator to find the element.</param>
    /// <param name="pattern">The regex pattern.</param>
    /// <returns>True when the text matches the pattern.</returns>
    public static Func<AvaloniaDriver, bool> TextToMatch(By locator, string pattern)
    {
        var regex = new Regex(pattern);
        return driver =>
        {
            try
            {
                var element = driver.FindElement(locator);
                return element.Text != null && regex.IsMatch(element.Text);
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        };
    }

    /// <summary>
    /// An expectation for checking that an element's text is not empty.
    /// </summary>
    /// <param name="locator">The locator to find the element.</param>
    /// <returns>True when the text is not empty.</returns>
    public static Func<AvaloniaDriver, bool> TextToBeNotEmpty(By locator)
    {
        return driver =>
        {
            try
            {
                var element = driver.FindElement(locator);
                return !string.IsNullOrEmpty(element.Text);
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        };
    }

    #endregion

    #region Attribute Conditions

    /// <summary>
    /// An expectation for checking that an element's attribute matches the expected value.
    /// </summary>
    /// <param name="locator">The locator to find the element.</param>
    /// <param name="attribute">The attribute name.</param>
    /// <param name="value">The expected value.</param>
    /// <returns>True when the attribute matches.</returns>
    public static Func<AvaloniaDriver, bool> AttributeToBe(By locator, string attribute, string value)
    {
        return driver =>
        {
            try
            {
                var element = driver.FindElement(locator);
                return element.GetAttribute(attribute) == value;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        };
    }

    /// <summary>
    /// An expectation for checking that an element's attribute contains the specified value.
    /// </summary>
    /// <param name="locator">The locator to find the element.</param>
    /// <param name="attribute">The attribute name.</param>
    /// <param name="value">The value to search for.</param>
    /// <returns>True when the attribute contains the value.</returns>
    public static Func<AvaloniaDriver, bool> AttributeContains(By locator, string attribute, string value)
    {
        return driver =>
        {
            try
            {
                var element = driver.FindElement(locator);
                return element.GetAttribute(attribute)?.Contains(value) == true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        };
    }

    /// <summary>
    /// An expectation for checking that an element's attribute is not empty.
    /// </summary>
    /// <param name="locator">The locator to find the element.</param>
    /// <param name="attribute">The attribute name.</param>
    /// <returns>True when the attribute is not empty.</returns>
    public static Func<AvaloniaDriver, bool> AttributeToBeNotEmpty(By locator, string attribute)
    {
        return driver =>
        {
            try
            {
                var element = driver.FindElement(locator);
                return !string.IsNullOrEmpty(element.GetAttribute(attribute));
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        };
    }

    #endregion

    #region Element Count

    /// <summary>
    /// An expectation for checking that the number of elements matches the expected count.
    /// </summary>
    /// <param name="locator">The locator to find elements.</param>
    /// <param name="count">The expected count.</param>
    /// <returns>The elements when count matches.</returns>
    public static Func<AvaloniaDriver, IReadOnlyList<AvaloniaElement>> NumberOfElementsToBe(By locator, int count)
    {
        return driver =>
        {
            var elements = driver.FindElements(locator);
            return elements.Count == count ? elements : null!;
        };
    }

    /// <summary>
    /// An expectation for checking that the number of elements is at least the expected count.
    /// </summary>
    /// <param name="locator">The locator to find elements.</param>
    /// <param name="count">The minimum count.</param>
    /// <returns>The elements when count is at least the expected.</returns>
    public static Func<AvaloniaDriver, IReadOnlyList<AvaloniaElement>> NumberOfElementsToBeMoreThan(By locator, int count)
    {
        return driver =>
        {
            var elements = driver.FindElements(locator);
            return elements.Count > count ? elements : null!;
        };
    }

    /// <summary>
    /// An expectation for checking that the number of elements is at most the expected count.
    /// </summary>
    /// <param name="locator">The locator to find elements.</param>
    /// <param name="count">The maximum count.</param>
    /// <returns>The elements when count is at most the expected.</returns>
    public static Func<AvaloniaDriver, IReadOnlyList<AvaloniaElement>> NumberOfElementsToBeLessThan(By locator, int count)
    {
        return driver =>
        {
            var elements = driver.FindElements(locator);
            return elements.Count < count ? elements : null!;
        };
    }

    #endregion

    #region Window Conditions

    /// <summary>
    /// An expectation for checking the window title.
    /// </summary>
    /// <param name="title">The expected title.</param>
    /// <returns>True when the title matches.</returns>
    public static Func<AvaloniaDriver, bool> TitleIs(string title)
    {
        return driver => driver.Title == title;
    }

    /// <summary>
    /// An expectation for checking that the window title contains text.
    /// </summary>
    /// <param name="text">The text to search for.</param>
    /// <returns>True when the title contains the text.</returns>
    public static Func<AvaloniaDriver, bool> TitleContains(string text)
    {
        return driver => driver.Title?.Contains(text) == true;
    }

    #endregion

    #region Logical Conditions

    /// <summary>
    /// Combines conditions with AND logic.
    /// </summary>
    /// <param name="conditions">The conditions to combine.</param>
    /// <returns>True when all conditions are met.</returns>
    public static Func<AvaloniaDriver, bool> And(params Func<AvaloniaDriver, bool>[] conditions)
    {
        return driver => conditions.All(c => c(driver));
    }

    /// <summary>
    /// Combines conditions with OR logic.
    /// </summary>
    /// <param name="conditions">The conditions to combine.</param>
    /// <returns>True when any condition is met.</returns>
    public static Func<AvaloniaDriver, bool> Or(params Func<AvaloniaDriver, bool>[] conditions)
    {
        return driver => conditions.Any(c => c(driver));
    }

    /// <summary>
    /// Negates a condition.
    /// </summary>
    /// <param name="condition">The condition to negate.</param>
    /// <returns>True when the condition is not met.</returns>
    public static Func<AvaloniaDriver, bool> Not(Func<AvaloniaDriver, bool> condition)
    {
        return driver => !condition(driver);
    }

    #endregion

    #region Custom Conditions

    /// <summary>
    /// Creates a custom condition that waits for a specific state.
    /// </summary>
    /// <param name="condition">The condition function.</param>
    /// <param name="description">Description for error messages.</param>
    /// <returns>True when the condition is met.</returns>
    public static Func<AvaloniaDriver, bool> CustomCondition(Func<AvaloniaDriver, bool> condition, string description = "Custom condition")
    {
        return driver =>
        {
            try
            {
                return condition(driver);
            }
            catch
            {
                return false;
            }
        };
    }

    /// <summary>
    /// Creates a custom condition that returns an element.
    /// </summary>
    /// <param name="condition">The condition function.</param>
    /// <returns>The element when found.</returns>
    public static Func<AvaloniaDriver, AvaloniaElement> CustomElementCondition(Func<AvaloniaDriver, AvaloniaElement?> condition)
    {
        return driver =>
        {
            try
            {
                return condition(driver)!;
            }
            catch
            {
                return null!;
            }
        };
    }

    #endregion

    #region Staleness

    /// <summary>
    /// An expectation for checking that an element is no longer attached to the visual tree.
    /// </summary>
    /// <param name="element">The element to check.</param>
    /// <returns>True when the element is stale.</returns>
    public static Func<AvaloniaDriver, bool> StalenessOf(AvaloniaElement element)
    {
        return driver =>
        {
            try
            {
                // Check if element is still attached
                _ = element.Control.Parent;
                return element.Control.GetPresentationSource() == null;
            }
            catch
            {
                return true;
            }
        };
    }

    /// <summary>
    /// An expectation for checking that an element becomes stale and is replaced by a new one.
    /// </summary>
    /// <param name="locator">The locator to find the new element.</param>
    /// <param name="oldElement">The old element that should become stale.</param>
    /// <returns>The new element.</returns>
    public static Func<AvaloniaDriver, AvaloniaElement> RefreshedElement(By locator, AvaloniaElement oldElement)
    {
        return driver =>
        {
            try
            {
                var newElement = driver.FindElement(locator);
                return newElement.Control != oldElement.Control ? newElement : null!;
            }
            catch (NoSuchElementException)
            {
                return null!;
            }
        };
    }

    #endregion
}

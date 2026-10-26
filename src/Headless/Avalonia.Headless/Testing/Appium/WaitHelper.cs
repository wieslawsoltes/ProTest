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
/// Provides utilities for waiting on specific conditions.
/// More advanced than ExpectedConditions with support for custom predicates.
/// </summary>
/// <example>
/// <code>
/// var wait = new WaitHelper(TimeSpan.FromSeconds(10));
///
/// // Wait for element to be visible
/// await wait.UntilAsync(() => element.IsVisible);
///
/// // Wait for element with custom polling
/// var found = await wait.UntilNotNullAsync(() => FindElement("button"));
///
/// // Wait with progress callback
/// await wait.UntilAsync(() => IsComplete(),
///     onProgress: remaining => Console.WriteLine($"Time remaining: {remaining}"));
/// </code>
/// </example>
public class WaitHelper
{
    private readonly TimeSpan _timeout;
    private readonly TimeSpan _pollingInterval;

    /// <summary>
    /// Creates a new WaitHelper with default timeout (30 seconds).
    /// </summary>
    public WaitHelper() : this(TimeSpan.FromSeconds(30))
    {
    }

    /// <summary>
    /// Creates a new WaitHelper with specified timeout.
    /// </summary>
    /// <param name="timeout">The maximum wait time.</param>
    /// <param name="pollingInterval">The interval between condition checks (default 100ms).</param>
    public WaitHelper(TimeSpan timeout, TimeSpan? pollingInterval = null)
    {
        _timeout = timeout;
        _pollingInterval = pollingInterval ?? TimeSpan.FromMilliseconds(100);
    }

    #region Basic Wait Methods

    /// <summary>
    /// Waits until a condition is true.
    /// </summary>
    /// <param name="condition">The condition to wait for.</param>
    /// <param name="message">Optional message for timeout exception.</param>
    /// <param name="onProgress">Optional callback with remaining time.</param>
    /// <returns>True when condition is met.</returns>
    /// <exception cref="TimeoutException">Thrown if timeout expires.</exception>
    public async Task<bool> UntilAsync(Func<bool> condition, string? message = null, Action<TimeSpan>? onProgress = null)
    {
        if (condition == null)
            throw new ArgumentNullException(nameof(condition));

        var startTime = DateTime.UtcNow;
        var endTime = startTime + _timeout;

        while (DateTime.UtcNow < endTime)
        {
            try
            {
                if (condition())
                    return true;
            }
            catch
            {
                // Ignore exceptions during condition check
            }

            onProgress?.Invoke(endTime - DateTime.UtcNow);
            await Task.Delay(_pollingInterval);
        }

        throw new TimeoutException(message ?? $"Condition not met within {_timeout.TotalSeconds} seconds");
    }

    /// <summary>
    /// Waits until an async condition is true.
    /// </summary>
    /// <param name="condition">The async condition to wait for.</param>
    /// <param name="message">Optional message for timeout exception.</param>
    /// <returns>True when condition is met.</returns>
    public async Task<bool> UntilAsync(Func<Task<bool>> condition, string? message = null)
    {
        if (condition == null)
            throw new ArgumentNullException(nameof(condition));

        var startTime = DateTime.UtcNow;
        var endTime = startTime + _timeout;

        while (DateTime.UtcNow < endTime)
        {
            try
            {
                if (await condition())
                    return true;
            }
            catch
            {
                // Ignore exceptions during condition check
            }

            await Task.Delay(_pollingInterval);
        }

        throw new TimeoutException(message ?? $"Condition not met within {_timeout.TotalSeconds} seconds");
    }

    /// <summary>
    /// Waits until a condition is true (runs on UI thread).
    /// </summary>
    /// <param name="condition">The condition to check on UI thread.</param>
    /// <param name="message">Optional message for timeout exception.</param>
    /// <returns>True when condition is met.</returns>
    public async Task<bool> UntilOnUIThreadAsync(Func<bool> condition, string? message = null)
    {
        return await UntilAsync(async () =>
            await Dispatcher.UIThread.InvokeAsync(condition), message);
    }

    /// <summary>
    /// Waits until a function returns a non-null value.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="func">The function to evaluate.</param>
    /// <param name="message">Optional message for timeout exception.</param>
    /// <returns>The non-null result.</returns>
    public async Task<T> UntilNotNullAsync<T>(Func<T?> func, string? message = null) where T : class
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        var startTime = DateTime.UtcNow;
        var endTime = startTime + _timeout;

        while (DateTime.UtcNow < endTime)
        {
            try
            {
                var result = func();
                if (result != null)
                    return result;
            }
            catch
            {
                // Ignore exceptions during evaluation
            }

            await Task.Delay(_pollingInterval);
        }

        throw new TimeoutException(message ?? $"Function did not return non-null within {_timeout.TotalSeconds} seconds");
    }

    /// <summary>
    /// Waits until a function returns a non-null value (async).
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="func">The async function to evaluate.</param>
    /// <param name="message">Optional message for timeout exception.</param>
    /// <returns>The non-null result.</returns>
    public async Task<T> UntilNotNullAsync<T>(Func<Task<T?>> func, string? message = null) where T : class
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        var startTime = DateTime.UtcNow;
        var endTime = startTime + _timeout;

        while (DateTime.UtcNow < endTime)
        {
            try
            {
                var result = await func();
                if (result != null)
                    return result;
            }
            catch
            {
                // Ignore exceptions during evaluation
            }

            await Task.Delay(_pollingInterval);
        }

        throw new TimeoutException(message ?? $"Function did not return non-null within {_timeout.TotalSeconds} seconds");
    }

    #endregion

    #region Element Wait Methods

    /// <summary>
    /// Waits for an element to become visible.
    /// </summary>
    /// <param name="element">The element to wait for.</param>
    /// <param name="message">Optional message for timeout exception.</param>
    public async Task ForVisibleAsync(Control element, string? message = null)
    {
        await UntilOnUIThreadAsync(() => element.IsVisible,
            message ?? $"Element {element.GetType().Name} did not become visible");
    }

    /// <summary>
    /// Waits for an element to become hidden.
    /// </summary>
    /// <param name="element">The element to wait for.</param>
    /// <param name="message">Optional message for timeout exception.</param>
    public async Task ForHiddenAsync(Control element, string? message = null)
    {
        await UntilOnUIThreadAsync(() => !element.IsVisible,
            message ?? $"Element {element.GetType().Name} did not become hidden");
    }

    /// <summary>
    /// Waits for an element to become enabled.
    /// </summary>
    /// <param name="element">The element to wait for.</param>
    /// <param name="message">Optional message for timeout exception.</param>
    public async Task ForEnabledAsync(Control element, string? message = null)
    {
        await UntilOnUIThreadAsync(() => element.IsEnabled,
            message ?? $"Element {element.GetType().Name} did not become enabled");
    }

    /// <summary>
    /// Waits for an element to become disabled.
    /// </summary>
    /// <param name="element">The element to wait for.</param>
    /// <param name="message">Optional message for timeout exception.</param>
    public async Task ForDisabledAsync(Control element, string? message = null)
    {
        await UntilOnUIThreadAsync(() => !element.IsEnabled,
            message ?? $"Element {element.GetType().Name} did not become disabled");
    }

    /// <summary>
    /// Waits for an element to receive focus.
    /// </summary>
    /// <param name="element">The element to wait for.</param>
    /// <param name="message">Optional message for timeout exception.</param>
    public async Task ForFocusedAsync(Control element, string? message = null)
    {
        await UntilOnUIThreadAsync(() => element.IsFocused,
            message ?? $"Element {element.GetType().Name} did not receive focus");
    }

    /// <summary>
    /// Waits for an element's property to have a specific value.
    /// </summary>
    /// <typeparam name="T">The property value type.</typeparam>
    /// <param name="element">The element.</param>
    /// <param name="property">The property to check.</param>
    /// <param name="expectedValue">The expected value.</param>
    /// <param name="message">Optional message for timeout exception.</param>
    public async Task ForPropertyValueAsync<T>(AvaloniaObject element, AvaloniaProperty<T> property, T expectedValue, string? message = null)
    {
        await UntilOnUIThreadAsync(() =>
        {
            var actual = element.GetValue(property);
            if (actual == null && expectedValue == null) return true;
            if (actual == null || expectedValue == null) return false;
            return actual.Equals(expectedValue);
        },
            message ?? $"Property {property.Name} did not reach expected value {expectedValue}");
    }

    /// <summary>
    /// Waits for an element to exist (be found).
    /// </summary>
    /// <param name="findFunc">Function to find the element.</param>
    /// <param name="message">Optional message for timeout exception.</param>
    /// <returns>The found element.</returns>
    public async Task<Control> ForElementAsync(Func<Control?> findFunc, string? message = null)
    {
        return await UntilNotNullAsync(findFunc, message ?? "Element was not found");
    }

    #endregion

    #region Text Wait Methods

    /// <summary>
    /// Waits for text content to match.
    /// </summary>
    /// <param name="element">The text element.</param>
    /// <param name="expectedText">The expected text.</param>
    /// <param name="message">Optional message for timeout exception.</param>
    public async Task ForTextAsync(Control element, string expectedText, string? message = null)
    {
        await UntilOnUIThreadAsync(() =>
        {
            var text = GetElementText(element);
            return text == expectedText;
        }, message ?? $"Text did not become '{expectedText}'");
    }

    /// <summary>
    /// Waits for text content to contain a substring.
    /// </summary>
    /// <param name="element">The text element.</param>
    /// <param name="substring">The substring to find.</param>
    /// <param name="message">Optional message for timeout exception.</param>
    public async Task ForTextContainsAsync(Control element, string substring, string? message = null)
    {
        await UntilOnUIThreadAsync(() =>
        {
            var text = GetElementText(element);
            return text?.Contains(substring) ?? false;
        }, message ?? $"Text did not contain '{substring}'");
    }

    /// <summary>
    /// Waits for text to be non-empty.
    /// </summary>
    /// <param name="element">The text element.</param>
    /// <param name="message">Optional message for timeout exception.</param>
    public async Task ForTextNotEmptyAsync(Control element, string? message = null)
    {
        await UntilOnUIThreadAsync(() =>
        {
            var text = GetElementText(element);
            return !string.IsNullOrEmpty(text);
        }, message ?? "Text remained empty");
    }

    #endregion

    #region Count Wait Methods

    /// <summary>
    /// Waits for a collection to have a specific count.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="getCollection">Function to get the collection.</param>
    /// <param name="expectedCount">The expected count.</param>
    /// <param name="message">Optional message for timeout exception.</param>
    public async Task ForCountAsync<T>(Func<IEnumerable<T>> getCollection, int expectedCount, string? message = null)
    {
        await UntilAsync(() =>
        {
            var count = 0;
            foreach (var _ in getCollection())
                count++;
            return count == expectedCount;
        }, message ?? $"Collection count did not reach {expectedCount}");
    }

    /// <summary>
    /// Waits for a collection to have at least a certain count.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="getCollection">Function to get the collection.</param>
    /// <param name="minimumCount">The minimum required count.</param>
    /// <param name="message">Optional message for timeout exception.</param>
    public async Task ForMinimumCountAsync<T>(Func<IEnumerable<T>> getCollection, int minimumCount, string? message = null)
    {
        await UntilAsync(() =>
        {
            var count = 0;
            foreach (var _ in getCollection())
            {
                count++;
                if (count >= minimumCount)
                    return true;
            }
            return false;
        }, message ?? $"Collection count did not reach minimum of {minimumCount}");
    }

    #endregion

    #region Try Wait Methods (Non-throwing)

    /// <summary>
    /// Tries to wait for a condition, returns false on timeout.
    /// </summary>
    /// <param name="condition">The condition to wait for.</param>
    /// <returns>True if condition was met, false on timeout.</returns>
    public async Task<bool> TryUntilAsync(Func<bool> condition)
    {
        try
        {
            await UntilAsync(condition);
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    /// <summary>
    /// Tries to wait for an element, returns null on timeout.
    /// </summary>
    /// <param name="findFunc">Function to find the element.</param>
    /// <returns>The element, or null if not found within timeout.</returns>
    public async Task<Control?> TryForElementAsync(Func<Control?> findFunc)
    {
        try
        {
            return await ForElementAsync(findFunc);
        }
        catch (TimeoutException)
        {
            return null;
        }
    }

    #endregion

    #region Fluent Builder

    /// <summary>
    /// Creates a fluent wait builder.
    /// </summary>
    /// <param name="timeout">The timeout duration.</param>
    /// <returns>A new WaitBuilder.</returns>
    public static WaitBuilder With(TimeSpan timeout) => new WaitBuilder().WithTimeout(timeout);

    #endregion

    #region Private Methods

    private static string? GetElementText(Control element)
    {
        if (element is TextBlock textBlock)
            return textBlock.Text;
        if (element is TextBox textBox)
            return textBox.Text;
        if (element is ContentControl contentControl)
            return contentControl.Content?.ToString();
        return null;
    }

    #endregion
}

/// <summary>
/// Fluent builder for WaitHelper configuration.
/// </summary>
public class WaitBuilder
{
    private TimeSpan _timeout = TimeSpan.FromSeconds(30);
    private TimeSpan _pollingInterval = TimeSpan.FromMilliseconds(100);
    private List<Type> _ignoredExceptions = new();
    private string? _message;

    /// <summary>
    /// Sets the timeout duration.
    /// </summary>
    /// <param name="timeout">The timeout.</param>
    /// <returns>This builder.</returns>
    public WaitBuilder WithTimeout(TimeSpan timeout)
    {
        _timeout = timeout;
        return this;
    }

    /// <summary>
    /// Sets the polling interval.
    /// </summary>
    /// <param name="interval">The polling interval.</param>
    /// <returns>This builder.</returns>
    public WaitBuilder WithPollingInterval(TimeSpan interval)
    {
        _pollingInterval = interval;
        return this;
    }

    /// <summary>
    /// Sets the timeout message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>This builder.</returns>
    public WaitBuilder WithMessage(string message)
    {
        _message = message;
        return this;
    }

    /// <summary>
    /// Adds an exception type to ignore during polling.
    /// </summary>
    /// <typeparam name="T">The exception type.</typeparam>
    /// <returns>This builder.</returns>
    public WaitBuilder IgnoreException<T>() where T : Exception
    {
        _ignoredExceptions.Add(typeof(T));
        return this;
    }

    /// <summary>
    /// Builds the WaitHelper and waits for a condition.
    /// </summary>
    /// <param name="condition">The condition to wait for.</param>
    /// <returns>True when condition is met.</returns>
    public async Task<bool> UntilAsync(Func<bool> condition)
    {
        var wait = new WaitHelper(_timeout, _pollingInterval);
        return await wait.UntilAsync(condition, _message);
    }

    /// <summary>
    /// Builds the WaitHelper and waits for an async condition.
    /// </summary>
    /// <param name="condition">The async condition to wait for.</param>
    /// <returns>True when condition is met.</returns>
    public async Task<bool> UntilAsync(Func<Task<bool>> condition)
    {
        var wait = new WaitHelper(_timeout, _pollingInterval);
        return await wait.UntilAsync(condition, _message);
    }

    /// <summary>
    /// Builds the WaitHelper and waits for a non-null result.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="func">The function to evaluate.</param>
    /// <returns>The non-null result.</returns>
    public async Task<T> UntilNotNullAsync<T>(Func<T?> func) where T : class
    {
        var wait = new WaitHelper(_timeout, _pollingInterval);
        return await wait.UntilNotNullAsync(func, _message);
    }
}

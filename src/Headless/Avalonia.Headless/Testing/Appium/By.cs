// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System;
using System.Text.RegularExpressions;

namespace Avalonia.HeadlessTestingFramework.Appium;

/// <summary>
/// Provides element location strategies similar to Appium/Selenium WebDriver.
/// Use these methods to create locator strategies for finding UI elements.
/// </summary>
public class By
{
    /// <summary>
    /// Gets the locator strategy type.
    /// </summary>
    public LocatorStrategy Strategy { get; }

    /// <summary>
    /// Gets the locator value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets additional options for the locator.
    /// </summary>
    public LocatorOptions Options { get; }

    private By(LocatorStrategy strategy, string value, LocatorOptions? options = null)
    {
        Strategy = strategy;
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Options = options ?? new LocatorOptions();
    }

    /// <summary>
    /// Locates an element by its unique ID (Name property in Avalonia).
    /// </summary>
    /// <param name="id">The ID/Name of the element.</param>
    /// <returns>A By locator for ID matching.</returns>
    /// <example>
    /// <code>
    /// var element = driver.FindElement(By.Id("submitButton"));
    /// </code>
    /// </example>
    public static By Id(string id) => new(LocatorStrategy.Id, id);

    /// <summary>
    /// Locates an element by its Name property.
    /// Alias for Id() in Avalonia context.
    /// </summary>
    /// <param name="name">The Name of the element.</param>
    /// <returns>A By locator for Name matching.</returns>
    public static By Name(string name) => new(LocatorStrategy.Name, name);

    /// <summary>
    /// Locates an element by its AutomationId (for accessibility).
    /// Maps to AutomationProperties.AutomationId in Avalonia.
    /// </summary>
    /// <param name="automationId">The AutomationId of the element.</param>
    /// <returns>A By locator for AutomationId matching.</returns>
    /// <example>
    /// <code>
    /// var element = driver.FindElement(By.AutomationId("login_button"));
    /// </code>
    /// </example>
    public static By AutomationId(string automationId) => new(LocatorStrategy.AutomationId, automationId);

    /// <summary>
    /// Locates elements by their class name (type name).
    /// </summary>
    /// <param name="className">The class/type name of the element.</param>
    /// <returns>A By locator for class name matching.</returns>
    /// <example>
    /// <code>
    /// var buttons = driver.FindElements(By.ClassName("Button"));
    /// </code>
    /// </example>
    public static By ClassName(string className) => new(LocatorStrategy.ClassName, className);

    /// <summary>
    /// Locates elements by their full type name including namespace.
    /// </summary>
    /// <param name="fullClassName">The full type name including namespace.</param>
    /// <returns>A By locator for full class name matching.</returns>
    /// <example>
    /// <code>
    /// var element = driver.FindElement(By.FullClassName("Avalonia.Controls.Button"));
    /// </code>
    /// </example>
    public static By FullClassName(string fullClassName) => new(LocatorStrategy.FullClassName, fullClassName);

    /// <summary>
    /// Locates elements by their type.
    /// </summary>
    /// <typeparam name="T">The type of element to find.</typeparam>
    /// <returns>A By locator for type matching.</returns>
    /// <example>
    /// <code>
    /// var element = driver.FindElement(By.Type&lt;Button&gt;());
    /// </code>
    /// </example>
    public static By Type<T>() => new(LocatorStrategy.Type, typeof(T).FullName ?? typeof(T).Name);

    /// <summary>
    /// Locates elements by their type.
    /// </summary>
    /// <param name="type">The type of element to find.</param>
    /// <returns>A By locator for type matching.</returns>
    public static By Type(Type type) => new(LocatorStrategy.Type, type.FullName ?? type.Name);

    /// <summary>
    /// Locates elements using XPath-like expressions.
    /// Supports custom XPath syntax for Avalonia visual trees.
    /// </summary>
    /// <param name="xpath">The XPath expression.</param>
    /// <returns>A By locator for XPath matching.</returns>
    /// <example>
    /// <code>
    /// var element = driver.FindElement(By.XPath("//Button[@Name='submit']"));
    /// var nested = driver.FindElement(By.XPath("//StackPanel/Button[1]"));
    /// </code>
    /// </example>
    public static By XPath(string xpath) => new(LocatorStrategy.XPath, xpath);

    /// <summary>
    /// Locates elements by their text content.
    /// Searches in common text properties like Text, Content, Header.
    /// </summary>
    /// <param name="text">The text to match.</param>
    /// <param name="exact">If true, requires exact match. If false, uses contains.</param>
    /// <returns>A By locator for text matching.</returns>
    /// <example>
    /// <code>
    /// var element = driver.FindElement(By.Text("Submit"));
    /// var partial = driver.FindElement(By.Text("Sub", exact: false));
    /// </code>
    /// </example>
    public static By Text(string text, bool exact = true) =>
        new(LocatorStrategy.Text, text, new LocatorOptions { ExactMatch = exact });

    /// <summary>
    /// Locates elements containing the specified text.
    /// </summary>
    /// <param name="text">The text to search for.</param>
    /// <returns>A By locator for partial text matching.</returns>
    public static By ContainsText(string text) =>
        new(LocatorStrategy.Text, text, new LocatorOptions { ExactMatch = false });

    /// <summary>
    /// Locates elements by a CSS selector.
    /// In Avalonia context, this maps to StyleClass/Classes matching.
    /// This is the standard Selenium-compatible method name.
    /// </summary>
    /// <param name="cssSelector">The CSS selector (class name in Avalonia).</param>
    /// <returns>A By locator for CSS class matching.</returns>
    /// <remarks>
    /// NOTE: Only class selectors (.className) are supported. Complex CSS selectors
    /// like "div > button" or "input[type='text']" are not supported.
    /// For complex queries, use <see cref="XPath"/> instead.
    /// </remarks>
    /// <example>
    /// <code>
    /// var element = driver.FindElement(By.CssSelector(".primary"));
    /// // Also works without the dot:
    /// var element2 = driver.FindElement(By.CssSelector("primary"));
    /// </code>
    /// </example>
    public static By CssSelector(string cssSelector)
    {
        // Remove leading dot if present (standard CSS class selector syntax)
        var className = cssSelector.StartsWith(".") ? cssSelector.Substring(1) : cssSelector;
        return new(LocatorStrategy.CssClass, className);
    }

    /// <summary>
    /// Locates elements by a CSS-like class selector.
    /// Maps to StyleClass/Classes in Avalonia.
    /// </summary>
    /// <param name="cssClass">The CSS class name.</param>
    /// <returns>A By locator for CSS class matching.</returns>
    /// <example>
    /// <code>
    /// var element = driver.FindElement(By.CssClass("primary"));
    /// </code>
    /// </example>
    public static By CssClass(string cssClass) => new(LocatorStrategy.CssClass, cssClass);

    /// <summary>
    /// Locates elements by a tag name (simplified type name).
    /// </summary>
    /// <param name="tagName">The tag/type name.</param>
    /// <returns>A By locator for tag name matching.</returns>
    public static By TagName(string tagName) => new(LocatorStrategy.TagName, tagName);

    /// <summary>
    /// Locates elements by their link text (for hyperlinks/buttons).
    /// This is the standard Selenium locator - maps to Text matching in Avalonia.
    /// </summary>
    /// <param name="linkText">The exact link text.</param>
    /// <returns>A By locator for link text matching.</returns>
    /// <example>
    /// <code>
    /// var link = driver.FindElement(By.LinkText("Click here"));
    /// </code>
    /// </example>
    public static By LinkText(string linkText) => Text(linkText, exact: true);

    /// <summary>
    /// Locates elements by partial link text (for hyperlinks/buttons).
    /// This is the standard Selenium locator - maps to partial Text matching in Avalonia.
    /// </summary>
    /// <param name="partialLinkText">The partial link text.</param>
    /// <returns>A By locator for partial link text matching.</returns>
    /// <example>
    /// <code>
    /// var link = driver.FindElement(By.PartialLinkText("Click"));
    /// </code>
    /// </example>
    public static By PartialLinkText(string partialLinkText) => ContainsText(partialLinkText);

    /// <summary>
    /// Locates elements by Avalonia property value.
    /// </summary>
    /// <param name="propertyName">The name of the Avalonia property.</param>
    /// <param name="value">The expected value.</param>
    /// <returns>A By locator for property matching.</returns>
    /// <example>
    /// <code>
    /// var element = driver.FindElement(By.Property("IsEnabled", true));
    /// </code>
    /// </example>
    public static By Property(string propertyName, object? value) =>
        new(LocatorStrategy.Property, propertyName, new LocatorOptions { PropertyValue = value });

    /// <summary>
    /// Locates elements by accessibility name.
    /// Maps to AutomationProperties.Name in Avalonia.
    /// </summary>
    /// <param name="accessibilityName">The accessibility name.</param>
    /// <returns>A By locator for accessibility name matching.</returns>
    public static By AccessibilityName(string accessibilityName) =>
        new(LocatorStrategy.AccessibilityName, accessibilityName);

    /// <summary>
    /// Locates elements by accessibility label.
    /// </summary>
    /// <param name="accessibilityLabel">The accessibility label.</param>
    /// <returns>A By locator for accessibility label matching.</returns>
    public static By AccessibilityLabel(string accessibilityLabel) =>
        new(LocatorStrategy.AccessibilityLabel, accessibilityLabel);

    /// <summary>
    /// Locates elements using a custom predicate.
    /// </summary>
    /// <param name="description">Description of the predicate for error messages.</param>
    /// <param name="predicateKey">A unique key identifying this predicate.</param>
    /// <returns>A By locator for custom predicate matching.</returns>
    /// <remarks>
    /// The actual predicate function is passed when calling FindElement.
    /// </remarks>
    public static By Predicate(string description, string predicateKey) =>
        new(LocatorStrategy.Predicate, predicateKey, new LocatorOptions { Description = description });

    /// <summary>
    /// Locates elements by regex pattern on their Name.
    /// </summary>
    /// <param name="pattern">The regex pattern to match.</param>
    /// <returns>A By locator for regex matching.</returns>
    /// <example>
    /// <code>
    /// var elements = driver.FindElements(By.NameRegex("btn_.*"));
    /// </code>
    /// </example>
    public static By NameRegex(string pattern) =>
        new(LocatorStrategy.NameRegex, pattern);

    /// <summary>
    /// Locates the currently focused element.
    /// </summary>
    /// <returns>A By locator for focus matching.</returns>
    public static By Focused() => new(LocatorStrategy.Focused, "focused");

    /// <summary>
    /// Combines multiple locators with AND logic.
    /// </summary>
    /// <param name="locators">The locators to combine.</param>
    /// <returns>A By locator that matches all conditions.</returns>
    /// <example>
    /// <code>
    /// var element = driver.FindElement(By.All(By.ClassName("Button"), By.Text("Submit")));
    /// </code>
    /// </example>
    public static By All(params By[] locators) =>
        new(LocatorStrategy.Composite, "AND",
            new LocatorOptions { CompositeLocators = locators, CompositeMode = CompositeMode.And });

    /// <summary>
    /// Combines multiple locators with OR logic.
    /// </summary>
    /// <param name="locators">The locators to combine.</param>
    /// <returns>A By locator that matches any condition.</returns>
    /// <example>
    /// <code>
    /// var element = driver.FindElement(By.Any(By.Id("btn1"), By.Id("btn2")));
    /// </code>
    /// </example>
    public static By Any(params By[] locators) =>
        new(LocatorStrategy.Composite, "OR",
            new LocatorOptions { CompositeLocators = locators, CompositeMode = CompositeMode.Or });

    /// <summary>
    /// Creates a chained locator for finding nested elements.
    /// </summary>
    /// <param name="locators">The locators to chain.</param>
    /// <returns>A By locator for chained searching.</returns>
    /// <example>
    /// <code>
    /// var element = driver.FindElement(By.Chained(By.Id("container"), By.ClassName("Button")));
    /// </code>
    /// </example>
    public static By Chained(params By[] locators) =>
        new(LocatorStrategy.Chained, "CHAIN",
            new LocatorOptions { CompositeLocators = locators });

    /// <summary>
    /// Returns a string representation of this locator.
    /// </summary>
    public override string ToString() => $"By.{Strategy}(\"{Value}\")";
}

/// <summary>
/// Defines the available locator strategies.
/// </summary>
public enum LocatorStrategy
{
    /// <summary>Locate by ID/Name.</summary>
    Id,
    /// <summary>Locate by Name property.</summary>
    Name,
    /// <summary>Locate by AutomationId.</summary>
    AutomationId,
    /// <summary>Locate by class/type name.</summary>
    ClassName,
    /// <summary>Locate by full class name with namespace.</summary>
    FullClassName,
    /// <summary>Locate by Type.</summary>
    Type,
    /// <summary>Locate by XPath expression.</summary>
    XPath,
    /// <summary>Locate by text content.</summary>
    Text,
    /// <summary>Locate by CSS class.</summary>
    CssClass,
    /// <summary>Locate by tag name.</summary>
    TagName,
    /// <summary>Locate by property value.</summary>
    Property,
    /// <summary>Locate by accessibility name.</summary>
    AccessibilityName,
    /// <summary>Locate by accessibility label.</summary>
    AccessibilityLabel,
    /// <summary>Locate by custom predicate.</summary>
    Predicate,
    /// <summary>Locate by regex pattern.</summary>
    NameRegex,
    /// <summary>Locate focused element.</summary>
    Focused,
    /// <summary>Composite locator (AND/OR).</summary>
    Composite,
    /// <summary>Chained locator.</summary>
    Chained
}

/// <summary>
/// Additional options for locator strategies.
/// </summary>
public class LocatorOptions
{
    /// <summary>
    /// For text matching, whether to require exact match.
    /// </summary>
    public bool ExactMatch { get; set; } = true;

    /// <summary>
    /// For property matching, the expected value.
    /// </summary>
    public object? PropertyValue { get; set; }

    /// <summary>
    /// Description for custom predicates.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// For composite locators, the child locators.
    /// </summary>
    public By[]? CompositeLocators { get; set; }

    /// <summary>
    /// For composite locators, the combination mode.
    /// </summary>
    public CompositeMode CompositeMode { get; set; }
}

/// <summary>
/// Defines how composite locators combine their results.
/// </summary>
public enum CompositeMode
{
    /// <summary>All locators must match (AND).</summary>
    And,
    /// <summary>Any locator can match (OR).</summary>
    Or
}

// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace Avalonia.HeadlessTestingFramework;

/// <summary>
/// Provides comprehensive validation for visual and logical trees.
/// </summary>
public class TreeValidator
{
    private readonly List<TreeValidationRule> _rules = new();
    private readonly List<TreeValidationResult> _results = new();

    /// <summary>
    /// Gets the validation results.
    /// </summary>
    public IReadOnlyList<TreeValidationResult> Results => _results;

    /// <summary>
    /// Gets whether all validations passed.
    /// </summary>
    public bool IsValid => _results.All(r => r.IsValid);

    /// <summary>
    /// Gets the failed validation results.
    /// </summary>
    public IEnumerable<TreeValidationResult> Failures => _results.Where(r => !r.IsValid);

    /// <summary>
    /// Creates a new tree validator.
    /// </summary>
    public static TreeValidator Create() => new();

    #region Rule Configuration

    /// <summary>
    /// Requires that a control with the specified name exists.
    /// </summary>
    /// <param name="name">The control name.</param>
    /// <returns>This validator for chaining.</returns>
    public TreeValidator RequireName(string name)
    {
        _rules.Add(new TreeValidationRule
        {
            Type = ValidationRuleType.RequireName,
            Name = name,
            Description = $"Control with name '{name}' must exist"
        });
        return this;
    }

    /// <summary>
    /// Requires that controls with all specified names exist.
    /// </summary>
    /// <param name="names">The control names.</param>
    /// <returns>This validator for chaining.</returns>
    public TreeValidator RequireNames(params string[] names)
    {
        foreach (var name in names)
        {
            RequireName(name);
        }
        return this;
    }

    /// <summary>
    /// Requires that at least one control of the specified type exists.
    /// </summary>
    /// <typeparam name="T">The control type.</typeparam>
    /// <returns>This validator for chaining.</returns>
    public TreeValidator RequireType<T>() where T : class
    {
        _rules.Add(new TreeValidationRule
        {
            Type = ValidationRuleType.RequireType,
            ControlType = typeof(T),
            Description = $"At least one {typeof(T).Name} must exist"
        });
        return this;
    }

    /// <summary>
    /// Requires that exactly the specified count of a type exists.
    /// </summary>
    /// <typeparam name="T">The control type.</typeparam>
    /// <param name="count">The exact count required.</param>
    /// <returns>This validator for chaining.</returns>
    public TreeValidator RequireExactCount<T>(int count) where T : class
    {
        _rules.Add(new TreeValidationRule
        {
            Type = ValidationRuleType.RequireExactCount,
            ControlType = typeof(T),
            Count = count,
            Description = $"Exactly {count} {typeof(T).Name} controls must exist"
        });
        return this;
    }

    /// <summary>
    /// Requires that at least the specified count of a type exists.
    /// </summary>
    /// <typeparam name="T">The control type.</typeparam>
    /// <param name="minCount">The minimum count required.</param>
    /// <returns>This validator for chaining.</returns>
    public TreeValidator RequireMinCount<T>(int minCount) where T : class
    {
        _rules.Add(new TreeValidationRule
        {
            Type = ValidationRuleType.RequireMinCount,
            ControlType = typeof(T),
            Count = minCount,
            Description = $"At least {minCount} {typeof(T).Name} controls must exist"
        });
        return this;
    }

    /// <summary>
    /// Requires that no more than the specified count of a type exists.
    /// </summary>
    /// <typeparam name="T">The control type.</typeparam>
    /// <param name="maxCount">The maximum count allowed.</param>
    /// <returns>This validator for chaining.</returns>
    public TreeValidator RequireMaxCount<T>(int maxCount) where T : class
    {
        _rules.Add(new TreeValidationRule
        {
            Type = ValidationRuleType.RequireMaxCount,
            ControlType = typeof(T),
            Count = maxCount,
            Description = $"At most {maxCount} {typeof(T).Name} controls must exist"
        });
        return this;
    }

    /// <summary>
    /// Requires that a specific tree structure pattern exists.
    /// </summary>
    /// <param name="pattern">The XPath pattern to match.</param>
    /// <returns>This validator for chaining.</returns>
    public TreeValidator RequirePattern(string pattern)
    {
        _rules.Add(new TreeValidationRule
        {
            Type = ValidationRuleType.RequirePattern,
            Pattern = pattern,
            Description = $"Pattern '{pattern}' must match at least one node"
        });
        return this;
    }

    /// <summary>
    /// Requires that a named control is of the specified type.
    /// </summary>
    /// <typeparam name="T">The expected type.</typeparam>
    /// <param name="name">The control name.</param>
    /// <returns>This validator for chaining.</returns>
    public TreeValidator RequireNameOfType<T>(string name) where T : class
    {
        _rules.Add(new TreeValidationRule
        {
            Type = ValidationRuleType.RequireNameOfType,
            Name = name,
            ControlType = typeof(T),
            Description = $"Control '{name}' must be of type {typeof(T).Name}"
        });
        return this;
    }

    /// <summary>
    /// Requires that a named control is enabled.
    /// </summary>
    /// <param name="name">The control name.</param>
    /// <returns>This validator for chaining.</returns>
    public TreeValidator RequireEnabled(string name)
    {
        _rules.Add(new TreeValidationRule
        {
            Type = ValidationRuleType.RequireEnabled,
            Name = name,
            Description = $"Control '{name}' must be enabled"
        });
        return this;
    }

    /// <summary>
    /// Requires that a named control is visible.
    /// </summary>
    /// <param name="name">The control name.</param>
    /// <returns>This validator for chaining.</returns>
    public TreeValidator RequireVisible(string name)
    {
        _rules.Add(new TreeValidationRule
        {
            Type = ValidationRuleType.RequireVisible,
            Name = name,
            Description = $"Control '{name}' must be visible"
        });
        return this;
    }

    /// <summary>
    /// Requires that a named control has a specific property value.
    /// </summary>
    /// <typeparam name="TValue">The property value type.</typeparam>
    /// <param name="name">The control name.</param>
    /// <param name="property">The property to check.</param>
    /// <param name="expectedValue">The expected value.</param>
    /// <returns>This validator for chaining.</returns>
    public TreeValidator RequireProperty<TValue>(string name, AvaloniaProperty<TValue> property, TValue expectedValue)
    {
        _rules.Add(new TreeValidationRule
        {
            Type = ValidationRuleType.RequireProperty,
            Name = name,
            Property = property,
            ExpectedValue = expectedValue,
            Description = $"Control '{name}' must have {property.Name} = {expectedValue}"
        });
        return this;
    }

    /// <summary>
    /// Requires that a control is a descendant of another.
    /// </summary>
    /// <param name="childName">The child control name.</param>
    /// <param name="parentName">The parent control name.</param>
    /// <returns>This validator for chaining.</returns>
    public TreeValidator RequireChildOf(string childName, string parentName)
    {
        _rules.Add(new TreeValidationRule
        {
            Type = ValidationRuleType.RequireChildOf,
            Name = childName,
            ParentName = parentName,
            Description = $"Control '{childName}' must be a descendant of '{parentName}'"
        });
        return this;
    }

    /// <summary>
    /// Requires that a control has a specific style class.
    /// </summary>
    /// <param name="name">The control name.</param>
    /// <param name="className">The class name.</param>
    /// <returns>This validator for chaining.</returns>
    public TreeValidator RequireClass(string name, string className)
    {
        _rules.Add(new TreeValidationRule
        {
            Type = ValidationRuleType.RequireClass,
            Name = name,
            ClassName = className,
            Description = $"Control '{name}' must have class '{className}'"
        });
        return this;
    }

    /// <summary>
    /// Forbids a specific pattern from existing.
    /// </summary>
    /// <param name="pattern">The XPath pattern that must not match.</param>
    /// <returns>This validator for chaining.</returns>
    public TreeValidator ForbidPattern(string pattern)
    {
        _rules.Add(new TreeValidationRule
        {
            Type = ValidationRuleType.ForbidPattern,
            Pattern = pattern,
            Description = $"Pattern '{pattern}' must not match any nodes"
        });
        return this;
    }

    /// <summary>
    /// Forbids a specific type from existing.
    /// </summary>
    /// <typeparam name="T">The control type.</typeparam>
    /// <returns>This validator for chaining.</returns>
    public TreeValidator ForbidType<T>() where T : class
    {
        _rules.Add(new TreeValidationRule
        {
            Type = ValidationRuleType.ForbidType,
            ControlType = typeof(T),
            Description = $"{typeof(T).Name} controls must not exist"
        });
        return this;
    }

    /// <summary>
    /// Adds a custom validation rule.
    /// </summary>
    /// <param name="description">The rule description.</param>
    /// <param name="validator">The validation function.</param>
    /// <returns>This validator for chaining.</returns>
    public TreeValidator Custom(string description, Func<Visual, bool> validator)
    {
        _rules.Add(new TreeValidationRule
        {
            Type = ValidationRuleType.Custom,
            Description = description,
            CustomValidator = validator
        });
        return this;
    }

    #endregion

    #region Validation

    /// <summary>
    /// Validates the visual tree against all configured rules.
    /// </summary>
    /// <param name="root">The root visual to validate.</param>
    /// <returns>This validator with results populated.</returns>
    public TreeValidator ValidateVisualTree(Visual root)
    {
        _results.Clear();
        var xpath = new TreeXPath(root);

        foreach (var rule in _rules)
        {
            var result = ValidateRule(rule, root, xpath, useVisualTree: true);
            _results.Add(result);
        }

        return this;
    }

    /// <summary>
    /// Validates the logical tree against all configured rules.
    /// </summary>
    /// <param name="root">The root logical to validate.</param>
    /// <returns>This validator with results populated.</returns>
    public TreeValidator ValidateLogicalTree(ILogical root)
    {
        _results.Clear();
        var xpath = new TreeXPath(root, useVisualTree: false);
        var visualRoot = root as Visual;

        foreach (var rule in _rules)
        {
            var result = ValidateRule(rule, visualRoot, xpath, useVisualTree: false);
            _results.Add(result);
        }

        return this;
    }

    private TreeValidationResult ValidateRule(TreeValidationRule rule, Visual? root, TreeXPath xpath, bool useVisualTree)
    {
        var result = new TreeValidationResult
        {
            RuleDescription = rule.Description
        };

        try
        {
            switch (rule.Type)
            {
                case ValidationRuleType.RequireName:
                    result.IsValid = xpath.Exists($"//*[@Name='{rule.Name}']");
                    if (!result.IsValid)
                        result.Message = $"Control with name '{rule.Name}' not found";
                    break;

                case ValidationRuleType.RequireType:
                    result.IsValid = xpath.Exists($"//{rule.ControlType!.Name}");
                    if (!result.IsValid)
                        result.Message = $"No {rule.ControlType.Name} controls found";
                    break;

                case ValidationRuleType.RequireExactCount:
                    var exactCount = xpath.Count($"//{rule.ControlType!.Name}");
                    result.IsValid = exactCount == rule.Count;
                    if (!result.IsValid)
                        result.Message = $"Expected exactly {rule.Count} {rule.ControlType.Name} controls, found {exactCount}";
                    break;

                case ValidationRuleType.RequireMinCount:
                    var minCount = xpath.Count($"//{rule.ControlType!.Name}");
                    result.IsValid = minCount >= rule.Count;
                    if (!result.IsValid)
                        result.Message = $"Expected at least {rule.Count} {rule.ControlType.Name} controls, found {minCount}";
                    break;

                case ValidationRuleType.RequireMaxCount:
                    var maxCount = xpath.Count($"//{rule.ControlType!.Name}");
                    result.IsValid = maxCount <= rule.Count;
                    if (!result.IsValid)
                        result.Message = $"Expected at most {rule.Count} {rule.ControlType.Name} controls, found {maxCount}";
                    break;

                case ValidationRuleType.RequirePattern:
                    result.IsValid = xpath.Exists(rule.Pattern!);
                    if (!result.IsValid)
                        result.Message = $"Pattern '{rule.Pattern}' did not match any nodes";
                    break;

                case ValidationRuleType.RequireNameOfType:
                    var namedControl = xpath.SelectFirst($"//*[@Name='{rule.Name}']");
                    result.IsValid = namedControl != null && rule.ControlType!.IsInstanceOfType(namedControl);
                    if (!result.IsValid)
                    {
                        if (namedControl == null)
                            result.Message = $"Control with name '{rule.Name}' not found";
                        else
                            result.Message = $"Control '{rule.Name}' is {namedControl.GetType().Name}, expected {rule.ControlType!.Name}";
                    }
                    break;

                case ValidationRuleType.RequireEnabled:
                    var enabledControl = xpath.SelectFirst<Control>($"//*[@Name='{rule.Name}']");
                    result.IsValid = enabledControl?.IsEnabled == true;
                    if (!result.IsValid)
                        result.Message = enabledControl == null
                            ? $"Control with name '{rule.Name}' not found"
                            : $"Control '{rule.Name}' is disabled";
                    break;

                case ValidationRuleType.RequireVisible:
                    var visibleControl = xpath.SelectFirst<Control>($"//*[@Name='{rule.Name}']");
                    result.IsValid = visibleControl?.IsVisible == true;
                    if (!result.IsValid)
                        result.Message = visibleControl == null
                            ? $"Control with name '{rule.Name}' not found"
                            : $"Control '{rule.Name}' is not visible";
                    break;

                case ValidationRuleType.RequireProperty:
                    var propControl = xpath.SelectFirst<AvaloniaObject>($"//*[@Name='{rule.Name}']");
                    if (propControl != null && rule.Property != null)
                    {
                        var actualValue = propControl.GetValue(rule.Property);
                        result.IsValid = Equals(actualValue, rule.ExpectedValue);
                        if (!result.IsValid)
                            result.Message = $"Control '{rule.Name}' has {rule.Property.Name} = {actualValue}, expected {rule.ExpectedValue}";
                    }
                    else
                    {
                        result.IsValid = false;
                        result.Message = $"Control with name '{rule.Name}' not found";
                    }
                    break;

                case ValidationRuleType.RequireChildOf:
                    var childControl = xpath.SelectFirst($"//*[@Name='{rule.Name}']");
                    var parentControl = xpath.SelectFirst($"//*[@Name='{rule.ParentName}']");
                    if (childControl != null && parentControl != null)
                    {
                        result.IsValid = IsDescendantOf(childControl, parentControl, useVisualTree);
                        if (!result.IsValid)
                            result.Message = $"Control '{rule.Name}' is not a descendant of '{rule.ParentName}'";
                    }
                    else
                    {
                        result.IsValid = false;
                        result.Message = childControl == null
                            ? $"Control with name '{rule.Name}' not found"
                            : $"Control with name '{rule.ParentName}' not found";
                    }
                    break;

                case ValidationRuleType.RequireClass:
                    var classControl = xpath.SelectFirst<Control>($"//*[@Name='{rule.Name}']");
                    result.IsValid = classControl?.Classes.Contains(rule.ClassName!) == true;
                    if (!result.IsValid)
                        result.Message = classControl == null
                            ? $"Control with name '{rule.Name}' not found"
                            : $"Control '{rule.Name}' does not have class '{rule.ClassName}'";
                    break;

                case ValidationRuleType.ForbidPattern:
                    result.IsValid = !xpath.Exists(rule.Pattern!);
                    if (!result.IsValid)
                        result.Message = $"Forbidden pattern '{rule.Pattern}' matched {xpath.Count(rule.Pattern!)} nodes";
                    break;

                case ValidationRuleType.ForbidType:
                    result.IsValid = !xpath.Exists($"//{rule.ControlType!.Name}");
                    if (!result.IsValid)
                        result.Message = $"Found {xpath.Count($"//{rule.ControlType.Name}")} forbidden {rule.ControlType.Name} controls";
                    break;

                case ValidationRuleType.Custom:
                    result.IsValid = root != null && rule.CustomValidator!(root);
                    if (!result.IsValid)
                        result.Message = "Custom validation failed";
                    break;
            }
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Message = $"Validation error: {ex.Message}";
        }

        return result;
    }

    private bool IsDescendantOf(object child, object parent, bool useVisualTree)
    {
        if (useVisualTree && child is Visual visualChild && parent is Visual visualParent)
        {
            var current = visualChild.GetVisualParent();
            while (current != null)
            {
                if (current == visualParent)
                    return true;
                current = current.GetVisualParent();
            }
        }
        else if (child is ILogical logicalChild && parent is ILogical logicalParent)
        {
            var current = logicalChild.GetLogicalParent();
            while (current != null)
            {
                if (current == logicalParent)
                    return true;
                current = current.GetLogicalParent();
            }
        }
        return false;
    }

    #endregion

    #region Assertion Helpers

    /// <summary>
    /// Throws if any validations failed.
    /// </summary>
    public void AssertValid()
    {
        if (!IsValid)
        {
            var failures = Failures.ToList();
            var message = $"Tree validation failed with {failures.Count} error(s):\n" +
                string.Join("\n", failures.Select(f => $"  - {f.RuleDescription}: {f.Message}"));
            throw new TreeValidationException(message, failures);
        }
    }

    /// <summary>
    /// Returns a summary of the validation results.
    /// </summary>
    public string GetSummary()
    {
        var passed = _results.Count(r => r.IsValid);
        var failed = _results.Count(r => !r.IsValid);

        var summary = $"Validation: {passed} passed, {failed} failed\n";
        foreach (var result in _results)
        {
            var status = result.IsValid ? "✓" : "✗";
            summary += $"  {status} {result.RuleDescription}";
            if (!result.IsValid && !string.IsNullOrEmpty(result.Message))
                summary += $" - {result.Message}";
            summary += "\n";
        }

        return summary;
    }

    #endregion
}

/// <summary>
/// Result of a single validation rule.
/// </summary>
public class TreeValidationResult
{
    /// <summary>
    /// Gets or sets whether the validation passed.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets the rule description.
    /// </summary>
    public string RuleDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error message if validation failed.
    /// </summary>
    public string? Message { get; set; }
}

/// <summary>
/// Exception thrown when tree validation fails.
/// </summary>
public class TreeValidationException : Exception
{
    /// <summary>
    /// Gets the failed validation results.
    /// </summary>
    public IReadOnlyList<TreeValidationResult> Failures { get; }

    /// <summary>
    /// Creates a new tree validation exception.
    /// </summary>
    public TreeValidationException(string message, IEnumerable<TreeValidationResult> failures)
        : base(message)
    {
        Failures = failures.ToList();
    }
}

/// <summary>
/// Internal validation rule definition.
/// </summary>
internal class TreeValidationRule
{
    public ValidationRuleType Type { get; set; }
    public string? Name { get; set; }
    public Type? ControlType { get; set; }
    public int Count { get; set; }
    public string? Pattern { get; set; }
    public string? ParentName { get; set; }
    public string? ClassName { get; set; }
    public AvaloniaProperty? Property { get; set; }
    public object? ExpectedValue { get; set; }
    public Func<Visual, bool>? CustomValidator { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Types of validation rules.
/// </summary>
internal enum ValidationRuleType
{
    RequireName,
    RequireType,
    RequireExactCount,
    RequireMinCount,
    RequireMaxCount,
    RequirePattern,
    RequireNameOfType,
    RequireEnabled,
    RequireVisible,
    RequireProperty,
    RequireChildOf,
    RequireClass,
    ForbidPattern,
    ForbidType,
    Custom
}

/// <summary>
/// Extension methods for tree validation.
/// </summary>
public static class TreeValidatorExtensions
{
    /// <summary>
    /// Creates a validator for the visual tree.
    /// </summary>
    public static TreeValidator Validate(this Visual root)
    {
        return TreeValidator.Create();
    }

    /// <summary>
    /// Validates that the visual tree contains a control with the specified name.
    /// </summary>
    public static bool HasControl(this Visual root, string name)
    {
        return root.ExistsXPath($"//*[@Name='{name}']");
    }

    /// <summary>
    /// Validates that the visual tree contains at least one control of the specified type.
    /// </summary>
    public static bool HasControlOfType<T>(this Visual root) where T : class
    {
        return root.ExistsXPath($"//{typeof(T).Name}");
    }

    /// <summary>
    /// Validates a named control exists and is enabled.
    /// </summary>
    public static bool IsControlEnabled(this Visual root, string name)
    {
        var control = root.SelectFirstXPath<Control>($"//*[@Name='{name}']");
        return control?.IsEnabled == true;
    }

    /// <summary>
    /// Validates a named control exists and is visible.
    /// </summary>
    public static bool IsControlVisible(this Visual root, string name)
    {
        var control = root.SelectFirstXPath<Control>($"//*[@Name='{name}']");
        return control?.IsVisible == true;
    }
}

// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace Avalonia.HeadlessTestingFramework;

/// <summary>
/// Fluent API for finding controls in visual or logical trees with complex query support.
/// Provides chainable methods for building sophisticated control queries.
/// </summary>
public class ControlFinder
{
    private readonly Visual _root;
    private readonly List<Func<object, bool>> _filters = new();
    private bool _useLogicalTree;
    private bool _includeSelf;
    private int? _maxDepth;
    private int? _take;
    private int _skip;

    /// <summary>
    /// Creates a new ControlFinder starting from the specified root.
    /// </summary>
    /// <param name="root">The root visual to search from.</param>
    public ControlFinder(Visual root)
    {
        _root = root ?? throw new ArgumentNullException(nameof(root));
    }

    /// <summary>
    /// Creates a ControlFinder for the specified root.
    /// </summary>
    /// <param name="root">The root visual to search from.</param>
    /// <returns>A new ControlFinder instance.</returns>
    public static ControlFinder From(Visual root) => new(root);

    #region Tree Selection

    /// <summary>
    /// Search in the visual tree (default).
    /// </summary>
    public ControlFinder InVisualTree()
    {
        _useLogicalTree = false;
        return this;
    }

    /// <summary>
    /// Search in the logical tree.
    /// </summary>
    public ControlFinder InLogicalTree()
    {
        _useLogicalTree = true;
        return this;
    }

    /// <summary>
    /// Include the root element in the search.
    /// </summary>
    public ControlFinder IncludeSelf()
    {
        _includeSelf = true;
        return this;
    }

    /// <summary>
    /// Limit the search to a maximum depth.
    /// </summary>
    /// <param name="depth">Maximum depth to search (1 = direct children only).</param>
    public ControlFinder MaxDepth(int depth)
    {
        _maxDepth = depth;
        return this;
    }

    #endregion

    #region Type Filters

    /// <summary>
    /// Filter to only controls of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to filter by.</typeparam>
    public ControlFinder OfType<T>() where T : class
    {
        _filters.Add(obj => obj is T);
        return this;
    }

    /// <summary>
    /// Filter to controls that are exactly the specified type (not derived).
    /// </summary>
    /// <typeparam name="T">The exact type to filter by.</typeparam>
    public ControlFinder ExactType<T>() where T : class
    {
        _filters.Add(obj => obj.GetType() == typeof(T));
        return this;
    }

    /// <summary>
    /// Filter to controls that are assignable from the specified type.
    /// </summary>
    /// <param name="type">The type to check assignability from.</param>
    public ControlFinder AssignableFrom(Type type)
    {
        _filters.Add(obj => type.IsInstanceOfType(obj));
        return this;
    }

    #endregion

    #region Name Filters

    /// <summary>
    /// Filter by exact control name.
    /// </summary>
    /// <param name="name">The name to match.</param>
    public ControlFinder WithName(string name)
    {
        _filters.Add(obj => obj is Control c && c.Name == name);
        return this;
    }

    /// <summary>
    /// Filter by name starting with a prefix.
    /// </summary>
    /// <param name="prefix">The prefix to match.</param>
    public ControlFinder WithNameStartingWith(string prefix)
    {
        _filters.Add(obj => obj is Control c && c.Name?.StartsWith(prefix) == true);
        return this;
    }

    /// <summary>
    /// Filter by name ending with a suffix.
    /// </summary>
    /// <param name="suffix">The suffix to match.</param>
    public ControlFinder WithNameEndingWith(string suffix)
    {
        _filters.Add(obj => obj is Control c && c.Name?.EndsWith(suffix) == true);
        return this;
    }

    /// <summary>
    /// Filter by name containing a substring.
    /// </summary>
    /// <param name="substring">The substring to match.</param>
    public ControlFinder WithNameContaining(string substring)
    {
        _filters.Add(obj => obj is Control c && c.Name?.Contains(substring) == true);
        return this;
    }

    /// <summary>
    /// Filter by name matching a pattern (supports * and ? wildcards).
    /// </summary>
    /// <param name="pattern">The pattern to match (e.g., "btn*", "*Dialog", "item?").</param>
    public ControlFinder WithNameMatching(string pattern)
    {
        var regex = "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
            .Replace("\\*", ".*")
            .Replace("\\?", ".") + "$";
        var compiledRegex = new System.Text.RegularExpressions.Regex(regex,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        _filters.Add(obj => obj is Control c && c.Name != null && compiledRegex.IsMatch(c.Name));
        return this;
    }

    #endregion

    #region Property Filters

    /// <summary>
    /// Filter by Avalonia property value.
    /// </summary>
    /// <typeparam name="TProperty">The property value type.</typeparam>
    /// <param name="property">The property to check.</param>
    /// <param name="value">The expected value.</param>
    public ControlFinder WithProperty<TProperty>(AvaloniaProperty<TProperty> property, TProperty value)
    {
        _filters.Add(obj => obj is AvaloniaObject ao && Equals(ao.GetValue(property), value));
        return this;
    }

    /// <summary>
    /// Filter by property using a predicate.
    /// </summary>
    /// <typeparam name="TProperty">The property value type.</typeparam>
    /// <param name="property">The property to check.</param>
    /// <param name="predicate">The predicate to apply to the property value.</param>
    public ControlFinder WithProperty<TProperty>(AvaloniaProperty<TProperty> property, Func<TProperty, bool> predicate)
    {
        _filters.Add(obj => obj is AvaloniaObject ao && predicate((TProperty)ao.GetValue(property)!));
        return this;
    }

    /// <summary>
    /// Filter by Tag property value.
    /// </summary>
    /// <param name="tag">The tag value to match.</param>
    public ControlFinder WithTag(object tag)
    {
        _filters.Add(obj => obj is Control c && Equals(c.Tag, tag));
        return this;
    }

    /// <summary>
    /// Filter by DataContext type.
    /// </summary>
    /// <typeparam name="T">The expected DataContext type.</typeparam>
    public ControlFinder WithDataContext<T>() where T : class
    {
        _filters.Add(obj => obj is StyledElement se && se.DataContext is T);
        return this;
    }

    /// <summary>
    /// Filter by DataContext value.
    /// </summary>
    /// <param name="dataContext">The exact DataContext to match.</param>
    public ControlFinder WithDataContext(object dataContext)
    {
        _filters.Add(obj => obj is StyledElement se && ReferenceEquals(se.DataContext, dataContext));
        return this;
    }

    /// <summary>
    /// Filter by DataContext property value.
    /// </summary>
    /// <typeparam name="TContext">The DataContext type.</typeparam>
    /// <typeparam name="TProperty">The property type.</typeparam>
    /// <param name="selector">Property selector expression.</param>
    /// <param name="value">The expected property value.</param>
    public ControlFinder WithDataContextProperty<TContext, TProperty>(
        Expression<Func<TContext, TProperty>> selector,
        TProperty value) where TContext : class
    {
        var compiled = selector.Compile();
        _filters.Add(obj => obj is StyledElement se &&
            se.DataContext is TContext dc &&
            Equals(compiled(dc), value));
        return this;
    }

    #endregion

    #region CSS Class Filters

    /// <summary>
    /// Filter by having a specific CSS class.
    /// </summary>
    /// <param name="className">The class name to check for.</param>
    public ControlFinder WithClass(string className)
    {
        _filters.Add(obj => obj is StyledElement se && se.Classes.Contains(className));
        return this;
    }

    /// <summary>
    /// Filter by having all specified CSS classes.
    /// </summary>
    /// <param name="classNames">The class names to check for.</param>
    public ControlFinder WithClasses(params string[] classNames)
    {
        _filters.Add(obj => obj is StyledElement se && classNames.All(cn => se.Classes.Contains(cn)));
        return this;
    }

    /// <summary>
    /// Filter by having any of the specified CSS classes.
    /// </summary>
    /// <param name="classNames">The class names to check for.</param>
    public ControlFinder WithAnyClass(params string[] classNames)
    {
        _filters.Add(obj => obj is StyledElement se && classNames.Any(cn => se.Classes.Contains(cn)));
        return this;
    }

    /// <summary>
    /// Filter by not having a specific CSS class.
    /// </summary>
    /// <param name="className">The class name to exclude.</param>
    public ControlFinder WithoutClass(string className)
    {
        _filters.Add(obj => obj is StyledElement se && !se.Classes.Contains(className));
        return this;
    }

    /// <summary>
    /// Filter by pseudo-class state.
    /// </summary>
    /// <param name="pseudoClass">The pseudo-class (e.g., ":pressed", ":pointerover").</param>
    public ControlFinder WithPseudoClass(string pseudoClass)
    {
        var normalizedClass = pseudoClass.StartsWith(":") ? pseudoClass : ":" + pseudoClass;
        _filters.Add(obj => obj is StyledElement se && se.Classes.Contains(normalizedClass));
        return this;
    }

    #endregion

    #region State Filters

    /// <summary>
    /// Filter to enabled controls only.
    /// </summary>
    public ControlFinder Enabled()
    {
        _filters.Add(obj => obj is InputElement ie && ie.IsEnabled);
        return this;
    }

    /// <summary>
    /// Filter to disabled controls only.
    /// </summary>
    public ControlFinder Disabled()
    {
        _filters.Add(obj => obj is InputElement ie && !ie.IsEnabled);
        return this;
    }

    /// <summary>
    /// Filter to visible controls only.
    /// </summary>
    public ControlFinder Visible()
    {
        _filters.Add(obj => obj is Visual v && v.IsVisible);
        return this;
    }

    /// <summary>
    /// Filter to hidden controls only.
    /// </summary>
    public ControlFinder Hidden()
    {
        _filters.Add(obj => obj is Visual v && !v.IsVisible);
        return this;
    }

    /// <summary>
    /// Filter to focusable controls only.
    /// </summary>
    public ControlFinder Focusable()
    {
        _filters.Add(obj => obj is InputElement ie && ie.Focusable);
        return this;
    }

    /// <summary>
    /// Filter to focused controls only.
    /// </summary>
    public ControlFinder Focused()
    {
        _filters.Add(obj => obj is InputElement ie && ie.IsFocused);
        return this;
    }

    /// <summary>
    /// Filter to controls that are hit-testable (IsHitTestVisible = true).
    /// </summary>
    public ControlFinder HitTestable()
    {
        _filters.Add(obj => obj is InputElement ie && ie.IsHitTestVisible);
        return this;
    }

    /// <summary>
    /// Filter to controls that are effectively visible (visible and all ancestors visible).
    /// </summary>
    public ControlFinder EffectivelyVisible()
    {
        _filters.Add(obj =>
        {
            if (obj is not Visual v)
                return false;

            Visual? current = v;
            while (current != null)
            {
                if (!current.IsVisible)
                    return false;
                current = current.GetVisualParent();
            }
            return true;
        });
        return this;
    }

    /// <summary>
    /// Filter to controls that are effectively enabled (enabled and all ancestors enabled).
    /// </summary>
    public ControlFinder EffectivelyEnabled()
    {
        _filters.Add(obj =>
        {
            if (obj is not InputElement ie)
                return false;

            return ie.IsEffectivelyEnabled;
        });
        return this;
    }

    #endregion

    #region Bounds Filters

    /// <summary>
    /// Filter by minimum width.
    /// </summary>
    /// <param name="minWidth">Minimum width in pixels.</param>
    public ControlFinder WithMinWidth(double minWidth)
    {
        _filters.Add(obj => obj is Visual v && v.Bounds.Width >= minWidth);
        return this;
    }

    /// <summary>
    /// Filter by minimum height.
    /// </summary>
    /// <param name="minHeight">Minimum height in pixels.</param>
    public ControlFinder WithMinHeight(double minHeight)
    {
        _filters.Add(obj => obj is Visual v && v.Bounds.Height >= minHeight);
        return this;
    }

    /// <summary>
    /// Filter by bounds containing a point.
    /// </summary>
    /// <param name="point">The point to check.</param>
    public ControlFinder ContainingPoint(Point point)
    {
        _filters.Add(obj => obj is Visual v && v.Bounds.Contains(point));
        return this;
    }

    /// <summary>
    /// Filter by bounds intersecting a rectangle.
    /// </summary>
    /// <param name="rect">The rectangle to check.</param>
    public ControlFinder IntersectingRect(Rect rect)
    {
        _filters.Add(obj => obj is Visual v && v.Bounds.Intersects(rect));
        return this;
    }

    #endregion

    #region Content Filters

    /// <summary>
    /// Filter TextBlock/TextBox by text content.
    /// </summary>
    /// <param name="text">The text to match.</param>
    public ControlFinder WithText(string text)
    {
        _filters.Add(obj =>
        {
            if (obj is TextBlock tb)
                return tb.Text == text;
            if (obj is TextBox textBox)
                return textBox.Text == text;
            return false;
        });
        return this;
    }

    /// <summary>
    /// Filter TextBlock/TextBox by text containing a substring.
    /// </summary>
    /// <param name="substring">The substring to match.</param>
    public ControlFinder WithTextContaining(string substring)
    {
        _filters.Add(obj =>
        {
            if (obj is TextBlock tb)
                return tb.Text?.Contains(substring) == true;
            if (obj is TextBox textBox)
                return textBox.Text?.Contains(substring) == true;
            return false;
        });
        return this;
    }

    /// <summary>
    /// Filter ContentControl by content type.
    /// </summary>
    /// <typeparam name="T">The expected content type.</typeparam>
    public ControlFinder WithContent<T>() where T : class
    {
        _filters.Add(obj => obj is ContentControl cc && cc.Content is T);
        return this;
    }

    /// <summary>
    /// Filter ItemsControl by item count.
    /// </summary>
    /// <param name="count">The expected item count.</param>
    public ControlFinder WithItemCount(int count)
    {
        _filters.Add(obj => obj is ItemsControl ic && ic.Items.Count == count);
        return this;
    }

    /// <summary>
    /// Filter ItemsControl by having at least a certain number of items.
    /// </summary>
    /// <param name="minCount">Minimum item count.</param>
    public ControlFinder WithMinItemCount(int minCount)
    {
        _filters.Add(obj => obj is ItemsControl ic && ic.Items.Count >= minCount);
        return this;
    }

    #endregion

    #region Custom Filters

    /// <summary>
    /// Add a custom filter predicate.
    /// </summary>
    /// <param name="predicate">The predicate to apply.</param>
    public ControlFinder Where(Func<object, bool> predicate)
    {
        _filters.Add(predicate);
        return this;
    }

    /// <summary>
    /// Add a typed custom filter predicate.
    /// </summary>
    /// <typeparam name="T">The type to filter and check.</typeparam>
    /// <param name="predicate">The predicate to apply to controls of the specified type.</param>
    public ControlFinder Where<T>(Func<T, bool> predicate) where T : class
    {
        _filters.Add(obj => obj is T typed && predicate(typed));
        return this;
    }

    /// <summary>
    /// Exclude controls matching a predicate.
    /// </summary>
    /// <param name="predicate">The predicate for exclusion.</param>
    public ControlFinder Except(Func<object, bool> predicate)
    {
        _filters.Add(obj => !predicate(obj));
        return this;
    }

    /// <summary>
    /// Exclude controls of a specific type.
    /// </summary>
    /// <typeparam name="T">The type to exclude.</typeparam>
    public ControlFinder Except<T>() where T : class
    {
        _filters.Add(obj => obj is not T);
        return this;
    }

    #endregion

    #region Result Limiting

    /// <summary>
    /// Skip a number of results.
    /// </summary>
    /// <param name="count">Number of results to skip.</param>
    public ControlFinder Skip(int count)
    {
        _skip = count;
        return this;
    }

    /// <summary>
    /// Take only a limited number of results.
    /// </summary>
    /// <param name="count">Maximum number of results.</param>
    public ControlFinder Take(int count)
    {
        _take = count;
        return this;
    }

    #endregion

    #region Execution

    /// <summary>
    /// Gets all descendants to search through.
    /// </summary>
    private IEnumerable<object> GetDescendants()
    {
        IEnumerable<object> source;

        if (_useLogicalTree && _root is ILogical logical)
        {
            source = _includeSelf
                ? logical.GetSelfAndLogicalDescendants()
                : logical.GetLogicalDescendants();
        }
        else
        {
            source = _includeSelf
                ? _root.GetSelfAndVisualDescendants()
                : _root.GetVisualDescendants();
        }

        // Apply max depth filter if specified
        if (_maxDepth.HasValue)
        {
            var maxDepthValue = _maxDepth.Value;
            source = source.Where(obj =>
            {
                var depth = 0;
                object? current = obj;
                while (current != null && !ReferenceEquals(current, _root))
                {
                    depth++;
                    if (depth > maxDepthValue)
                        return false;

                    current = _useLogicalTree && current is ILogical l
                        ? l.GetLogicalParent()
                        : (current as Visual)?.GetVisualParent();
                }
                return true;
            });
        }

        return source;
    }

    /// <summary>
    /// Applies all filters to the source.
    /// </summary>
    private IEnumerable<object> ApplyFilters(IEnumerable<object> source)
    {
        var result = source;

        foreach (var filter in _filters)
        {
            result = result.Where(filter);
        }

        result = result.Skip(_skip);

        if (_take.HasValue)
        {
            result = result.Take(_take.Value);
        }

        return result;
    }

    /// <summary>
    /// Finds all matching controls.
    /// </summary>
    /// <returns>All controls matching the filters.</returns>
    public IEnumerable<object> FindAll()
    {
        return ApplyFilters(GetDescendants());
    }

    /// <summary>
    /// Finds all matching controls of a specific type.
    /// </summary>
    /// <typeparam name="T">The type to return.</typeparam>
    /// <returns>All controls matching the filters, cast to the specified type.</returns>
    public IEnumerable<T> FindAll<T>() where T : class
    {
        return FindAll().OfType<T>();
    }

    /// <summary>
    /// Finds the first matching control.
    /// </summary>
    /// <returns>The first matching control, or null if none found.</returns>
    public object? FindFirst()
    {
        return FindAll().FirstOrDefault();
    }

    /// <summary>
    /// Finds the first matching control of a specific type.
    /// </summary>
    /// <typeparam name="T">The type to return.</typeparam>
    /// <returns>The first matching control, or null if none found.</returns>
    public T? FindFirst<T>() where T : class
    {
        return FindAll<T>().FirstOrDefault();
    }

    /// <summary>
    /// Gets the first matching control, throwing if not found.
    /// </summary>
    /// <returns>The first matching control.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no matching control is found.</exception>
    public object GetFirst()
    {
        return FindFirst() ?? throw new InvalidOperationException(
            $"No matching control found. Root: {_root.GetType().Name}, Filters applied: {_filters.Count}. " +
            "Check that the control exists in the tree and matches all filter criteria.");
    }

    /// <summary>
    /// Gets the first matching control of a specific type, throwing if not found.
    /// </summary>
    /// <typeparam name="T">The type to return.</typeparam>
    /// <returns>The first matching control.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no matching control is found.</exception>
    public T GetFirst<T>() where T : class
    {
        return FindFirst<T>() ?? throw new InvalidOperationException(
            $"No matching control of type {typeof(T).Name} found. Root: {_root.GetType().Name}, Filters applied: {_filters.Count}. " +
            "Check that the control exists in the tree and matches all filter criteria.");
    }

    /// <summary>
    /// Finds the single matching control (throws if 0 or more than 1).
    /// </summary>
    /// <returns>The single matching control.</returns>
    /// <exception cref="InvalidOperationException">Thrown when 0 or more than 1 control is found.</exception>
    public object Single()
    {
        return FindAll().Single();
    }

    /// <summary>
    /// Finds the single matching control of a specific type.
    /// </summary>
    /// <typeparam name="T">The type to return.</typeparam>
    /// <returns>The single matching control.</returns>
    public T Single<T>() where T : class
    {
        return FindAll<T>().Single();
    }

    /// <summary>
    /// Counts matching controls.
    /// </summary>
    /// <returns>The number of matching controls.</returns>
    public int Count()
    {
        return FindAll().Count();
    }

    /// <summary>
    /// Checks if any control matches the filters.
    /// </summary>
    /// <returns>True if at least one control matches.</returns>
    public bool Any()
    {
        return FindAll().Any();
    }

    /// <summary>
    /// Checks if no controls match the filters.
    /// </summary>
    /// <returns>True if no controls match.</returns>
    public bool None()
    {
        return !Any();
    }

    /// <summary>
    /// Checks if all descendants match the filters (for verification).
    /// </summary>
    /// <returns>True if all descendants match.</returns>
    public bool All()
    {
        var descendants = GetDescendants().ToList();
        var filtered = ApplyFilters(descendants).ToList();
        return descendants.Count == filtered.Count;
    }

    #endregion
}

/// <summary>
/// Extension methods for creating ControlFinder instances.
/// </summary>
public static class ControlFinderExtensions
{
    /// <summary>
    /// Creates a ControlFinder for the specified root.
    /// </summary>
    /// <param name="root">The root visual to search from.</param>
    /// <returns>A new ControlFinder instance.</returns>
    public static ControlFinder Find(this Visual root) => new(root);

    /// <summary>
    /// Creates a ControlFinder for the specified root and immediately filters by type.
    /// </summary>
    /// <typeparam name="T">The type to filter by.</typeparam>
    /// <param name="root">The root visual to search from.</param>
    /// <returns>A new ControlFinder instance with type filter applied.</returns>
    public static ControlFinder Find<T>(this Visual root) where T : class => new ControlFinder(root).OfType<T>();
}

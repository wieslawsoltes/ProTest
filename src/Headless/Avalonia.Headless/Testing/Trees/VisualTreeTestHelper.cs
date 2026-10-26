// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace Avalonia.HeadlessTestingFramework;

/// <summary>
/// Provides helper methods for traversing and querying the visual tree in headless tests.
/// Simplifies common patterns for finding controls, validating tree structure, and debugging.
/// </summary>
public static class VisualTreeTestHelper
{
    #region Find By Type

    /// <summary>
    /// Finds the first descendant of a specific type in the visual tree.
    /// </summary>
    /// <typeparam name="T">The type of control to find.</typeparam>
    /// <param name="root">The root visual to search from.</param>
    /// <param name="includeSelf">Whether to include the root in the search.</param>
    /// <returns>The first descendant of the specified type, or null if not found.</returns>
    public static T? FindFirst<T>(this Visual root, bool includeSelf = false) where T : class
    {
        return root.FindDescendantOfType<T>(includeSelf);
    }

    /// <summary>
    /// Finds all descendants of a specific type in the visual tree.
    /// </summary>
    /// <typeparam name="T">The type of controls to find.</typeparam>
    /// <param name="root">The root visual to search from.</param>
    /// <param name="includeSelf">Whether to include the root in the search.</param>
    /// <returns>All descendants of the specified type.</returns>
    public static IEnumerable<T> FindAll<T>(this Visual root, bool includeSelf = false) where T : class
    {
        var descendants = includeSelf
            ? root.GetSelfAndVisualDescendants()
            : root.GetVisualDescendants();
        return descendants.OfType<T>();
    }

    /// <summary>
    /// Finds all descendants matching a predicate.
    /// </summary>
    /// <typeparam name="T">The type of controls to find.</typeparam>
    /// <param name="root">The root visual to search from.</param>
    /// <param name="predicate">The predicate to match controls against.</param>
    /// <param name="includeSelf">Whether to include the root in the search.</param>
    /// <returns>All descendants matching the predicate.</returns>
    public static IEnumerable<T> FindAll<T>(this Visual root, Func<T, bool> predicate, bool includeSelf = false) where T : class
    {
        return root.FindAll<T>(includeSelf).Where(predicate);
    }

    /// <summary>
    /// Finds the first descendant matching a predicate.
    /// </summary>
    /// <typeparam name="T">The type of control to find.</typeparam>
    /// <param name="root">The root visual to search from.</param>
    /// <param name="predicate">The predicate to match.</param>
    /// <param name="includeSelf">Whether to include the root in the search.</param>
    /// <returns>The first descendant matching the predicate, or null if not found.</returns>
    public static T? FindFirst<T>(this Visual root, Func<T, bool> predicate, bool includeSelf = false) where T : class
    {
        return root.FindAll<T>(predicate, includeSelf).FirstOrDefault();
    }

    #endregion

    #region Find By Name

    /// <summary>
    /// Finds a control by its Name property in the visual tree.
    /// </summary>
    /// <param name="root">The root visual to search from.</param>
    /// <param name="name">The name to search for.</param>
    /// <returns>The control with the specified name, or null if not found.</returns>
    public static Control? FindByName(this Visual root, string name)
    {
        return root.FindAll<Control>(c => c.Name == name).FirstOrDefault();
    }

    /// <summary>
    /// Finds a control by its Name property with a specific type.
    /// </summary>
    /// <typeparam name="T">The expected type of the control.</typeparam>
    /// <param name="root">The root visual to search from.</param>
    /// <param name="name">The name to search for.</param>
    /// <returns>The control with the specified name and type, or null if not found.</returns>
    public static T? FindByName<T>(this Visual root, string name) where T : Control
    {
        return root.FindAll<T>(c => c.Name == name).FirstOrDefault();
    }

    /// <summary>
    /// Gets a control by name, throwing if not found.
    /// </summary>
    /// <typeparam name="T">The expected type of the control.</typeparam>
    /// <param name="root">The root visual to search from.</param>
    /// <param name="name">The name to search for.</param>
    /// <returns>The control with the specified name.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the control is not found.</exception>
    public static T GetByName<T>(this Visual root, string name) where T : Control
    {
        return root.FindByName<T>(name)
            ?? throw new InvalidOperationException($"Control '{name}' of type {typeof(T).Name} not found in visual tree.");
    }

    #endregion

    #region Find By Property

    /// <summary>
    /// Finds controls by a specific property value.
    /// </summary>
    /// <typeparam name="T">The type of controls to find.</typeparam>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="root">The root visual to search from.</param>
    /// <param name="property">The Avalonia property to check.</param>
    /// <param name="value">The expected property value.</param>
    /// <returns>All controls with the specified property value.</returns>
    public static IEnumerable<T> FindByProperty<T, TProperty>(
        this Visual root,
        AvaloniaProperty<TProperty> property,
        TProperty value) where T : AvaloniaObject
    {
        return root.FindAll<T>(c => Equals(c.GetValue(property), value));
    }

    /// <summary>
    /// Finds the first control with a specific property value.
    /// </summary>
    /// <typeparam name="T">The type of control to find.</typeparam>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="root">The root visual to search from.</param>
    /// <param name="property">The Avalonia property to check.</param>
    /// <param name="value">The expected property value.</param>
    /// <returns>The first control with the specified property value, or null if not found.</returns>
    public static T? FindFirstByProperty<T, TProperty>(
        this Visual root,
        AvaloniaProperty<TProperty> property,
        TProperty value) where T : AvaloniaObject
    {
        return root.FindByProperty<T, TProperty>(property, value).FirstOrDefault();
    }

    /// <summary>
    /// Finds controls by Tag property.
    /// </summary>
    /// <param name="root">The root visual to search from.</param>
    /// <param name="tag">The tag value to search for.</param>
    /// <returns>All controls with the specified tag.</returns>
    public static IEnumerable<Control> FindByTag(this Visual root, object tag)
    {
        return root.FindAll<Control>(c => Equals(c.Tag, tag));
    }

    /// <summary>
    /// Finds controls by CSS classes.
    /// </summary>
    /// <typeparam name="T">The type of controls to find.</typeparam>
    /// <param name="root">The root visual to search from.</param>
    /// <param name="className">The CSS class name to search for.</param>
    /// <returns>All controls with the specified CSS class.</returns>
    public static IEnumerable<T> FindByClass<T>(this Visual root, string className) where T : StyledElement
    {
        return root.FindAll<T>(c => c.Classes.Contains(className));
    }

    /// <summary>
    /// Finds controls that have all specified CSS classes.
    /// </summary>
    /// <typeparam name="T">The type of controls to find.</typeparam>
    /// <param name="root">The root visual to search from.</param>
    /// <param name="classNames">The CSS class names to search for.</param>
    /// <returns>All controls with all specified CSS classes.</returns>
    public static IEnumerable<T> FindByClasses<T>(this Visual root, params string[] classNames) where T : StyledElement
    {
        return root.FindAll<T>(c => classNames.All(cn => c.Classes.Contains(cn)));
    }

    #endregion

    #region Find By State

    /// <summary>
    /// Finds all enabled controls of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of controls to find.</typeparam>
    /// <param name="root">The root visual to search from.</param>
    /// <returns>All enabled controls of the specified type.</returns>
    public static IEnumerable<T> FindEnabled<T>(this Visual root) where T : InputElement
    {
        return root.FindAll<T>(c => c.IsEnabled);
    }

    /// <summary>
    /// Finds all disabled controls of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of controls to find.</typeparam>
    /// <param name="root">The root visual to search from.</param>
    /// <returns>All disabled controls of the specified type.</returns>
    public static IEnumerable<T> FindDisabled<T>(this Visual root) where T : InputElement
    {
        return root.FindAll<T>(c => !c.IsEnabled);
    }

    /// <summary>
    /// Finds all visible controls of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of controls to find.</typeparam>
    /// <param name="root">The root visual to search from.</param>
    /// <returns>All visible controls of the specified type.</returns>
    public static IEnumerable<T> FindVisible<T>(this Visual root) where T : Visual
    {
        return root.FindAll<T>(c => c.IsVisible);
    }

    /// <summary>
    /// Finds all hidden (not visible) controls of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of controls to find.</typeparam>
    /// <param name="root">The root visual to search from.</param>
    /// <returns>All hidden controls of the specified type.</returns>
    public static IEnumerable<T> FindHidden<T>(this Visual root) where T : Visual
    {
        return root.FindAll<T>(c => !c.IsVisible);
    }

    /// <summary>
    /// Finds all focused controls (should typically be 0 or 1).
    /// </summary>
    /// <typeparam name="T">The type of controls to find.</typeparam>
    /// <param name="root">The root visual to search from.</param>
    /// <returns>All focused controls of the specified type.</returns>
    public static IEnumerable<T> FindFocused<T>(this Visual root) where T : InputElement
    {
        return root.FindAll<T>(c => c.IsFocused);
    }

    /// <summary>
    /// Finds all focusable controls of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of controls to find.</typeparam>
    /// <param name="root">The root visual to search from.</param>
    /// <returns>All focusable controls of the specified type.</returns>
    public static IEnumerable<T> FindFocusable<T>(this Visual root) where T : InputElement
    {
        return root.FindAll<T>(c => c.Focusable);
    }

    /// <summary>
    /// Finds controls in a specific pseudo-class state.
    /// </summary>
    /// <typeparam name="T">The type of controls to find.</typeparam>
    /// <param name="root">The root visual to search from.</param>
    /// <param name="pseudoClass">The pseudo-class name (e.g., ":pressed", ":pointerover").</param>
    /// <returns>All controls in the specified pseudo-class state.</returns>
    public static IEnumerable<T> FindByPseudoClass<T>(this Visual root, string pseudoClass) where T : StyledElement
    {
        var normalizedClass = pseudoClass.StartsWith(":") ? pseudoClass : ":" + pseudoClass;
        return root.FindAll<T>(c => c.Classes.Contains(normalizedClass));
    }

    #endregion

    #region Ancestor Queries

    /// <summary>
    /// Finds the first ancestor of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of ancestor to find.</typeparam>
    /// <param name="visual">The visual to start from.</param>
    /// <param name="includeSelf">Whether to include the visual itself in the search.</param>
    /// <returns>The first ancestor of the specified type, or null if not found.</returns>
    public static T? FindAncestor<T>(this Visual visual, bool includeSelf = false) where T : class
    {
        return visual.FindAncestorOfType<T>(includeSelf);
    }

    /// <summary>
    /// Finds an ancestor matching a predicate.
    /// </summary>
    /// <typeparam name="T">The type of ancestor to find.</typeparam>
    /// <param name="visual">The visual to start from.</param>
    /// <param name="predicate">The predicate to match.</param>
    /// <param name="includeSelf">Whether to include the visual itself in the search.</param>
    /// <returns>The first ancestor matching the predicate, or null if not found.</returns>
    public static T? FindAncestor<T>(this Visual visual, Func<T, bool> predicate, bool includeSelf = false) where T : class
    {
        var ancestors = includeSelf
            ? visual.GetSelfAndVisualAncestors()
            : visual.GetVisualAncestors();
        return ancestors.OfType<T>().FirstOrDefault(predicate);
    }

    /// <summary>
    /// Gets all ancestors of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of ancestors to find.</typeparam>
    /// <param name="visual">The visual to start from.</param>
    /// <returns>All ancestors of the specified type.</returns>
    public static IEnumerable<T> GetAncestors<T>(this Visual visual) where T : class
    {
        return visual.GetVisualAncestors().OfType<T>();
    }

    /// <summary>
    /// Checks if the visual has an ancestor of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of ancestor to check for.</typeparam>
    /// <param name="visual">The visual to check.</param>
    /// <returns>True if an ancestor of the specified type exists.</returns>
    public static bool HasAncestor<T>(this Visual visual) where T : class
    {
        return visual.FindAncestorOfType<T>() != null;
    }

    #endregion

    #region Children Queries

    /// <summary>
    /// Gets the direct children of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of children to get.</typeparam>
    /// <param name="visual">The parent visual.</param>
    /// <returns>Direct children of the specified type.</returns>
    public static IEnumerable<T> GetChildren<T>(this Visual visual) where T : class
    {
        return visual.GetVisualChildren().OfType<T>();
    }

    /// <summary>
    /// Gets the number of direct children.
    /// </summary>
    /// <param name="visual">The parent visual.</param>
    /// <returns>The count of direct children.</returns>
    public static int GetChildCount(this Visual visual)
    {
        return visual.GetVisualChildren().Count();
    }

    /// <summary>
    /// Gets the number of descendants (all levels).
    /// </summary>
    /// <param name="visual">The root visual.</param>
    /// <returns>The count of all descendants.</returns>
    public static int GetDescendantCount(this Visual visual)
    {
        return visual.GetVisualDescendants().Count();
    }

    /// <summary>
    /// Gets a child at a specific index.
    /// </summary>
    /// <param name="visual">The parent visual.</param>
    /// <param name="index">The index of the child.</param>
    /// <returns>The child at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
    public static Visual GetChildAt(this Visual visual, int index)
    {
        return visual.GetVisualChildren().ElementAt(index);
    }

    /// <summary>
    /// Gets a child at a specific index, cast to a type.
    /// </summary>
    /// <typeparam name="T">The expected type of the child.</typeparam>
    /// <param name="visual">The parent visual.</param>
    /// <param name="index">The index of the child.</param>
    /// <returns>The child at the specified index, cast to the type.</returns>
    public static T GetChildAt<T>(this Visual visual, int index) where T : Visual
    {
        var child = visual.GetChildAt(index);
        return child as T ?? throw new InvalidCastException(
            $"Child at index {index} is {child.GetType().Name}, expected {typeof(T).Name}.");
    }

    /// <summary>
    /// Gets the first child of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of child to get.</typeparam>
    /// <param name="visual">The parent visual.</param>
    /// <returns>The first child of the specified type, or null if not found.</returns>
    public static T? GetFirstChild<T>(this Visual visual) where T : class
    {
        return visual.GetVisualChildren().OfType<T>().FirstOrDefault();
    }

    /// <summary>
    /// Gets the last child of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of child to get.</typeparam>
    /// <param name="visual">The parent visual.</param>
    /// <returns>The last child of the specified type, or null if not found.</returns>
    public static T? GetLastChild<T>(this Visual visual) where T : class
    {
        return visual.GetVisualChildren().OfType<T>().LastOrDefault();
    }

    #endregion

    #region Tree Path

    /// <summary>
    /// Gets the path from root to the specified visual.
    /// </summary>
    /// <param name="visual">The visual to get the path to.</param>
    /// <returns>The path as a list of visuals from root to the visual.</returns>
    public static IReadOnlyList<Visual> GetPathFromRoot(this Visual visual)
    {
        var path = new List<Visual>();
        Visual? current = visual;
        while (current != null)
        {
            path.Insert(0, current);
            current = current.GetVisualParent();
        }
        return path;
    }

    /// <summary>
    /// Gets the path as type names for debugging.
    /// </summary>
    /// <param name="visual">The visual to get the path for.</param>
    /// <returns>A string representing the path of type names.</returns>
    public static string GetPathString(this Visual visual)
    {
        var path = visual.GetPathFromRoot();
        return string.Join(" > ", path.Select(v => v.GetType().Name));
    }

    /// <summary>
    /// Gets a detailed path string including names where available.
    /// </summary>
    /// <param name="visual">The visual to get the path for.</param>
    /// <returns>A detailed string representing the path.</returns>
    public static string GetDetailedPathString(this Visual visual)
    {
        var path = visual.GetPathFromRoot();
        return string.Join(" > ", path.Select(v =>
        {
            var name = (v as Control)?.Name;
            return string.IsNullOrEmpty(name) ? v.GetType().Name : $"{v.GetType().Name}[{name}]";
        }));
    }

    #endregion

    #region Tree Validation

    /// <summary>
    /// Checks if a visual contains a descendant of a specific type.
    /// </summary>
    /// <typeparam name="T">The type to check for.</typeparam>
    /// <param name="root">The root visual to search from.</param>
    /// <returns>True if a descendant of the specified type exists.</returns>
    public static bool Contains<T>(this Visual root) where T : class
    {
        return root.FindDescendantOfType<T>() != null;
    }

    /// <summary>
    /// Checks if a visual contains a descendant with a specific name.
    /// </summary>
    /// <param name="root">The root visual to search from.</param>
    /// <param name="name">The name to search for.</param>
    /// <returns>True if a descendant with the specified name exists.</returns>
    public static bool ContainsNamed(this Visual root, string name)
    {
        return root.FindByName(name) != null;
    }

    /// <summary>
    /// Counts all descendants of a specific type.
    /// </summary>
    /// <typeparam name="T">The type to count.</typeparam>
    /// <param name="root">The root visual to search from.</param>
    /// <returns>The number of descendants of the specified type.</returns>
    public static int Count<T>(this Visual root) where T : class
    {
        return root.FindAll<T>().Count();
    }

    /// <summary>
    /// Checks if the tree structure matches expected types at each level.
    /// </summary>
    /// <param name="visual">The starting visual.</param>
    /// <param name="expectedPath">The expected types from this visual downward.</param>
    /// <returns>True if the tree structure matches.</returns>
    public static bool MatchesStructure(this Visual visual, params Type[] expectedPath)
    {
        if (expectedPath.Length == 0)
            return true;

        if (!expectedPath[0].IsInstanceOfType(visual))
            return false;

        if (expectedPath.Length == 1)
            return true;

        var firstChild = visual.GetVisualChildren().FirstOrDefault();
        if (firstChild == null)
            return false;

        return firstChild.MatchesStructure(expectedPath.Skip(1).ToArray());
    }

    #endregion

    #region Hit Testing

    /// <summary>
    /// Gets the visual at a specific point in the visual's coordinate space.
    /// </summary>
    /// <param name="root">The root visual to perform hit testing from.</param>
    /// <param name="point">The point to test.</param>
    /// <returns>The visual at the specified point, or null if none.</returns>
    public static Visual? HitTest(this Visual root, Point point)
    {
        return root.GetVisualAt(point);
    }

    /// <summary>
    /// Gets the control at a specific point.
    /// </summary>
    /// <param name="root">The root visual to perform hit testing from.</param>
    /// <param name="point">The point to test.</param>
    /// <returns>The control at the specified point, or null if none.</returns>
    public static Control? HitTestControl(this Visual root, Point point)
    {
        var visual = root.GetVisualAt(point);
        return visual as Control ?? visual?.FindAncestorOfType<Control>();
    }

    /// <summary>
    /// Gets all visuals at a specific point.
    /// </summary>
    /// <param name="root">The root visual to perform hit testing from.</param>
    /// <param name="point">The point to test.</param>
    /// <returns>All visuals at the specified point.</returns>
    public static IEnumerable<Visual> HitTestAll(this Visual root, Point point)
    {
        return root.GetVisualsAt(point);
    }

    /// <summary>
    /// Gets all controls at a specific point.
    /// </summary>
    /// <param name="root">The root visual to perform hit testing from.</param>
    /// <param name="point">The point to test.</param>
    /// <returns>All controls at the specified point.</returns>
    public static IEnumerable<Control> HitTestAllControls(this Visual root, Point point)
    {
        return root.GetVisualsAt(point)
            .Select(v => v as Control ?? v.FindAncestorOfType<Control>())
            .Where(c => c != null)
            .Cast<Control>()
            .Distinct();
    }

    #endregion

    #region Bounds and Layout

    /// <summary>
    /// Gets the bounds of a visual in the coordinate space of another visual.
    /// </summary>
    /// <param name="visual">The visual to get bounds for.</param>
    /// <param name="relativeTo">The visual to get bounds relative to.</param>
    /// <returns>The bounds, or null if no common ancestor.</returns>
    public static Rect? GetBoundsRelativeTo(this Visual visual, Visual relativeTo)
    {
        var matrix = visual.TransformToVisual(relativeTo);
        if (matrix == null)
            return null;

        return new Rect(visual.Bounds.Size).TransformToAABB(matrix.Value);
    }

    /// <summary>
    /// Gets the center point of a visual.
    /// </summary>
    /// <param name="visual">The visual to get the center for.</param>
    /// <returns>The center point in the visual's coordinate space.</returns>
    public static Point GetCenter(this Visual visual)
    {
        return new Point(visual.Bounds.Width / 2, visual.Bounds.Height / 2);
    }

    /// <summary>
    /// Gets the center point of a visual relative to another visual.
    /// </summary>
    /// <param name="visual">The visual to get the center for.</param>
    /// <param name="relativeTo">The visual to get the center relative to.</param>
    /// <returns>The center point, or null if no common ancestor.</returns>
    public static Point? GetCenterRelativeTo(this Visual visual, Visual relativeTo)
    {
        return visual.TranslatePoint(visual.GetCenter(), relativeTo);
    }

    /// <summary>
    /// Checks if a visual is within the visible bounds of an ancestor.
    /// </summary>
    /// <param name="visual">The visual to check.</param>
    /// <param name="ancestor">The ancestor to check visibility within.</param>
    /// <returns>True if the visual is at least partially visible.</returns>
    public static bool IsVisibleWithin(this Visual visual, Visual ancestor)
    {
        var bounds = visual.GetBoundsRelativeTo(ancestor);
        if (bounds == null)
            return false;

        var ancestorBounds = new Rect(ancestor.Bounds.Size);
        return bounds.Value.Intersects(ancestorBounds);
    }

    #endregion

    #region Debug Helpers

    /// <summary>
    /// Prints the visual tree structure to a string for debugging.
    /// </summary>
    /// <param name="root">The root visual to print from.</param>
    /// <param name="maxDepth">Maximum depth to print (default is unlimited).</param>
    /// <returns>A string representation of the tree structure.</returns>
    public static string PrintTree(this Visual root, int maxDepth = int.MaxValue)
    {
        var lines = new List<string>();
        PrintTreeInternal(root, lines, "", true, 0, maxDepth);
        return string.Join(Environment.NewLine, lines);
    }

    private static void PrintTreeInternal(Visual visual, List<string> lines, string prefix, bool isLast, int depth, int maxDepth)
    {
        if (depth > maxDepth)
            return;

        var name = (visual as Control)?.Name;
        var nameStr = string.IsNullOrEmpty(name) ? "" : $" [{name}]";
        var visibilityStr = !visual.IsVisible ? " (hidden)" : "";
        var enabledStr = (visual as InputElement)?.IsEnabled == false ? " (disabled)" : "";

        lines.Add($"{prefix}{(isLast ? "└── " : "├── ")}{visual.GetType().Name}{nameStr}{visibilityStr}{enabledStr}");

        var children = visual.GetVisualChildren().ToList();
        for (int i = 0; i < children.Count; i++)
        {
            var newPrefix = prefix + (isLast ? "    " : "│   ");
            PrintTreeInternal(children[i], lines, newPrefix, i == children.Count - 1, depth + 1, maxDepth);
        }
    }

    /// <summary>
    /// Gets a summary of control types in the tree.
    /// </summary>
    /// <param name="root">The root visual to analyze.</param>
    /// <returns>A dictionary of type names and their counts.</returns>
    public static Dictionary<string, int> GetTypeSummary(this Visual root)
    {
        return root.GetSelfAndVisualDescendants()
            .GroupBy(v => v.GetType().Name)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>
    /// Gets all named controls in the tree. If duplicate names exist, only the first occurrence is kept.
    /// </summary>
    /// <param name="root">The root visual to search from.</param>
    /// <returns>A dictionary of control names to controls.</returns>
    public static Dictionary<string, Control> GetNamedControls(this Visual root)
    {
        var result = new Dictionary<string, Control>();
        foreach (var control in root.FindAll<Control>(c => !string.IsNullOrEmpty(c.Name)))
        {
            // Only add if not already present (handles duplicates like PART_ContentPresenter)
            if (!result.ContainsKey(control.Name!))
            {
                result[control.Name!] = control;
            }
        }
        return result;
    }

    #endregion
}

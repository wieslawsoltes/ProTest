// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;

namespace Avalonia.HeadlessTestingFramework;

/// <summary>
/// Provides helper methods for traversing and querying the logical tree in headless tests.
/// The logical tree represents the conceptual parent-child relationships as defined in XAML,
/// while the visual tree represents the actual rendered elements including templates.
/// </summary>
public static class LogicalTreeTestHelper
{
    #region Find By Type

    /// <summary>
    /// Finds the first logical descendant of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of control to find.</typeparam>
    /// <param name="root">The root logical to search from.</param>
    /// <param name="includeSelf">Whether to include the root in the search.</param>
    /// <returns>The first descendant of the specified type, or null if not found.</returns>
    public static T? FindFirstLogical<T>(this ILogical root, bool includeSelf = false) where T : class
    {
        return root.FindLogicalDescendantOfType<T>(includeSelf);
    }

    /// <summary>
    /// Finds all logical descendants of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of controls to find.</typeparam>
    /// <param name="root">The root logical to search from.</param>
    /// <param name="includeSelf">Whether to include the root in the search.</param>
    /// <returns>All descendants of the specified type.</returns>
    public static IEnumerable<T> FindAllLogical<T>(this ILogical root, bool includeSelf = false) where T : class
    {
        var descendants = includeSelf
            ? root.GetSelfAndLogicalDescendants()
            : root.GetLogicalDescendants();
        return descendants.OfType<T>();
    }

    /// <summary>
    /// Finds all logical descendants matching a predicate.
    /// </summary>
    /// <typeparam name="T">The type of controls to find.</typeparam>
    /// <param name="root">The root logical to search from.</param>
    /// <param name="predicate">The predicate to match controls against.</param>
    /// <param name="includeSelf">Whether to include the root in the search.</param>
    /// <returns>All descendants matching the predicate.</returns>
    public static IEnumerable<T> FindAllLogical<T>(this ILogical root, Func<T, bool> predicate, bool includeSelf = false) where T : class
    {
        return root.FindAllLogical<T>(includeSelf).Where(predicate);
    }

    /// <summary>
    /// Finds the first logical descendant matching a predicate.
    /// </summary>
    /// <typeparam name="T">The type of control to find.</typeparam>
    /// <param name="root">The root logical to search from.</param>
    /// <param name="predicate">The predicate to match.</param>
    /// <param name="includeSelf">Whether to include the root in the search.</param>
    /// <returns>The first descendant matching the predicate, or null if not found.</returns>
    public static T? FindFirstLogical<T>(this ILogical root, Func<T, bool> predicate, bool includeSelf = false) where T : class
    {
        return root.FindAllLogical<T>(predicate, includeSelf).FirstOrDefault();
    }

    #endregion

    #region Find By Name

    /// <summary>
    /// Finds a control by its Name property in the logical tree.
    /// </summary>
    /// <param name="root">The root logical to search from.</param>
    /// <param name="name">The name to search for.</param>
    /// <returns>The control with the specified name, or null if not found.</returns>
    public static Control? FindLogicalByName(this ILogical root, string name)
    {
        return root.FindAllLogical<Control>(c => c.Name == name).FirstOrDefault();
    }

    /// <summary>
    /// Finds a control by its Name property with a specific type in the logical tree.
    /// </summary>
    /// <typeparam name="T">The expected type of the control.</typeparam>
    /// <param name="root">The root logical to search from.</param>
    /// <param name="name">The name to search for.</param>
    /// <returns>The control with the specified name and type, or null if not found.</returns>
    public static T? FindLogicalByName<T>(this ILogical root, string name) where T : Control
    {
        return root.FindAllLogical<T>(c => c.Name == name).FirstOrDefault();
    }

    /// <summary>
    /// Gets a control by name in the logical tree, throwing if not found.
    /// </summary>
    /// <typeparam name="T">The expected type of the control.</typeparam>
    /// <param name="root">The root logical to search from.</param>
    /// <param name="name">The name to search for.</param>
    /// <returns>The control with the specified name.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the control is not found.</exception>
    public static T GetLogicalByName<T>(this ILogical root, string name) where T : Control
    {
        return root.FindLogicalByName<T>(name)
            ?? throw new InvalidOperationException($"Control '{name}' of type {typeof(T).Name} not found in logical tree.");
    }

    #endregion

    #region Find By Property

    /// <summary>
    /// Finds logical controls by a specific property value.
    /// </summary>
    /// <typeparam name="T">The type of controls to find.</typeparam>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="root">The root logical to search from.</param>
    /// <param name="property">The Avalonia property to check.</param>
    /// <param name="value">The expected property value.</param>
    /// <returns>All controls with the specified property value.</returns>
    public static IEnumerable<T> FindLogicalByProperty<T, TProperty>(
        this ILogical root,
        AvaloniaProperty<TProperty> property,
        TProperty value) where T : AvaloniaObject
    {
        return root.FindAllLogical<T>(c => Equals(c.GetValue(property), value));
    }

    /// <summary>
    /// Finds logical controls by Tag property.
    /// </summary>
    /// <param name="root">The root logical to search from.</param>
    /// <param name="tag">The tag value to search for.</param>
    /// <returns>All controls with the specified tag.</returns>
    public static IEnumerable<Control> FindLogicalByTag(this ILogical root, object tag)
    {
        return root.FindAllLogical<Control>(c => Equals(c.Tag, tag));
    }

    /// <summary>
    /// Finds logical controls by CSS classes.
    /// </summary>
    /// <typeparam name="T">The type of controls to find.</typeparam>
    /// <param name="root">The root logical to search from.</param>
    /// <param name="className">The CSS class name to search for.</param>
    /// <returns>All controls with the specified CSS class.</returns>
    public static IEnumerable<T> FindLogicalByClass<T>(this ILogical root, string className) where T : StyledElement
    {
        return root.FindAllLogical<T>(c => c.Classes.Contains(className));
    }

    #endregion

    #region Find By State

    /// <summary>
    /// Finds all enabled controls of a specific type in the logical tree.
    /// </summary>
    /// <typeparam name="T">The type of controls to find.</typeparam>
    /// <param name="root">The root logical to search from.</param>
    /// <returns>All enabled controls of the specified type.</returns>
    public static IEnumerable<T> FindLogicalEnabled<T>(this ILogical root) where T : InputElement
    {
        return root.FindAllLogical<T>(c => c.IsEnabled);
    }

    /// <summary>
    /// Finds all disabled controls of a specific type in the logical tree.
    /// </summary>
    /// <typeparam name="T">The type of controls to find.</typeparam>
    /// <param name="root">The root logical to search from.</param>
    /// <returns>All disabled controls of the specified type.</returns>
    public static IEnumerable<T> FindLogicalDisabled<T>(this ILogical root) where T : InputElement
    {
        return root.FindAllLogical<T>(c => !c.IsEnabled);
    }

    /// <summary>
    /// Finds all visible controls of a specific type in the logical tree.
    /// </summary>
    /// <typeparam name="T">The type of controls to find.</typeparam>
    /// <param name="root">The root logical to search from.</param>
    /// <returns>All visible controls of the specified type.</returns>
    public static IEnumerable<T> FindLogicalVisible<T>(this ILogical root) where T : Visual
    {
        return root.FindAllLogical<T>(c => c.IsVisible);
    }

    #endregion

    #region Ancestor Queries

    /// <summary>
    /// Finds the first logical ancestor of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of ancestor to find.</typeparam>
    /// <param name="logical">The logical to start from.</param>
    /// <param name="includeSelf">Whether to include the logical itself in the search.</param>
    /// <returns>The first ancestor of the specified type, or null if not found.</returns>
    public static T? FindLogicalAncestor<T>(this ILogical logical, bool includeSelf = false) where T : class
    {
        return logical.FindLogicalAncestorOfType<T>(includeSelf);
    }

    /// <summary>
    /// Finds a logical ancestor matching a predicate.
    /// </summary>
    /// <typeparam name="T">The type of ancestor to find.</typeparam>
    /// <param name="logical">The logical to start from.</param>
    /// <param name="predicate">The predicate to match.</param>
    /// <param name="includeSelf">Whether to include the logical itself in the search.</param>
    /// <returns>The first ancestor matching the predicate, or null if not found.</returns>
    public static T? FindLogicalAncestor<T>(this ILogical logical, Func<T, bool> predicate, bool includeSelf = false) where T : class
    {
        var ancestors = includeSelf
            ? logical.GetSelfAndLogicalAncestors()
            : logical.GetLogicalAncestors();
        return ancestors.OfType<T>().FirstOrDefault(predicate);
    }

    /// <summary>
    /// Gets all logical ancestors of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of ancestors to find.</typeparam>
    /// <param name="logical">The logical to start from.</param>
    /// <returns>All ancestors of the specified type.</returns>
    public static IEnumerable<T> GetLogicalAncestors<T>(this ILogical logical) where T : class
    {
        return logical.GetLogicalAncestors().OfType<T>();
    }

    /// <summary>
    /// Checks if the logical has an ancestor of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of ancestor to check for.</typeparam>
    /// <param name="logical">The logical to check.</param>
    /// <returns>True if an ancestor of the specified type exists.</returns>
    public static bool HasLogicalAncestor<T>(this ILogical logical) where T : class
    {
        return logical.FindLogicalAncestorOfType<T>() != null;
    }

    #endregion

    #region Children Queries

    /// <summary>
    /// Gets the direct logical children of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of children to get.</typeparam>
    /// <param name="logical">The parent logical.</param>
    /// <returns>Direct children of the specified type.</returns>
    public static IEnumerable<T> GetLogicalChildren<T>(this ILogical logical) where T : class
    {
        return logical.GetLogicalChildren().OfType<T>();
    }

    /// <summary>
    /// Gets the number of direct logical children.
    /// </summary>
    /// <param name="logical">The parent logical.</param>
    /// <returns>The count of direct logical children.</returns>
    public static int GetLogicalChildCount(this ILogical logical)
    {
        return logical.LogicalChildren.Count;
    }

    /// <summary>
    /// Gets the number of logical descendants (all levels).
    /// </summary>
    /// <param name="logical">The root logical.</param>
    /// <returns>The count of all logical descendants.</returns>
    public static int GetLogicalDescendantCount(this ILogical logical)
    {
        return logical.GetLogicalDescendants().Count();
    }

    /// <summary>
    /// Gets a logical child at a specific index.
    /// </summary>
    /// <param name="logical">The parent logical.</param>
    /// <param name="index">The index of the child.</param>
    /// <returns>The child at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
    public static ILogical GetLogicalChildAt(this ILogical logical, int index)
    {
        return logical.LogicalChildren[index];
    }

    /// <summary>
    /// Gets a logical child at a specific index, cast to a type.
    /// </summary>
    /// <typeparam name="T">The expected type of the child.</typeparam>
    /// <param name="logical">The parent logical.</param>
    /// <param name="index">The index of the child.</param>
    /// <returns>The child at the specified index, cast to the type.</returns>
    public static T GetLogicalChildAt<T>(this ILogical logical, int index) where T : ILogical
    {
        var child = logical.GetLogicalChildAt(index);
        return child is T typed ? typed : throw new InvalidCastException(
            $"Child at index {index} is {child.GetType().Name}, expected {typeof(T).Name}.");
    }

    /// <summary>
    /// Gets the first logical child of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of child to get.</typeparam>
    /// <param name="logical">The parent logical.</param>
    /// <returns>The first child of the specified type, or null if not found.</returns>
    public static T? GetFirstLogicalChild<T>(this ILogical logical) where T : class
    {
        return logical.LogicalChildren.OfType<T>().FirstOrDefault();
    }

    /// <summary>
    /// Gets the last logical child of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of child to get.</typeparam>
    /// <param name="logical">The parent logical.</param>
    /// <returns>The last child of the specified type, or null if not found.</returns>
    public static T? GetLastLogicalChild<T>(this ILogical logical) where T : class
    {
        return logical.LogicalChildren.OfType<T>().LastOrDefault();
    }

    #endregion

    #region Sibling Queries

    /// <summary>
    /// Gets the logical siblings of a specific type (excluding self).
    /// </summary>
    /// <typeparam name="T">The type of siblings to get.</typeparam>
    /// <param name="logical">The logical element.</param>
    /// <returns>All siblings of the specified type excluding self.</returns>
    public static IEnumerable<T> GetLogicalSiblings<T>(this ILogical logical) where T : class
    {
        return logical.GetLogicalSiblings().OfType<T>().Where(s => !ReferenceEquals(s, logical));
    }

    /// <summary>
    /// Gets the next sibling of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of sibling to find.</typeparam>
    /// <param name="logical">The logical element.</param>
    /// <returns>The next sibling of the specified type, or null if not found.</returns>
    public static T? GetNextLogicalSibling<T>(this ILogical logical) where T : class
    {
        var parent = logical.GetLogicalParent();
        if (parent == null)
            return null;

        var siblings = parent.LogicalChildren.ToList();
        var index = siblings.IndexOf(logical);
        if (index < 0 || index >= siblings.Count - 1)
            return null;

        for (int i = index + 1; i < siblings.Count; i++)
        {
            if (siblings[i] is T typed)
                return typed;
        }
        return null;
    }

    /// <summary>
    /// Gets the previous sibling of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of sibling to find.</typeparam>
    /// <param name="logical">The logical element.</param>
    /// <returns>The previous sibling of the specified type, or null if not found.</returns>
    public static T? GetPreviousLogicalSibling<T>(this ILogical logical) where T : class
    {
        var parent = logical.GetLogicalParent();
        if (parent == null)
            return null;

        var siblings = parent.LogicalChildren.ToList();
        var index = siblings.IndexOf(logical);
        if (index <= 0)
            return null;

        for (int i = index - 1; i >= 0; i--)
        {
            if (siblings[i] is T typed)
                return typed;
        }
        return null;
    }

    /// <summary>
    /// Gets the index of this logical among its siblings.
    /// </summary>
    /// <param name="logical">The logical element.</param>
    /// <returns>The index, or -1 if no parent.</returns>
    public static int GetLogicalSiblingIndex(this ILogical logical)
    {
        var parent = logical.GetLogicalParent();
        if (parent == null)
            return -1;
        return parent.LogicalChildren.ToList().IndexOf(logical);
    }

    /// <summary>
    /// Checks if this is the first child of its parent.
    /// </summary>
    /// <param name="logical">The logical element.</param>
    /// <returns>True if this is the first child.</returns>
    public static bool IsFirstLogicalChild(this ILogical logical)
    {
        return logical.GetLogicalSiblingIndex() == 0;
    }

    /// <summary>
    /// Checks if this is the last child of its parent.
    /// </summary>
    /// <param name="logical">The logical element.</param>
    /// <returns>True if this is the last child.</returns>
    public static bool IsLastLogicalChild(this ILogical logical)
    {
        var parent = logical.GetLogicalParent();
        if (parent == null)
            return false;
        return logical.GetLogicalSiblingIndex() == parent.LogicalChildren.Count - 1;
    }

    /// <summary>
    /// Checks if this logical is an only child.
    /// </summary>
    /// <param name="logical">The logical element.</param>
    /// <returns>True if this is an only child.</returns>
    public static bool IsOnlyLogicalChild(this ILogical logical)
    {
        var parent = logical.GetLogicalParent();
        return parent?.LogicalChildren.Count == 1;
    }

    #endregion

    #region Tree Path

    /// <summary>
    /// Gets the logical path from root to the specified logical.
    /// </summary>
    /// <param name="logical">The logical to get the path to.</param>
    /// <returns>The path as a list of logicals from root to the logical.</returns>
    public static IReadOnlyList<ILogical> GetLogicalPathFromRoot(this ILogical logical)
    {
        var path = new List<ILogical>();
        ILogical? current = logical;
        while (current != null)
        {
            path.Insert(0, current);
            current = current.GetLogicalParent();
        }
        return path;
    }

    /// <summary>
    /// Gets the logical path as type names for debugging.
    /// </summary>
    /// <param name="logical">The logical to get the path for.</param>
    /// <returns>A string representing the path of type names.</returns>
    public static string GetLogicalPathString(this ILogical logical)
    {
        var path = logical.GetLogicalPathFromRoot();
        return string.Join(" > ", path.Select(l => l.GetType().Name));
    }

    /// <summary>
    /// Gets a detailed logical path string including names where available.
    /// </summary>
    /// <param name="logical">The logical to get the path for.</param>
    /// <returns>A detailed string representing the path.</returns>
    public static string GetDetailedLogicalPathString(this ILogical logical)
    {
        var path = logical.GetLogicalPathFromRoot();
        return string.Join(" > ", path.Select(l =>
        {
            var name = (l as Control)?.Name;
            return string.IsNullOrEmpty(name) ? l.GetType().Name : $"{l.GetType().Name}[{name}]";
        }));
    }

    #endregion

    #region Tree Validation

    /// <summary>
    /// Checks if a logical contains a descendant of a specific type.
    /// </summary>
    /// <typeparam name="T">The type to check for.</typeparam>
    /// <param name="root">The root logical to search from.</param>
    /// <returns>True if a descendant of the specified type exists.</returns>
    public static bool ContainsLogical<T>(this ILogical root) where T : class
    {
        return root.FindLogicalDescendantOfType<T>() != null;
    }

    /// <summary>
    /// Checks if a logical contains a descendant with a specific name.
    /// </summary>
    /// <param name="root">The root logical to search from.</param>
    /// <param name="name">The name to search for.</param>
    /// <returns>True if a descendant with the specified name exists.</returns>
    public static bool ContainsLogicalNamed(this ILogical root, string name)
    {
        return root.FindLogicalByName(name) != null;
    }

    /// <summary>
    /// Counts all logical descendants of a specific type.
    /// </summary>
    /// <typeparam name="T">The type to count.</typeparam>
    /// <param name="root">The root logical to search from.</param>
    /// <returns>The number of descendants of the specified type.</returns>
    public static int CountLogical<T>(this ILogical root) where T : class
    {
        return root.FindAllLogical<T>().Count();
    }

    /// <summary>
    /// Checks if the logical tree structure matches expected types at each level.
    /// </summary>
    /// <param name="logical">The starting logical.</param>
    /// <param name="expectedPath">The expected types from this logical downward.</param>
    /// <returns>True if the tree structure matches.</returns>
    public static bool MatchesLogicalStructure(this ILogical logical, params Type[] expectedPath)
    {
        if (expectedPath.Length == 0)
            return true;

        if (!expectedPath[0].IsInstanceOfType(logical))
            return false;

        if (expectedPath.Length == 1)
            return true;

        var firstChild = logical.LogicalChildren.FirstOrDefault();
        if (firstChild == null)
            return false;

        return firstChild.MatchesLogicalStructure(expectedPath.Skip(1).ToArray());
    }

    #endregion

    #region Content Host Queries

    /// <summary>
    /// Gets the content of a ContentControl in the logical tree.
    /// </summary>
    /// <typeparam name="T">The expected type of the content.</typeparam>
    /// <param name="contentControl">The content control.</param>
    /// <returns>The content cast to the specified type, or null if not matching.</returns>
    public static T? GetContent<T>(this ContentControl contentControl) where T : class
    {
        return contentControl.Content as T;
    }

    /// <summary>
    /// Gets all items from an ItemsControl as a typed enumerable.
    /// </summary>
    /// <typeparam name="T">The expected type of the items.</typeparam>
    /// <param name="itemsControl">The items control.</param>
    /// <returns>The items cast to the specified type.</returns>
    public static IEnumerable<T> GetItems<T>(this ItemsControl itemsControl) where T : class
    {
        return itemsControl.Items.OfType<T>();
    }

    /// <summary>
    /// Gets an item at a specific index from an ItemsControl.
    /// </summary>
    /// <typeparam name="T">The expected type of the item.</typeparam>
    /// <param name="itemsControl">The items control.</param>
    /// <param name="index">The index of the item.</param>
    /// <returns>The item at the specified index.</returns>
    public static T? GetItemAt<T>(this ItemsControl itemsControl, int index) where T : class
    {
        var items = itemsControl.Items;
        if (index < 0 || index >= items.Count)
            return null;
        return items[index] as T;
    }

    /// <summary>
    /// Gets the item count from an ItemsControl.
    /// </summary>
    /// <param name="itemsControl">The items control.</param>
    /// <returns>The number of items.</returns>
    public static int GetItemCount(this ItemsControl itemsControl)
    {
        return itemsControl.Items.Count;
    }

    #endregion

    #region Debug Helpers

    /// <summary>
    /// Prints the logical tree structure to a string for debugging.
    /// </summary>
    /// <param name="root">The root logical to print from.</param>
    /// <param name="maxDepth">Maximum depth to print (default is unlimited).</param>
    /// <returns>A string representation of the tree structure.</returns>
    public static string PrintLogicalTree(this ILogical root, int maxDepth = int.MaxValue)
    {
        var lines = new List<string>();
        PrintLogicalTreeInternal(root, lines, "", true, 0, maxDepth);
        return string.Join(Environment.NewLine, lines);
    }

    private static void PrintLogicalTreeInternal(ILogical logical, List<string> lines, string prefix, bool isLast, int depth, int maxDepth)
    {
        if (depth > maxDepth)
            return;

        var name = (logical as Control)?.Name;
        var nameStr = string.IsNullOrEmpty(name) ? "" : $" [{name}]";
        var visibilityStr = (logical as Visual)?.IsVisible == false ? " (hidden)" : "";
        var enabledStr = (logical as InputElement)?.IsEnabled == false ? " (disabled)" : "";

        lines.Add($"{prefix}{(isLast ? "└── " : "├── ")}{logical.GetType().Name}{nameStr}{visibilityStr}{enabledStr}");

        var children = logical.LogicalChildren.ToList();
        for (int i = 0; i < children.Count; i++)
        {
            var newPrefix = prefix + (isLast ? "    " : "│   ");
            PrintLogicalTreeInternal(children[i], lines, newPrefix, i == children.Count - 1, depth + 1, maxDepth);
        }
    }

    /// <summary>
    /// Gets a summary of control types in the logical tree.
    /// </summary>
    /// <param name="root">The root logical to analyze.</param>
    /// <returns>A dictionary of type names and their counts.</returns>
    public static Dictionary<string, int> GetLogicalTypeSummary(this ILogical root)
    {
        return root.GetSelfAndLogicalDescendants()
            .GroupBy(l => l.GetType().Name)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>
    /// Gets all named controls in the logical tree.
    /// </summary>
    /// <param name="root">The root logical to search from.</param>
    /// <returns>A dictionary of control names to controls.</returns>
    public static Dictionary<string, Control> GetLogicalNamedControls(this ILogical root)
    {
        return root.FindAllLogical<Control>(c => !string.IsNullOrEmpty(c.Name))
            .ToDictionary(c => c.Name!, c => c);
    }

    /// <summary>
    /// Compares logical and visual tree depths for a control.
    /// Useful for understanding template expansion.
    /// </summary>
    /// <param name="control">The control to analyze.</param>
    /// <returns>A tuple of (logical depth, visual depth).</returns>
    public static (int LogicalDepth, int VisualDepth) CompareTreeDepths(this Control control)
    {
        var logicalDepth = control.GetLogicalPathFromRoot().Count;
        var visualDepth = control is Visual visual
            ? visual.GetPathFromRoot().Count
            : 0;
        return (logicalDepth, visualDepth);
    }

    #endregion
}

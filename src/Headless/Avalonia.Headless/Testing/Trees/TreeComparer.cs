// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace Avalonia.HeadlessTestingFramework;

/// <summary>
/// Result of a tree comparison operation.
/// </summary>
public class TreeComparisonResult
{
    /// <summary>
    /// Gets whether the trees are equal.
    /// </summary>
    public bool AreEqual => Differences.Count == 0;

    /// <summary>
    /// Gets the list of differences found.
    /// </summary>
    public List<TreeDifference> Differences { get; } = new();

    /// <summary>
    /// Gets the total node count in the expected tree.
    /// </summary>
    public int ExpectedNodeCount { get; set; }

    /// <summary>
    /// Gets the total node count in the actual tree.
    /// </summary>
    public int ActualNodeCount { get; set; }

    /// <summary>
    /// Gets the number of matching nodes.
    /// </summary>
    public int MatchingNodeCount { get; set; }

    /// <summary>
    /// Gets the match percentage (0-100).
    /// </summary>
    public double MatchPercentage => ExpectedNodeCount > 0
        ? (MatchingNodeCount / (double)ExpectedNodeCount) * 100
        : 100;

    /// <summary>
    /// Returns a summary of the comparison.
    /// </summary>
    public override string ToString()
    {
        if (AreEqual)
            return $"Trees are equal ({ExpectedNodeCount} nodes)";

        return $"Trees differ: {Differences.Count} differences, {MatchPercentage:F1}% match ({MatchingNodeCount}/{ExpectedNodeCount} nodes)";
    }
}

/// <summary>
/// Represents a difference between two trees.
/// </summary>
public class TreeDifference
{
    /// <summary>
    /// Gets the type of difference.
    /// </summary>
    public TreeDifferenceType Type { get; set; }

    /// <summary>
    /// Gets the path to the differing node.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets the expected value or node.
    /// </summary>
    public string? Expected { get; set; }

    /// <summary>
    /// Gets the actual value or node.
    /// </summary>
    public string? Actual { get; set; }

    /// <summary>
    /// Gets additional details about the difference.
    /// </summary>
    public string? Details { get; set; }

    public override string ToString()
    {
        return Type switch
        {
            TreeDifferenceType.MissingNode => $"Missing: {Path} (expected {Expected})",
            TreeDifferenceType.ExtraNode => $"Extra: {Path} ({Actual})",
            TreeDifferenceType.TypeMismatch => $"Type mismatch at {Path}: expected {Expected}, got {Actual}",
            TreeDifferenceType.PropertyMismatch => $"Property mismatch at {Path}: {Details}",
            TreeDifferenceType.ChildCountMismatch => $"Child count mismatch at {Path}: expected {Expected}, got {Actual}",
            TreeDifferenceType.OrderMismatch => $"Order mismatch at {Path}: {Details}",
            TreeDifferenceType.NameMismatch => $"Name mismatch at {Path}: expected {Expected}, got {Actual}",
            _ => $"{Type} at {Path}"
        };
    }
}

/// <summary>
/// Types of tree differences.
/// </summary>
public enum TreeDifferenceType
{
    /// <summary>Node exists in expected but not actual.</summary>
    MissingNode,
    /// <summary>Node exists in actual but not expected.</summary>
    ExtraNode,
    /// <summary>Node types don't match.</summary>
    TypeMismatch,
    /// <summary>Property values don't match.</summary>
    PropertyMismatch,
    /// <summary>Number of children doesn't match.</summary>
    ChildCountMismatch,
    /// <summary>Child order doesn't match.</summary>
    OrderMismatch,
    /// <summary>Node names don't match.</summary>
    NameMismatch
}

/// <summary>
/// Options for tree comparison.
/// </summary>
public class TreeComparisonOptions
{
    /// <summary>
    /// Gets or sets whether to compare node types.
    /// </summary>
    public bool CompareTypes { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to compare node names.
    /// </summary>
    public bool CompareNames { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to compare child order.
    /// </summary>
    public bool CompareOrder { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to compare child counts strictly.
    /// </summary>
    public bool CompareChildCount { get; set; } = true;

    /// <summary>
    /// Gets or sets the properties to compare.
    /// </summary>
    public List<AvaloniaProperty>? PropertiesToCompare { get; set; }

    /// <summary>
    /// Gets or sets types to ignore during comparison.
    /// </summary>
    public HashSet<Type>? IgnoreTypes { get; set; }

    /// <summary>
    /// Gets or sets names to ignore during comparison.
    /// </summary>
    public HashSet<string>? IgnoreNames { get; set; }

    /// <summary>
    /// Gets or sets the maximum depth to compare (-1 for unlimited).
    /// </summary>
    public int MaxDepth { get; set; } = -1;

    /// <summary>
    /// Gets or sets whether to ignore internal controls (PART_*).
    /// </summary>
    public bool IgnoreInternalControls { get; set; } = false;

    /// <summary>
    /// Default comparison options.
    /// </summary>
    public static TreeComparisonOptions Default => new();

    /// <summary>
    /// Strict comparison options (compares everything).
    /// </summary>
    public static TreeComparisonOptions Strict => new()
    {
        CompareTypes = true,
        CompareNames = true,
        CompareOrder = true,
        CompareChildCount = true
    };

    /// <summary>
    /// Lenient comparison options (structure only).
    /// </summary>
    public static TreeComparisonOptions StructureOnly => new()
    {
        CompareTypes = true,
        CompareNames = false,
        CompareOrder = false,
        CompareChildCount = true
    };
}

/// <summary>
/// Compares visual and logical trees for testing and validation.
/// </summary>
public static class TreeComparer
{
    #region Visual Tree Comparison

    /// <summary>
    /// Compares two visual trees.
    /// </summary>
    /// <param name="expected">The expected visual tree root.</param>
    /// <param name="actual">The actual visual tree root.</param>
    /// <param name="options">Comparison options.</param>
    /// <returns>The comparison result.</returns>
    public static TreeComparisonResult CompareVisualTrees(
        Visual expected,
        Visual actual,
        TreeComparisonOptions? options = null)
    {
        options ??= TreeComparisonOptions.Default;
        var result = new TreeComparisonResult();

        CompareVisualNode(expected, actual, "", 0, options, result);

        result.ExpectedNodeCount = CountVisualNodes(expected, options);
        result.ActualNodeCount = CountVisualNodes(actual, options);
        result.MatchingNodeCount = result.ExpectedNodeCount - result.Differences.Count(d =>
            d.Type == TreeDifferenceType.MissingNode ||
            d.Type == TreeDifferenceType.TypeMismatch);

        return result;
    }

    private static void CompareVisualNode(
        Visual? expected,
        Visual? actual,
        string path,
        int depth,
        TreeComparisonOptions options,
        TreeComparisonResult result)
    {
        if (options.MaxDepth >= 0 && depth > options.MaxDepth)
            return;

        if (expected == null && actual == null)
            return;

        if (expected == null)
        {
            result.Differences.Add(new TreeDifference
            {
                Type = TreeDifferenceType.ExtraNode,
                Path = path,
                Actual = actual?.GetType().Name
            });
            return;
        }

        if (actual == null)
        {
            result.Differences.Add(new TreeDifference
            {
                Type = TreeDifferenceType.MissingNode,
                Path = path,
                Expected = expected.GetType().Name
            });
            return;
        }

        // Check if should ignore
        if (ShouldIgnore(expected, options) || ShouldIgnore(actual, options))
            return;

        var currentPath = string.IsNullOrEmpty(path)
            ? expected.GetType().Name
            : $"{path}/{expected.GetType().Name}";

        // Compare types
        if (options.CompareTypes && expected.GetType() != actual.GetType())
        {
            result.Differences.Add(new TreeDifference
            {
                Type = TreeDifferenceType.TypeMismatch,
                Path = currentPath,
                Expected = expected.GetType().Name,
                Actual = actual.GetType().Name
            });
            return; // Don't compare children if types differ
        }

        // Compare names
        if (options.CompareNames)
        {
            var expectedName = (expected as Control)?.Name;
            var actualName = (actual as Control)?.Name;
            if (!string.IsNullOrEmpty(expectedName) && expectedName != actualName)
            {
                result.Differences.Add(new TreeDifference
                {
                    Type = TreeDifferenceType.NameMismatch,
                    Path = currentPath,
                    Expected = expectedName,
                    Actual = actualName ?? "(null)"
                });
            }
        }

        // Compare properties
        if (options.PropertiesToCompare != null)
        {
            foreach (var property in options.PropertiesToCompare)
            {
                CompareProperty(expected, actual, property, currentPath, result);
            }
        }

        // Compare children
        var expectedChildren = GetVisualChildren(expected, options).ToList();
        var actualChildren = GetVisualChildren(actual, options).ToList();

        if (options.CompareChildCount && expectedChildren.Count != actualChildren.Count)
        {
            result.Differences.Add(new TreeDifference
            {
                Type = TreeDifferenceType.ChildCountMismatch,
                Path = currentPath,
                Expected = expectedChildren.Count.ToString(),
                Actual = actualChildren.Count.ToString()
            });
        }

        // Compare each child
        var minCount = Math.Min(expectedChildren.Count, actualChildren.Count);
        for (int i = 0; i < minCount; i++)
        {
            var childPath = $"{currentPath}[{i}]";
            CompareVisualNode(expectedChildren[i], actualChildren[i], childPath, depth + 1, options, result);
        }

        // Report missing children
        for (int i = minCount; i < expectedChildren.Count; i++)
        {
            result.Differences.Add(new TreeDifference
            {
                Type = TreeDifferenceType.MissingNode,
                Path = $"{currentPath}[{i}]",
                Expected = expectedChildren[i].GetType().Name
            });
        }

        // Report extra children
        for (int i = minCount; i < actualChildren.Count; i++)
        {
            result.Differences.Add(new TreeDifference
            {
                Type = TreeDifferenceType.ExtraNode,
                Path = $"{currentPath}[{i}]",
                Actual = actualChildren[i].GetType().Name
            });
        }
    }

    #endregion

    #region Logical Tree Comparison

    /// <summary>
    /// Compares two logical trees.
    /// </summary>
    /// <param name="expected">The expected logical tree root.</param>
    /// <param name="actual">The actual logical tree root.</param>
    /// <param name="options">Comparison options.</param>
    /// <returns>The comparison result.</returns>
    public static TreeComparisonResult CompareLogicalTrees(
        ILogical expected,
        ILogical actual,
        TreeComparisonOptions? options = null)
    {
        options ??= TreeComparisonOptions.Default;
        var result = new TreeComparisonResult();

        CompareLogicalNode(expected, actual, "", 0, options, result);

        result.ExpectedNodeCount = CountLogicalNodes(expected, options);
        result.ActualNodeCount = CountLogicalNodes(actual, options);
        result.MatchingNodeCount = result.ExpectedNodeCount - result.Differences.Count(d =>
            d.Type == TreeDifferenceType.MissingNode ||
            d.Type == TreeDifferenceType.TypeMismatch);

        return result;
    }

    private static void CompareLogicalNode(
        ILogical? expected,
        ILogical? actual,
        string path,
        int depth,
        TreeComparisonOptions options,
        TreeComparisonResult result)
    {
        if (options.MaxDepth >= 0 && depth > options.MaxDepth)
            return;

        if (expected == null && actual == null)
            return;

        if (expected == null)
        {
            result.Differences.Add(new TreeDifference
            {
                Type = TreeDifferenceType.ExtraNode,
                Path = path,
                Actual = actual?.GetType().Name
            });
            return;
        }

        if (actual == null)
        {
            result.Differences.Add(new TreeDifference
            {
                Type = TreeDifferenceType.MissingNode,
                Path = path,
                Expected = expected.GetType().Name
            });
            return;
        }

        // Check if should ignore
        if (ShouldIgnoreLogical(expected, options) || ShouldIgnoreLogical(actual, options))
            return;

        var currentPath = string.IsNullOrEmpty(path)
            ? expected.GetType().Name
            : $"{path}/{expected.GetType().Name}";

        // Compare types
        if (options.CompareTypes && expected.GetType() != actual.GetType())
        {
            result.Differences.Add(new TreeDifference
            {
                Type = TreeDifferenceType.TypeMismatch,
                Path = currentPath,
                Expected = expected.GetType().Name,
                Actual = actual.GetType().Name
            });
            return;
        }

        // Compare names
        if (options.CompareNames)
        {
            var expectedName = (expected as Control)?.Name;
            var actualName = (actual as Control)?.Name;
            if (!string.IsNullOrEmpty(expectedName) && expectedName != actualName)
            {
                result.Differences.Add(new TreeDifference
                {
                    Type = TreeDifferenceType.NameMismatch,
                    Path = currentPath,
                    Expected = expectedName,
                    Actual = actualName ?? "(null)"
                });
            }
        }

        // Compare children
        var expectedChildren = GetLogicalChildren(expected, options).ToList();
        var actualChildren = GetLogicalChildren(actual, options).ToList();

        if (options.CompareChildCount && expectedChildren.Count != actualChildren.Count)
        {
            result.Differences.Add(new TreeDifference
            {
                Type = TreeDifferenceType.ChildCountMismatch,
                Path = currentPath,
                Expected = expectedChildren.Count.ToString(),
                Actual = actualChildren.Count.ToString()
            });
        }

        var minCount = Math.Min(expectedChildren.Count, actualChildren.Count);
        for (int i = 0; i < minCount; i++)
        {
            var childPath = $"{currentPath}[{i}]";
            CompareLogicalNode(expectedChildren[i], actualChildren[i], childPath, depth + 1, options, result);
        }

        for (int i = minCount; i < expectedChildren.Count; i++)
        {
            result.Differences.Add(new TreeDifference
            {
                Type = TreeDifferenceType.MissingNode,
                Path = $"{currentPath}[{i}]",
                Expected = expectedChildren[i].GetType().Name
            });
        }

        for (int i = minCount; i < actualChildren.Count; i++)
        {
            result.Differences.Add(new TreeDifference
            {
                Type = TreeDifferenceType.ExtraNode,
                Path = $"{currentPath}[{i}]",
                Actual = actualChildren[i].GetType().Name
            });
        }
    }

    #endregion

    #region Tree Snapshots

    /// <summary>
    /// Creates a snapshot of a visual tree structure.
    /// </summary>
    /// <param name="root">The root visual.</param>
    /// <param name="options">Snapshot options.</param>
    /// <returns>A tree snapshot.</returns>
    public static TreeSnapshot CreateVisualSnapshot(Visual root, TreeComparisonOptions? options = null)
    {
        options ??= TreeComparisonOptions.Default;
        var snapshot = new TreeSnapshot
        {
            TreeType = TreeSnapshotType.Visual,
            RootType = root.GetType().FullName ?? root.GetType().Name,
            CreatedAt = DateTime.UtcNow
        };

        BuildVisualSnapshot(root, snapshot.Root, 0, options);
        return snapshot;
    }

    /// <summary>
    /// Creates a snapshot of a logical tree structure.
    /// </summary>
    /// <param name="root">The root logical.</param>
    /// <param name="options">Snapshot options.</param>
    /// <returns>A tree snapshot.</returns>
    public static TreeSnapshot CreateLogicalSnapshot(ILogical root, TreeComparisonOptions? options = null)
    {
        options ??= TreeComparisonOptions.Default;
        var snapshot = new TreeSnapshot
        {
            TreeType = TreeSnapshotType.Logical,
            RootType = root.GetType().FullName ?? root.GetType().Name,
            CreatedAt = DateTime.UtcNow
        };

        BuildLogicalSnapshot(root, snapshot.Root, 0, options);
        return snapshot;
    }

    /// <summary>
    /// Compares a visual tree against a snapshot.
    /// </summary>
    /// <param name="actual">The actual visual tree.</param>
    /// <param name="snapshot">The expected snapshot.</param>
    /// <param name="options">Comparison options.</param>
    /// <returns>The comparison result.</returns>
    public static TreeComparisonResult CompareToSnapshot(
        Visual actual,
        TreeSnapshot snapshot,
        TreeComparisonOptions? options = null)
    {
        options ??= TreeComparisonOptions.Default;
        var result = new TreeComparisonResult();

        CompareToSnapshotNode(actual, snapshot.Root, "", 0, options, result);

        result.ExpectedNodeCount = CountSnapshotNodes(snapshot.Root);
        result.ActualNodeCount = CountVisualNodes(actual, options);
        result.MatchingNodeCount = result.ExpectedNodeCount - result.Differences.Count(d =>
            d.Type == TreeDifferenceType.MissingNode ||
            d.Type == TreeDifferenceType.TypeMismatch);

        return result;
    }

    private static void BuildVisualSnapshot(Visual visual, TreeSnapshotNode node, int depth, TreeComparisonOptions options)
    {
        if (options.MaxDepth >= 0 && depth > options.MaxDepth)
            return;

        if (ShouldIgnore(visual, options))
            return;

        node.TypeName = visual.GetType().Name;
        node.FullTypeName = visual.GetType().FullName ?? visual.GetType().Name;
        node.Name = (visual as Control)?.Name;

        if (options.PropertiesToCompare != null)
        {
            foreach (var prop in options.PropertiesToCompare)
            {
                if (visual is AvaloniaObject ao)
                {
                    var value = ao.GetValue(prop);
                    node.Properties[prop.Name] = value?.ToString();
                }
            }
        }

        foreach (var child in GetVisualChildren(visual, options))
        {
            var childNode = new TreeSnapshotNode();
            BuildVisualSnapshot(child, childNode, depth + 1, options);
            if (!string.IsNullOrEmpty(childNode.TypeName))
            {
                node.Children.Add(childNode);
            }
        }
    }

    private static void BuildLogicalSnapshot(ILogical logical, TreeSnapshotNode node, int depth, TreeComparisonOptions options)
    {
        if (options.MaxDepth >= 0 && depth > options.MaxDepth)
            return;

        if (ShouldIgnoreLogical(logical, options))
            return;

        node.TypeName = logical.GetType().Name;
        node.FullTypeName = logical.GetType().FullName ?? logical.GetType().Name;
        node.Name = (logical as Control)?.Name;

        foreach (var child in GetLogicalChildren(logical, options))
        {
            var childNode = new TreeSnapshotNode();
            BuildLogicalSnapshot(child, childNode, depth + 1, options);
            if (!string.IsNullOrEmpty(childNode.TypeName))
            {
                node.Children.Add(childNode);
            }
        }
    }

    private static void CompareToSnapshotNode(
        Visual? actual,
        TreeSnapshotNode expected,
        string path,
        int depth,
        TreeComparisonOptions options,
        TreeComparisonResult result)
    {
        if (options.MaxDepth >= 0 && depth > options.MaxDepth)
            return;

        var currentPath = string.IsNullOrEmpty(path)
            ? expected.TypeName
            : $"{path}/{expected.TypeName}";

        if (actual == null)
        {
            result.Differences.Add(new TreeDifference
            {
                Type = TreeDifferenceType.MissingNode,
                Path = currentPath,
                Expected = expected.TypeName
            });
            return;
        }

        // Compare types
        if (options.CompareTypes && actual.GetType().Name != expected.TypeName)
        {
            result.Differences.Add(new TreeDifference
            {
                Type = TreeDifferenceType.TypeMismatch,
                Path = currentPath,
                Expected = expected.TypeName,
                Actual = actual.GetType().Name
            });
            return;
        }

        // Compare names
        if (options.CompareNames && !string.IsNullOrEmpty(expected.Name))
        {
            var actualName = (actual as Control)?.Name;
            if (expected.Name != actualName)
            {
                result.Differences.Add(new TreeDifference
                {
                    Type = TreeDifferenceType.NameMismatch,
                    Path = currentPath,
                    Expected = expected.Name,
                    Actual = actualName ?? "(null)"
                });
            }
        }

        // Compare children
        var actualChildren = GetVisualChildren(actual, options).ToList();

        if (options.CompareChildCount && expected.Children.Count != actualChildren.Count)
        {
            result.Differences.Add(new TreeDifference
            {
                Type = TreeDifferenceType.ChildCountMismatch,
                Path = currentPath,
                Expected = expected.Children.Count.ToString(),
                Actual = actualChildren.Count.ToString()
            });
        }

        var minCount = Math.Min(expected.Children.Count, actualChildren.Count);
        for (int i = 0; i < minCount; i++)
        {
            var childPath = $"{currentPath}[{i}]";
            CompareToSnapshotNode(actualChildren[i], expected.Children[i], childPath, depth + 1, options, result);
        }

        for (int i = minCount; i < expected.Children.Count; i++)
        {
            result.Differences.Add(new TreeDifference
            {
                Type = TreeDifferenceType.MissingNode,
                Path = $"{currentPath}[{i}]",
                Expected = expected.Children[i].TypeName
            });
        }
    }

    private static int CountSnapshotNodes(TreeSnapshotNode node)
    {
        return 1 + node.Children.Sum(CountSnapshotNodes);
    }

    #endregion

    #region Helper Methods

    private static void CompareProperty(
        Visual expected,
        Visual actual,
        AvaloniaProperty property,
        string path,
        TreeComparisonResult result)
    {
        if (expected is not AvaloniaObject expectedAo || actual is not AvaloniaObject actualAo)
            return;

        var expectedValue = expectedAo.GetValue(property);
        var actualValue = actualAo.GetValue(property);

        if (!Equals(expectedValue, actualValue))
        {
            result.Differences.Add(new TreeDifference
            {
                Type = TreeDifferenceType.PropertyMismatch,
                Path = path,
                Expected = expectedValue?.ToString(),
                Actual = actualValue?.ToString(),
                Details = $"{property.Name}: expected '{expectedValue}', got '{actualValue}'"
            });
        }
    }

    private static bool ShouldIgnore(Visual visual, TreeComparisonOptions options)
    {
        if (options.IgnoreTypes?.Contains(visual.GetType()) == true)
            return true;

        if (visual is Control control)
        {
            if (options.IgnoreNames?.Contains(control.Name ?? "") == true)
                return true;

            if (options.IgnoreInternalControls && control.Name?.StartsWith("PART_") == true)
                return true;
        }

        return false;
    }

    private static bool ShouldIgnoreLogical(ILogical logical, TreeComparisonOptions options)
    {
        if (options.IgnoreTypes?.Contains(logical.GetType()) == true)
            return true;

        if (logical is Control control)
        {
            if (options.IgnoreNames?.Contains(control.Name ?? "") == true)
                return true;

            if (options.IgnoreInternalControls && control.Name?.StartsWith("PART_") == true)
                return true;
        }

        return false;
    }

    private static IEnumerable<Visual> GetVisualChildren(Visual visual, TreeComparisonOptions options)
    {
        return visual.GetVisualChildren()
            .Where(c => !ShouldIgnore(c, options));
    }

    private static IEnumerable<ILogical> GetLogicalChildren(ILogical logical, TreeComparisonOptions options)
    {
        return logical.LogicalChildren
            .Where(c => !ShouldIgnoreLogical(c, options));
    }

    private static int CountVisualNodes(Visual visual, TreeComparisonOptions options)
    {
        if (ShouldIgnore(visual, options))
            return 0;
        return 1 + GetVisualChildren(visual, options).Sum(c => CountVisualNodes(c, options));
    }

    private static int CountLogicalNodes(ILogical logical, TreeComparisonOptions options)
    {
        if (ShouldIgnoreLogical(logical, options))
            return 0;
        return 1 + GetLogicalChildren(logical, options).Sum(c => CountLogicalNodes(c, options));
    }

    #endregion

    #region Extension Methods

    /// <summary>
    /// Compares this visual tree to another.
    /// </summary>
    public static TreeComparisonResult CompareVisualTreeTo(
        this Visual expected,
        Visual actual,
        TreeComparisonOptions? options = null)
    {
        return CompareVisualTrees(expected, actual, options);
    }

    /// <summary>
    /// Compares this logical tree to another.
    /// </summary>
    public static TreeComparisonResult CompareLogicalTreeTo(
        this ILogical expected,
        ILogical actual,
        TreeComparisonOptions? options = null)
    {
        return CompareLogicalTrees(expected, actual, options);
    }

    /// <summary>
    /// Creates a visual tree snapshot.
    /// </summary>
    public static TreeSnapshot SnapshotVisualTree(
        this Visual root,
        TreeComparisonOptions? options = null)
    {
        return CreateVisualSnapshot(root, options);
    }

    /// <summary>
    /// Creates a logical tree snapshot.
    /// </summary>
    public static TreeSnapshot SnapshotLogicalTree(
        this ILogical root,
        TreeComparisonOptions? options = null)
    {
        return CreateLogicalSnapshot(root, options);
    }

    /// <summary>
    /// Verifies the visual tree matches the snapshot.
    /// </summary>
    public static TreeComparisonResult VerifyAgainstSnapshot(
        this Visual actual,
        TreeSnapshot snapshot,
        TreeComparisonOptions? options = null)
    {
        return CompareToSnapshot(actual, snapshot, options);
    }

    #endregion
}

/// <summary>
/// A snapshot of a tree structure.
/// </summary>
public class TreeSnapshot
{
    /// <summary>
    /// Gets or sets the type of tree.
    /// </summary>
    public TreeSnapshotType TreeType { get; set; }

    /// <summary>
    /// Gets or sets the root type name.
    /// </summary>
    public string RootType { get; set; } = string.Empty;

    /// <summary>
    /// Gets the root node of the snapshot.
    /// </summary>
    public TreeSnapshotNode Root { get; } = new();

    /// <summary>
    /// Gets or sets when the snapshot was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets a description of the snapshot.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Type of tree snapshot.
/// </summary>
public enum TreeSnapshotType
{
    /// <summary>Visual tree snapshot.</summary>
    Visual,
    /// <summary>Logical tree snapshot.</summary>
    Logical
}

/// <summary>
/// A node in a tree snapshot.
/// </summary>
public class TreeSnapshotNode
{
    /// <summary>
    /// Gets or sets the short type name.
    /// </summary>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full type name.
    /// </summary>
    public string FullTypeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the control name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets the property values.
    /// </summary>
    public Dictionary<string, string?> Properties { get; } = new();

    /// <summary>
    /// Gets the child nodes.
    /// </summary>
    public List<TreeSnapshotNode> Children { get; } = new();
}

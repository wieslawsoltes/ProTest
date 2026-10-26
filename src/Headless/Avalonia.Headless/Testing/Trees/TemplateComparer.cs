// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Avalonia.HeadlessTestingFramework;

/// <summary>
/// Result of a template comparison.
/// </summary>
public class TemplateComparisonResult
{
    /// <summary>
    /// Gets whether the templates are equivalent.
    /// </summary>
    public bool AreEqual => Differences.Count == 0;

    /// <summary>
    /// Gets the list of differences found.
    /// </summary>
    public List<TemplateDifference> Differences { get; } = new();

    /// <summary>
    /// Gets information about the expected template.
    /// </summary>
    public TemplateInfo? ExpectedTemplate { get; set; }

    /// <summary>
    /// Gets information about the actual template.
    /// </summary>
    public TemplateInfo? ActualTemplate { get; set; }

    /// <summary>
    /// Returns a summary of the comparison.
    /// </summary>
    public override string ToString()
    {
        if (AreEqual)
            return "Templates are equivalent";

        return $"Templates differ: {Differences.Count} differences found";
    }
}

/// <summary>
/// Information about a control template.
/// </summary>
public class TemplateInfo
{
    /// <summary>
    /// Gets or sets the target type of the template.
    /// </summary>
    public Type? TargetType { get; set; }

    /// <summary>
    /// Gets or sets the number of template parts.
    /// </summary>
    public int PartCount { get; set; }

    /// <summary>
    /// Gets the template parts.
    /// </summary>
    public List<TemplatePartInfo> Parts { get; } = new();

    /// <summary>
    /// Gets or sets the visual tree depth.
    /// </summary>
    public int TreeDepth { get; set; }

    /// <summary>
    /// Gets or sets the total visual count.
    /// </summary>
    public int VisualCount { get; set; }
}

/// <summary>
/// Information about a template part.
/// </summary>
public class TemplatePartInfo
{
    /// <summary>
    /// Gets or sets the part name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the part type.
    /// </summary>
    public Type? Type { get; set; }

    /// <summary>
    /// Gets or sets whether the part is required.
    /// </summary>
    public bool IsRequired { get; set; }
}

/// <summary>
/// A difference found during template comparison.
/// </summary>
public class TemplateDifference
{
    /// <summary>
    /// Gets the type of difference.
    /// </summary>
    public TemplateDifferenceType Type { get; set; }

    /// <summary>
    /// Gets the path to the difference.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets the expected value.
    /// </summary>
    public string? Expected { get; set; }

    /// <summary>
    /// Gets the actual value.
    /// </summary>
    public string? Actual { get; set; }

    /// <summary>
    /// Gets additional details.
    /// </summary>
    public string? Details { get; set; }

    public override string ToString()
    {
        return Type switch
        {
            TemplateDifferenceType.MissingPart => $"Missing part: {Expected}",
            TemplateDifferenceType.ExtraPart => $"Extra part: {Actual}",
            TemplateDifferenceType.PartTypeMismatch => $"Part type mismatch at {Path}: expected {Expected}, got {Actual}",
            TemplateDifferenceType.StructureMismatch => $"Structure mismatch at {Path}: {Details}",
            TemplateDifferenceType.PropertyMismatch => $"Property mismatch at {Path}: {Details}",
            _ => $"{Type} at {Path}"
        };
    }
}

/// <summary>
/// Types of template differences.
/// </summary>
public enum TemplateDifferenceType
{
    /// <summary>A template part is missing.</summary>
    MissingPart,
    /// <summary>An unexpected template part was found.</summary>
    ExtraPart,
    /// <summary>Template part type doesn't match.</summary>
    PartTypeMismatch,
    /// <summary>Template structure doesn't match.</summary>
    StructureMismatch,
    /// <summary>Property values don't match.</summary>
    PropertyMismatch
}

/// <summary>
/// Options for template comparison.
/// </summary>
public class TemplateComparisonOptions
{
    /// <summary>
    /// Gets or sets whether to compare template parts.
    /// </summary>
    public bool CompareParts { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to compare structure.
    /// </summary>
    public bool CompareStructure { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to compare properties.
    /// </summary>
    public bool CompareProperties { get; set; } = false;

    /// <summary>
    /// Gets or sets part names to ignore.
    /// </summary>
    public HashSet<string>? IgnoreParts { get; set; }

    /// <summary>
    /// Gets or sets the maximum depth to compare.
    /// </summary>
    public int MaxDepth { get; set; } = -1;

    /// <summary>
    /// Default comparison options.
    /// </summary>
    public static TemplateComparisonOptions Default => new();

    /// <summary>
    /// Strict comparison options.
    /// </summary>
    public static TemplateComparisonOptions Strict => new()
    {
        CompareParts = true,
        CompareStructure = true,
        CompareProperties = true
    };

    /// <summary>
    /// Parts-only comparison options.
    /// </summary>
    public static TemplateComparisonOptions PartsOnly => new()
    {
        CompareParts = true,
        CompareStructure = false,
        CompareProperties = false
    };
}

/// <summary>
/// Compares control templates for testing and validation.
/// </summary>
public static class TemplateComparer
{
    #region Template Comparison

    /// <summary>
    /// Compares the templates of two controls.
    /// </summary>
    /// <param name="expected">The control with the expected template.</param>
    /// <param name="actual">The control with the actual template.</param>
    /// <param name="options">Comparison options.</param>
    /// <returns>The comparison result.</returns>
    public static TemplateComparisonResult CompareTemplates(
        TemplatedControl expected,
        TemplatedControl actual,
        TemplateComparisonOptions? options = null)
    {
        options ??= TemplateComparisonOptions.Default;
        var result = new TemplateComparisonResult();

        // Get template info
        result.ExpectedTemplate = GetTemplateInfo(expected);
        result.ActualTemplate = GetTemplateInfo(actual);

        // Compare parts
        if (options.CompareParts)
        {
            CompareTemplateParts(expected, actual, options, result);
        }

        // Compare structure
        if (options.CompareStructure)
        {
            CompareTemplateStructure(expected, actual, options, result);
        }

        return result;
    }

    /// <summary>
    /// Gets information about a control's template.
    /// </summary>
    /// <param name="control">The templated control.</param>
    /// <returns>Template information.</returns>
    public static TemplateInfo GetTemplateInfo(TemplatedControl control)
    {
        var info = new TemplateInfo
        {
            TargetType = control.GetType()
        };

        // Find template parts
        var templateParts = GetTemplateParts(control);
        info.Parts.AddRange(templateParts);
        info.PartCount = templateParts.Count;

        // Calculate tree metrics
        info.TreeDepth = CalculateVisualDepth(control);
        info.VisualCount = CountVisuals(control);

        return info;
    }

    /// <summary>
    /// Gets the template parts of a control.
    /// </summary>
    /// <param name="control">The templated control.</param>
    /// <returns>List of template parts.</returns>
    public static List<TemplatePartInfo> GetTemplateParts(TemplatedControl control)
    {
        var parts = new List<TemplatePartInfo>();

        // Get template part attributes from the control type
        var type = control.GetType();
        while (type != null && type != typeof(object))
        {
            var attrs = type.GetCustomAttributes(typeof(TemplatePartAttribute), false);
            foreach (TemplatePartAttribute attr in attrs)
            {
                if (!parts.Any(p => p.Name == attr.Name))
                {
                    parts.Add(new TemplatePartInfo
                    {
                        Name = attr.Name,
                        Type = attr.Type,
                        IsRequired = attr.IsRequired
                    });
                }
            }
            type = type.BaseType;
        }

        // Also find PART_ named controls in the visual tree
        foreach (var visual in control.GetSelfAndVisualDescendants())
        {
            if (visual is Control c && c.Name?.StartsWith("PART_") == true)
            {
                if (!parts.Any(p => p.Name == c.Name))
                {
                    parts.Add(new TemplatePartInfo
                    {
                        Name = c.Name,
                        Type = c.GetType(),
                        IsRequired = false
                    });
                }
            }
        }

        return parts;
    }

    /// <summary>
    /// Validates that a control has all required template parts.
    /// </summary>
    /// <param name="control">The templated control.</param>
    /// <returns>List of missing required parts.</returns>
    public static List<string> GetMissingRequiredParts(TemplatedControl control)
    {
        var missing = new List<string>();
        var parts = GetTemplateParts(control);

        foreach (var part in parts.Where(p => p.IsRequired))
        {
            var found = control.GetSelfAndVisualDescendants()
                .OfType<Control>()
                .Any(c => c.Name == part.Name);

            if (!found)
            {
                missing.Add(part.Name);
            }
        }

        return missing;
    }

    #endregion

    #region Template Validation

    /// <summary>
    /// Validates that a control's template contains specific parts.
    /// </summary>
    /// <param name="control">The templated control.</param>
    /// <param name="partNames">The required part names.</param>
    /// <returns>True if all parts exist.</returns>
    public static bool HasTemplateParts(TemplatedControl control, params string[] partNames)
    {
        foreach (var partName in partNames)
        {
            var found = control.GetSelfAndVisualDescendants()
                .OfType<Control>()
                .Any(c => c.Name == partName);

            if (!found)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Gets a template part by name.
    /// </summary>
    /// <typeparam name="T">The expected part type.</typeparam>
    /// <param name="control">The templated control.</param>
    /// <param name="partName">The part name.</param>
    /// <returns>The template part, or null if not found.</returns>
    public static T? GetTemplatePart<T>(TemplatedControl control, string partName) where T : class
    {
        return control.GetSelfAndVisualDescendants()
            .OfType<Control>()
            .FirstOrDefault(c => c.Name == partName) as T;
    }

    /// <summary>
    /// Validates that a template part is of the expected type.
    /// </summary>
    /// <typeparam name="T">The expected type.</typeparam>
    /// <param name="control">The templated control.</param>
    /// <param name="partName">The part name.</param>
    /// <returns>True if the part exists and is of the correct type.</returns>
    public static bool IsTemplatePartOfType<T>(TemplatedControl control, string partName) where T : class
    {
        return GetTemplatePart<T>(control, partName) != null;
    }

    #endregion

    #region Template Snapshot

    /// <summary>
    /// Creates a snapshot of a control's template structure.
    /// </summary>
    /// <param name="control">The templated control.</param>
    /// <returns>A template snapshot.</returns>
    public static TemplateSnapshot CreateSnapshot(TemplatedControl control)
    {
        var snapshot = new TemplateSnapshot
        {
            ControlType = control.GetType().Name,
            CreatedAt = DateTime.UtcNow,
            Info = GetTemplateInfo(control)
        };

        BuildTemplateSnapshot(control, snapshot.Root, 0);

        return snapshot;
    }

    /// <summary>
    /// Compares a control's template against a snapshot.
    /// </summary>
    /// <param name="control">The control to compare.</param>
    /// <param name="snapshot">The expected snapshot.</param>
    /// <param name="options">Comparison options.</param>
    /// <returns>The comparison result.</returns>
    public static TemplateComparisonResult CompareToSnapshot(
        TemplatedControl control,
        TemplateSnapshot snapshot,
        TemplateComparisonOptions? options = null)
    {
        options ??= TemplateComparisonOptions.Default;
        var result = new TemplateComparisonResult();

        result.ActualTemplate = GetTemplateInfo(control);
        result.ExpectedTemplate = snapshot.Info;

        // Compare snapshot nodes
        CompareSnapshotNode(control, snapshot.Root, "", 0, options, result);

        return result;
    }

    private static void BuildTemplateSnapshot(Visual visual, TemplateSnapshotNode node, int depth)
    {
        node.TypeName = visual.GetType().Name;
        node.FullTypeName = visual.GetType().FullName ?? visual.GetType().Name;
        node.Name = (visual as Control)?.Name;

        if (visual is Control control)
        {
            node.Classes.AddRange(control.Classes);
        }

        foreach (var child in visual.GetVisualChildren())
        {
            var childNode = new TemplateSnapshotNode();
            BuildTemplateSnapshot(child, childNode, depth + 1);
            node.Children.Add(childNode);
        }
    }

    private static void CompareSnapshotNode(
        Visual? actual,
        TemplateSnapshotNode expected,
        string path,
        int depth,
        TemplateComparisonOptions options,
        TemplateComparisonResult result)
    {
        if (options.MaxDepth >= 0 && depth > options.MaxDepth)
            return;

        var currentPath = string.IsNullOrEmpty(path)
            ? expected.TypeName
            : $"{path}/{expected.TypeName}";

        if (actual == null)
        {
            result.Differences.Add(new TemplateDifference
            {
                Type = TemplateDifferenceType.StructureMismatch,
                Path = currentPath,
                Expected = expected.TypeName,
                Details = "Node missing in actual template"
            });
            return;
        }

        // Check part names
        if (!string.IsNullOrEmpty(expected.Name) && expected.Name.StartsWith("PART_"))
        {
            if (options.IgnoreParts?.Contains(expected.Name) == true)
                return;

            var actualName = (actual as Control)?.Name;
            if (actualName != expected.Name)
            {
                result.Differences.Add(new TemplateDifference
                {
                    Type = TemplateDifferenceType.MissingPart,
                    Path = currentPath,
                    Expected = expected.Name,
                    Actual = actualName
                });
            }
        }

        // Compare type
        if (options.CompareStructure && actual.GetType().Name != expected.TypeName)
        {
            result.Differences.Add(new TemplateDifference
            {
                Type = TemplateDifferenceType.StructureMismatch,
                Path = currentPath,
                Expected = expected.TypeName,
                Actual = actual.GetType().Name,
                Details = "Type mismatch"
            });
        }

        // Compare children
        var actualChildren = actual.GetVisualChildren().ToList();
        var minCount = Math.Min(expected.Children.Count, actualChildren.Count);

        for (int i = 0; i < minCount; i++)
        {
            var childPath = $"{currentPath}[{i}]";
            CompareSnapshotNode(actualChildren[i], expected.Children[i], childPath, depth + 1, options, result);
        }
    }

    #endregion

    #region Private Helpers

    private static void CompareTemplateParts(
        TemplatedControl expected,
        TemplatedControl actual,
        TemplateComparisonOptions options,
        TemplateComparisonResult result)
    {
        var expectedParts = GetTemplateParts(expected)
            .Where(p => options.IgnoreParts?.Contains(p.Name) != true)
            .ToList();

        var actualParts = GetTemplateParts(actual)
            .Where(p => options.IgnoreParts?.Contains(p.Name) != true)
            .ToList();

        // Check for missing parts
        foreach (var part in expectedParts)
        {
            var actualPart = actualParts.FirstOrDefault(p => p.Name == part.Name);
            if (actualPart == null)
            {
                result.Differences.Add(new TemplateDifference
                {
                    Type = TemplateDifferenceType.MissingPart,
                    Path = part.Name,
                    Expected = $"{part.Name} ({part.Type?.Name})"
                });
            }
            else if (part.Type != null && actualPart.Type != part.Type && !actualPart.Type!.IsSubclassOf(part.Type))
            {
                result.Differences.Add(new TemplateDifference
                {
                    Type = TemplateDifferenceType.PartTypeMismatch,
                    Path = part.Name,
                    Expected = part.Type?.Name,
                    Actual = actualPart.Type?.Name
                });
            }
        }

        // Check for extra parts
        foreach (var part in actualParts)
        {
            if (!expectedParts.Any(p => p.Name == part.Name))
            {
                result.Differences.Add(new TemplateDifference
                {
                    Type = TemplateDifferenceType.ExtraPart,
                    Path = part.Name,
                    Actual = $"{part.Name} ({part.Type?.Name})"
                });
            }
        }
    }

    private static void CompareTemplateStructure(
        TemplatedControl expected,
        TemplatedControl actual,
        TemplateComparisonOptions options,
        TemplateComparisonResult result)
    {
        // Use tree comparer for structure comparison
        var treeOptions = new TreeComparisonOptions
        {
            CompareTypes = true,
            CompareNames = true,
            CompareOrder = true,
            IgnoreInternalControls = options.IgnoreParts?.Count > 0,
            MaxDepth = options.MaxDepth
        };

        var treeResult = TreeComparer.CompareVisualTrees(expected, actual, treeOptions);

        foreach (var diff in treeResult.Differences)
        {
            result.Differences.Add(new TemplateDifference
            {
                Type = TemplateDifferenceType.StructureMismatch,
                Path = diff.Path,
                Expected = diff.Expected,
                Actual = diff.Actual,
                Details = diff.ToString()
            });
        }
    }

    private static int CalculateVisualDepth(Visual visual)
    {
        var maxDepth = 0;
        foreach (var child in visual.GetVisualChildren())
        {
            var childDepth = CalculateVisualDepth(child) + 1;
            maxDepth = Math.Max(maxDepth, childDepth);
        }
        return maxDepth;
    }

    private static int CountVisuals(Visual visual)
    {
        return 1 + visual.GetVisualChildren().Sum(CountVisuals);
    }

    #endregion

    #region Extension Methods

    /// <summary>
    /// Compares this control's template to another.
    /// </summary>
    public static TemplateComparisonResult CompareTemplateTo(
        this TemplatedControl expected,
        TemplatedControl actual,
        TemplateComparisonOptions? options = null)
    {
        return CompareTemplates(expected, actual, options);
    }

    /// <summary>
    /// Creates a template snapshot.
    /// </summary>
    public static TemplateSnapshot SnapshotTemplate(this TemplatedControl control)
    {
        return CreateSnapshot(control);
    }

    /// <summary>
    /// Validates required template parts exist.
    /// </summary>
    public static bool ValidateTemplateParts(this TemplatedControl control)
    {
        return TemplateComparer.GetMissingRequiredParts(control).Count == 0;
    }

    #endregion
}

/// <summary>
/// A snapshot of a control template.
/// </summary>
public class TemplateSnapshot
{
    /// <summary>
    /// Gets or sets the control type name.
    /// </summary>
    public string ControlType { get; set; } = string.Empty;

    /// <summary>
    /// Gets the root node.
    /// </summary>
    public TemplateSnapshotNode Root { get; } = new();

    /// <summary>
    /// Gets or sets when the snapshot was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the template info.
    /// </summary>
    public TemplateInfo? Info { get; set; }

    /// <summary>
    /// Gets or sets a description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// A node in a template snapshot.
/// </summary>
public class TemplateSnapshotNode
{
    /// <summary>
    /// Gets or sets the type name.
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
    /// Gets the style classes.
    /// </summary>
    public List<string> Classes { get; } = new();

    /// <summary>
    /// Gets the property values.
    /// </summary>
    public Dictionary<string, string?> Properties { get; } = new();

    /// <summary>
    /// Gets the child nodes.
    /// </summary>
    public List<TemplateSnapshotNode> Children { get; } = new();
}

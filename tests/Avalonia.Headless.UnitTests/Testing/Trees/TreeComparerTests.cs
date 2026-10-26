#nullable enable

// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.HeadlessTestingFramework;
using Avalonia.VisualTree;
using Xunit;

namespace Avalonia.Headless.UnitTests.Testing.Trees;

/// <summary>
/// Tests for TreeComparer functionality.
/// </summary>
public class TreeComparerTests
{
    #region Test Infrastructure

    private static Window CreateTestWindow()
    {
        var window = new Window
        {
            Width = 800,
            Height = 600,
            Content = CreateTestTree()
        };
        window.Show();
        return window;
    }

    private static Panel CreateTestTree()
    {
        var root = new StackPanel { Name = "CompareRoot" };

        root.Children.Add(new Button { Name = "Button1", Content = "First" });
        root.Children.Add(new Button { Name = "Button2", Content = "Second" });
        root.Children.Add(new TextBlock { Name = "Label1", Text = "Hello" });

        var nested = new Border
        {
            Name = "Container",
            Child = new StackPanel
            {
                Name = "InnerPanel",
                Children =
                {
                    new CheckBox { Name = "Check1", Content = "Option" },
                    new TextBox { Name = "Input1", Text = "Text" }
                }
            }
        };

        root.Children.Add(nested);

        return root;
    }

    private static Panel CreateModifiedTree()
    {
        var root = new StackPanel { Name = "CompareRoot" };

        root.Children.Add(new Button { Name = "Button1", Content = "First" });
        // Button2 missing
        root.Children.Add(new TextBlock { Name = "Label1", Text = "Hello" });
        root.Children.Add(new TextBlock { Name = "Label2", Text = "Extra" }); // Extra

        var nested = new Border
        {
            Name = "Container",
            Child = new StackPanel
            {
                Name = "InnerPanel",
                Children =
                {
                    new RadioButton { Name = "Check1", Content = "Option" }, // Type changed
                    new TextBox { Name = "Input1", Text = "Text" }
                }
            }
        };

        root.Children.Add(nested);

        return root;
    }

    #endregion

    #region Visual Tree Comparison Tests

    [AvaloniaFact]
    public void CompareVisualTrees_IdenticalTrees_ReturnsEqual()
    {
        var window1 = CreateTestWindow();
        var window2 = CreateTestWindow();
        var root1 = window1.FindByName<StackPanel>("CompareRoot")!;
        var root2 = window2.FindByName<StackPanel>("CompareRoot")!;

        var result = TreeComparer.CompareVisualTrees(root1, root2);

        Assert.True(result.AreEqual);
        Assert.Empty(result.Differences);
    }

    [AvaloniaFact]
    public void CompareVisualTrees_DifferentTrees_ReturnsDifferences()
    {
        var window1 = new Window { Content = CreateTestTree() };
        var window2 = new Window { Content = CreateModifiedTree() };
        window1.Show();
        window2.Show();

        var root1 = window1.FindByName<StackPanel>("CompareRoot")!;
        var root2 = window2.FindByName<StackPanel>("CompareRoot")!;

        var result = TreeComparer.CompareVisualTrees(root1, root2);

        Assert.False(result.AreEqual);
        Assert.NotEmpty(result.Differences);
    }

    [AvaloniaFact]
    public void CompareVisualTrees_TypeMismatch_ReportsCorrectly()
    {
        var window1 = new Window { Content = new Button { Name = "Test" } };
        var window2 = new Window { Content = new TextBox { Name = "Test" } };
        window1.Show();
        window2.Show();

        var result = TreeComparer.CompareVisualTrees(
            (Visual)window1.Content!,
            (Visual)window2.Content!);

        Assert.False(result.AreEqual);
        Assert.Contains(result.Differences, d => d.Type == TreeDifferenceType.TypeMismatch);
    }

    [AvaloniaFact]
    public void CompareVisualTrees_NameMismatch_ReportsCorrectly()
    {
        var window1 = new Window { Content = new Button { Name = "Expected" } };
        var window2 = new Window { Content = new Button { Name = "Actual" } };
        window1.Show();
        window2.Show();

        var result = TreeComparer.CompareVisualTrees(
            (Visual)window1.Content!,
            (Visual)window2.Content!);

        Assert.False(result.AreEqual);
        Assert.Contains(result.Differences, d => d.Type == TreeDifferenceType.NameMismatch);
    }

    [AvaloniaFact]
    public void CompareVisualTrees_ChildCountMismatch_ReportsCorrectly()
    {
        var panel1 = new StackPanel { Children = { new Button(), new Button() } };
        var panel2 = new StackPanel { Children = { new Button() } };
        var window1 = new Window { Content = panel1 };
        var window2 = new Window { Content = panel2 };
        window1.Show();
        window2.Show();

        var result = TreeComparer.CompareVisualTrees(panel1, panel2);

        Assert.False(result.AreEqual);
        Assert.Contains(result.Differences, d => d.Type == TreeDifferenceType.ChildCountMismatch);
    }

    [AvaloniaFact]
    public void CompareVisualTrees_WithIgnoreTypes_ExcludesTypes()
    {
        var panel1 = new StackPanel { Children = { new Button(), new TextBlock() } };
        var panel2 = new StackPanel { Children = { new Button() } };
        var window1 = new Window { Content = panel1 };
        var window2 = new Window { Content = panel2 };
        window1.Show();
        window2.Show();

        var options = new TreeComparisonOptions
        {
            IgnoreTypes = new HashSet<Type> { typeof(TextBlock) }
        };

        var result = TreeComparer.CompareVisualTrees(panel1, panel2, options);

        // With TextBlock ignored, both panels have just 1 Button - should be equal
        Assert.True(result.AreEqual);
    }

    [AvaloniaFact]
    public void CompareVisualTrees_StructureOnly_IgnoresNames()
    {
        var window1 = new Window { Content = new Button { Name = "A" } };
        var window2 = new Window { Content = new Button { Name = "B" } };
        window1.Show();
        window2.Show();

        var result = TreeComparer.CompareVisualTrees(
            (Visual)window1.Content!,
            (Visual)window2.Content!,
            TreeComparisonOptions.StructureOnly);

        Assert.True(result.AreEqual);
    }

    #endregion

    #region Logical Tree Comparison Tests

    [AvaloniaFact]
    public void CompareLogicalTrees_IdenticalTrees_ReturnsEqual()
    {
        var window1 = CreateTestWindow();
        var window2 = CreateTestWindow();
        var root1 = window1.FindLogicalByName<StackPanel>("CompareRoot")!;
        var root2 = window2.FindLogicalByName<StackPanel>("CompareRoot")!;

        var result = TreeComparer.CompareLogicalTrees(root1, root2);

        Assert.True(result.AreEqual);
    }

    [AvaloniaFact]
    public void CompareLogicalTrees_DifferentTrees_ReturnsDifferences()
    {
        var window1 = new Window { Content = CreateTestTree() };
        var window2 = new Window { Content = CreateModifiedTree() };
        window1.Show();
        window2.Show();

        var root1 = window1.FindLogicalByName<StackPanel>("CompareRoot")!;
        var root2 = window2.FindLogicalByName<StackPanel>("CompareRoot")!;

        var result = TreeComparer.CompareLogicalTrees(root1, root2);

        Assert.False(result.AreEqual);
    }

    #endregion

    #region Snapshot Tests

    [AvaloniaFact]
    public void CreateVisualSnapshot_CapturesTreeStructure()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("CompareRoot")!;

        var snapshot = TreeComparer.CreateVisualSnapshot(root);

        Assert.NotNull(snapshot);
        Assert.Equal("StackPanel", snapshot.Root.TypeName);
        Assert.NotEmpty(snapshot.Root.Children);
    }

    [AvaloniaFact]
    public void CreateLogicalSnapshot_CapturesTreeStructure()
    {
        var window = CreateTestWindow();
        var root = window.FindLogicalByName<StackPanel>("CompareRoot")!;

        var snapshot = TreeComparer.CreateLogicalSnapshot(root);

        Assert.NotNull(snapshot);
        Assert.Equal("StackPanel", snapshot.Root.TypeName);
    }

    [AvaloniaFact]
    public void CompareToSnapshot_MatchingTree_ReturnsEqual()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("CompareRoot")!;

        var snapshot = TreeComparer.CreateVisualSnapshot(root);
        var result = TreeComparer.CompareToSnapshot(root, snapshot);

        Assert.True(result.AreEqual);
    }

    [AvaloniaFact]
    public void CompareToSnapshot_ModifiedTree_ReturnsDifferences()
    {
        var window1 = new Window { Content = CreateTestTree() };
        window1.Show();
        var root1 = window1.FindByName<StackPanel>("CompareRoot")!;
        var snapshot = TreeComparer.CreateVisualSnapshot(root1);

        var window2 = new Window { Content = CreateModifiedTree() };
        window2.Show();
        var root2 = window2.FindByName<StackPanel>("CompareRoot")!;

        var result = TreeComparer.CompareToSnapshot(root2, snapshot);

        Assert.False(result.AreEqual);
    }

    #endregion

    #region Extension Methods Tests

    [AvaloniaFact]
    public void CompareVisualTreeTo_ExtensionMethod_Works()
    {
        var window1 = CreateTestWindow();
        var window2 = CreateTestWindow();
        var root1 = window1.FindByName<StackPanel>("CompareRoot")!;
        var root2 = window2.FindByName<StackPanel>("CompareRoot")!;

        var result = root1.CompareVisualTreeTo(root2);

        Assert.True(result.AreEqual);
    }

    [AvaloniaFact]
    public void SnapshotVisualTree_ExtensionMethod_Works()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("CompareRoot")!;

        var snapshot = root.SnapshotVisualTree();

        Assert.NotNull(snapshot);
        Assert.Equal(TreeSnapshotType.Visual, snapshot.TreeType);
    }

    [AvaloniaFact]
    public void SnapshotLogicalTree_ExtensionMethod_Works()
    {
        var window = CreateTestWindow();
        var root = window.FindLogicalByName<StackPanel>("CompareRoot")!;

        var snapshot = root.SnapshotLogicalTree();

        Assert.NotNull(snapshot);
        Assert.Equal(TreeSnapshotType.Logical, snapshot.TreeType);
    }

    #endregion

    #region Match Percentage Tests

    [AvaloniaFact]
    public void ComparisonResult_MatchPercentage_CalculatesCorrectly()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("CompareRoot")!;

        var result = TreeComparer.CompareVisualTrees(root, root);

        Assert.Equal(100.0, result.MatchPercentage);
    }

    [AvaloniaFact]
    public void ComparisonResult_ToString_ReturnsReadableString()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("CompareRoot")!;

        var result = TreeComparer.CompareVisualTrees(root, root);

        Assert.Contains("equal", result.ToString().ToLower());
    }

    #endregion
}

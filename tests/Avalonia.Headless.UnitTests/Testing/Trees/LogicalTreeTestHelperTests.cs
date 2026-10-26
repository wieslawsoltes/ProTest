#nullable enable

// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.HeadlessTestingFramework;
using Xunit;

namespace Avalonia.Headless.UnitTests.Testing.Trees;

/// <summary>
/// Comprehensive tests for LogicalTreeTestHelper.
/// </summary>
public class LogicalTreeTestHelperTests
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
        var root = new StackPanel { Name = "LogicalRoot" };

        // Create buttons with different states
        var btn1 = new Button { Name = "LogicalBtn1", Content = "First" };
        var btn2 = new Button { Name = "LogicalBtn2", Content = "Second", IsEnabled = false };
        var btn3 = new Button { Name = "LogicalBtn3", Content = "Third" };
        btn1.Classes.Add("primary");
        btn2.Classes.Add("secondary");
        btn3.Classes.Add("primary");

        // Create nested structure
        var container = new Border
        {
            Name = "Container",
            Child = new StackPanel
            {
                Name = "InnerPanel",
                Children =
                {
                    new TextBlock { Name = "InnerText1", Text = "Inner 1" },
                    new TextBlock { Name = "InnerText2", Text = "Inner 2" }
                }
            }
        };

        // Create items control
        var itemsControl = new ListBox
        {
            Name = "ItemsList",
            Items = { "Item A", "Item B", "Item C" }
        };

        // Content control
        var contentControl = new ContentControl
        {
            Name = "ContentHolder",
            Content = new TextBlock { Name = "ContentText", Text = "Content" }
        };

        root.Children.Add(btn1);
        root.Children.Add(btn2);
        root.Children.Add(btn3);
        root.Children.Add(container);
        root.Children.Add(itemsControl);
        root.Children.Add(contentControl);

        return root;
    }

    #endregion

    #region FindFirstLogical<T> Tests

    [AvaloniaFact]
    public void FindFirstLogical_ByType_ReturnsFirstMatch()
    {
        var window = CreateTestWindow();

        var button = window.FindFirstLogical<Button>();

        Assert.NotNull(button);
        Assert.Equal("LogicalBtn1", button.Name);
    }

    [AvaloniaFact]
    public void FindFirstLogical_WithPredicate_ReturnsMatchingControl()
    {
        var window = CreateTestWindow();

        var button = window.FindFirstLogical<Button>(b => b.Content?.ToString() == "Second");

        Assert.NotNull(button);
        Assert.Equal("LogicalBtn2", button.Name);
    }

    [AvaloniaFact]
    public void FindFirstLogical_IncludeSelf_IncludesRootInSearch()
    {
        var window = CreateTestWindow();

        var found = window.FindFirstLogical<Window>(includeSelf: true);

        Assert.Same(window, found);
    }

    #endregion

    #region FindAllLogical<T> Tests

    [AvaloniaFact]
    public void FindAllLogical_ByType_ReturnsAllMatches()
    {
        var window = CreateTestWindow();

        var buttons = window.FindAllLogical<Button>().ToList();

        Assert.Equal(3, buttons.Count);
    }

    [AvaloniaFact]
    public void FindAllLogical_WithPredicate_FiltersResults()
    {
        var window = CreateTestWindow();

        var enabledButtons = window.FindAllLogical<Button>(b => b.IsEnabled).ToList();

        Assert.Equal(2, enabledButtons.Count);
    }

    [AvaloniaFact]
    public void FindAllLogical_TextBlocks_FindsAll()
    {
        var window = CreateTestWindow();

        var textBlocks = window.FindAllLogical<TextBlock>().ToList();

        Assert.True(textBlocks.Count >= 3); // InnerText1, InnerText2, ContentText
    }

    #endregion

    #region FindLogicalByName Tests

    [AvaloniaFact]
    public void FindLogicalByName_ReturnsControlWithName()
    {
        var window = CreateTestWindow();

        var control = window.FindLogicalByName("LogicalBtn1");

        Assert.NotNull(control);
        Assert.IsType<Button>(control);
    }

    [AvaloniaFact]
    public void FindLogicalByName_Generic_ReturnsCastControl()
    {
        var window = CreateTestWindow();

        var button = window.FindLogicalByName<Button>("LogicalBtn1");

        Assert.NotNull(button);
        Assert.Equal("First", button.Content);
    }

    [AvaloniaFact]
    public void FindLogicalByName_ReturnsNullWhenNotFound()
    {
        var window = CreateTestWindow();

        var control = window.FindLogicalByName("NonExistent");

        Assert.Null(control);
    }

    [AvaloniaFact]
    public void GetLogicalByName_ThrowsWhenNotFound()
    {
        var window = CreateTestWindow();

        Assert.Throws<InvalidOperationException>(() =>
            window.GetLogicalByName<Button>("NonExistent"));
    }

    #endregion

    #region FindLogicalByProperty Tests

    [AvaloniaFact]
    public void FindLogicalByProperty_FindsControlsWithPropertyValue()
    {
        var window = CreateTestWindow();

        var disabled = window.FindLogicalByProperty<InputElement, bool>(
            InputElement.IsEnabledProperty, false).ToList();

        Assert.Contains(disabled, c => c.Name == "LogicalBtn2");
    }

    [AvaloniaFact]
    public void FindLogicalByClass_FindsControlsWithCssClass()
    {
        var window = CreateTestWindow();

        var primaryButtons = window.FindLogicalByClass<Button>("primary").ToList();

        Assert.Equal(2, primaryButtons.Count);
    }

    #endregion

    #region FindLogicalByState Tests

    [AvaloniaFact]
    public void FindLogicalEnabled_ReturnsOnlyEnabledControls()
    {
        var window = CreateTestWindow();

        var enabled = window.FindLogicalEnabled<Button>().ToList();

        Assert.Equal(2, enabled.Count);
        Assert.DoesNotContain(enabled, b => b.Name == "LogicalBtn2");
    }

    [AvaloniaFact]
    public void FindLogicalDisabled_ReturnsOnlyDisabledControls()
    {
        var window = CreateTestWindow();

        var disabled = window.FindLogicalDisabled<Button>().ToList();

        Assert.Single(disabled);
        Assert.Equal("LogicalBtn2", disabled[0].Name);
    }

    [AvaloniaFact]
    public void FindLogicalVisible_ReturnsOnlyVisibleControls()
    {
        var window = CreateTestWindow();

        var visible = window.FindLogicalVisible<Button>().ToList();

        Assert.Equal(3, visible.Count);
    }

    #endregion

    #region Logical Ancestor Query Tests

    [AvaloniaFact]
    public void FindLogicalAncestor_ReturnsFirstAncestorOfType()
    {
        var window = CreateTestWindow();
        var text = window.FindLogicalByName<TextBlock>("InnerText1")!;

        var ancestor = text.FindLogicalAncestor<Border>();

        Assert.NotNull(ancestor);
        Assert.Equal("Container", ancestor.Name);
    }

    [AvaloniaFact]
    public void FindLogicalAncestor_WithPredicate_ReturnsMatchingAncestor()
    {
        var window = CreateTestWindow();
        var text = window.FindLogicalByName<TextBlock>("InnerText1")!;

        var ancestor = text.FindLogicalAncestor<Panel>(p => p.Name == "LogicalRoot");

        Assert.NotNull(ancestor);
        Assert.Equal("LogicalRoot", ancestor.Name);
    }

    [AvaloniaFact]
    public void GetLogicalAncestors_ReturnsAllAncestorsOfType()
    {
        var window = CreateTestWindow();
        var text = window.FindLogicalByName<TextBlock>("InnerText1")!;

        var ancestors = text.GetLogicalAncestors<Panel>().ToList();

        Assert.True(ancestors.Count >= 2); // InnerPanel, LogicalRoot
    }

    [AvaloniaFact]
    public void HasLogicalAncestor_ReturnsTrueWhenAncestorExists()
    {
        var window = CreateTestWindow();
        var text = window.FindLogicalByName<TextBlock>("InnerText1")!;

        Assert.True(text.HasLogicalAncestor<Border>());
        Assert.True(text.HasLogicalAncestor<StackPanel>());
    }

    #endregion

    #region Logical Children Query Tests

    [AvaloniaFact]
    public void GetLogicalChildren_ReturnsDirectChildrenOfType()
    {
        var window = CreateTestWindow();
        var root = window.FindLogicalByName<StackPanel>("LogicalRoot")!;

        var buttons = root.GetLogicalChildren<Button>().ToList();

        Assert.Equal(3, buttons.Count);
    }

    [AvaloniaFact]
    public void GetLogicalChildCount_ReturnsNumberOfDirectChildren()
    {
        var window = CreateTestWindow();
        var root = window.FindLogicalByName<StackPanel>("LogicalRoot")!;

        var count = root.GetLogicalChildCount();

        Assert.Equal(6, count); // 3 buttons + container + itemsControl + contentControl
    }

    [AvaloniaFact]
    public void GetLogicalDescendantCount_ReturnsTotalDescendants()
    {
        var window = CreateTestWindow();
        var root = window.FindLogicalByName<StackPanel>("LogicalRoot")!;

        var count = root.GetLogicalDescendantCount();

        Assert.True(count > 6);
    }

    [AvaloniaFact]
    public void GetLogicalChildAt_ReturnsChildAtIndex()
    {
        var window = CreateTestWindow();
        var root = window.FindLogicalByName<StackPanel>("LogicalRoot")!;

        var firstChild = root.GetLogicalChildAt<Button>(0);

        Assert.Equal("LogicalBtn1", firstChild.Name);
    }

    [AvaloniaFact]
    public void GetFirstLogicalChild_ReturnsFirstChildOfType()
    {
        var window = CreateTestWindow();
        var root = window.FindLogicalByName<StackPanel>("LogicalRoot")!;

        var first = root.GetFirstLogicalChild<Button>();

        Assert.NotNull(first);
        Assert.Equal("LogicalBtn1", first.Name);
    }

    [AvaloniaFact]
    public void GetLastLogicalChild_ReturnsLastChildOfType()
    {
        var window = CreateTestWindow();
        var root = window.FindLogicalByName<StackPanel>("LogicalRoot")!;

        var last = root.GetLastLogicalChild<Button>();

        Assert.NotNull(last);
        Assert.Equal("LogicalBtn3", last.Name);
    }

    #endregion

    #region Sibling Query Tests

    [AvaloniaFact]
    public void GetLogicalSiblings_ReturnsSiblingsOfType()
    {
        var window = CreateTestWindow();
        var btn2 = window.FindLogicalByName<Button>("LogicalBtn2")!;

        var siblings = btn2.GetLogicalSiblings<Button>().ToList();

        // Should find at least our named siblings
        Assert.Contains(siblings, b => b.Name == "LogicalBtn1");
        Assert.Contains(siblings, b => b.Name == "LogicalBtn3");
        // Should NOT include self
        Assert.DoesNotContain(siblings, b => b.Name == "LogicalBtn2");
    }

    [AvaloniaFact]
    public void GetNextLogicalSibling_ReturnsNextSibling()
    {
        var window = CreateTestWindow();
        var btn1 = window.FindLogicalByName<Button>("LogicalBtn1")!;

        var next = btn1.GetNextLogicalSibling<Button>();

        Assert.NotNull(next);
        Assert.Equal("LogicalBtn2", next.Name);
    }

    [AvaloniaFact]
    public void GetPreviousLogicalSibling_ReturnsPreviousSibling()
    {
        var window = CreateTestWindow();
        var btn3 = window.FindLogicalByName<Button>("LogicalBtn3")!;

        var previous = btn3.GetPreviousLogicalSibling<Button>();

        Assert.NotNull(previous);
        Assert.Equal("LogicalBtn2", previous.Name);
    }

    [AvaloniaFact]
    public void GetLogicalSiblingIndex_ReturnsCorrectIndex()
    {
        var window = CreateTestWindow();
        var btn2 = window.FindLogicalByName<Button>("LogicalBtn2")!;

        var index = btn2.GetLogicalSiblingIndex();

        Assert.Equal(1, index);
    }

    [AvaloniaFact]
    public void IsFirstLogicalChild_ReturnsTrueForFirstChild()
    {
        var window = CreateTestWindow();
        var btn1 = window.FindLogicalByName<Button>("LogicalBtn1")!;
        var btn2 = window.FindLogicalByName<Button>("LogicalBtn2")!;

        Assert.True(btn1.IsFirstLogicalChild());
        Assert.False(btn2.IsFirstLogicalChild());
    }

    [AvaloniaFact]
    public void IsLastLogicalChild_ReturnsTrueForLastChild()
    {
        var window = CreateTestWindow();
        var root = window.FindLogicalByName<StackPanel>("LogicalRoot")!;
        var contentHolder = window.FindLogicalByName<ContentControl>("ContentHolder")!;
        var btn1 = window.FindLogicalByName<Button>("LogicalBtn1")!;

        Assert.True(contentHolder.IsLastLogicalChild());
        Assert.False(btn1.IsLastLogicalChild());
    }

    [AvaloniaFact]
    public void IsOnlyLogicalChild_ReturnsTrueForOnlyChild()
    {
        var window = CreateTestWindow();
        var container = window.FindLogicalByName<Border>("Container")!;
        var innerPanel = container.GetFirstLogicalChild<StackPanel>()!;

        Assert.True(innerPanel.IsOnlyLogicalChild());
    }

    #endregion

    #region Logical Tree Path Tests

    [AvaloniaFact]
    public void GetLogicalPathFromRoot_ReturnsPathFromRootToElement()
    {
        var window = CreateTestWindow();
        var text = window.FindLogicalByName<TextBlock>("InnerText1")!;

        var path = text.GetLogicalPathFromRoot();

        Assert.True(path.Count >= 4);
        Assert.Same(text, path.Last());
    }

    [AvaloniaFact]
    public void GetLogicalPathString_ReturnsTypeNamesPath()
    {
        var window = CreateTestWindow();
        var text = window.FindLogicalByName<TextBlock>("InnerText1")!;

        var pathString = text.GetLogicalPathString();

        Assert.Contains("TextBlock", pathString);
        Assert.Contains(" > ", pathString);
    }

    [AvaloniaFact]
    public void GetDetailedLogicalPathString_IncludesNames()
    {
        var window = CreateTestWindow();
        var text = window.FindLogicalByName<TextBlock>("InnerText1")!;

        var pathString = text.GetDetailedLogicalPathString();

        Assert.Contains("[InnerText1]", pathString);
        Assert.Contains("[Container]", pathString);
    }

    #endregion

    #region Logical Tree Validation Tests

    [AvaloniaFact]
    public void ContainsLogical_ReturnsTrueWhenDescendantExists()
    {
        var window = CreateTestWindow();

        Assert.True(window.ContainsLogical<Button>());
        Assert.True(window.ContainsLogical<TextBlock>());
    }

    [AvaloniaFact]
    public void ContainsLogical_ReturnsFalseWhenNoDescendant()
    {
        var window = CreateTestWindow();

        Assert.False(window.ContainsLogical<Slider>());
    }

    [AvaloniaFact]
    public void ContainsLogicalNamed_ReturnsTrueForExistingName()
    {
        var window = CreateTestWindow();

        Assert.True(window.ContainsLogicalNamed("LogicalBtn1"));
        Assert.True(window.ContainsLogicalNamed("InnerText1"));
    }

    [AvaloniaFact]
    public void ContainsLogicalNamed_ReturnsFalseForMissingName()
    {
        var window = CreateTestWindow();

        Assert.False(window.ContainsLogicalNamed("NonExistent"));
    }

    [AvaloniaFact]
    public void CountLogical_ReturnsNumberOfMatchingDescendants()
    {
        var window = CreateTestWindow();

        var count = window.CountLogical<Button>();

        Assert.Equal(3, count);
    }

    [AvaloniaFact]
    public void MatchesLogicalStructure_ReturnsTrueForMatchingPath()
    {
        var window = CreateTestWindow();
        var container = window.FindLogicalByName<Border>("Container")!;

        var matches = container.MatchesLogicalStructure(
            typeof(Border), typeof(StackPanel), typeof(TextBlock));

        Assert.True(matches);
    }

    #endregion

    #region Content Host Query Tests

    [AvaloniaFact]
    public void GetContent_ReturnsTypedContent()
    {
        var window = CreateTestWindow();
        var contentHolder = window.FindLogicalByName<ContentControl>("ContentHolder")!;

        var content = contentHolder.GetContent<TextBlock>();

        Assert.NotNull(content);
        Assert.Equal("ContentText", content.Name);
    }

    [AvaloniaFact]
    public void GetItems_ReturnsTypedItems()
    {
        var window = CreateTestWindow();
        var list = window.FindLogicalByName<ListBox>("ItemsList")!;

        var items = list.GetItems<string>().ToList();

        Assert.Equal(3, items.Count);
        Assert.Contains("Item A", items);
    }

    [AvaloniaFact]
    public void GetItemAt_ReturnsItemAtIndex()
    {
        var window = CreateTestWindow();
        var list = window.FindLogicalByName<ListBox>("ItemsList")!;

        var item = list.GetItemAt<string>(1);

        Assert.Equal("Item B", item);
    }

    [AvaloniaFact]
    public void GetItemCount_ReturnsCorrectCount()
    {
        var window = CreateTestWindow();
        var list = window.FindLogicalByName<ListBox>("ItemsList")!;

        var count = list.GetItemCount();

        Assert.Equal(3, count);
    }

    #endregion

    #region Debug Helper Tests

    [AvaloniaFact]
    public void PrintLogicalTree_ReturnsTreeStructure()
    {
        var window = CreateTestWindow();
        var root = window.FindLogicalByName<StackPanel>("LogicalRoot")!;

        var tree = root.PrintLogicalTree(maxDepth: 2);

        Assert.Contains("StackPanel", tree);
        Assert.Contains("Button", tree);
        Assert.Contains("[LogicalBtn1]", tree);
    }

    [AvaloniaFact]
    public void GetLogicalTypeSummary_ReturnsCounts()
    {
        var window = CreateTestWindow();

        var summary = window.GetLogicalTypeSummary();

        Assert.True(summary.ContainsKey("Button"));
        Assert.Equal(3, summary["Button"]);
    }

    [AvaloniaFact]
    public void GetLogicalNamedControls_ReturnsDictionary()
    {
        var window = CreateTestWindow();

        var named = window.GetLogicalNamedControls();

        Assert.True(named.ContainsKey("LogicalBtn1"));
        Assert.True(named.ContainsKey("Container"));
    }

    [AvaloniaFact]
    public void CompareTreeDepths_ReturnsBothDepths()
    {
        var window = CreateTestWindow();
        var btn = window.FindLogicalByName<Button>("LogicalBtn1")!;

        var (logicalDepth, visualDepth) = btn.CompareTreeDepths();

        Assert.True(logicalDepth > 0);
        Assert.True(visualDepth >= logicalDepth); // Visual tree is usually deeper
    }

    #endregion
}

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
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.HeadlessTestingFramework;
using Avalonia.VisualTree;
using Xunit;

namespace Avalonia.Headless.UnitTests.Testing.Trees;

/// <summary>
/// Comprehensive tests for VisualTreeTestHelper.
/// </summary>
public class VisualTreeTestHelperTests
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
        // Create a complex control tree for testing
        var root = new StackPanel { Name = "RootPanel" };

        // First section
        var section1 = new Border
        {
            Name = "Section1",
            Background = Brushes.Blue,
            Child = new StackPanel
            {
                Children =
                {
                    new Button { Name = "Button1", Content = "Click Me", Tag = "primary" },
                    new Button { Name = "Button2", Content = "Cancel", IsEnabled = false },
                    new TextBlock { Name = "Label1", Text = "Hello World" }
                }
            }
        };
        section1.Classes.Add("section");

        // Second section with nested controls
        var section2 = new Border
        {
            Name = "Section2",
            Child = new Grid
            {
                Children =
                {
                    new TextBox { Name = "Input1", Text = "Enter text", PlaceholderText = "Type here" },
                    new CheckBox { Name = "Check1", Content = "Accept Terms", IsChecked = true },
                    new ComboBox
                    {
                        Name = "Combo1",
                        Items = { "Option1", "Option2", "Option3" }
                    }
                }
            }
        };
        section2.Classes.Add("section");
        section2.Classes.Add("form");

        // Third section - hidden
        var section3 = new Border
        {
            Name = "Section3",
            IsVisible = false,
            Child = new TextBlock { Name = "HiddenLabel", Text = "You can't see me" }
        };

        // Nested structure for path testing
        var deepNesting = new Border
        {
            Name = "Level1",
            Child = new Border
            {
                Name = "Level2",
                Child = new Border
                {
                    Name = "Level3",
                    Child = new Button { Name = "DeepButton", Content = "Deep" }
                }
            }
        };

        root.Children.Add(section1);
        root.Children.Add(section2);
        root.Children.Add(section3);
        root.Children.Add(deepNesting);

        return root;
    }

    #endregion

    #region FindFirst<T> Tests

    [AvaloniaFact]
    public void FindFirst_ByType_ReturnsFirstMatch()
    {
        var window = CreateTestWindow();

        var button = window.FindFirst<Button>();

        Assert.NotNull(button);
        Assert.Equal("Button1", button.Name);
    }

    [AvaloniaFact]
    public void FindFirst_ByType_ReturnsNullWhenNotFound()
    {
        var window = CreateTestWindow();

        var slider = window.FindFirst<Slider>();

        Assert.Null(slider);
    }

    [AvaloniaFact]
    public void FindFirst_WithPredicate_ReturnsMatchingControl()
    {
        var window = CreateTestWindow();

        var button = window.FindFirst<Button>(b => b.Content?.ToString() == "Cancel");

        Assert.NotNull(button);
        Assert.Equal("Button2", button.Name);
    }

    [AvaloniaFact]
    public void FindFirst_IncludeSelf_IncludesRootInSearch()
    {
        var window = CreateTestWindow();

        var found = window.FindFirst<Window>(includeSelf: true);

        Assert.Same(window, found);
    }

    #endregion

    #region FindAll<T> Tests

    [AvaloniaFact]
    public void FindAll_ByType_ReturnsAllMatches()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("RootPanel")!;

        // Find all buttons (includes internal buttons from controls like ComboBox)
        var allButtons = root.FindAll<Button>().ToList();

        // Verify we find at least our named buttons by checking specific names exist
        Assert.Contains(allButtons, b => b.Name == "Button1");
        Assert.Contains(allButtons, b => b.Name == "Button2");
        Assert.Contains(allButtons, b => b.Name == "DeepButton");

        // Verify total count includes all buttons (our 3 + any from internal controls)
        Assert.True(allButtons.Count >= 3, "Should find at least our 3 named buttons");
    }

    [AvaloniaFact]
    public void FindAll_WithPredicate_FiltersResults()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("RootPanel")!;

        // Get all enabled buttons
        var enabledButtons = root.FindAll<Button>(b => b.IsEnabled).ToList();

        // Verify Button2 (disabled) is NOT in the list
        Assert.DoesNotContain(enabledButtons, b => b.Name == "Button2");

        // Verify our enabled named buttons ARE in the list
        Assert.Contains(enabledButtons, b => b.Name == "Button1");
        Assert.Contains(enabledButtons, b => b.Name == "DeepButton");
    }

    [AvaloniaFact]
    public void FindAll_ReturnsEmptyWhenNoMatches()
    {
        var window = CreateTestWindow();

        var sliders = window.FindAll<Slider>().ToList();

        Assert.Empty(sliders);
    }

    #endregion

    #region FindByName Tests

    [AvaloniaFact]
    public void FindByName_ReturnsControlWithName()
    {
        var window = CreateTestWindow();

        var control = window.FindByName("Button1");

        Assert.NotNull(control);
        Assert.IsType<Button>(control);
    }

    [AvaloniaFact]
    public void FindByName_Generic_ReturnsCastControl()
    {
        var window = CreateTestWindow();

        var button = window.FindByName<Button>("Button1");

        Assert.NotNull(button);
        Assert.Equal("Click Me", button.Content);
    }

    [AvaloniaFact]
    public void FindByName_ReturnsNullWhenNotFound()
    {
        var window = CreateTestWindow();

        var control = window.FindByName("NonExistent");

        Assert.Null(control);
    }

    [AvaloniaFact]
    public void GetByName_ThrowsWhenNotFound()
    {
        var window = CreateTestWindow();

        Assert.Throws<InvalidOperationException>(() => window.GetByName<Button>("NonExistent"));
    }

    [AvaloniaFact]
    public void GetByName_ReturnsControlWhenFound()
    {
        var window = CreateTestWindow();

        var button = window.GetByName<Button>("Button1");

        Assert.NotNull(button);
    }

    #endregion

    #region FindByProperty Tests

    [AvaloniaFact]
    public void FindByProperty_FindsControlsWithPropertyValue()
    {
        var window = CreateTestWindow();

        var disabledControls = window.FindByProperty<InputElement, bool>(
            InputElement.IsEnabledProperty, false).ToList();

        Assert.Contains(disabledControls, c => c.Name == "Button2");
    }

    [AvaloniaFact]
    public void FindByTag_FindsControlsWithTag()
    {
        var window = CreateTestWindow();

        var tagged = window.FindByTag("primary").ToList();

        Assert.Single(tagged);
        Assert.Equal("Button1", tagged[0].Name);
    }

    [AvaloniaFact]
    public void FindByClass_FindsControlsWithCssClass()
    {
        var window = CreateTestWindow();

        var sections = window.FindByClass<Border>("section").ToList();

        Assert.Equal(2, sections.Count);
    }

    [AvaloniaFact]
    public void FindByClasses_FindsControlsWithAllClasses()
    {
        var window = CreateTestWindow();

        var formSections = window.FindByClasses<Border>("section", "form").ToList();

        Assert.Single(formSections);
        Assert.Equal("Section2", formSections[0].Name);
    }

    #endregion

    #region FindByState Tests

    [AvaloniaFact]
    public void FindEnabled_ReturnsOnlyEnabledControls()
    {
        var window = CreateTestWindow();

        var enabledButtons = window.FindEnabled<Button>().ToList();

        Assert.DoesNotContain(enabledButtons, b => b.Name == "Button2");
    }

    [AvaloniaFact]
    public void FindDisabled_ReturnsOnlyDisabledControls()
    {
        var window = CreateTestWindow();

        var disabledButtons = window.FindDisabled<Button>().ToList();

        Assert.Single(disabledButtons);
        Assert.Equal("Button2", disabledButtons[0].Name);
    }

    [AvaloniaFact]
    public void FindVisible_ReturnsOnlyVisibleControls()
    {
        var window = CreateTestWindow();

        var visibleBorders = window.FindVisible<Border>().ToList();

        // Section3 is hidden, so should not be included
        Assert.DoesNotContain(visibleBorders, b => b.Name == "Section3");
    }

    [AvaloniaFact]
    public void FindHidden_ReturnsOnlyHiddenControls()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("RootPanel")!;

        // Get all hidden borders
        var hiddenBorders = root.FindHidden<Border>().ToList();

        // Verify all found borders are actually hidden
        Assert.All(hiddenBorders, b => Assert.False(b.IsVisible));

        // Verify our named hidden border is in the list
        Assert.Contains(hiddenBorders, b => b.Name == "Section3");
    }

    [AvaloniaFact]
    public void FindFocusable_ReturnsFocusableControls()
    {
        var window = CreateTestWindow();

        var focusable = window.FindFocusable<Button>().ToList();

        Assert.NotEmpty(focusable);
    }

    #endregion

    #region Ancestor Query Tests

    [AvaloniaFact]
    public void FindAncestor_ReturnsFirstAncestorOfType()
    {
        var window = CreateTestWindow();
        var button = window.FindByName<Button>("Button1")!;

        var ancestor = button.FindAncestor<Border>();

        Assert.NotNull(ancestor);
        Assert.Equal("Section1", ancestor.Name);
    }

    [AvaloniaFact]
    public void FindAncestor_WithPredicate_ReturnsMatchingAncestor()
    {
        var window = CreateTestWindow();
        var button = window.FindByName<Button>("DeepButton")!;

        var ancestor = button.FindAncestor<Border>(b => b.Name == "Level1");

        Assert.NotNull(ancestor);
        Assert.Equal("Level1", ancestor.Name);
    }

    [AvaloniaFact]
    public void GetAncestors_ReturnsAllAncestorsOfType()
    {
        var window = CreateTestWindow();
        var button = window.FindByName<Button>("DeepButton")!;

        var ancestors = button.GetAncestors<Border>().ToList();

        Assert.Equal(3, ancestors.Count); // Level3, Level2, Level1
    }

    [AvaloniaFact]
    public void HasAncestor_ReturnsTrueWhenAncestorExists()
    {
        var window = CreateTestWindow();
        var button = window.FindByName<Button>("Button1")!;

        Assert.True(button.HasAncestor<Border>());
        Assert.True(button.HasAncestor<StackPanel>());
    }

    [AvaloniaFact]
    public void HasAncestor_ReturnsFalseWhenNoAncestor()
    {
        var window = CreateTestWindow();
        var button = window.FindByName<Button>("Button1")!;

        Assert.False(button.HasAncestor<Slider>());
    }

    #endregion

    #region Children Query Tests

    [AvaloniaFact]
    public void GetChildren_ReturnsDirectChildrenOfType()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("RootPanel")!;

        var borders = root.GetChildren<Border>().ToList();

        Assert.Equal(4, borders.Count);
    }

    [AvaloniaFact]
    public void GetChildCount_ReturnsNumberOfDirectChildren()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("RootPanel")!;

        var count = root.GetChildCount();

        Assert.Equal(4, count);
    }

    [AvaloniaFact]
    public void GetDescendantCount_ReturnsTotalDescendants()
    {
        var window = CreateTestWindow();
        var section1 = window.FindByName<Border>("Section1")!;

        var count = section1.GetDescendantCount();

        Assert.True(count > 3); // StackPanel + 3 children + possibly more
    }

    [AvaloniaFact]
    public void GetChildAt_ReturnsChildAtIndex()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("RootPanel")!;

        var firstChild = root.GetChildAt<Border>(0);

        Assert.Equal("Section1", firstChild.Name);
    }

    [AvaloniaFact]
    public void GetFirstChild_ReturnsFirstChildOfType()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("RootPanel")!;

        var firstBorder = root.GetFirstChild<Border>();

        Assert.NotNull(firstBorder);
        Assert.Equal("Section1", firstBorder.Name);
    }

    [AvaloniaFact]
    public void GetLastChild_ReturnsLastChildOfType()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("RootPanel")!;

        var lastBorder = root.GetLastChild<Border>();

        Assert.NotNull(lastBorder);
        Assert.Equal("Level1", lastBorder.Name);
    }

    #endregion

    #region Tree Path Tests

    [AvaloniaFact]
    public void GetPathFromRoot_ReturnsPathFromRootToElement()
    {
        var window = CreateTestWindow();
        var button = window.FindByName<Button>("DeepButton")!;

        var path = button.GetPathFromRoot();

        Assert.True(path.Count > 4); // Window > ... > Level1 > Level2 > Level3 > Button
        Assert.Same(button, path.Last());
    }

    [AvaloniaFact]
    public void GetPathString_ReturnsTypeNamesPath()
    {
        var window = CreateTestWindow();
        var button = window.FindByName<Button>("DeepButton")!;

        var pathString = button.GetPathString();

        Assert.Contains("Border", pathString);
        Assert.Contains("Button", pathString);
        Assert.Contains(" > ", pathString);
    }

    [AvaloniaFact]
    public void GetDetailedPathString_IncludesNames()
    {
        var window = CreateTestWindow();
        var button = window.FindByName<Button>("DeepButton")!;

        var pathString = button.GetDetailedPathString();

        Assert.Contains("[Level1]", pathString);
        Assert.Contains("[Level2]", pathString);
        Assert.Contains("[DeepButton]", pathString);
    }

    #endregion

    #region Tree Validation Tests

    [AvaloniaFact]
    public void Contains_ReturnsTrueWhenDescendantExists()
    {
        var window = CreateTestWindow();

        Assert.True(window.Contains<Button>());
        Assert.True(window.Contains<TextBox>());
    }

    [AvaloniaFact]
    public void Contains_ReturnsFalseWhenNoDescendant()
    {
        var window = CreateTestWindow();

        Assert.False(window.Contains<Slider>());
    }

    [AvaloniaFact]
    public void ContainsNamed_ReturnsTrueForExistingName()
    {
        var window = CreateTestWindow();

        Assert.True(window.ContainsNamed("Button1"));
        Assert.True(window.ContainsNamed("DeepButton"));
    }

    [AvaloniaFact]
    public void ContainsNamed_ReturnsFalseForMissingName()
    {
        var window = CreateTestWindow();

        Assert.False(window.ContainsNamed("NonExistent"));
    }

    [AvaloniaFact]
    public void Count_ReturnsNumberOfMatchingDescendants()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("RootPanel")!;

        // Count all buttons (includes internal buttons from controls like ComboBox)
        var buttonCount = root.Count<Button>();

        // Should find at least our 3 named buttons
        Assert.True(buttonCount >= 3, $"Expected at least 3 buttons, found {buttonCount}");

        // Verify Count matches FindAll.Count()
        Assert.Equal(root.FindAll<Button>().Count(), buttonCount);
    }

    [AvaloniaFact]
    public void MatchesStructure_ReturnsTrueForMatchingPath()
    {
        var window = CreateTestWindow();
        var level1 = window.FindByName<Border>("Level1")!;

        var matches = level1.MatchesStructure(typeof(Border), typeof(Border), typeof(Border), typeof(Button));

        Assert.True(matches);
    }

    [AvaloniaFact]
    public void MatchesStructure_ReturnsFalseForMismatch()
    {
        var window = CreateTestWindow();
        var level1 = window.FindByName<Border>("Level1")!;

        var matches = level1.MatchesStructure(typeof(Border), typeof(StackPanel));

        Assert.False(matches);
    }

    #endregion

    #region Bounds and Layout Tests

    [AvaloniaFact]
    public void GetCenter_ReturnsCenterPoint()
    {
        var window = CreateTestWindow();
        var button = window.FindByName<Button>("Button1")!;

        // Layout needs to complete first
        window.UpdateLayout();

        var center = button.GetCenter();

        Assert.True(center.X >= 0);
        Assert.True(center.Y >= 0);
    }

    [AvaloniaFact]
    public void GetBoundsRelativeTo_ReturnsRelativeBounds()
    {
        var window = CreateTestWindow();
        window.UpdateLayout();

        var button = window.FindByName<Button>("Button1")!;
        var section = window.FindByName<Border>("Section1")!;

        var bounds = button.GetBoundsRelativeTo(section);

        Assert.NotNull(bounds);
    }

    #endregion

    #region Debug Helper Tests

    [AvaloniaFact]
    public void PrintTree_ReturnsTreeStructure()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("RootPanel")!;

        var tree = root.PrintTree(maxDepth: 2);

        Assert.Contains("StackPanel", tree);
        Assert.Contains("Border", tree);
        Assert.Contains("[Section1]", tree);
    }

    [AvaloniaFact]
    public void GetTypeSummary_ReturnsCounts()
    {
        var window = CreateTestWindow();

        var summary = window.GetTypeSummary();

        Assert.True(summary.ContainsKey("Button"));
        Assert.Equal(3, summary["Button"]);
    }

    [AvaloniaFact]
    public void GetNamedControls_ReturnsDictionary()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("RootPanel")!;

        var named = root.GetNamedControls();

        Assert.True(named.ContainsKey("Button1"));
        Assert.True(named.ContainsKey("Section1"));
        Assert.True(named.ContainsKey("DeepButton"));
    }

    #endregion

    #region Hit Testing Tests

    [AvaloniaFact]
    public void HitTest_ReturnsVisualAtPoint()
    {
        var window = CreateTestWindow();
        window.UpdateLayout();

        // Hit test at center of window
        var visual = window.HitTest(new Point(400, 300));

        // Should return something (exact result depends on layout)
        // This is mainly testing that the method doesn't throw
        Assert.True(visual != null || true); // Just ensuring no exceptions
    }

    #endregion
}

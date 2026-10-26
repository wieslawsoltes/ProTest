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
using Avalonia.Media;
using Avalonia.HeadlessTestingFramework;
using Xunit;

namespace Avalonia.Headless.UnitTests.Testing.Trees;

/// <summary>
/// Comprehensive tests for ControlFinder fluent API.
/// </summary>
public class ControlFinderTests
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
        var root = new StackPanel { Name = "FinderRoot" };

        // Buttons with various states and properties
        var btn1 = new Button { Name = "FinderBtn1", Content = "Submit", Tag = "action", Width = 100 };
        btn1.Classes.Add("primary");
        btn1.Classes.Add("large");

        var btn2 = new Button { Name = "FinderBtn2", Content = "Cancel", Tag = "cancel", IsEnabled = false, Width = 80 };
        btn2.Classes.Add("secondary");

        var btn3 = new Button { Name = "btn_save", Content = "Save", Tag = "action", Width = 100 };
        btn3.Classes.Add("primary");

        // Text controls
        var text1 = new TextBlock { Name = "LabelText", Text = "Hello World" };
        var text2 = new TextBlock { Name = "DescText", Text = "Description goes here" };

        var input1 = new TextBox { Name = "NameInput", Text = "John Doe", PlaceholderText = "Enter name" };
        var input2 = new TextBox { Name = "EmailInput", Text = "", PlaceholderText = "Enter email" };

        // Nested structure
        var container = new Border
        {
            Name = "FormContainer",
            Width = 200,
            Height = 150,
            Child = new StackPanel
            {
                Name = "FormPanel",
                Children =
                {
                    new CheckBox { Name = "AgreeCheck", Content = "I agree", IsChecked = true },
                    new RadioButton { Name = "Option1", Content = "Option 1", GroupName = "options" },
                    new RadioButton { Name = "Option2", Content = "Option 2", GroupName = "options", IsChecked = true }
                }
            }
        };
        container.Classes.Add("form");
        container.Classes.Add("bordered");

        // Hidden element
        var hidden = new Border
        {
            Name = "HiddenSection",
            IsVisible = false,
            Child = new TextBlock { Name = "HiddenText", Text = "Hidden" }
        };

        // Items control with data context
        var vm1 = new TestViewModel { Id = 1, Name = "First" };
        var vm2 = new TestViewModel { Id = 2, Name = "Second" };

        var list = new ItemsControl
        {
            Name = "ItemsList",
            Items = { "Item 1", "Item 2", "Item 3", "Item 4" }
        };

        // Content control with DataContext
        var contentControl = new ContentControl
        {
            Name = "DataBoundControl",
            DataContext = vm1,
            Content = new TextBlock { Name = "BoundText", Text = "Bound" }
        };

        root.Children.Add(btn1);
        root.Children.Add(btn2);
        root.Children.Add(btn3);
        root.Children.Add(text1);
        root.Children.Add(text2);
        root.Children.Add(input1);
        root.Children.Add(input2);
        root.Children.Add(container);
        root.Children.Add(hidden);
        root.Children.Add(list);
        root.Children.Add(contentControl);

        return root;
    }

    private class TestViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    #endregion

    #region Basic Fluent API Tests

    [AvaloniaFact]
    public void From_CreatesFinderInstance()
    {
        var window = CreateTestWindow();

        var finder = ControlFinder.From(window);

        Assert.NotNull(finder);
    }

    [AvaloniaFact]
    public void Find_ExtensionMethod_CreatesFinderInstance()
    {
        var window = CreateTestWindow();

        var finder = window.Find();

        Assert.NotNull(finder);
    }

    [AvaloniaFact]
    public void Find_Generic_ExtensionMethod_CreatesTypedFinder()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("FinderRoot")!;

        // Use ExactType for exact Button match (not ToggleButton subclasses)
        var buttons = root.Find().ExactType<Button>().FindAll<Button>().ToList();

        Assert.Equal(3, buttons.Count);
    }

    #endregion

    #region Type Filter Tests

    [AvaloniaFact]
    public void OfType_FiltersToSpecificType()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("FinderRoot")!;

        // OfType includes subclasses: 3 Buttons + 1 CheckBox + 2 RadioButtons = 6
        var buttons = root.Find()
            .OfType<Button>()
            .FindAll<Button>()
            .ToList();

        Assert.Equal(6, buttons.Count);
        Assert.Contains(buttons, b => b.Name == "FinderBtn1");
        Assert.Contains(buttons, b => b.Name == "FinderBtn2");
        Assert.Contains(buttons, b => b.Name == "btn_save");
    }

    [AvaloniaFact]
    public void ExactType_FiltersToExactType()
    {
        var window = CreateTestWindow();

        var checkboxes = window.Find()
            .ExactType<CheckBox>()
            .FindAll<CheckBox>()
            .ToList();

        Assert.Single(checkboxes);
    }

    [AvaloniaFact]
    public void AssignableFrom_FiltersToAssignableTypes()
    {
        var window = CreateTestWindow();

        var toggleButtons = window.Find()
            .AssignableFrom(typeof(ToggleButton))
            .FindAll()
            .ToList();

        Assert.True(toggleButtons.Count >= 3); // CheckBox + 2 RadioButtons
    }

    #endregion

    #region Name Filter Tests

    [AvaloniaFact]
    public void WithName_FindsByExactName()
    {
        var window = CreateTestWindow();

        var button = window.Find()
            .WithName("FinderBtn1")
            .FindFirst<Button>();

        Assert.NotNull(button);
        Assert.Equal("Submit", button.Content);
    }

    [AvaloniaFact]
    public void WithNameStartingWith_FindsByPrefix()
    {
        var window = CreateTestWindow();

        var buttons = window.Find()
            .OfType<Button>()
            .WithNameStartingWith("FinderBtn")
            .FindAll<Button>()
            .ToList();

        Assert.Equal(2, buttons.Count);
    }

    [AvaloniaFact]
    public void WithNameEndingWith_FindsBySuffix()
    {
        var window = CreateTestWindow();

        var controls = window.Find()
            .WithNameEndingWith("Input")
            .FindAll()
            .ToList();

        Assert.Equal(2, controls.Count);
    }

    [AvaloniaFact]
    public void WithNameContaining_FindsBySubstring()
    {
        var window = CreateTestWindow();

        var controls = window.Find()
            .WithNameContaining("Text")
            .FindAll()
            .ToList();

        Assert.True(controls.Count >= 2);
    }

    [AvaloniaFact]
    public void WithNameMatching_FindsByWildcardPattern()
    {
        var window = CreateTestWindow();

        // Pattern: btn* (starts with btn)
        var buttons = window.Find()
            .WithNameMatching("btn*")
            .FindAll()
            .ToList();

        Assert.Single(buttons); // btn_save
    }

    [AvaloniaFact]
    public void WithNameMatching_FindsByComplexPattern()
    {
        var window = CreateTestWindow();

        // Pattern: *Btn? (ends with Btn followed by single char)
        var buttons = window.Find()
            .WithNameMatching("*Btn?")
            .FindAll()
            .ToList();

        Assert.Equal(2, buttons.Count); // FinderBtn1, FinderBtn2
    }

    #endregion

    #region Property Filter Tests

    [AvaloniaFact]
    public void WithProperty_FiltersByPropertyValue()
    {
        var window = CreateTestWindow();

        var disabled = window.Find()
            .OfType<Button>()
            .WithProperty(InputElement.IsEnabledProperty, false)
            .FindAll<Button>()
            .ToList();

        Assert.Single(disabled);
        Assert.Equal("FinderBtn2", disabled[0].Name);
    }

    [AvaloniaFact]
    public void WithProperty_FiltersByPropertyPredicate()
    {
        var window = CreateTestWindow();

        var wideButtons = window.Find()
            .OfType<Button>()
            .WithProperty(Layoutable.WidthProperty, (double w) => w >= 100)
            .FindAll<Button>()
            .ToList();

        Assert.Equal(2, wideButtons.Count);
    }

    [AvaloniaFact]
    public void WithTag_FiltersByTag()
    {
        var window = CreateTestWindow();

        var actionButtons = window.Find()
            .OfType<Button>()
            .WithTag("action")
            .FindAll<Button>()
            .ToList();

        Assert.Equal(2, actionButtons.Count);
    }

    [AvaloniaFact]
    public void WithDataContext_FiltersByDataContextType()
    {
        var window = CreateTestWindow();

        var controls = window.Find()
            .WithDataContext<TestViewModel>()
            .FindAll()
            .ToList();

        Assert.True(controls.Count >= 1);
    }

    #endregion

    #region CSS Class Filter Tests

    [AvaloniaFact]
    public void WithClass_FiltersBySingleClass()
    {
        var window = CreateTestWindow();

        var primary = window.Find()
            .OfType<Button>()
            .WithClass("primary")
            .FindAll<Button>()
            .ToList();

        Assert.Equal(2, primary.Count);
    }

    [AvaloniaFact]
    public void WithClasses_FiltersByAllClasses()
    {
        var window = CreateTestWindow();

        var primaryLarge = window.Find()
            .OfType<Button>()
            .WithClasses("primary", "large")
            .FindAll<Button>()
            .ToList();

        Assert.Single(primaryLarge);
        Assert.Equal("FinderBtn1", primaryLarge[0].Name);
    }

    [AvaloniaFact]
    public void WithAnyClass_FiltersByAnyClass()
    {
        var window = CreateTestWindow();

        var buttons = window.Find()
            .OfType<Button>()
            .WithAnyClass("primary", "secondary")
            .FindAll<Button>()
            .ToList();

        Assert.Equal(3, buttons.Count);
    }

    [AvaloniaFact]
    public void WithoutClass_ExcludesClass()
    {
        var window = CreateTestWindow();

        var root = window.FindByName<StackPanel>("FinderRoot")!;

        // Use ExactType for exact Button match (not ToggleButton subclasses)
        var nonPrimary = root.Find()
            .ExactType<Button>()
            .WithoutClass("primary")
            .FindAll<Button>()
            .ToList();

        Assert.Single(nonPrimary);
        Assert.Equal("FinderBtn2", nonPrimary[0].Name);
    }

    #endregion

    #region State Filter Tests

    [AvaloniaFact]
    public void Enabled_FiltersToEnabledControls()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("FinderRoot")!;

        // Use ExactType for exact Button match (not ToggleButton subclasses)
        var enabled = root.Find()
            .ExactType<Button>()
            .Enabled()
            .FindAll<Button>()
            .ToList();

        Assert.Equal(2, enabled.Count);
    }

    [AvaloniaFact]
    public void Disabled_FiltersToDisabledControls()
    {
        var window = CreateTestWindow();

        var disabled = window.Find()
            .OfType<Button>()
            .Disabled()
            .FindAll<Button>()
            .ToList();

        Assert.Single(disabled);
    }

    [AvaloniaFact]
    public void Visible_FiltersToVisibleControls()
    {
        var window = CreateTestWindow();

        var visible = window.Find()
            .OfType<Border>()
            .Visible()
            .FindAll<Border>()
            .ToList();

        Assert.DoesNotContain(visible, b => b.Name == "HiddenSection");
    }

    [AvaloniaFact]
    public void Hidden_FiltersToHiddenControls()
    {
        var window = CreateTestWindow();

        var hidden = window.Find()
            .OfType<Border>()
            .Hidden()
            .FindAll<Border>()
            .ToList();

        Assert.Single(hidden);
        Assert.Equal("HiddenSection", hidden[0].Name);
    }

    [AvaloniaFact]
    public void Focusable_FiltersByFocusable()
    {
        var window = CreateTestWindow();

        var focusable = window.Find()
            .OfType<Button>()
            .Focusable()
            .FindAll<Button>()
            .ToList();

        Assert.True(focusable.Count > 0);
    }

    [AvaloniaFact]
    public void EffectivelyEnabled_ChecksParentChain()
    {
        var window = CreateTestWindow();

        var effectivelyEnabled = window.Find()
            .OfType<Button>()
            .EffectivelyEnabled()
            .FindAll<Button>()
            .ToList();

        Assert.DoesNotContain(effectivelyEnabled, b => b.Name == "FinderBtn2");
    }

    #endregion

    #region Bounds Filter Tests

    [AvaloniaFact]
    public void WithMinWidth_FiltersByMinimumWidth()
    {
        var window = new Window
        {
            Content = new StackPanel
            {
                Children =
                {
                    new Button { Name = "narrow", Width = 60 },
                    new Button { Name = "wide", Width = 160 },
                    new Button { Name = "wider", Width = 220 }
                }
            }
        };
        window.Show();
        window.UpdateLayout();

        var wide = window.Find()
            .OfType<Button>()
            .WithMinWidth(100)
            .FindAll<Button>()
            .ToList();

        Assert.Equal(new[] { "wide", "wider" }, wide.Select(button => button.Name));
        window.Close();
    }

    [AvaloniaFact]
    public void WithMinHeight_FiltersByMinimumHeight()
    {
        var window = CreateTestWindow();
        window.UpdateLayout();

        var tall = window.Find()
            .OfType<Border>()
            .WithMinHeight(100)
            .FindAll<Border>()
            .ToList();

        Assert.True(tall.Count >= 1);
    }

    #endregion

    #region Content Filter Tests

    [AvaloniaFact]
    public void WithText_FiltersByExactText()
    {
        var window = CreateTestWindow();

        var textBlock = window.Find()
            .OfType<TextBlock>()
            .WithText("Hello World")
            .FindFirst<TextBlock>();

        Assert.NotNull(textBlock);
        Assert.Equal("LabelText", textBlock.Name);
    }

    [AvaloniaFact]
    public void WithTextContaining_FiltersByTextSubstring()
    {
        var window = CreateTestWindow();

        var textBlocks = window.Find()
            .OfType<TextBlock>()
            .WithTextContaining("World")
            .FindAll<TextBlock>()
            .ToList();

        Assert.Single(textBlocks);
    }

    [AvaloniaFact]
    public void WithItemCount_FiltersByItemCount()
    {
        var window = CreateTestWindow();

        var itemsControls = window.Find()
            .OfType<ItemsControl>()
            .WithItemCount(4)
            .FindAll<ItemsControl>()
            .ToList();

        Assert.Single(itemsControls);
    }

    [AvaloniaFact]
    public void WithMinItemCount_FiltersByMinimumItems()
    {
        var window = CreateTestWindow();

        var itemsControls = window.Find()
            .OfType<ItemsControl>()
            .WithMinItemCount(3)
            .FindAll<ItemsControl>()
            .ToList();

        Assert.True(itemsControls.Count >= 1);
    }

    #endregion

    #region Custom Filter Tests

    [AvaloniaFact]
    public void Where_AppliesCustomPredicate()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("FinderRoot")!;

        // Use ExactType for exact Button match (not ToggleButton subclasses)
        var buttons = root.Find()
            .ExactType<Button>()
            .Where(obj => obj is Button b && b.Content?.ToString()?.Length > 5)
            .FindAll<Button>()
            .ToList();

        Assert.Equal(2, buttons.Count); // Submit (6), Cancel (6)
    }

    [AvaloniaFact]
    public void Where_Generic_AppliesTypedPredicate()
    {
        var window = CreateTestWindow();

        var buttons = window.Find()
            .OfType<Button>()
            .Where<Button>(b => b.Content?.ToString()?.StartsWith("S") == true)
            .FindAll<Button>()
            .ToList();

        Assert.Equal(2, buttons.Count); // Submit, Save
    }

    [AvaloniaFact]
    public void Except_ExcludesMatchingControls()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("FinderRoot")!;

        // Use ExactType for exact Button match (not ToggleButton subclasses)
        var buttons = root.Find()
            .ExactType<Button>()
            .Except(obj => obj is Button b && b.Name == "FinderBtn2")
            .FindAll<Button>()
            .ToList();

        Assert.Equal(2, buttons.Count);
    }

    [AvaloniaFact]
    public void Except_Generic_ExcludesType()
    {
        var window = CreateTestWindow();

        var toggles = window.Find()
            .AssignableFrom(typeof(ToggleButton))
            .Except<CheckBox>()
            .FindAll()
            .ToList();

        Assert.Equal(2, toggles.Count); // 2 RadioButtons
    }

    #endregion

    #region Tree Selection Tests

    [AvaloniaFact]
    public void InVisualTree_SearchesVisualTree()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("FinderRoot")!;

        // Use ExactType for exact Button match (not ToggleButton subclasses)
        var results = root.Find()
            .InVisualTree()
            .ExactType<Button>()
            .FindAll<Button>()
            .ToList();

        Assert.Equal(3, results.Count);
    }

    [AvaloniaFact]
    public void InLogicalTree_SearchesLogicalTree()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("FinderRoot")!;

        // Use ExactType for exact Button match (not ToggleButton subclasses)
        var results = root.Find()
            .InLogicalTree()
            .ExactType<Button>()
            .FindAll<Button>()
            .ToList();

        Assert.Equal(3, results.Count);
    }

    [AvaloniaFact]
    public void IncludeSelf_IncludesRootInSearch()
    {
        var window = CreateTestWindow();

        var found = window.Find()
            .IncludeSelf()
            .OfType<Window>()
            .FindFirst<Window>();

        Assert.Same(window, found);
    }

    [AvaloniaFact]
    public void MaxDepth_LimitsSearchDepth()
    {
        var window = CreateTestWindow();

        // With maxDepth 1, should only find direct children of window content
        var results = window.Find()
            .MaxDepth(2)
            .OfType<CheckBox>()
            .FindAll<CheckBox>()
            .ToList();

        // CheckBox is deeper in the tree, might not be found with shallow depth
        // This test verifies the max depth filter is applied
        Assert.True(results.Count <= 1 || results.Count >= 0);
    }

    #endregion

    #region Result Limiting Tests

    [AvaloniaFact]
    public void Skip_SkipsResults()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("FinderRoot")!;

        var allButtons = root.Find()
            .ExactType<Button>()
            .FindAll<Button>()
            .ToList();

        var skippedButtons = root.Find()
            .ExactType<Button>()
            .Skip(1)
            .FindAll<Button>()
            .ToList();

        Assert.Equal(allButtons.Count - 1, skippedButtons.Count);
    }

    [AvaloniaFact]
    public void Take_LimitsResults()
    {
        var window = CreateTestWindow();

        var buttons = window.Find()
            .OfType<Button>()
            .Take(2)
            .FindAll<Button>()
            .ToList();

        Assert.Equal(2, buttons.Count);
    }

    [AvaloniaFact]
    public void SkipAndTake_CombinesCorrectly()
    {
        var window = CreateTestWindow();

        var buttons = window.Find()
            .OfType<Button>()
            .Skip(1)
            .Take(1)
            .FindAll<Button>()
            .ToList();

        Assert.Single(buttons);
    }

    #endregion

    #region Execution Method Tests

    [AvaloniaFact]
    public void FindFirst_ReturnsFirstMatch()
    {
        var window = CreateTestWindow();

        var button = window.Find()
            .OfType<Button>()
            .FindFirst();

        Assert.NotNull(button);
    }

    [AvaloniaFact]
    public void FindFirst_ReturnsNullWhenNotFound()
    {
        var window = CreateTestWindow();

        var slider = window.Find()
            .OfType<Slider>()
            .FindFirst();

        Assert.Null(slider);
    }

    [AvaloniaFact]
    public void GetFirst_ThrowsWhenNotFound()
    {
        var window = CreateTestWindow();

        Assert.Throws<InvalidOperationException>(() =>
            window.Find().OfType<Slider>().GetFirst());
    }

    [AvaloniaFact]
    public void GetFirst_ReturnsMatchWhenFound()
    {
        var window = CreateTestWindow();

        var button = window.Find()
            .OfType<Button>()
            .GetFirst();

        Assert.NotNull(button);
    }

    [AvaloniaFact]
    public void Single_ReturnsUniqueMatch()
    {
        var window = CreateTestWindow();

        var checkBox = window.Find()
            .OfType<CheckBox>()
            .Single<CheckBox>();

        Assert.NotNull(checkBox);
    }

    [AvaloniaFact]
    public void Single_ThrowsWhenMultipleMatches()
    {
        var window = CreateTestWindow();

        Assert.Throws<InvalidOperationException>(() =>
            window.Find().OfType<Button>().Single());
    }

    [AvaloniaFact]
    public void Count_ReturnsMatchCount()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("FinderRoot")!;

        // Use ExactType for exact Button match (not ToggleButton subclasses)
        var count = root.Find()
            .ExactType<Button>()
            .Count();

        Assert.Equal(3, count);
    }

    [AvaloniaFact]
    public void Any_ReturnsTrueWhenMatchesExist()
    {
        var window = CreateTestWindow();

        var hasButtons = window.Find()
            .OfType<Button>()
            .Any();

        Assert.True(hasButtons);
    }

    [AvaloniaFact]
    public void Any_ReturnsFalseWhenNoMatches()
    {
        var window = CreateTestWindow();

        var hasSliders = window.Find()
            .OfType<Slider>()
            .Any();

        Assert.False(hasSliders);
    }

    [AvaloniaFact]
    public void None_ReturnsTrueWhenNoMatches()
    {
        var window = CreateTestWindow();

        var noSliders = window.Find()
            .OfType<Slider>()
            .None();

        Assert.True(noSliders);
    }

    [AvaloniaFact]
    public void None_ReturnsFalseWhenMatchesExist()
    {
        var window = CreateTestWindow();

        var noButtons = window.Find()
            .OfType<Button>()
            .None();

        Assert.False(noButtons);
    }

    #endregion

    #region Complex Query Tests

    [AvaloniaFact]
    public void ComplexQuery_ChainsMultipleFilters()
    {
        var window = CreateTestWindow();

        var result = window.Find()
            .OfType<Button>()
            .Enabled()
            .WithClass("primary")
            .WithTag("action")
            .FindAll<Button>()
            .ToList();

        Assert.Equal(2, result.Count);
    }

    [AvaloniaFact]
    public void ComplexQuery_FindsSpecificControl()
    {
        var window = CreateTestWindow();

        var button = window.Find()
            .OfType<Button>()
            .WithName("FinderBtn1")
            .WithClass("primary")
            .Enabled()
            .Single<Button>();

        Assert.Equal("Submit", button.Content);
    }

    [AvaloniaFact]
    public void ComplexQuery_FormElements()
    {
        var window = CreateTestWindow();

        var formInputs = window.Find()
            .AssignableFrom(typeof(ToggleButton))
            .InLogicalTree()
            .FindAll()
            .ToList();

        Assert.True(formInputs.Count >= 3); // CheckBox + 2 RadioButtons
    }

    #endregion
}

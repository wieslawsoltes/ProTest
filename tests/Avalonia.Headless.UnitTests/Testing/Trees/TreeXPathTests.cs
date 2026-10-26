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
using Avalonia.Media;
using Avalonia.HeadlessTestingFramework;
using Xunit;

namespace Avalonia.Headless.UnitTests.Testing.Trees;

/// <summary>
/// Tests for TreeXPath functionality.
/// </summary>
public class TreeXPathTests
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
        var root = new StackPanel { Name = "XPathRoot" };

        // Buttons with various attributes
        var btn1 = new Button { Name = "SubmitButton", Content = "Submit", Tag = "primary" };
        btn1.Classes.Add("action");

        var btn2 = new Button { Name = "CancelButton", Content = "Cancel", Tag = "secondary", IsEnabled = false };
        btn2.Classes.Add("action");

        var btn3 = new Button { Name = "SaveButton", Content = "Save", Tag = "primary" };
        btn3.Classes.Add("action");

        root.Children.Add(btn1);
        root.Children.Add(btn2);
        root.Children.Add(btn3);

        // Nested structure
        var container = new Border
        {
            Name = "FormContainer",
            Child = new StackPanel
            {
                Name = "FormPanel",
                Children =
                {
                    new TextBox { Name = "NameInput", Text = "John" },
                    new TextBox { Name = "EmailInput", Text = "john@example.com" },
                    new CheckBox { Name = "AgreeCheckbox", Content = "I agree", IsChecked = true }
                }
            }
        };

        root.Children.Add(container);

        // Deep nested
        var deep = new Border
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

        root.Children.Add(deep);

        return root;
    }

    #endregion

    #region Basic Selection Tests

    [AvaloniaFact]
    public void Select_AllButtons_FindsAllButtons()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("XPathRoot")!;
        var xpath = new TreeXPath(root);

        var buttons = xpath.Select<Button>("//Button").ToList();

        // Note: CheckBox inherits from ToggleButton inherits from Button
        Assert.True(buttons.Count >= 4);
    }

    [AvaloniaFact]
    public void Select_ByName_FindsNamedControl()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("XPathRoot")!;
        var xpath = new TreeXPath(root);

        var button = xpath.SelectFirst<Button>("//Button[@Name='SubmitButton']");

        Assert.NotNull(button);
        Assert.Equal("SubmitButton", button.Name);
    }

    [AvaloniaFact]
    public void Select_ByTag_FindsTaggedControls()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("XPathRoot")!;
        var xpath = new TreeXPath(root);

        var primary = xpath.Select<Button>("//Button[@Tag='primary']").ToList();

        Assert.Equal(2, primary.Count);
        Assert.All(primary, b => Assert.Equal("primary", b.Tag?.ToString()));
    }

    [AvaloniaFact]
    public void Select_Wildcard_FindsAllChildren()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("XPathRoot")!;
        var xpath = new TreeXPath(root);

        var children = xpath.Select("/*").ToList();

        Assert.NotEmpty(children);
    }

    #endregion

    #region Predicate Tests

    [AvaloniaFact]
    public void Select_Position_FindsNthElement()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("XPathRoot")!;
        var xpath = new TreeXPath(root);

        var first = xpath.SelectFirst<Button>("//Button[1]");

        Assert.NotNull(first);
    }

    [AvaloniaFact]
    public void Select_Last_FindsLastElement()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("XPathRoot")!;
        var xpath = new TreeXPath(root);

        var last = xpath.SelectFirst<Button>("//Button[last()]");

        Assert.NotNull(last);
    }

    [AvaloniaFact]
    public void Select_Contains_FindsMatchingControls()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("XPathRoot")!;
        var xpath = new TreeXPath(root);

        var inputs = xpath.Select<TextBox>("//TextBox[contains(@Name, 'Input')]").ToList();

        Assert.Equal(2, inputs.Count);
    }

    [AvaloniaFact]
    public void Select_StartsWith_FindsMatchingControls()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("XPathRoot")!;
        var xpath = new TreeXPath(root);

        var buttons = xpath.Select<Button>("//Button[starts-with(@Name, 'S')]").ToList();

        // SubmitButton, SaveButton
        Assert.Equal(2, buttons.Count);
    }

    [AvaloniaFact]
    public void Select_EndsWith_FindsMatchingControls()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("XPathRoot")!;
        var xpath = new TreeXPath(root);

        var buttons = xpath.Select<Button>("//Button[ends-with(@Name, 'Button')]").ToList();

        Assert.True(buttons.Count >= 3);
    }

    [AvaloniaFact]
    public void Select_Matches_FindsWithRegex()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("XPathRoot")!;
        var xpath = new TreeXPath(root);

        var inputs = xpath.Select<TextBox>("//TextBox[matches(@Name, '.*Input$')]").ToList();

        Assert.Equal(2, inputs.Count);
    }

    [AvaloniaFact]
    public void Select_Not_ExcludesMatching()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("XPathRoot")!;
        var xpath = new TreeXPath(root);

        // Find buttons that are NOT named CancelButton
        var buttons = xpath.Select<Button>("//Button[not(@Name='CancelButton')]").ToList();

        // Negation must preserve the other buttons, not merely return no matches.
        Assert.NotEmpty(buttons);
        Assert.DoesNotContain(buttons, b => b.Name == "CancelButton");
    }

    [AvaloniaFact]
    public void Select_AttributeExists_FindsWithAttribute()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("XPathRoot")!;
        var xpath = new TreeXPath(root);

        var named = xpath.Select<Control>("//Button[@Name]").ToList();

        Assert.All(named, c => Assert.NotNull(c.Name));
    }

    #endregion

    #region Axis Tests

    [AvaloniaFact]
    public void Select_DescendantAxis_FindsAllDescendants()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("XPathRoot")!;
        var xpath = new TreeXPath(root);

        var allTextBoxes = xpath.Select<TextBox>("//TextBox").ToList();

        Assert.Equal(2, allTextBoxes.Count);
    }

    [AvaloniaFact]
    public void Select_DeepNested_FindsDeepElement()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("XPathRoot")!;
        var xpath = new TreeXPath(root);

        var deepButton = xpath.SelectFirst<Button>("//Button[@Name='DeepButton']");

        Assert.NotNull(deepButton);
        Assert.Equal("DeepButton", deepButton.Name);
    }

    #endregion

    #region Count and Exists Tests

    [AvaloniaFact]
    public void Count_ReturnsCorrectCount()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("XPathRoot")!;
        var xpath = new TreeXPath(root);

        var count = xpath.Count("//TextBox");

        Assert.Equal(2, count);
    }

    [AvaloniaFact]
    public void Exists_ReturnsTrueForExistingPath()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("XPathRoot")!;
        var xpath = new TreeXPath(root);

        var exists = xpath.Exists("//Button[@Name='SubmitButton']");

        Assert.True(exists);
    }

    [AvaloniaFact]
    public void Exists_ReturnsFalseForMissingPath()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("XPathRoot")!;
        var xpath = new TreeXPath(root);

        var exists = xpath.Exists("//Slider[@Name='NonExistent']");

        Assert.False(exists);
    }

    #endregion

    #region Extension Methods Tests

    [AvaloniaFact]
    public void XPath_ExtensionMethod_CreatesEngine()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("XPathRoot")!;

        var xpath = root.XPath();

        Assert.NotNull(xpath);
    }

    [AvaloniaFact]
    public void SelectXPath_ExtensionMethod_FindsControls()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("XPathRoot")!;

        var buttons = root.SelectXPath<Button>("//Button[@Tag='primary']").ToList();

        Assert.NotEmpty(buttons);
    }

    [AvaloniaFact]
    public void SelectFirstXPath_ExtensionMethod_FindsFirst()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("XPathRoot")!;

        var button = root.SelectFirstXPath<Button>("//Button[@Name='SubmitButton']");

        Assert.NotNull(button);
    }

    [AvaloniaFact]
    public void ExistsXPath_ExtensionMethod_ChecksExistence()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("XPathRoot")!;

        var exists = root.ExistsXPath("//CheckBox");

        Assert.True(exists);
    }

    [AvaloniaFact]
    public void CountXPath_ExtensionMethod_CountsMatches()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("XPathRoot")!;

        var count = root.CountXPath("//TextBox");

        Assert.Equal(2, count);
    }

    #endregion

    #region HasChild and HasDescendant Tests

    [AvaloniaFact]
    public void Select_HasChild_FindsParentsWithChild()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("XPathRoot")!;
        var xpath = new TreeXPath(root);

        // Find StackPanel that has Button children
        var panels = xpath.Select<StackPanel>("//StackPanel[Button]").ToList();

        Assert.NotEmpty(panels);
    }

    [AvaloniaFact]
    public void Select_HasDescendant_FindsParentsWithDescendant()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("XPathRoot")!;
        var xpath = new TreeXPath(root);

        // Find Border that has TextBox descendants
        var borders = xpath.Select<Border>("//Border[//TextBox]").ToList();

        Assert.NotEmpty(borders);
    }

    #endregion

    #region Property Access Tests

    [AvaloniaFact]
    public void Select_ByIsEnabled_FindsDisabledControls()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("XPathRoot")!;
        var xpath = new TreeXPath(root);

        var disabled = xpath.Select<Button>("//Button[@IsEnabled='False']").ToList();

        Assert.Single(disabled);
        Assert.Equal("CancelButton", disabled[0].Name);
    }

    [AvaloniaFact]
    public void Select_ByIsVisible_FindsVisibleControls()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("XPathRoot")!;
        var xpath = new TreeXPath(root);

        var visible = xpath.Select<Control>("//Button[@IsVisible='True']").ToList();

        Assert.NotEmpty(visible);
    }

    #endregion
}

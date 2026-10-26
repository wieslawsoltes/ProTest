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
using Avalonia.VisualTree;
using Xunit;

namespace Avalonia.Headless.UnitTests.Testing.Trees;

/// <summary>
/// Tests for TreeValidator functionality.
/// </summary>
public class TreeValidatorTests
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
        var root = new StackPanel { Name = "ValidationRoot" };

        // Buttons
        var btn1 = new Button { Name = "SaveButton", Content = "Save", IsEnabled = true };
        btn1.Classes.Add("primary");

        var btn2 = new Button { Name = "CancelButton", Content = "Cancel", IsEnabled = true };
        btn2.Classes.Add("secondary");

        var btn3 = new Button { Name = "DisabledButton", Content = "Disabled", IsEnabled = false };

        root.Children.Add(btn1);
        root.Children.Add(btn2);
        root.Children.Add(btn3);

        // Form inputs
        var form = new StackPanel { Name = "FormPanel" };
        form.Children.Add(new TextBox { Name = "NameInput" });
        form.Children.Add(new TextBox { Name = "EmailInput" });
        form.Children.Add(new CheckBox { Name = "AgreeCheckbox" });
        root.Children.Add(form);

        // Hidden control
        var hidden = new Border { Name = "HiddenBorder", IsVisible = false };
        root.Children.Add(hidden);

        return root;
    }

    #endregion

    #region RequireName Tests

    [AvaloniaFact]
    public void RequireName_PassesWhenNameExists()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .RequireName("SaveButton")
            .ValidateVisualTree(root);

        Assert.True(validator.IsValid);
    }

    [AvaloniaFact]
    public void RequireName_FailsWhenNameMissing()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .RequireName("NonExistentControl")
            .ValidateVisualTree(root);

        Assert.False(validator.IsValid);
        Assert.Single(validator.Failures);
    }

    [AvaloniaFact]
    public void RequireNames_PassesWhenAllNamesExist()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .RequireNames("SaveButton", "CancelButton", "DisabledButton")
            .ValidateVisualTree(root);

        Assert.True(validator.IsValid);
    }

    [AvaloniaFact]
    public void RequireNames_FailsWhenAnyNameMissing()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .RequireNames("SaveButton", "NonExistent", "CancelButton")
            .ValidateVisualTree(root);

        Assert.False(validator.IsValid);
    }

    #endregion

    #region RequireType Tests

    [AvaloniaFact]
    public void RequireType_PassesWhenTypeExists()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .RequireType<TextBox>()
            .ValidateVisualTree(root);

        Assert.True(validator.IsValid);
    }

    [AvaloniaFact]
    public void RequireType_FailsWhenTypeMissing()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .RequireType<Slider>()
            .ValidateVisualTree(root);

        Assert.False(validator.IsValid);
    }

    #endregion

    #region RequireCount Tests

    [AvaloniaFact]
    public void RequireExactCount_PassesWhenCountMatches()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .RequireExactCount<TextBox>(2)
            .ValidateVisualTree(root);

        Assert.True(validator.IsValid);
    }

    [AvaloniaFact]
    public void RequireExactCount_FailsWhenCountDoesntMatch()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .RequireExactCount<TextBox>(5)
            .ValidateVisualTree(root);

        Assert.False(validator.IsValid);
    }

    [AvaloniaFact]
    public void RequireMinCount_PassesWhenCountMeetsMinimum()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .RequireMinCount<Button>(2)
            .ValidateVisualTree(root);

        Assert.True(validator.IsValid);
    }

    [AvaloniaFact]
    public void RequireMinCount_FailsWhenCountBelowMinimum()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .RequireMinCount<TextBox>(10)
            .ValidateVisualTree(root);

        Assert.False(validator.IsValid);
    }

    [AvaloniaFact]
    public void RequireMaxCount_PassesWhenCountWithinMaximum()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .RequireMaxCount<TextBox>(5)
            .ValidateVisualTree(root);

        Assert.True(validator.IsValid);
    }

    [AvaloniaFact]
    public void RequireMaxCount_FailsWhenCountExceedsMaximum()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .RequireMaxCount<TextBox>(1)
            .ValidateVisualTree(root);

        Assert.False(validator.IsValid);
    }

    #endregion

    #region RequirePattern Tests

    [AvaloniaFact]
    public void RequirePattern_PassesWhenPatternMatches()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .RequirePattern("//Button[@Name='SaveButton']")
            .ValidateVisualTree(root);

        Assert.True(validator.IsValid);
    }

    [AvaloniaFact]
    public void RequirePattern_FailsWhenPatternDoesntMatch()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .RequirePattern("//Slider[@Name='Volume']")
            .ValidateVisualTree(root);

        Assert.False(validator.IsValid);
    }

    #endregion

    #region RequireNameOfType Tests

    [AvaloniaFact]
    public void RequireNameOfType_PassesWhenNamedTypeExists()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .RequireNameOfType<Button>("SaveButton")
            .ValidateVisualTree(root);

        Assert.True(validator.IsValid);
    }

    [AvaloniaFact]
    public void RequireNameOfType_FailsWhenWrongType()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .RequireNameOfType<TextBox>("SaveButton")
            .ValidateVisualTree(root);

        Assert.False(validator.IsValid);
    }

    #endregion

    #region RequireEnabled/RequireVisible Tests

    [AvaloniaFact]
    public void RequireEnabled_PassesWhenControlIsEnabled()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .RequireEnabled("SaveButton")
            .ValidateVisualTree(root);

        Assert.True(validator.IsValid);
    }

    [AvaloniaFact]
    public void RequireEnabled_FailsWhenControlIsDisabled()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .RequireEnabled("DisabledButton")
            .ValidateVisualTree(root);

        Assert.False(validator.IsValid);
    }

    [AvaloniaFact]
    public void RequireVisible_PassesWhenControlIsVisible()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .RequireVisible("SaveButton")
            .ValidateVisualTree(root);

        Assert.True(validator.IsValid);
    }

    [AvaloniaFact]
    public void RequireVisible_FailsWhenControlIsHidden()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .RequireVisible("HiddenBorder")
            .ValidateVisualTree(root);

        Assert.False(validator.IsValid);
    }

    #endregion

    #region RequireClass Tests

    [AvaloniaFact]
    public void RequireClass_PassesWhenControlHasClass()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .RequireClass("SaveButton", "primary")
            .ValidateVisualTree(root);

        Assert.True(validator.IsValid);
    }

    [AvaloniaFact]
    public void RequireClass_FailsWhenControlMissingClass()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .RequireClass("SaveButton", "nonexistent-class")
            .ValidateVisualTree(root);

        Assert.False(validator.IsValid);
    }

    #endregion

    #region ForbidPattern/ForbidType Tests

    [AvaloniaFact]
    public void ForbidPattern_PassesWhenPatternNotFound()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .ForbidPattern("//Slider")
            .ValidateVisualTree(root);

        Assert.True(validator.IsValid);
    }

    [AvaloniaFact]
    public void ForbidPattern_FailsWhenPatternFound()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .ForbidPattern("//Button[@Name='SaveButton']")
            .ValidateVisualTree(root);

        Assert.False(validator.IsValid);
    }

    [AvaloniaFact]
    public void ForbidType_PassesWhenTypeNotFound()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .ForbidType<Slider>()
            .ValidateVisualTree(root);

        Assert.True(validator.IsValid);
    }

    [AvaloniaFact]
    public void ForbidType_FailsWhenTypeFound()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .ForbidType<Button>()
            .ValidateVisualTree(root);

        Assert.False(validator.IsValid);
    }

    #endregion

    #region Custom Rule Tests

    [AvaloniaFact]
    public void Custom_PassesWhenConditionMet()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .Custom("Has at least 2 buttons", tree =>
            {
                var count = tree.GetVisualDescendants().OfType<Button>().Count();
                return count >= 2;
            })
            .ValidateVisualTree(root);

        Assert.True(validator.IsValid);
    }

    [AvaloniaFact]
    public void Custom_FailsWhenConditionNotMet()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .Custom("Has exactly 10 buttons", tree =>
            {
                var count = tree.GetVisualDescendants().OfType<Button>().Count();
                return count == 10;
            })
            .ValidateVisualTree(root);

        Assert.False(validator.IsValid);
    }

    #endregion

    #region AssertValid Tests

    [AvaloniaFact]
    public void AssertValid_DoesNotThrowWhenValid()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .RequireName("SaveButton")
            .RequireType<Button>()
            .ValidateVisualTree(root);

        // Should not throw
        validator.AssertValid();
    }

    [AvaloniaFact]
    public void AssertValid_ThrowsWhenInvalid()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .RequireName("NonExistentControl")
            .ValidateVisualTree(root);

        Assert.Throws<TreeValidationException>(() => validator.AssertValid());
    }

    [AvaloniaFact]
    public void AssertValid_ExceptionContainsDetails()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .RequireName("NonExistentControl")
            .RequireType<Slider>()
            .ValidateVisualTree(root);

        var ex = Assert.Throws<TreeValidationException>(() => validator.AssertValid());
        Assert.NotNull(ex.Failures);
        Assert.Equal(2, ex.Failures.Count);
    }

    #endregion

    #region Summary Tests

    [AvaloniaFact]
    public void GetSummary_ReturnsDetailedSummary()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .RequireName("SaveButton")
            .RequireName("NonExistent")
            .RequireType<Button>()
            .ValidateVisualTree(root);

        var summary = validator.GetSummary();

        Assert.Contains("passed", summary);
        Assert.Contains("failed", summary);
    }

    #endregion

    #region Fluent Chaining Tests

    [AvaloniaFact]
    public void FluentChaining_MultipleRules_AllApplied()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .RequireName("SaveButton")
            .RequireName("CancelButton")
            .RequireType<TextBox>()
            .RequireMinCount<Button>(2)
            .ForbidType<Slider>()
            .ValidateVisualTree(root);

        Assert.True(validator.IsValid);
        Assert.Equal(5, validator.Results.Count);
        Assert.Equal(5, validator.Results.Count(r => r.IsValid));
    }

    #endregion

    #region Logical Tree Validation Tests

    [AvaloniaFact]
    public void ValidateLogicalTree_FindsControlsInLogicalTree()
    {
        var window = CreateTestWindow();
        var root = window.FindByName<StackPanel>("ValidationRoot")!;

        var validator = new TreeValidator()
            .RequireName("SaveButton")
            .RequireType<Button>()
            .ValidateLogicalTree(root);

        Assert.True(validator.IsValid);
    }

    #endregion
}

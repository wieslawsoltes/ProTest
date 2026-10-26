#nullable enable

// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.HeadlessTestingFramework;
using Avalonia.HeadlessTestingFramework.Appium;
using Xunit;

namespace Avalonia.Headless.UnitTests.Testing.Appium;

/// <summary>
/// Tests for Appium-like API.
/// </summary>
public class AppiumApiTests
{
    #region By Locator Tests

    [Fact]
    public void By_Id_CreatesLocator()
    {
        var locator = By.Id("myButton");

        Assert.Equal(LocatorStrategy.Id, locator.Strategy);
        Assert.Equal("myButton", locator.Value);
    }

    [Fact]
    public void By_Name_CreatesLocator()
    {
        var locator = By.Name("submitBtn");

        Assert.Equal(LocatorStrategy.Name, locator.Strategy);
        Assert.Equal("submitBtn", locator.Value);
    }

    [Fact]
    public void By_AutomationId_CreatesLocator()
    {
        var locator = By.AutomationId("login_button");

        Assert.Equal(LocatorStrategy.AutomationId, locator.Strategy);
        Assert.Equal("login_button", locator.Value);
    }

    [Fact]
    public void By_ClassName_CreatesLocator()
    {
        var locator = By.ClassName("Button");

        Assert.Equal(LocatorStrategy.ClassName, locator.Strategy);
        Assert.Equal("Button", locator.Value);
    }

    [Fact]
    public void By_XPath_CreatesLocator()
    {
        var locator = By.XPath("//Button[@Name='submit']");

        Assert.Equal(LocatorStrategy.XPath, locator.Strategy);
        Assert.Equal("//Button[@Name='submit']", locator.Value);
    }

    [Fact]
    public void By_Text_ExactMatch_CreatesLocator()
    {
        var locator = By.Text("Submit", exact: true);

        Assert.Equal(LocatorStrategy.Text, locator.Strategy);
        Assert.Equal("Submit", locator.Value);
        Assert.True(locator.Options.ExactMatch);
    }

    [Fact]
    public void By_Text_PartialMatch_CreatesLocator()
    {
        var locator = By.Text("Sub", exact: false);

        Assert.Equal(LocatorStrategy.Text, locator.Strategy);
        Assert.False(locator.Options.ExactMatch);
    }

    [Fact]
    public void By_ContainsText_CreatesLocator()
    {
        var locator = By.ContainsText("Submit");

        Assert.Equal(LocatorStrategy.Text, locator.Strategy);
        Assert.False(locator.Options.ExactMatch);
    }

    [Fact]
    public void By_CssClass_CreatesLocator()
    {
        var locator = By.CssClass("primary");

        Assert.Equal(LocatorStrategy.CssClass, locator.Strategy);
        Assert.Equal("primary", locator.Value);
    }

    [Fact]
    public void By_Property_CreatesLocator()
    {
        var locator = By.Property("IsEnabled", true);

        Assert.Equal(LocatorStrategy.Property, locator.Strategy);
        Assert.Equal("IsEnabled", locator.Value);
        Assert.Equal(true, locator.Options.PropertyValue);
    }

    [Fact]
    public void By_Type_Generic_CreatesLocator()
    {
        var locator = By.Type<Button>();

        Assert.Equal(LocatorStrategy.Type, locator.Strategy);
        Assert.Contains("Button", locator.Value);
    }

    [Fact]
    public void By_NameRegex_CreatesLocator()
    {
        var locator = By.NameRegex("btn_.*");

        Assert.Equal(LocatorStrategy.NameRegex, locator.Strategy);
        Assert.Equal("btn_.*", locator.Value);
    }

    [Fact]
    public void By_All_CombinesLocators()
    {
        var locator = By.All(By.ClassName("Button"), By.Text("Submit"));

        Assert.Equal(LocatorStrategy.Composite, locator.Strategy);
        Assert.Equal(CompositeMode.And, locator.Options.CompositeMode);
        Assert.Equal(2, locator.Options.CompositeLocators?.Length);
    }

    [Fact]
    public void By_Any_CombinesLocators()
    {
        var locator = By.Any(By.Id("btn1"), By.Id("btn2"));

        Assert.Equal(LocatorStrategy.Composite, locator.Strategy);
        Assert.Equal(CompositeMode.Or, locator.Options.CompositeMode);
    }

    [Fact]
    public void By_Chained_CombinesLocators()
    {
        var locator = By.Chained(By.Id("container"), By.ClassName("Button"));

        Assert.Equal(LocatorStrategy.Chained, locator.Strategy);
        Assert.Equal(2, locator.Options.CompositeLocators?.Length);
    }

    [Fact]
    public void By_ToString_ReturnsDescription()
    {
        var locator = By.Id("myButton");

        Assert.Equal("By.Id(\"myButton\")", locator.ToString());
    }

    #endregion

    #region AvaloniaDriver Tests

    [AvaloniaFact]
    public void Driver_FindElement_ById_FindsElement()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Id("TestButton"));

        Assert.NotNull(element);
        Assert.Equal("TestButton", element.Id);
    }

    [AvaloniaFact]
    public void Driver_FindElement_ByClassName_FindsElement()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.ClassName("Button"));

        Assert.NotNull(element);
        Assert.Equal("Button", element.TagName);
    }

    [AvaloniaFact]
    public void Driver_FindElement_ByText_FindsElement()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Text("Click Me"));

        Assert.NotNull(element);
        Assert.Contains("Click Me", element.Text);
    }

    [AvaloniaFact]
    public void Driver_FindElement_NotFound_ThrowsException()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        Assert.Throws<NoSuchElementException>(() => driver.FindElement(By.Id("NonExistent")));
    }

    [AvaloniaFact]
    public void Driver_TryFindElement_NotFound_ReturnsNull()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var element = driver.TryFindElement(By.Id("NonExistent"));

        Assert.Null(element);
    }

    [AvaloniaFact]
    public void Driver_FindElements_FindsMultiple()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var elements = driver.FindElements(By.ClassName("Button"));

        Assert.True(elements.Count >= 2);
    }

    [AvaloniaFact]
    public void Driver_ElementExists_ReturnsTrue()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        Assert.True(driver.ElementExists(By.Id("TestButton")));
        Assert.False(driver.ElementExists(By.Id("NonExistent")));
    }

    [AvaloniaFact]
    public void Driver_ElementCount_ReturnsCorrectCount()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var count = driver.ElementCount(By.ClassName("Button"));

        Assert.True(count >= 2);
    }

    [AvaloniaFact]
    public void Driver_FindElement_ByCssClass_FindsElement()
    {
        var window = CreateTestWindowWithClasses();
        using var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.CssClass("primary"));

        Assert.NotNull(element);
        Assert.True(element.HasClass("primary"));
    }

    [AvaloniaFact]
    public void Driver_FindElement_ByNameRegex_FindsElement()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var elements = driver.FindElements(By.NameRegex("Test.*"));

        Assert.True(elements.Count > 0);
    }

    [AvaloniaFact]
    public void Driver_FindElement_ByComposite_And_FindsElement()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var locator = By.All(By.ClassName("Button"), By.Id("TestButton"));
        var element = driver.FindElement(locator);

        Assert.NotNull(element);
        Assert.Equal("TestButton", element.Id);
    }

    [AvaloniaFact]
    public void Driver_FindElement_ByComposite_Or_FindsElement()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var locator = By.Any(By.Id("NonExistent"), By.Id("TestButton"));
        var element = driver.FindElement(locator);

        Assert.NotNull(element);
        Assert.Equal("TestButton", element.Id);
    }

    [AvaloniaFact]
    public void Driver_FindElement_ByAutomationId_FindsElement()
    {
        var window = CreateTestWindowWithAutomation();
        using var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.AutomationId("auto_submit"));

        Assert.NotNull(element);
        Assert.Equal("auto_submit", element.GetAttribute("automationid"));
    }

    [AvaloniaFact]
    public void Driver_PageSource_ReturnsXml()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var source = driver.PageSource;

        Assert.NotEmpty(source);
        Assert.Contains("Button", source);
        Assert.Contains("TestButton", source);
    }

    [AvaloniaFact]
    public void Driver_RegisterPredicate_WorksWithFindElement()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        driver.RegisterPredicate("isButton", c => c is Button);
        var elements = driver.FindElements(By.Predicate("Is a button", "isButton"));

        Assert.True(elements.Count > 0);
    }

    #endregion

    #region AvaloniaElement Tests

    [AvaloniaFact]
    public void Element_Properties_ReturnCorrectValues()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Id("TestButton"));

        Assert.Equal("TestButton", element.Id);
        Assert.Equal("Button", element.TagName);
        Assert.True(element.Enabled);
        Assert.True(element.Displayed);
        Assert.True(element.Size.Width > 0);
        Assert.True(element.Size.Height > 0);
    }

    [AvaloniaFact]
    public void Element_Text_ReturnsContent()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Id("TestButton"));

        Assert.Contains("Click Me", element.Text);
    }

    [AvaloniaFact]
    public void Element_GetAttribute_ReturnsValue()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Id("TestButton"));

        Assert.Equal("TestButton", element.GetAttribute("name"));
        Assert.Equal("true", element.GetAttribute("enabled"));
        Assert.Equal("Button", element.GetAttribute("className"));
    }

    [AvaloniaFact]
    public void Element_GetClasses_ReturnsClasses()
    {
        var window = CreateTestWindowWithClasses();
        using var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.CssClass("primary"));
        var classes = element.GetClasses();

        Assert.Contains("primary", classes);
    }

    [AvaloniaFact]
    public void Element_HasClass_ReturnsCorrectValue()
    {
        var window = CreateTestWindowWithClasses();
        using var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.CssClass("primary"));

        Assert.True(element.HasClass("primary"));
        Assert.False(element.HasClass("nonexistent"));
    }

    [AvaloniaFact]
    public void Element_Click_ReturnsElement()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Id("TestButton"));
        // Verify the method exists and returns the element for chaining
        var result = element.Click();

        Assert.Same(element, result);
    }

    [AvaloniaFact]
    public void Element_SendKeys_EntersText()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Id("TestTextBox"));
        element.SendKeys("Hello World");

        Assert.Equal("Hello World", element.Text);
    }

    [AvaloniaFact]
    public void Element_Clear_ClearsText()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Id("TestTextBox"));
        element.SendKeys("Hello World");
        element.Clear();

        Assert.Equal("", element.Text);
    }

    [AvaloniaFact]
    public void Element_Focus_SetsFocus()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Id("TestButton"));
        element.Focus();

        Assert.True(element.Focused);
    }

    [AvaloniaFact]
    public void Element_FindElement_FindsChildElement()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var container = driver.FindElement(By.Id("Container"));
        var button = container.FindElement(By.ClassName("Button"));

        Assert.NotNull(button);
    }

    [AvaloniaFact]
    public void Element_Parent_ReturnsParent()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Id("TestButton"));
        var parent = element.Parent;

        Assert.NotNull(parent);
    }

    [AvaloniaFact]
    public void Element_Children_ReturnsChildren()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var container = driver.FindElement(By.Id("Container"));
        var children = container.Children;

        Assert.True(children.Count > 0);
    }

    [AvaloniaFact]
    public void Element_SetProperty_SetsValue()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Id("TestButton"));
        element.SetProperty("IsEnabled", false);

        Assert.False(element.Enabled);
    }

    [AvaloniaFact]
    public void Element_GetProperty_ReturnsValue()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Id("TestButton"));
        var isEnabled = element.GetProperty<bool>("IsEnabled");

        Assert.True(isEnabled);
    }

    [AvaloniaFact]
    public void Element_ToString_ReturnsDescription()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Id("TestButton"));

        Assert.Contains("Button", element.ToString());
        Assert.Contains("TestButton", element.ToString());
    }

    #endregion

    #region TouchAction Tests

    [AvaloniaFact]
    public void TouchAction_Tap_PerformsTap()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var action = driver.CreateTouchAction();

        Assert.NotNull(action);
        // Basic creation test - actual tap would require event handling verification
    }

    [AvaloniaFact]
    public void TouchAction_Press_MoveTo_Release_Chain()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Id("TestButton"));
        var action = driver.CreateTouchAction()
            .Press(element)
            .Wait(100)
            .MoveTo(150, 150)
            .Release();

        // Verify chain was created
        Assert.NotNull(action);
    }

    [AvaloniaFact]
    public void TouchAction_LongPress_CreatesChain()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Id("TestButton"));
        var action = driver.CreateTouchAction()
            .LongPress(element, 500)
            .Release();

        Assert.NotNull(action);
    }

    [AvaloniaFact]
    public void TouchAction_Swipe_CreatesChain()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var action = driver.CreateTouchAction()
            .Press(100, 100)
            .Swipe(Avalonia.HeadlessTestingFramework.SwipeDirection.Up, 200)
            .Release();

        Assert.NotNull(action);
    }

    [AvaloniaFact]
    public void TouchAction_DoubleTap_CreatesChain()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Id("TestButton"));
        var action = driver.CreateTouchAction()
            .DoubleTap(element);

        Assert.NotNull(action);
    }

    #endregion

    #region Wait Tests

    [AvaloniaFact]
    public void Wait_Until_WaitsForCondition()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var result = driver.Wait.Until(d => d.ElementExists(By.Id("TestButton")));

        Assert.True(result);
    }

    [AvaloniaFact]
    public void Wait_ForElement_ReturnsElement()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var element = driver.Wait.ForElement(By.Id("TestButton"), TimeSpan.FromSeconds(1));

        Assert.NotNull(element);
    }

    [AvaloniaFact]
    public void Wait_ForElementVisible_ReturnsVisibleElement()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var element = driver.Wait.ForElementVisible(By.Id("TestButton"), TimeSpan.FromSeconds(1));

        Assert.NotNull(element);
        Assert.True(element.Displayed);
    }

    [AvaloniaFact]
    public void Wait_ForElementClickable_ReturnsClickableElement()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var element = driver.Wait.ForElementClickable(By.Id("TestButton"), TimeSpan.FromSeconds(1));

        Assert.NotNull(element);
        Assert.True(element.Displayed);
        Assert.True(element.Enabled);
    }

    [AvaloniaFact]
    public void Wait_Until_Timeout_ThrowsException()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);
        driver.Wait.Timeout = TimeSpan.FromMilliseconds(100);

        Assert.Throws<TimeoutException>(() =>
            driver.Wait.Until(d => d.ElementExists(By.Id("NonExistent"))));
    }

    #endregion

    #region ExpectedConditions Tests

    [AvaloniaFact]
    public void ExpectedConditions_ElementExists_ReturnsElement()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var condition = ExpectedConditions.ElementExists(By.Id("TestButton"));
        var element = condition(driver);

        Assert.NotNull(element);
    }

    [AvaloniaFact]
    public void ExpectedConditions_ElementIsVisible_ReturnsElement()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var condition = ExpectedConditions.ElementIsVisible(By.Id("TestButton"));
        var element = condition(driver);

        Assert.NotNull(element);
        Assert.True(element.Displayed);
    }

    [AvaloniaFact]
    public void ExpectedConditions_ElementToBeClickable_ReturnsElement()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var condition = ExpectedConditions.ElementToBeClickable(By.Id("TestButton"));
        var element = condition(driver);

        Assert.NotNull(element);
        Assert.True(element.Enabled);
    }

    [AvaloniaFact]
    public void ExpectedConditions_InvisibilityOfElement_ReturnsTrue_WhenNotFound()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var condition = ExpectedConditions.InvisibilityOfElement(By.Id("NonExistent"));
        var result = condition(driver);

        Assert.True(result);
    }

    [AvaloniaFact]
    public void ExpectedConditions_TextToBe_ReturnsTrue()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var condition = ExpectedConditions.TextToBe(By.Id("TestButton"), "Click Me");
        var result = condition(driver);

        Assert.True(result);
    }

    [AvaloniaFact]
    public void ExpectedConditions_TextToBePresentInElement_ReturnsTrue()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var condition = ExpectedConditions.TextToBePresentInElement(By.Id("TestButton"), "Click");
        var result = condition(driver);

        Assert.True(result);
    }

    [AvaloniaFact]
    public void ExpectedConditions_ElementToBeEnabled_ReturnsTrue()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var condition = ExpectedConditions.ElementToBeEnabled(By.Id("TestButton"));
        var result = condition(driver);

        Assert.True(result);
    }

    [AvaloniaFact]
    public void ExpectedConditions_NumberOfElementsToBe_ReturnsElements()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var condition = ExpectedConditions.NumberOfElementsToBeMoreThan(By.ClassName("Button"), 0);
        var elements = condition(driver);

        Assert.NotNull(elements);
        Assert.True(elements.Count > 0);
    }

    [AvaloniaFact]
    public void ExpectedConditions_And_CombinesConditions()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var condition = ExpectedConditions.And(
            ExpectedConditions.ElementToBeEnabled(By.Id("TestButton")),
            d => d.ElementExists(By.Id("TestTextBox")));
        var result = condition(driver);

        Assert.True(result);
    }

    [AvaloniaFact]
    public void ExpectedConditions_Or_CombinesConditions()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var condition = ExpectedConditions.Or(
            d => d.ElementExists(By.Id("NonExistent")),
            d => d.ElementExists(By.Id("TestButton")));
        var result = condition(driver);

        Assert.True(result);
    }

    [AvaloniaFact]
    public void ExpectedConditions_Not_NegatesCondition()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var condition = ExpectedConditions.Not(d => d.ElementExists(By.Id("NonExistent")));
        var result = condition(driver);

        Assert.True(result);
    }

    [AvaloniaFact]
    public void ExpectedConditions_StalenessOf_ReturnsFalse_WhenElementIsAttached()
    {
        var panel = new StackPanel { Name = "RootPanel" };
        var button = new Button { Name = "TrackedButton", Content = "Tracked" };
        panel.Children.Add(button);

        var window = new Window
        {
            Title = "Attached Root",
            Width = 300,
            Height = 200,
            Content = panel
        };
        window.Show();

        using var driver = new AvaloniaDriver(panel);

        var element = driver.FindElement(By.Id("TrackedButton"));
        var isStale = ExpectedConditions.StalenessOf(element)(driver);

        Assert.False(isStale);
    }

    [AvaloniaFact]
    public void ExpectedConditions_StalenessOf_ReturnsTrue_WhenElementIsDetached()
    {
        var panel = new StackPanel { Name = "RootPanel" };
        var button = new Button { Name = "TrackedButton", Content = "Tracked" };
        panel.Children.Add(button);

        var window = new Window
        {
            Title = "Detached Root",
            Width = 300,
            Height = 200,
            Content = panel
        };
        window.Show();

        using var driver = new AvaloniaDriver(panel);

        var element = driver.FindElement(By.Id("TrackedButton"));
        panel.Children.Remove(button);

        var isStale = ExpectedConditions.StalenessOf(element)(driver);

        Assert.True(isStale);
    }

    #endregion

    #region Window Management Tests

    [AvaloniaFact]
    public void Driver_GetWindowSize_ReturnsSize()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        var size = driver.GetWindowSize();

        Assert.True(size.Width > 0);
        Assert.True(size.Height > 0);
    }

    [AvaloniaFact]
    public void Driver_SetWindowSize_SetsSize()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        driver.SetWindowSize(800, 600);

        Assert.Equal(800, window.Width);
        Assert.Equal(600, window.Height);
    }

    [AvaloniaFact]
    public void Driver_Title_ReturnsWindowTitle()
    {
        var window = CreateTestWindow();
        using var driver = new AvaloniaDriver(window);

        Assert.Equal("Test Window", driver.Title);
    }

    #endregion

    #region Helper Methods

    private Window CreateTestWindow()
    {
        var window = new Window
        {
            Title = "Test Window",
            Width = 400,
            Height = 300
        };

        var container = new StackPanel { Name = "Container" };
        container.Children.Add(new Button { Name = "TestButton", Content = "Click Me" });
        container.Children.Add(new Button { Name = "SecondButton", Content = "Second" });
        container.Children.Add(new TextBox { Name = "TestTextBox" });
        container.Children.Add(new TextBlock { Name = "TestLabel", Text = "Label Text" });

        window.Content = container;
        window.Show();

        return window;
    }

    private Window CreateTestWindowWithClasses()
    {
        var window = new Window
        {
            Title = "Test Window",
            Width = 400,
            Height = 300
        };

        var container = new StackPanel { Name = "Container" };

        var primaryButton = new Button { Name = "PrimaryButton", Content = "Primary" };
        primaryButton.Classes.Add("primary");
        primaryButton.Classes.Add("large");
        container.Children.Add(primaryButton);

        var secondaryButton = new Button { Name = "SecondaryButton", Content = "Secondary" };
        secondaryButton.Classes.Add("secondary");
        container.Children.Add(secondaryButton);

        window.Content = container;
        window.Show();

        return window;
    }

    private Window CreateTestWindowWithAutomation()
    {
        var window = new Window
        {
            Title = "Test Window",
            Width = 400,
            Height = 300
        };

        var container = new StackPanel { Name = "Container" };

        var submitButton = new Button { Name = "SubmitButton", Content = "Submit" };
        AutomationProperties.SetAutomationId(submitButton, "auto_submit");
        AutomationProperties.SetName(submitButton, "Submit Form Button");
        container.Children.Add(submitButton);

        window.Content = container;
        window.Show();

        return window;
    }

    #endregion
}

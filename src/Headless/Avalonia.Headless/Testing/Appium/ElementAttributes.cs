// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace Avalonia.HeadlessTestingFramework.Appium;

/// <summary>
/// Provides methods to query element properties and attributes.
/// Inspired by Selenium's attribute and property access patterns.
/// </summary>
public static class ElementAttributes
{
    /// <summary>
    /// Gets a standard attribute value from an element.
    /// Supports common attributes like: id, name, class, text, value, enabled, visible, etc.
    /// </summary>
    /// <param name="element">The element to query.</param>
    /// <param name="attributeName">The attribute name.</param>
    /// <returns>The attribute value, or null if not found.</returns>
    public static string? GetAttribute(Control element, string attributeName)
    {
        if (element == null)
            return null;

        return attributeName.ToLowerInvariant() switch
        {
            // Identity attributes
            "id" => element.Name,
            "name" => element.Name,
            "automationid" => AutomationProperties.GetAutomationId(element),

            // Class attributes
            "class" or "classname" => string.Join(" ", element.Classes),
            "type" => element.GetType().Name,
            "fulltype" => element.GetType().FullName,

            // Text attributes
            "text" => GetText(element),
            "innertext" => GetInnerText(element),
            "value" => GetValue(element),

            // State attributes
            "enabled" => element.IsEnabled.ToString().ToLower(),
            "disabled" => (!element.IsEnabled).ToString().ToLower(),
            "visible" => element.IsVisible.ToString().ToLower(),
            "hidden" => (!element.IsVisible).ToString().ToLower(),
            "focused" => element.IsFocused.ToString().ToLower(),
            "focusable" => element.Focusable.ToString().ToLower(),
            "readonly" => IsReadOnly(element).ToString().ToLower(),
            "selected" => IsSelected(element).ToString().ToLower(),
            "checked" => IsChecked(element).ToString().ToLower(),

            // Geometry attributes
            "x" => element.Bounds.X.ToString(),
            "y" => element.Bounds.Y.ToString(),
            "width" => element.Bounds.Width.ToString(),
            "height" => element.Bounds.Height.ToString(),
            "rect" => $"{element.Bounds.X},{element.Bounds.Y},{element.Bounds.Width},{element.Bounds.Height}",

            // Visual attributes
            "opacity" => element.Opacity.ToString(),
            "background" => GetBrushColor(element.GetValue(Border.BackgroundProperty)),
            "foreground" => GetBrushColor(GetForeground(element)),
            "fontsize" => GetFontSize(element)?.ToString(),
            "fontfamily" => GetFontFamily(element)?.Name,

            // Interaction attributes
            "clickable" => CanReceivePointerInput(element).ToString().ToLower(),
            "interactable" => IsInteractable(element).ToString().ToLower(),
            "hittestvisible" => element.IsHitTestVisible.ToString().ToLower(),

            // Accessibility attributes
            "accessiblename" => AutomationProperties.GetName(element),
            "helptext" => AutomationProperties.GetHelpText(element),
            "labeledby" => AutomationProperties.GetLabeledBy(element)?.Name,

            // Hierarchy attributes
            "tagname" => element.GetType().Name,
            "parenttype" => element.Parent?.GetType().Name,
            "childcount" => element.GetVisualChildren().Count().ToString(),

            // Content attributes
            "contenttype" => GetContentType(element),
            "haschildren" => element.GetVisualChildren().Any().ToString().ToLower(),

            _ => GetCustomProperty(element, attributeName)
        };
    }

    /// <summary>
    /// Gets multiple attributes from an element.
    /// </summary>
    /// <param name="element">The element to query.</param>
    /// <param name="attributeNames">The attribute names to get.</param>
    /// <returns>A dictionary of attribute names to values.</returns>
    public static Dictionary<string, string?> GetAttributes(Control element, params string[] attributeNames)
    {
        var result = new Dictionary<string, string?>();
        foreach (var name in attributeNames)
        {
            result[name] = GetAttribute(element, name);
        }
        return result;
    }

    /// <summary>
    /// Gets all available attributes from an element.
    /// </summary>
    /// <param name="element">The element to query.</param>
    /// <returns>A dictionary of all attribute names to values.</returns>
    public static Dictionary<string, string?> GetAllAttributes(Control element)
    {
        return GetAttributes(element,
            "id", "name", "automationid", "class", "type", "fulltype",
            "text", "value", "enabled", "visible", "focused", "selected",
            "x", "y", "width", "height", "opacity", "clickable",
            "accessiblename", "tagname", "childcount");
    }

    /// <summary>
    /// Gets the CSS value for a property (Avalonia equivalent).
    /// </summary>
    /// <param name="element">The element to query.</param>
    /// <param name="propertyName">The CSS property name.</param>
    /// <returns>The computed CSS value.</returns>
    public static string? GetCssValue(Control element, string propertyName)
    {
        if (element == null)
            return null;

        return propertyName.ToLowerInvariant() switch
        {
            "display" => element.IsVisible ? "block" : "none",
            "visibility" => element.IsVisible ? "visible" : "hidden",
            "opacity" => element.Opacity.ToString(),
            "background-color" or "backgroundcolor" => GetBrushColor(element.GetValue(Border.BackgroundProperty)),
            "color" => GetBrushColor(GetForeground(element)),
            "width" => $"{element.Width}px",
            "height" => $"{element.Height}px",
            "min-width" or "minwidth" => $"{element.MinWidth}px",
            "min-height" or "minheight" => $"{element.MinHeight}px",
            "max-width" or "maxwidth" => $"{element.MaxWidth}px",
            "max-height" or "maxheight" => $"{element.MaxHeight}px",
            "margin" => FormatThickness(element.Margin),
            "padding" => GetPadding(element),
            "font-size" or "fontsize" => $"{GetFontSize(element)}px",
            "font-family" or "fontfamily" => GetFontFamily(element)?.Name,
            "font-weight" or "fontweight" => GetFontWeight(element)?.ToString(),
            "text-align" or "textalign" => GetTextAlignment(element),
            "cursor" => element.Cursor?.ToString() ?? "default",
            "z-index" or "zindex" => element.ZIndex.ToString(),
            _ => null
        };
    }

    /// <summary>
    /// Checks if the element has a specific attribute.
    /// </summary>
    /// <param name="element">The element to check.</param>
    /// <param name="attributeName">The attribute name.</param>
    /// <returns>True if the attribute exists and has a non-null value.</returns>
    public static bool HasAttribute(Control element, string attributeName)
    {
        return GetAttribute(element, attributeName) != null;
    }

    /// <summary>
    /// Checks if the element has a specific class.
    /// </summary>
    /// <param name="element">The element to check.</param>
    /// <param name="className">The class name.</param>
    /// <returns>True if the element has the class.</returns>
    public static bool HasClass(Control element, string className)
    {
        return element?.Classes.Contains(className) ?? false;
    }

    /// <summary>
    /// Gets the accessible name for an element (for accessibility testing).
    /// </summary>
    /// <param name="element">The element to query.</param>
    /// <returns>The accessible name.</returns>
    public static string GetAccessibleName(Control element)
    {
        if (element == null)
            return string.Empty;

        // Try AutomationProperties.Name first
        var automationName = AutomationProperties.GetName(element);
        if (!string.IsNullOrEmpty(automationName))
            return automationName;

        // Try element name
        if (!string.IsNullOrEmpty(element.Name))
            return element.Name;

        // Try text content
        var text = GetText(element);
        if (!string.IsNullOrEmpty(text))
            return text;

        // Fall back to type name
        return element.GetType().Name;
    }

    /// <summary>
    /// Gets the ARIA role equivalent for an element.
    /// </summary>
    /// <param name="element">The element to query.</param>
    /// <returns>The ARIA role name.</returns>
    public static string GetRole(Control element)
    {
        if (element == null)
            return "none";

        // Order matters - check most specific types first
        return element switch
        {
            CheckBox => "checkbox",
            RadioButton => "radio",
            Button => "button",
            TextBox => "textbox",
            ComboBox => "combobox",
            ListBoxItem => "option",
            ListBox => "listbox",
            MenuItem => "menuitem",
            Menu => "menu",
            Slider => "slider",
            ProgressBar => "progressbar",
            TabItem => "tab",
            TabControl => "tablist",
            TreeViewItem => "treeitem",
            TreeView => "tree",
            ScrollViewer => "region",
            Window => "dialog",
            Panel => "group",
            TextBlock => "text",
            Image => "img",
            _ => "generic"
        };
    }

    #region Private Helpers

    private static string? GetText(Control element)
    {
        return element switch
        {
            TextBlock tb => tb.Text,
            TextBox tb => tb.Text,
            ContentControl cc when cc.Content is string s => s,
            HeaderedContentControl hcc => hcc.Header?.ToString(),
            _ => null
        };
    }

    private static string? GetInnerText(Control element)
    {
        if (element == null)
            return null;

        var text = GetText(element);
        if (!string.IsNullOrEmpty(text))
            return text;

        // Recursively get text from children
        var texts = new List<string>();
        foreach (var child in element.GetVisualChildren().OfType<Control>())
        {
            var childText = GetInnerText(child);
            if (!string.IsNullOrEmpty(childText))
                texts.Add(childText);
        }

        return texts.Count > 0 ? string.Join(" ", texts) : null;
    }

    private static string? GetValue(Control element)
    {
        return element switch
        {
            TextBox tb => tb.Text,
            NumericUpDown nud => nud.Value?.ToString(),
            Slider s => s.Value.ToString(),
            ProgressBar pb => pb.Value.ToString(),
            ComboBox cb => cb.SelectedItem?.ToString(),
            DatePicker dp => dp.SelectedDate?.ToString(),
            TimePicker tp => tp.SelectedTime?.ToString(),
            _ => null
        };
    }

    private static bool IsReadOnly(Control element)
    {
        if (element is TextBox tb)
            return tb.IsReadOnly;
        return false;
    }

    private static bool IsChecked(Control element)
    {
        if (element is ToggleButton tb)
            return tb.IsChecked == true;
        if (element is CheckBox cb)
            return cb.IsChecked == true;
        if (element is RadioButton rb)
            return rb.IsChecked == true;
        return false;
    }

    private static string? GetBrushColor(IBrush? brush)
    {
        if (brush is ISolidColorBrush solid)
            return $"#{solid.Color.A:X2}{solid.Color.R:X2}{solid.Color.G:X2}{solid.Color.B:X2}";
        if (brush is ILinearGradientBrush)
            return "linear-gradient";
        if (brush is IRadialGradientBrush)
            return "radial-gradient";
        return null;
    }

    private static IBrush? GetForeground(Control element)
    {
        if (element is TextBlock tb)
            return tb.Foreground;
        if (element is TemplatedControl tc)
            return tc.Foreground;
        return null;
    }

    private static double? GetFontSize(Control element)
    {
        if (element is TextBlock tb)
            return tb.FontSize;
        if (element is TemplatedControl tc)
            return tc.FontSize;
        return null;
    }

    private static FontFamily? GetFontFamily(Control element)
    {
        if (element is TextBlock tb)
            return tb.FontFamily;
        if (element is TemplatedControl tc)
            return tc.FontFamily;
        return null;
    }

    private static FontWeight? GetFontWeight(Control element)
    {
        if (element is TextBlock tb)
            return tb.FontWeight;
        if (element is TemplatedControl tc)
            return tc.FontWeight;
        return null;
    }

    private static string? GetTextAlignment(Control element)
    {
        if (element is TextBlock tb)
            return tb.TextAlignment.ToString().ToLower();
        return null;
    }

    private static string? GetPadding(Control element)
    {
        if (element is Border b)
            return FormatThickness(b.Padding);
        if (element is Decorator d)
            return FormatThickness(d.Padding);
        if (element is TemplatedControl tc)
            return FormatThickness(tc.Padding);
        return null;
    }

    private static string FormatThickness(Thickness t)
    {
        if (t.Left == t.Right && t.Top == t.Bottom && t.Left == t.Top)
            return $"{t.Left}px";
        if (t.Left == t.Right && t.Top == t.Bottom)
            return $"{t.Top}px {t.Left}px";
        return $"{t.Top}px {t.Right}px {t.Bottom}px {t.Left}px";
    }

    private static string? GetContentType(Control element)
    {
        if (element is ContentControl cc)
            return cc.Content?.GetType().Name;
        return null;
    }

    private static bool IsSelected(Control element)
    {
        if (element is ListBoxItem lbi)
            return lbi.IsSelected;
        if (element is TreeViewItem tvi)
            return tvi.IsSelected;
        if (element is TabItem ti)
            return ti.IsSelected;
        return false;
    }

    private static bool CanReceivePointerInput(Control element)
    {
        return element.IsVisible &&
               element.IsEnabled &&
               element.IsHitTestVisible &&
               element.Opacity > 0;
    }

    private static bool IsInteractable(Control element)
    {
        return element.IsVisible &&
               element.IsEnabled &&
               element.IsHitTestVisible &&
               element.Opacity > 0 &&
               element.Bounds.Width > 0 &&
               element.Bounds.Height > 0;
    }

    private static string? GetCustomProperty(Control element, string propertyName)
    {
        // Try to get property via reflection
        var property = element.GetType().GetProperty(propertyName);
        if (property != null)
        {
            var value = property.GetValue(element);
            return value?.ToString();
        }

        // Try to get Avalonia property
        var avaloniaProperty = AvaloniaPropertyRegistry.Instance
            .GetRegistered(element.GetType())
            .FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));

        if (avaloniaProperty != null)
        {
            var value = element.GetValue(avaloniaProperty);
            return value?.ToString();
        }

        return null;
    }

    #endregion
}

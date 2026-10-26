// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See licence.md and NOTICE.md in the repository root for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace Avalonia.HeadlessTestingFramework;

/// <summary>
/// XPath-like query engine for Avalonia visual and logical trees.
/// Supports patterns like: //Button, /Window/StackPanel/Button, //Button[@Name='Submit']
/// </summary>
public class TreeXPath
{
    private readonly object _root;
    private readonly bool _useVisualTree;

    /// <summary>
    /// Creates a TreeXPath query engine for a visual root.
    /// </summary>
    /// <param name="root">The root visual.</param>
    public TreeXPath(Visual root)
    {
        _root = root;
        _useVisualTree = true;
    }

    /// <summary>
    /// Creates a TreeXPath query engine for a logical root.
    /// </summary>
    /// <param name="root">The root logical.</param>
    /// <param name="useVisualTree">Whether to search visual tree instead of logical.</param>
    public TreeXPath(ILogical root, bool useVisualTree = false)
    {
        _root = root;
        _useVisualTree = useVisualTree;
    }

    /// <summary>
    /// Selects nodes matching the XPath expression.
    /// </summary>
    /// <param name="xpath">The XPath expression.</param>
    /// <returns>Matching nodes.</returns>
    public IEnumerable<object> Select(string xpath)
    {
        var expression = ParseExpression(xpath);
        return EvaluateExpression(expression, new[] { _root });
    }

    /// <summary>
    /// Selects nodes matching the XPath expression, cast to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to cast results to.</typeparam>
    /// <param name="xpath">The XPath expression.</param>
    /// <returns>Matching nodes of the specified type.</returns>
    public IEnumerable<T> Select<T>(string xpath) where T : class
    {
        return Select(xpath).OfType<T>();
    }

    /// <summary>
    /// Selects the first node matching the XPath expression.
    /// </summary>
    /// <param name="xpath">The XPath expression.</param>
    /// <returns>The first matching node, or null.</returns>
    public object? SelectFirst(string xpath)
    {
        return Select(xpath).FirstOrDefault();
    }

    /// <summary>
    /// Selects the first node matching the XPath expression.
    /// </summary>
    /// <typeparam name="T">The type to cast result to.</typeparam>
    /// <param name="xpath">The XPath expression.</param>
    /// <returns>The first matching node, or null.</returns>
    public T? SelectFirst<T>(string xpath) where T : class
    {
        return Select<T>(xpath).FirstOrDefault();
    }

    /// <summary>
    /// Checks if any nodes match the XPath expression.
    /// </summary>
    /// <param name="xpath">The XPath expression.</param>
    /// <returns>True if any nodes match.</returns>
    public bool Exists(string xpath)
    {
        return Select(xpath).Any();
    }

    /// <summary>
    /// Counts nodes matching the XPath expression.
    /// </summary>
    /// <param name="xpath">The XPath expression.</param>
    /// <returns>The number of matching nodes.</returns>
    public int Count(string xpath)
    {
        return Select(xpath).Count();
    }

    #region Expression Parsing

    private XPathExpression ParseExpression(string xpath)
    {
        var expression = new XPathExpression();
        var remaining = xpath.Trim();

        // Check for descendant axis (//)
        if (remaining.StartsWith("//"))
        {
            expression.IsDescendant = true;
            remaining = remaining.Substring(2);
        }
        else if (remaining.StartsWith("/"))
        {
            remaining = remaining.Substring(1);
        }

        // Parse steps
        var steps = SplitBySlash(remaining);
        foreach (var step in steps)
        {
            if (!string.IsNullOrEmpty(step))
            {
                expression.Steps.Add(ParseStep(step));
            }
        }

        return expression;
    }

    private List<string> SplitBySlash(string path)
    {
        var result = new List<string>();
        var current = "";
        var bracketDepth = 0;

        foreach (var c in path)
        {
            if (c == '[') bracketDepth++;
            else if (c == ']') bracketDepth--;

            if (c == '/' && bracketDepth == 0)
            {
                if (!string.IsNullOrEmpty(current))
                    result.Add(current);
                current = "";
            }
            else
            {
                current += c;
            }
        }

        if (!string.IsNullOrEmpty(current))
            result.Add(current);

        return result;
    }

    private XPathStep ParseStep(string step)
    {
        var result = new XPathStep();

        // Check for descendant axis
        if (step.StartsWith("//"))
        {
            result.IsDescendantAxis = true;
            step = step.Substring(2);
        }

        // Check for parent axis
        if (step == "..")
        {
            result.IsParentAxis = true;
            return result;
        }

        // Check for self axis
        if (step == ".")
        {
            result.IsSelfAxis = true;
            return result;
        }

        // Parse element name and predicates
        var bracketIndex = step.IndexOf('[');
        if (bracketIndex >= 0)
        {
            result.ElementName = step.Substring(0, bracketIndex);
            var predicateStr = step.Substring(bracketIndex);
            result.Predicates = ParsePredicates(predicateStr);
        }
        else
        {
            result.ElementName = step;
        }

        // Check for wildcard
        if (result.ElementName == "*")
        {
            result.IsWildcard = true;
        }

        return result;
    }

    private List<XPathPredicate> ParsePredicates(string predicateString)
    {
        var result = new List<XPathPredicate>();
        var regex = new Regex(@"\[([^\]]+)\]");
        var matches = regex.Matches(predicateString);

        foreach (Match match in matches)
        {
            var content = match.Groups[1].Value.Trim();
            result.Add(ParsePredicate(content));
        }

        return result;
    }

    private XPathPredicate ParsePredicate(string content)
    {
        var predicate = new XPathPredicate();

        // Position predicate: [1], [2], [last()]
        if (int.TryParse(content, out var position))
        {
            predicate.Type = PredicateType.Position;
            predicate.Position = position;
            return predicate;
        }

        if (content == "last()")
        {
            predicate.Type = PredicateType.Last;
            return predicate;
        }

        if (content.StartsWith("position()"))
        {
            predicate.Type = PredicateType.PositionExpression;
            predicate.Expression = content;
            return predicate;
        }

        // Attribute predicate: [@Name='value'] or [@Name="value"]
        var attrMatch = Regex.Match(content, @"^@(\w+)\s*=\s*(['""])((?:(?!\2).)*)\2$");
        if (attrMatch.Success)
        {
            predicate.Type = PredicateType.Attribute;
            predicate.AttributeName = attrMatch.Groups[1].Value;
            predicate.AttributeValue = attrMatch.Groups[3].Value;
            return predicate;
        }

        // Attribute exists: [@Name]
        var attrExistsMatch = Regex.Match(content, @"^@(\w+)$");
        if (attrExistsMatch.Success)
        {
            predicate.Type = PredicateType.AttributeExists;
            predicate.AttributeName = attrExistsMatch.Groups[1].Value;
            return predicate;
        }

        // Contains: [contains(@Name, 'value')]
        var containsMatch = Regex.Match(content, @"contains\s*\(\s*@(\w+)\s*,\s*(['""])((?:(?!\2).)*)\2\s*\)");
        if (containsMatch.Success)
        {
            predicate.Type = PredicateType.Contains;
            predicate.AttributeName = containsMatch.Groups[1].Value;
            predicate.AttributeValue = containsMatch.Groups[3].Value;
            return predicate;
        }

        // Starts-with: [starts-with(@Name, 'value')]
        var startsWithMatch = Regex.Match(content, @"starts-with\s*\(\s*@(\w+)\s*,\s*(['""])((?:(?!\2).)*)\2\s*\)");
        if (startsWithMatch.Success)
        {
            predicate.Type = PredicateType.StartsWith;
            predicate.AttributeName = startsWithMatch.Groups[1].Value;
            predicate.AttributeValue = startsWithMatch.Groups[3].Value;
            return predicate;
        }

        // Ends-with: [ends-with(@Name, 'value')]
        var endsWithMatch = Regex.Match(content, @"ends-with\s*\(\s*@(\w+)\s*,\s*(['""])((?:(?!\2).)*)\2\s*\)");
        if (endsWithMatch.Success)
        {
            predicate.Type = PredicateType.EndsWith;
            predicate.AttributeName = endsWithMatch.Groups[1].Value;
            predicate.AttributeValue = endsWithMatch.Groups[3].Value;
            return predicate;
        }

        // Matches (regex): [matches(@Name, 'pattern')]
        var matchesMatch = Regex.Match(content, @"matches\s*\(\s*@(\w+)\s*,\s*(['""])((?:(?!\2).)*)\2\s*\)");
        if (matchesMatch.Success)
        {
            predicate.Type = PredicateType.Matches;
            predicate.AttributeName = matchesMatch.Groups[1].Value;
            predicate.AttributeValue = matchesMatch.Groups[3].Value;
            return predicate;
        }

        // Not: [not(@Name='value')]
        var notMatch = Regex.Match(content, @"not\s*\(\s*@(\w+)\s*=\s*(['""])((?:(?!\2).)*)\2\s*\)");
        if (notMatch.Success)
        {
            predicate.Type = PredicateType.Not;
            predicate.AttributeName = notMatch.Groups[1].Value;
            predicate.AttributeValue = notMatch.Groups[3].Value;
            return predicate;
        }

        // Has child: [Button] or [//Button]
        if (content.StartsWith("//"))
        {
            predicate.Type = PredicateType.HasDescendant;
            predicate.ChildExpression = content.Substring(2);
        }
        else
        {
            predicate.Type = PredicateType.HasChild;
            predicate.ChildExpression = content;
        }

        return predicate;
    }

    #endregion

    #region Expression Evaluation

    private IEnumerable<object> EvaluateExpression(XPathExpression expression, IEnumerable<object> context)
    {
        var current = context;

        if (expression.IsDescendant && expression.Steps.Count > 0)
        {
            // For //Type, include self and all descendants for first step filtering
            var firstStep = expression.Steps[0];
            var candidates = new List<object>();

            foreach (var root in context)
            {
                // Include root itself
                candidates.Add(root);
                // Include all descendants
                candidates.AddRange(GetDescendantsRecursive(root));
            }

            // Filter by first step's type and predicates
            if (!firstStep.IsWildcard && !string.IsNullOrEmpty(firstStep.ElementName))
            {
                candidates = candidates.Where(c => MatchesTypeName(c, firstStep.ElementName)).ToList();
            }

            // Apply first step predicates
            foreach (var predicate in firstStep.Predicates)
            {
                candidates = ApplyPredicate(predicate, candidates).ToList();
            }

            current = candidates;

            // Process remaining steps normally
            for (int i = 1; i < expression.Steps.Count; i++)
            {
                current = EvaluateStep(expression.Steps[i], current);
            }
        }
        else
        {
            foreach (var step in expression.Steps)
            {
                current = EvaluateStep(step, current);
            }
        }

        return current;
    }

    private IEnumerable<object> EvaluateStep(XPathStep step, IEnumerable<object> context)
    {
        if (step.IsParentAxis)
        {
            return context.SelectMany(GetParent).Where(p => p != null)!;
        }

        if (step.IsSelfAxis)
        {
            return context;
        }

        IEnumerable<object> candidates;

        if (step.IsDescendantAxis)
        {
            candidates = GetAllDescendants(context);
        }
        else
        {
            candidates = context.SelectMany(GetChildren);
        }

        // Filter by element name
        if (!step.IsWildcard && !string.IsNullOrEmpty(step.ElementName))
        {
            candidates = candidates.Where(c => MatchesTypeName(c, step.ElementName));
        }

        // Apply predicates
        var candidateList = candidates.ToList();
        foreach (var predicate in step.Predicates)
        {
            candidateList = ApplyPredicate(predicate, candidateList).ToList();
        }

        return candidateList;
    }

    private IEnumerable<object> GetAllDescendants(IEnumerable<object> roots)
    {
        foreach (var root in roots)
        {
            foreach (var descendant in GetDescendantsRecursive(root))
            {
                yield return descendant;
            }
        }
    }

    private IEnumerable<object> GetDescendantsRecursive(object node)
    {
        foreach (var child in GetChildren(node))
        {
            yield return child;
            foreach (var descendant in GetDescendantsRecursive(child))
            {
                yield return descendant;
            }
        }
    }

    private IEnumerable<object> GetChildren(object node)
    {
        if (_useVisualTree && node is Visual visual)
        {
            return visual.GetVisualChildren().Cast<object>();
        }
        else if (node is ILogical logical)
        {
            return logical.LogicalChildren.Cast<object>();
        }
        return Enumerable.Empty<object>();
    }

    private IEnumerable<object?> GetParent(object node)
    {
        if (_useVisualTree && node is Visual visual)
        {
            yield return visual.GetVisualParent();
        }
        else if (node is ILogical logical)
        {
            yield return logical.GetLogicalParent();
        }
    }

    private bool MatchesTypeName(object node, string typeName)
    {
        var type = node.GetType();

        // Check exact name match
        if (type.Name == typeName)
            return true;

        // Check base types
        var baseType = type.BaseType;
        while (baseType != null)
        {
            if (baseType.Name == typeName)
                return true;
            baseType = baseType.BaseType;
        }

        // Check interfaces
        foreach (var iface in type.GetInterfaces())
        {
            if (iface.Name == typeName)
                return true;
        }

        return false;
    }

    private IEnumerable<object> ApplyPredicate(XPathPredicate predicate, List<object> candidates)
    {
        switch (predicate.Type)
        {
            case PredicateType.Position:
                if (predicate.Position > 0 && predicate.Position <= candidates.Count)
                    yield return candidates[predicate.Position - 1];
                break;

            case PredicateType.Last:
                if (candidates.Count > 0)
                    yield return candidates[candidates.Count - 1];
                break;

            case PredicateType.Attribute:
                foreach (var c in candidates)
                {
                    if (GetAttributeValue(c, predicate.AttributeName!) == predicate.AttributeValue)
                        yield return c;
                }
                break;

            case PredicateType.AttributeExists:
                foreach (var c in candidates)
                {
                    var value = GetAttributeValue(c, predicate.AttributeName!);
                    if (value != null)
                        yield return c;
                }
                break;

            case PredicateType.Contains:
                foreach (var c in candidates)
                {
                    var value = GetAttributeValue(c, predicate.AttributeName!);
                    if (value?.Contains(predicate.AttributeValue!) == true)
                        yield return c;
                }
                break;

            case PredicateType.StartsWith:
                foreach (var c in candidates)
                {
                    var value = GetAttributeValue(c, predicate.AttributeName!);
                    if (value?.StartsWith(predicate.AttributeValue!) == true)
                        yield return c;
                }
                break;

            case PredicateType.EndsWith:
                foreach (var c in candidates)
                {
                    var value = GetAttributeValue(c, predicate.AttributeName!);
                    if (value?.EndsWith(predicate.AttributeValue!) == true)
                        yield return c;
                }
                break;

            case PredicateType.Matches:
                foreach (var c in candidates)
                {
                    var value = GetAttributeValue(c, predicate.AttributeName!);
                    if (value != null && Regex.IsMatch(value, predicate.AttributeValue!))
                        yield return c;
                }
                break;

            case PredicateType.Not:
                foreach (var c in candidates)
                {
                    if (GetAttributeValue(c, predicate.AttributeName!) != predicate.AttributeValue)
                        yield return c;
                }
                break;

            case PredicateType.HasChild:
                foreach (var c in candidates)
                {
                    if (GetChildren(c).Any(child => MatchesTypeName(child, predicate.ChildExpression!)))
                        yield return c;
                }
                break;

            case PredicateType.HasDescendant:
                foreach (var c in candidates)
                {
                    if (GetDescendantsRecursive(c).Any(d => MatchesTypeName(d, predicate.ChildExpression!)))
                        yield return c;
                }
                break;

            default:
                foreach (var c in candidates)
                    yield return c;
                break;
        }
    }

    private string? GetAttributeValue(object node, string attributeName)
    {
        if (node is Control control)
        {
            return attributeName.ToLowerInvariant() switch
            {
                "name" => control.Name,
                "tag" => control.Tag?.ToString(),
                "isenabled" => control.IsEnabled.ToString(),
                "isvisible" => control.IsVisible.ToString(),
                "width" => control.Width.ToString(),
                "height" => control.Height.ToString(),
                "classes" => string.Join(" ", control.Classes),
                "isfocused" => control.IsFocused.ToString(),
                _ => GetPropertyValue(node, attributeName)
            };
        }

        return GetPropertyValue(node, attributeName);
    }

    private string? GetPropertyValue(object node, string propertyName)
    {
        // Try to get CLR property
        var property = node.GetType().GetProperty(propertyName);
        if (property != null)
        {
            return property.GetValue(node)?.ToString();
        }

        // Try to get Avalonia property
        if (node is AvaloniaObject ao)
        {
            var avaloniaProperty = AvaloniaPropertyRegistry.Instance
                .GetRegistered(ao.GetType())
                .FirstOrDefault(p => p.Name == propertyName);

            if (avaloniaProperty != null)
            {
                return ao.GetValue(avaloniaProperty)?.ToString();
            }
        }

        return null;
    }

    #endregion

    #region Expression Classes

    private class XPathExpression
    {
        public bool IsDescendant { get; set; }
        public List<XPathStep> Steps { get; } = new();
    }

    private class XPathStep
    {
        public string ElementName { get; set; } = string.Empty;
        public bool IsWildcard { get; set; }
        public bool IsDescendantAxis { get; set; }
        public bool IsParentAxis { get; set; }
        public bool IsSelfAxis { get; set; }
        public List<XPathPredicate> Predicates { get; set; } = new();
    }

    private class XPathPredicate
    {
        public PredicateType Type { get; set; }
        public int Position { get; set; }
        public string? AttributeName { get; set; }
        public string? AttributeValue { get; set; }
        public string? Expression { get; set; }
        public string? ChildExpression { get; set; }
    }

    private enum PredicateType
    {
        Position,
        Last,
        PositionExpression,
        Attribute,
        AttributeExists,
        Contains,
        StartsWith,
        EndsWith,
        Matches,
        Not,
        HasChild,
        HasDescendant
    }

    #endregion
}

/// <summary>
/// Extension methods for XPath-like tree queries.
/// </summary>
public static class TreeXPathExtensions
{
    /// <summary>
    /// Creates an XPath query engine for the visual tree.
    /// </summary>
    /// <param name="root">The root visual.</param>
    /// <returns>An XPath query engine.</returns>
    public static TreeXPath XPath(this Visual root)
    {
        return new TreeXPath(root);
    }

    /// <summary>
    /// Creates an XPath query engine for the logical tree.
    /// </summary>
    /// <param name="root">The root logical.</param>
    /// <param name="useVisualTree">Whether to search visual tree instead.</param>
    /// <returns>An XPath query engine.</returns>
    public static TreeXPath XPath(this ILogical root, bool useVisualTree = false)
    {
        return new TreeXPath(root, useVisualTree);
    }

    /// <summary>
    /// Selects nodes using XPath expression from visual tree.
    /// </summary>
    /// <param name="root">The root visual.</param>
    /// <param name="xpath">The XPath expression.</param>
    /// <returns>Matching nodes.</returns>
    public static IEnumerable<object> SelectXPath(this Visual root, string xpath)
    {
        return new TreeXPath(root).Select(xpath);
    }

    /// <summary>
    /// Selects nodes using XPath expression from visual tree.
    /// </summary>
    /// <typeparam name="T">The type to cast results to.</typeparam>
    /// <param name="root">The root visual.</param>
    /// <param name="xpath">The XPath expression.</param>
    /// <returns>Matching nodes of the specified type.</returns>
    public static IEnumerable<T> SelectXPath<T>(this Visual root, string xpath) where T : class
    {
        return new TreeXPath(root).Select<T>(xpath);
    }

    /// <summary>
    /// Selects the first node using XPath expression.
    /// </summary>
    /// <typeparam name="T">The type to cast result to.</typeparam>
    /// <param name="root">The root visual.</param>
    /// <param name="xpath">The XPath expression.</param>
    /// <returns>The first matching node, or null.</returns>
    public static T? SelectFirstXPath<T>(this Visual root, string xpath) where T : class
    {
        return new TreeXPath(root).SelectFirst<T>(xpath);
    }

    /// <summary>
    /// Checks if any nodes match the XPath expression.
    /// </summary>
    /// <param name="root">The root visual.</param>
    /// <param name="xpath">The XPath expression.</param>
    /// <returns>True if any nodes match.</returns>
    public static bool ExistsXPath(this Visual root, string xpath)
    {
        return new TreeXPath(root).Exists(xpath);
    }

    /// <summary>
    /// Counts nodes matching the XPath expression.
    /// </summary>
    /// <param name="root">The root visual.</param>
    /// <param name="xpath">The XPath expression.</param>
    /// <returns>The number of matching nodes.</returns>
    public static int CountXPath(this Visual root, string xpath)
    {
        return new TreeXPath(root).Count(xpath);
    }
}

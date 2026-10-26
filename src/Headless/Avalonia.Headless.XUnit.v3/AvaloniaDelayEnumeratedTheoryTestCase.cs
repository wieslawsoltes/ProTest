using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xunit.v3;

namespace Avalonia.Headless.XUnit;

internal sealed partial class AvaloniaDelayEnumeratedTheoryTestCase
    : XunitDelayEnumeratedTheoryTestCase, ISelfExecutingXunitTestCase
{
    public AvaloniaDelayEnumeratedTheoryTestCase(
        IXunitTestMethod testMethod,
        string testCaseDisplayName,
        string uniqueID,
        bool @explicit,
        bool skipTestWithoutData,
        Type[]? skipExceptions = null,
        string? skipReason = null,
        Type? skipType = null,
        string? skipUnless = null,
        string? skipWhen = null,
        Dictionary<string, HashSet<string>>? traits = null,
        string? sourceFilePath = null,
        int? sourceLineNumber = null,
        int? timeout = null)
        : base(
            testMethod,
            testCaseDisplayName,
            uniqueID,
            @explicit,
            skipTestWithoutData,
            skipExceptions,
            skipReason,
            skipType,
            skipUnless,
            skipWhen,
            traits,
            sourceFilePath,
            sourceLineNumber,
            timeout)
    {
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
    public AvaloniaDelayEnumeratedTheoryTestCase()
    {
    }
}

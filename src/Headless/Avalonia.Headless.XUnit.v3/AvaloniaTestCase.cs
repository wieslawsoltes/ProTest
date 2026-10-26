using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xunit.v3;

namespace Avalonia.Headless.XUnit;

internal sealed partial class AvaloniaTestCase : XunitTestCase, ISelfExecutingXunitTestCase
{
    public AvaloniaTestCase(
        IXunitTestMethod testMethod,
        string testCaseDisplayName,
        string uniqueID,
        bool @explicit,
        Type[]? skipExceptions = null,
        string? skipReason = null,
        Type? skipType = null,
        string? skipUnless = null,
        string? skipWhen = null,
        Dictionary<string, HashSet<string>>? traits = null,
        object?[]? testMethodArguments = null,
        string? sourceFilePath = null,
        int? sourceLineNumber = null,
#if XUNIT_V4
        int? timeout = null,
        string? testLabel = null,
        bool disableParallelization = false)
#else
        int? timeout = null)
#endif
        : base(
            testMethod,
            testCaseDisplayName,
            uniqueID,
            @explicit,
#if XUNIT_V4
            testLabel,
            disableParallelization,
#endif
            skipExceptions,
            skipReason,
            skipType,
            skipUnless,
            skipWhen,
            traits,
            testMethodArguments,
            sourceFilePath,
            sourceLineNumber,
            timeout)
    {
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
    public AvaloniaTestCase()
    {
    }
}

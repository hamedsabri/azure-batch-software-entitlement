
// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Design",
    "CA1034:Nested types should not be visible",
    Justification = "This project uses nested public classes to group related tests.")]

[assembly:SuppressMessage(
    "Design",
    "CA1052",
    Justification = "Grouping tests in nested private classes for structure.")]

[assembly: SuppressMessage(
    "Globalization",
    "CA1308:Normalize strings to uppercase",
    Justification = "We don't round trip values, just transform for display.")]

[assembly:SuppressMessage(
    "Naming",
    "CA1707: Remove the underscores from member names",
    Justification = "This project uses '_' to separate parts of test names.")]

[assembly: SuppressMessage(
    "Performance",
    "CA1822:Mark members as static",
    Justification = "Some tests don't share intialization but still need to be non-static")]

[assembly: SuppressMessage(
    "Redundancy",
    "RCS1163:Unused parameter.",
    Justification = "Empty implementations used for testing.")]



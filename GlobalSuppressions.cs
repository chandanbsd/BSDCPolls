// This file is used by Code Analysis to maintain SuppressMessage attributes
// that are applied to this project. Project-level suppressions either have no target
// or are given a specific target and scoped to a namespace, type, member, etc.
// Each suppression must be reviewed and approved during code review.
// Justification for each suppression is mandatory.

using System.Diagnostics.CodeAnalysis;

// CSharpier is the sole formatter for this solution. SA1111 and SA1502 conflict with
// CSharpier's opinionated formatting for trailing commas and single-line elements.
// StyleCop's formatting rules are suppressed; style enforcement is CSharpier's domain.
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1111:ClosingParenthesisMustBeOnLineOfLastParameter", Justification = "CSharpier formatter")]
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1502:ElementMustNotBeOnSingleLine", Justification = "CSharpier formatter")]
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1519:BracesMustNotBeOmittedFromMultiLineChildStatement", Justification = "CSharpier formatter")]
[assembly: SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1515:SingleLineCommentMustBePrecededByBlankLine", Justification = "CSharpier formatter")]

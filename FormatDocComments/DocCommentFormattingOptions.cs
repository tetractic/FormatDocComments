// Copyright 2018 Carl Reinke
//
// This file is part of a library that is licensed under the terms of the GNU
// Lesser General Public License Version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

using Microsoft.CodeAnalysis.Options;

namespace FormatDocComments
{
    /// <summary>
    /// Defines options for documentation comment formatting.
    /// </summary>
    public static class DocCommentFormattingOptions
    {
        /// <summary>
        /// Gets the option that controls the column at which words in documentation comments are
        /// wrapped to the next line.
        /// </summary>
        public static Option<int?> WrapColumn { get; } = new Option<int?>("DocCommentFormattingOptions", "WrapColumn", null);
    }
}

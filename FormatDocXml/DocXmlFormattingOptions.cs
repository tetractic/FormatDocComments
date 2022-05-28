// Copyright 2018 Carl Reinke
//
// This file is part of a library that is licensed under the terms of GNU Lesser
// General Public License version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

using Microsoft.CodeAnalysis.Options;

namespace FormatDocXml
{
    /// <summary>
    /// Defines options for XML documentation comment formatting.
    /// </summary>
    public static class DocXmlFormattingOptions
    {
        /// <summary>
        /// Gets the option that controls the column at which words in XML documentation comments
        /// are wrapped to the next line.
        /// </summary>
        public static Option<int?> WrapColumn { get; } = new Option<int?>("DocXmlFormattingOptions", "WrapColumn", null);
    }
}

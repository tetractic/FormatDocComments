// Copyright 2022 Carl Reinke
//
// This file is part of a library that is licensed under the terms of the GNU
// Lesser General Public License Version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

using Microsoft.VisualStudio.CodingConventions;
using System.Collections.Generic;

namespace FormatDocComments
{
    /// <summary>
    /// The coding convention (i.e. editorconfig) settings for the text editor guide lines.
    /// </summary>
    internal static class CodingConventionSettings
    {
        /// <summary>
        /// Gets the maximum line length.
        /// </summary>
        public static int? GetMaxLineLength(ICodingConventionContext codingConventionContext)
        {
            if (codingConventionContext.CurrentConventions.TryGetConventionValue("max_line_length", out string maxLineLengthString) &&
                int.TryParse(maxLineLengthString, out int maxLineLength) && maxLineLength > 0)
            {
                return maxLineLength;
            }

            return null;
        }

        /// <summary>
        /// Gets the columns of the guide lines.
        /// </summary>
        public static IEnumerable<int> GetGuideColumns(ICodingConventionContext codingConventionContext)
        {
            if (codingConventionContext.CurrentConventions.TryGetConventionValue("guidelines", out string guidelinesString))
            {
                foreach (string guidelineString in guidelinesString.Split(','))
                {
                    int length;
                    for (length = 0; length < guidelineString.Length; ++length)
                    {
                        if (guidelineString[length] != ' ')
                        {
                            length = guidelineString.IndexOf(' ', length);
                            if (length < 0)
                                length = guidelineString.Length;
                            break;
                        }
                    }
                    string columnString = guidelineString.Substring(0, length);

                    if (int.TryParse(columnString, out int column) && column > 0)
                        yield return column;
                }
            }

        }
    }
}

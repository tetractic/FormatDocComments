// Copyright 2018 Carl Reinke
//
// This file is part of a library that is licensed under the terms of GNU Lesser
// General Public License version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

using System;
using System.Text;

namespace FormatDocXml
{
    internal static class StringBuilderExtensions
    {
        public static StringBuilder AppendIndent(this StringBuilder stringBuilder, int indent, bool useTabs = false, int tabSize = 4)
        {
            if (stringBuilder == null)
                throw new ArgumentNullException(nameof(stringBuilder));
            if (indent < 0)
                throw new ArgumentOutOfRangeException(nameof(indent));
            if (tabSize < 0)
                throw new ArgumentOutOfRangeException(nameof(tabSize));

            return useTabs
                ? stringBuilder
                    .Append('\t', indent / tabSize)
                    .Append(' ', indent % tabSize)
                : stringBuilder
                    .Append(' ', indent);
        }
    }
}

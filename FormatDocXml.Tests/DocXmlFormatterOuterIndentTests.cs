//
// Copyright (C) 2018  Carl Reinke
//
// This file is part of FormatDocXML.
//
// This program is free software; you can redistribute it and/or modify it under the terms of the
// GNU Lesser General Public License as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without
// even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License along with this program;
// if not, write to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA
// 02110-1301, USA.
//
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using Xunit;
using static FormatDocXml.Tests.DocXmlFormatterTests;

namespace FormatDocXml.Tests
{
    public class DocXmlFormatterOuterIndentTests
    {
        [Fact]
        public void TestOuterIndentFollowsFirstLine()
        {
            var inputText =
@"public class C {
    /// <summary>
/// Words and words.
  /// </summary>
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// <summary>
    /// Words and words.
    /// </summary>
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestOuterIndentEmpty()
        {
            var inputText =
@"public class C {
/// <summary>
/// Words and words.
/// </summary>
    public void M() { }
}";
            var expectedText = inputText;

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestOuterIndentExcessive()
        {
            var inputText =
@"public class C {
        /// <summary>
        /// Words and words.
        /// </summary>
    public void M() { }
}";
            var expectedText = inputText;

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestOuterIndentUseTabs()
        {
            var inputText =
@"public class C {
    /// <summary>
    /// Words and words.
    /// </summary>
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// <summary>
" + "\t" + @"/// Words and words.
" + "\t" + @"/// </summary>
    public void M() { }
}";

            AssertFormat(expectedText, inputText, (options) => options
                .WithChangedOption(FormattingOptions.UseTabs, LanguageNames.CSharp, true));
        }

        [Fact]
        public void TestOuterIndentTabSize()
        {
            var inputText =
@"public class C {
    /// <summary>
    /// Words and words.
    /// </summary>
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// <summary>
" + "\t" + @" /// Words and words.
" + "\t" + @" /// </summary>
    public void M() { }
}";

            AssertFormat(expectedText, inputText, (options) => options
                .WithChangedOption(FormattingOptions.UseTabs, LanguageNames.CSharp, true)
                .WithChangedOption(FormattingOptions.TabSize, LanguageNames.CSharp, 3));
        }
    }
}

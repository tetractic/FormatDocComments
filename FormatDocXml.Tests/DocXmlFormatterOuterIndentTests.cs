// Copyright 2018 Carl Reinke
//
// This file is part of a library that is licensed under the terms of GNU Lesser
// General Public License version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

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
            string inputText =
@"public class C {
    /// <summary>
/// Words and words.
  /// </summary>
    public void M() { }
}";
            string expectedText =
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
            string inputText =
@"public class C {
/// <summary>
/// Words and words.
/// </summary>
    public void M() { }
}";
            string expectedText = inputText;

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestOuterIndentExcessive()
        {
            string inputText =
@"public class C {
        /// <summary>
        /// Words and words.
        /// </summary>
    public void M() { }
}";
            string expectedText = inputText;

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestOuterIndentUseTabs()
        {
            string inputText =
@"public class C {
    /// <summary>
    /// Words and words.
    /// </summary>
    public void M() { }
}";
            string expectedText =
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
            string inputText =
@"public class C {
    /// <summary>
    /// Words and words.
    /// </summary>
    public void M() { }
}";
            string expectedText =
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

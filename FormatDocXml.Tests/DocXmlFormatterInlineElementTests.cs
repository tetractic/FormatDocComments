// Copyright 2018 Carl Reinke
//
// This file is part of a library that is licensed under the terms of GNU Lesser
// General Public License version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

using Xunit;
using static FormatDocXml.Tests.DocXmlFormatterTests;

namespace FormatDocXml.Tests
{
    public class DocXmlFormatterInlineElementTests
    {
        [Fact]
        public void TestInlineElement()
        {
            var inputText =
    @"public class C {
    /// Words and <see cref=""M1""/> and <c>words</c>.
    public void M1() { }

    /// Words and
    /// <see cref=""M2""/>
    /// and
    /// <c>words</c>.
    public void M2() { }
}";
            var expectedText =
    @"public class C {
    /// Words and <see cref=""M1""/> and <c>words</c>.
    public void M1() { }

    /// Words and <see cref=""M2""/> and <c>words</c>.
    public void M2() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestInlineElementBreaks()
        {
            var inputText =
@"public class C {
    /// Words <c>and</c> words.
    public void M1() { }

    /// Words<c> and</c> words.
    public void M2() { }

    /// Words <c>and </c>words.
    public void M3() { }

    /// Words<c> and </c>words.
    public void M4() { }
}";
            var expectedText =
@"public class C {
    /// Words <c>and</c> words.
    public void M1() { }

    /// Words<c> and</c> words.
    public void M2() { }

    /// Words <c>and </c>words.
    public void M3() { }

    /// Words<c> and </c>words.
    public void M4() { }
}";

            AssertFormat(expectedText, inputText, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 40));
        }

        [Fact]
        public void TestInlineElementWraps()
        {
            var inputText =
@"public class C {
    /// Words and words and words <c>and</c> words.
    public void M1() { }

    /// Words and words and words<c> and</c> words.
    public void M2() { }

    /// Words and words and <c>words </c>and words.
    public void M3() { }

    /// Words and words and <c>words</c> and words.
    public void M4() { }
}";
            var expectedText =
@"public class C {
    /// Words and words and words
    /// <c>and</c> words.
    public void M1() { }

    /// Words and words and words<c>
    /// and</c> words.
    public void M2() { }

    /// Words and words and <c>words
    /// </c>and words.
    public void M3() { }

    /// Words and words and <c>words</c>
    /// and words.
    public void M4() { }
}";

            AssertFormat(expectedText, inputText, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 40));
        }

        [Fact]
        public void TestInlineElementWrapsContent()
        {
            var inputText =
@"public class C {
    /// Words and words and <c>words and words and</c> words.
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// Words and words and <c>words and
    /// words and</c> words.
    public void M() { }
}";

            AssertFormat(expectedText, inputText, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 40));
        }

        [Fact]
        public void TestInlineElementSticksWithSurroundingWord()
        {
            var inputText =
@"public class C {
    /// __________<c>__________ __________</c>__________
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// __________<c>__________
    /// __________</c>__________
    public void M() { }
}";

            AssertFormat(expectedText, inputText, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 20));
        }

        [Fact]
        public void TestInlineEmptyElementWraps()
        {
            var inputText =
@"public class C {
    /// Words and words and <see cref=""M1""/> and words.
    public void M1() { }

    /// Words and <see cref=""M2(C)""/> and words.
    public void M2(C x) { }
}";
            var expectedText =
@"public class C {
    /// Words and words and
    /// <see cref=""M1""/> and words.
    public void M1() { }

    /// Words and <see cref=""M2(C)""/>
    /// and words.
    public void M2(C x) { }
}";

            AssertFormat(expectedText, inputText, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 40));
        }

        [Fact]
        public void TestInlineEmptyElementSticksWithSurroundingWord()
        {
            var inputText =
@"public class C {
    /// __________<see cref=""C""/>__________
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// __________<see cref=""C""/>__________
    public void M() { }
}";

            AssertFormat(expectedText, inputText, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 20));
        }
    }
}

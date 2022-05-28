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
    public class DocXmlFormatterCommentTests
    {
        [Fact]
        public void TestComment()
        {
            var inputText =
    @"public class C {
    /// Words and <!--words and--> words.
    public void M1() { }

    /// Words and
    /// <!--words
    /// and-->
    /// words.
    public void M2() { }
}";
            var expectedText =
    @"public class C {
    /// Words and <!--words and--> words.
    public void M1() { }

    /// Words and <!--words and--> words.
    public void M2() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestCommentBreaks()
        {
            var inputText =
@"public class C {
    /// Words <!--and--> words.
    public void M1() { }

    /// Words<!-- and--> words.
    public void M2() { }

    /// Words <!--and -->words.
    public void M3() { }

    /// Words<!-- and -->words.
    public void M4() { }
}";
            var expectedText =
@"public class C {
    /// Words <!--and--> words.
    public void M1() { }

    /// Words<!-- and--> words.
    public void M2() { }

    /// Words <!--and -->words.
    public void M3() { }

    /// Words<!-- and -->words.
    public void M4() { }
}";

            AssertFormat(expectedText, inputText, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 40));
        }

        [Fact]
        public void TestCommentWraps()
        {
            var inputText =
@"public class C {
    /// Words and words and words <!--and--> words.
    public void M1() { }

    /// Words and words and words<!-- and--> words.
    public void M2() { }

    /// Words and words and <!--words -->and words.
    public void M3() { }

    /// Words and words and <!--words--> and words.
    public void M4() { }
}";
            var expectedText =
@"public class C {
    /// Words and words and words
    /// <!--and--> words.
    public void M1() { }

    /// Words and words and words<!--
    /// and--> words.
    public void M2() { }

    /// Words and words and <!--words
    /// -->and words.
    public void M3() { }

    /// Words and words and <!--words-->
    /// and words.
    public void M4() { }
}";

            AssertFormat(expectedText, inputText, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 40));
        }

        [Fact]
        public void TestCommentWrapsContent()
        {
            var inputText =
@"public class C {
    /// Words and words and <!--words and words and--> words.
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// Words and words and <!--words
    /// and words and--> words.
    public void M() { }
}";

            AssertFormat(expectedText, inputText, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 40));
        }

        [Fact]
        public void TestCommentSticksWithSurroundingWord()
        {
            var inputText =
@"public class C {
    /// __________<!--__________ __________-->__________
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// __________<!--__________
    /// __________-->__________
    public void M() { }
}";

            AssertFormat(expectedText, inputText, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 20));
        }
    }
}

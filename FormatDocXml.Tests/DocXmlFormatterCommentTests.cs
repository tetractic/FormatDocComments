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
            string inputText =
    @"public class C {
    /// Words and <!--words and--> words.
    public void M1() { }

    /// Words and
    /// <!--words
    /// and-->
    /// words.
    public void M2() { }
}";
            string expectedText =
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
            string inputText =
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
            string expectedText =
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
            string inputText =
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
            string expectedText =
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
            string inputText =
@"public class C {
    /// Words and words and <!--words and words and--> words.
    public void M() { }
}";
            string expectedText =
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
            string inputText =
@"public class C {
    /// __________<!--__________ __________-->__________
    public void M() { }
}";
            string expectedText =
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

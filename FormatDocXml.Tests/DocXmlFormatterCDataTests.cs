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
    public class DocXmlFormatterCDataTests
    {
        [Fact]
        public void TestCData()
        {
            string inputText =
    @"public class C {
    /// Words and <![CDATA[words and]]> words.
    public void M1() { }

    /// Words and
    /// <![CDATA[words
    /// and]]>
    /// words.
    public void M2() { }
}";
            string expectedText =
    @"public class C {
    /// Words and <![CDATA[words and]]> words.
    public void M1() { }

    /// Words and <![CDATA[words and]]> words.
    public void M2() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestCDataBreaks()
        {
            string inputText =
@"public class C {
    /// Words <![CDATA[and]]> words.
    public void M1() { }

    /// Words<![CDATA[ and]]> words.
    public void M2() { }

    /// Words <![CDATA[and ]]>words.
    public void M3() { }

    /// Words<![CDATA[ and ]]>words.
    public void M4() { }
}";
            string expectedText =
@"public class C {
    /// Words <![CDATA[and]]> words.
    public void M1() { }

    /// Words<![CDATA[ and]]> words.
    public void M2() { }

    /// Words <![CDATA[and ]]>words.
    public void M3() { }

    /// Words<![CDATA[ and ]]>words.
    public void M4() { }
}";

            AssertFormat(expectedText, inputText, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 40));
        }

        [Fact]
        public void TestCDataWraps()
        {
            string inputText =
@"public class C {
    /// Words and words and <![CDATA[words]]>.
    public void M1() { }

    /// Words and words and<![CDATA[ words]]> and words.
    public void M2() { }

    /// Words and words <![CDATA[and ]]>words.
    public void M3() { }

    /// Words and words <![CDATA[and]]> words.
    public void M4() { }
}";
            string expectedText =
@"public class C {
    /// Words and words and
    /// <![CDATA[words]]>.
    public void M1() { }

    /// Words and words and<![CDATA[
    /// words]]> and words.
    public void M2() { }

    /// Words and words <![CDATA[and
    /// ]]>words.
    public void M3() { }

    /// Words and words <![CDATA[and]]>
    /// words.
    public void M4() { }
}";

            AssertFormat(expectedText, inputText, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 40));
        }

        [Fact]
        public void TestCDataWrapsContent()
        {
            string inputText =
@"public class C {
    /// Words and <![CDATA[words and words and]]> words.
    public void M() { }
}";
            string expectedText =
@"public class C {
    /// Words and <![CDATA[words and
    /// words and]]> words.
    public void M() { }
}";

            AssertFormat(expectedText, inputText, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 40));
        }

        [Fact]
        public void TestCDataSticksWithSurroundingWord()
        {
            string inputText =
@"public class C {
    /// __________<![CDATA[__________ __________]]>__________
    public void M() { }
}";
            string expectedText =
@"public class C {
    /// __________<![CDATA[__________
    /// __________]]>__________
    public void M() { }
}";

            AssertFormat(expectedText, inputText, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 20));
        }
    }
}

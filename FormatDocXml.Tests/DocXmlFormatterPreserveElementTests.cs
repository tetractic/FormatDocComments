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
    public class DocXmlFormatterPreserveElementTests
    {
        [Fact]
        public void TestPreserveNoInnerBreaks()
        {
            var inputText =
@"public class C {
    /// <code>Words and words.</code>
    public void M() { }
}";
            var expectedText = inputText;

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestPreserveStartBreak()
        {
            var inputText =
@"public class C {
    /// <code>
    /// Words and words.</code>
    public void M() { }
}";
            var expectedText = inputText;

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestPreserveEndBreak()
        {
            var inputText =
@"public class C {
    /// <code>Words and words.
    /// </code>
    public void M() { }
}";
            var expectedText = inputText;

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestPreserveWhitespace()
        {
            var inputText =
@"public class C {
    /// <code>
    ///   Words and    
    ///     words  and    words
    ///   words.
    /// </code>
    public void M() { }
}";
            var expectedText = inputText;

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestPreserveLineBreak()
        {
            var inputText =
@"public class C {
    /// <code>
/// Words and words.
    /// </code>
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// <code>
    /// Words and words.
    /// </code>
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestPreserveLongLines()
        {
            var inputText =
@"public class C {
    /// <code>
    ///   Words and words and words and words and words and words.
    /// </code>
    public void M() { }
}";
            var expectedText = inputText;

            AssertFormat(expectedText, inputText, (options) => options
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 40));
        }
    }
}

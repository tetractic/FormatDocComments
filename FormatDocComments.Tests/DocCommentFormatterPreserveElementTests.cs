// Copyright 2018 Carl Reinke
//
// This file is part of a library that is licensed under the terms of GNU Lesser
// General Public License version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

using Xunit;
using static FormatDocComments.Tests.DocCommentFormatterTests;

namespace FormatDocComments.Tests
{
    public class DocCommentFormatterPreserveElementTests
    {
        [Fact]
        public void TestPreserveNoInnerBreaks()
        {
            string inputText =
@"public class C {
    /// <code>Words and words.</code>
    public void M() { }
}";
            string expectedText = inputText;

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestPreserveStartBreak()
        {
            string inputText =
@"public class C {
    /// <code>
    /// Words and words.</code>
    public void M() { }
}";
            string expectedText = inputText;

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestPreserveEndBreak()
        {
            string inputText =
@"public class C {
    /// <code>Words and words.
    /// </code>
    public void M() { }
}";
            string expectedText = inputText;

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestPreserveWhitespace()
        {
            string inputText =
@"public class C {
    /// <code>
    ///   Words and    
    ///     words  and    words
    ///   words.
    /// </code>
    public void M() { }
}";
            string expectedText = inputText;

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestPreserveLineBreak()
        {
            string inputText =
@"public class C {
    /// <code>
/// Words and words.
    /// </code>
    public void M() { }
}";
            string expectedText =
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
            string inputText =
@"public class C {
    /// <code>
    ///   Words and words and words and words and words and words.
    /// </code>
    public void M() { }
}";
            string expectedText = inputText;

            AssertFormat(expectedText, inputText, (options) => options
                .WithChangedOption(DocCommentFormattingOptions.WrapColumn, 40));
        }
    }
}

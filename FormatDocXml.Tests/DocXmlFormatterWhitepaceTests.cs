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
    public class DocXmlFormatterWhitepaceTests
    {
        [Fact]
        public void TestEmpty()
        {
            var inputText =
@"public class C {
    ///
    public void M() { }
}";
            var expectedText = inputText;

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestSpace()
        {
            var inputText =
@"public class C {
    /// 
    public void M() { }
}";
            var expectedText = inputText;

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestTab()
        {
            var inputText =
@"public class C {
    ///" + "\t" + @"
    public void M() { }
}";
            var expectedText = inputText;

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestSpaceSpace()
        {
            var inputText =
@"public class C {
    ///  
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// 
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestTabSpace()
        {
            var inputText =
@"public class C {
    ///" + "\t" + @" 
    public void M() { }
}";
            var expectedText =
@"public class C {
    ///" + "\t" + @"
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestInternalLinePrefixEmpty()
        {
            var inputText =
@"public class C {
    ///<summary>
    ///Words and words.
    ///</summary>
    public void M() { }
}";
            var expectedText = inputText;

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestInternalLinePrefixMismatched()
        {
            var inputText =
@"public class C {
    /// <summary>
    ///" + "\t" + @"Words and words.
    /// </summary>
    public void M() { }
}";
            var expectedText =
@"public class C {
    ///<summary>
    ///Words and words.
    ///</summary>
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestLeadingSpace()
        {
            var inputText =
@"public class C {
    ///  Words and words.
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// Words and words.
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestTrailingSpace()
        {
            var inputText =
@"public class C {
    /// Words and words 
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// Words and words
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestLeadingLineBreak()
        {
            var inputText =
@"public class C {
    /// 
    /// Words and words.
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// Words and words.
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestTrailingLineBreak()
        {
            var inputText =
@"public class C {
    /// Words and words.
    /// 
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// Words and words.
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestSentenceSpaces()
        {
            var inputText =
@"public class C {
    /// An extra space is preserved after a sentence end.  Extra  spaces  inside
    /// sentences  are  not  preserved!   It's called ""sentence spacing.""  A
    /// study by Loh et al. published in 2002 indicates that it has no
    /// significant effect on readability...  I happen to prefer it anyway.
    public void M() { }
}";
            var wrapped80Text =
@"public class C {
    /// An extra space is preserved after a sentence end.  Extra spaces inside
    /// sentences are not preserved!  It's called ""sentence spacing.""  A study
    /// by Loh et al. published in 2002 indicates that it has no significant
    /// effect on readability...  I happen to prefer it anyway.
    public void M() { }
}";
            var wrapped60Text =
@"public class C {
    /// An extra space is preserved after a sentence end. 
    /// Extra spaces inside sentences are not preserved! 
    /// It's called ""sentence spacing.""  A study by Loh et
    /// al. published in 2002 indicates that it has no
    /// significant effect on readability...  I happen to
    /// prefer it anyway.
    public void M() { }
}";

            AssertFormat(wrapped80Text, inputText, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 80));
            AssertFormat(wrapped60Text, inputText, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 60));

            AssertFormat(wrapped80Text, wrapped80Text, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 80));
            AssertFormat(wrapped80Text, wrapped60Text, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 80));
            AssertFormat(wrapped60Text, wrapped80Text, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 60));
            AssertFormat(wrapped60Text, wrapped60Text, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 60));
        }
    }
}

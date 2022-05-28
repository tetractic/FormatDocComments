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
    public class DocCommentFormatterWhitepaceTests
    {
        [Fact]
        public void TestEmpty()
        {
            string inputText =
@"public class C {
    ///
    public void M() { }
}";
            string expectedText = inputText;

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestSpace()
        {
            string inputText =
@"public class C {
    /// 
    public void M() { }
}";
            string expectedText = inputText;

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestTab()
        {
            string inputText =
@"public class C {
    ///" + "\t" + @"
    public void M() { }
}";
            string expectedText = inputText;

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestSpaceSpace()
        {
            string inputText =
@"public class C {
    ///  
    public void M() { }
}";
            string expectedText =
@"public class C {
    /// 
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestTabSpace()
        {
            string inputText =
@"public class C {
    ///" + "\t" + @" 
    public void M() { }
}";
            string expectedText =
@"public class C {
    ///" + "\t" + @"
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestInternalLinePrefixEmpty()
        {
            string inputText =
@"public class C {
    ///<summary>
    ///Words and words.
    ///</summary>
    public void M() { }
}";
            string expectedText = inputText;

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestInternalLinePrefixMismatched()
        {
            string inputText =
@"public class C {
    /// <summary>
    ///" + "\t" + @"Words and words.
    /// </summary>
    public void M() { }
}";
            string expectedText =
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
            string inputText =
@"public class C {
    ///  Words and words.
    public void M() { }
}";
            string expectedText =
@"public class C {
    /// Words and words.
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestTrailingSpace()
        {
            string inputText =
@"public class C {
    /// Words and words 
    public void M() { }
}";
            string expectedText =
@"public class C {
    /// Words and words
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestLeadingLineBreak()
        {
            string inputText =
@"public class C {
    /// 
    /// Words and words.
    public void M() { }
}";
            string expectedText =
@"public class C {
    /// Words and words.
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestTrailingLineBreak()
        {
            string inputText =
@"public class C {
    /// Words and words.
    /// 
    public void M() { }
}";
            string expectedText =
@"public class C {
    /// Words and words.
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestSentenceSpaces()
        {
            string inputText =
@"public class C {
    /// An extra space is preserved after a sentence end.  Extra  spaces  inside
    /// sentences  are  not  preserved!   It's called ""sentence spacing.""  A
    /// study by Loh et al. published in 2002 indicates that it has no
    /// significant effect on readability...  I happen to prefer it anyway.
    public void M() { }
}";
            string wrapped80Text =
@"public class C {
    /// An extra space is preserved after a sentence end.  Extra spaces inside
    /// sentences are not preserved!  It's called ""sentence spacing.""  A study
    /// by Loh et al. published in 2002 indicates that it has no significant
    /// effect on readability...  I happen to prefer it anyway.
    public void M() { }
}";
            string wrapped60Text =
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
                .WithChangedOption(DocCommentFormattingOptions.WrapColumn, 80));
            AssertFormat(wrapped60Text, inputText, (config) => config
                .WithChangedOption(DocCommentFormattingOptions.WrapColumn, 60));

            AssertFormat(wrapped80Text, wrapped80Text, (config) => config
                .WithChangedOption(DocCommentFormattingOptions.WrapColumn, 80));
            AssertFormat(wrapped80Text, wrapped60Text, (config) => config
                .WithChangedOption(DocCommentFormattingOptions.WrapColumn, 80));
            AssertFormat(wrapped60Text, wrapped80Text, (config) => config
                .WithChangedOption(DocCommentFormattingOptions.WrapColumn, 60));
            AssertFormat(wrapped60Text, wrapped60Text, (config) => config
                .WithChangedOption(DocCommentFormattingOptions.WrapColumn, 60));
        }
    }
}

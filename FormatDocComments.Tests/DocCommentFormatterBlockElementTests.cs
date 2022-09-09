// Copyright 2018 Carl Reinke
//
// This file is part of a library that is licensed under the terms of the GNU
// Lesser General Public License Version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

using Xunit;
using static FormatDocComments.Tests.DocCommentFormatterTests;

namespace FormatDocComments.Tests
{
    public class DocCommentFormatterBlockElementTests
    {
        [Fact]
        public void TestBlockElement()
        {
            string inputText =
@"public class C {
    /// <summary>Words and words.</summary>
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
        public void TestBlockElementEmpty()
        {
            string inputText =
@"public class C {
    /// <summary></summary>
    public void M() { }
}";
            string expectedText =
@"public class C {
    /// <summary>
    /// </summary>
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestBlockElementSpace()
        {
            string inputText =
@"public class C {
    /// <summary> </summary>
    public void M() { }
}";
            string expectedText =
@"public class C {
    /// <summary>
    /// </summary>
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestBlockEmptyElement()
        {
            string inputText =
@"public class C {
    /// <seealso cref=""M""/>
    public void M() { }
}";
            string expectedText = inputText;

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestBlockElements()
        {
            string inputText =
@"public class C {
    /// <summary>Words and words.</summary><remarks>Words and words and words.</remarks>
    public void M() { }
}";
            string expectedText =
@"public class C {
    /// <summary>
    /// Words and words.
    /// </summary>
    /// <remarks>
    /// Words and words and words.
    /// </remarks>
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestBlockElementWrapsContent()
        {
            string inputText =
@"public class C {
    /// <summary>
    /// Words and words and words and words.
    /// </summary>
    public void M() { }
}";
            string expectedText =
@"public class C {
    /// <summary>
    /// Words and words and words and
    /// words.
    /// </summary>
    public void M() { }
}";

            AssertFormat(expectedText, inputText, (config) => config
                .WithChangedOption(DocCommentFormattingOptions.WrapColumn, 40));
        }

        [Fact]
        public void TestSnugBlockElementWrapsContent()
        {
            string inputText =
@"public class C {
    /// <param name=""x"">Words and words and words.</param>
    public void M(int x) { }
}";
            string expectedText =
@"public class C {
    /// <param name=""x"">Words and words
    ///     and words.</param>
    public void M(int x) { }
}";

            AssertFormat(expectedText, inputText, (config) => config
                .WithChangedOption(DocCommentFormattingOptions.WrapColumn, 40));
        }

        [Fact]
        public void TestSnugBlockElementBreaksEndTagFromContent()
        {
            string inputText =
@"public class C {
    /// <param name=""x"">Words and words and words and words...</param>
    public void M(int x) { }
}";
            string expectedText =
@"public class C {
    /// <param name=""x"">Words and words
    ///     and words and words...
    ///     </param>
    public void M(int x) { }
}";

            AssertFormat(expectedText, inputText, (config) => config
                .WithChangedOption(DocCommentFormattingOptions.WrapColumn, 40));
        }

        [Fact]
        public void TestSnugBlockElementBreaksStartTagFromContent()
        {
            string inputText =
@"public class C {
    /// <param name=""longParameterName"">Words and words.</param>
    public void M(int longParameterName) { }
}";
            string expectedText =
@"public class C {
    /// <param name=""longParameterName"">
    ///     Words and words.</param>
    public void M(int longParameterName) { }
}";

            AssertFormat(expectedText, inputText, (config) => config
                .WithChangedOption(DocCommentFormattingOptions.WrapColumn, 40));
        }

        [Fact]
        public void TestSnugBlockElementUnindentsAfterInnerBlock()
        {
            string inputText =
@"public class C {
    /// <param name=""x""><para>Words.</para></param>
    public void M(int x) { }
}";
            string expectedText =
@"public class C {
    /// <param name=""x"">
    ///     <para>
    ///     Words.
    ///     </para>
    /// </param>
    public void M(int x) { }
}";

            AssertFormat(expectedText, inputText, (config) => config
                .WithChangedOption(DocCommentFormattingOptions.WrapColumn, 40));
        }
    }
}

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
    public class DocXmlFormatterBlockElementTests
    {
        [Fact]
        public void TestBlockElement()
        {
            var inputText =
@"public class C {
    /// <summary>Words and words.</summary>
    public void M() { }
}";
            var expectedText =
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
            var inputText =
@"public class C {
    /// <summary></summary>
    public void M() { }
}";
            var expectedText =
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
            var inputText =
@"public class C {
    /// <summary> </summary>
    public void M() { }
}";
            var expectedText =
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
            var inputText =
@"public class C {
    /// <seealso cref=""M""/>
    public void M() { }
}";
            var expectedText = inputText;

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestBlockElements()
        {
            var inputText =
@"public class C {
    /// <summary>Words and words.</summary><remarks>Words and words and words.</remarks>
    public void M() { }
}";
            var expectedText =
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
            var inputText =
@"public class C {
    /// <summary>
    /// Words and words and words and words.
    /// </summary>
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// <summary>
    /// Words and words and words and
    /// words.
    /// </summary>
    public void M() { }
}";

            AssertFormat(expectedText, inputText, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 40));
        }

        [Fact]
        public void TestSnugBlockElementWrapsContent()
        {
            var inputText =
@"public class C {
    /// <param name=""x"">Words and words and words.</param>
    public void M(int x) { }
}";
            var expectedText =
@"public class C {
    /// <param name=""x"">Words and words
    ///     and words.</param>
    public void M(int x) { }
}";

            AssertFormat(expectedText, inputText, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 40));
        }

        [Fact]
        public void TestSnugBlockElementBreaksEndTagFromContent()
        {
            var inputText =
@"public class C {
    /// <param name=""x"">Words and words and words and words...</param>
    public void M(int x) { }
}";
            var expectedText =
@"public class C {
    /// <param name=""x"">Words and words
    ///     and words and words...
    ///     </param>
    public void M(int x) { }
}";

            AssertFormat(expectedText, inputText, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 40));
        }

        [Fact]
        public void TestSnugBlockElementBreaksStartTagFromContent()
        {
            var inputText =
@"public class C {
    /// <param name=""longParameterName"">Words and words.</param>
    public void M(int longParameterName) { }
}";
            var expectedText =
@"public class C {
    /// <param name=""longParameterName"">
    ///     Words and words.</param>
    public void M(int longParameterName) { }
}";

            AssertFormat(expectedText, inputText, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 40));
        }

        [Fact]
        public void TestSnugBlockElementUnindentsAfterInnerBlock()
        {
            var inputText =
@"public class C {
    /// <param name=""x""><para>Words.</para></param>
    public void M(int x) { }
}";
            var expectedText =
@"public class C {
    /// <param name=""x"">
    ///     <para>
    ///     Words.
    ///     </para>
    /// </param>
    public void M(int x) { }
}";

            AssertFormat(expectedText, inputText, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 40));
        }
    }
}

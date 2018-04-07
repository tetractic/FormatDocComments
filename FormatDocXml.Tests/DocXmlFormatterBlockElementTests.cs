//
// Copyright (C) 2018  Carl Reinke
//
// This file is part of FormatDocXML.
//
// This program is free software; you can redistribute it and/or modify it under the terms of the
// GNU Lesser General Public License as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without
// even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License along with this program;
// if not, write to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA
// 02110-1301, USA.
//
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
    }
}

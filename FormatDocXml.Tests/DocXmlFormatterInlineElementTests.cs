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
    public class DocXmlFormatterInlineElementTests
    {
        [Fact]
        public void TestInlineElement()
        {
            var inputText =
    @"public class C {
    /// Words and <see cref=""M1""/> and <c>words</c>.
    public void M1() { }

    /// Words and
    /// <see cref=""M2""/>
    /// and
    /// <c>words</c>.
    public void M2() { }
}";
            var expectedText =
    @"public class C {
    /// Words and <see cref=""M1""/> and <c>words</c>.
    public void M1() { }

    /// Words and <see cref=""M2""/> and <c>words</c>.
    public void M2() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestInlineElementWraps()
        {
            var inputText =
@"public class C {
    /// Words and words and <see cref=""M""/> and words.
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// Words and words and
    /// <see cref=""M""/> and words.
    public void M() { }
}";

            AssertFormat(expectedText, inputText, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 40));
        }

        [Fact]
        public void TestInlineElementWrapsContent()
        {
            var inputText =
@"public class C {
    /// Words and <c>words and words and words</c> and words.
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// Words and <c>words and words and
    /// words</c> and words.
    public void M() { }
}";

            AssertFormat(expectedText, inputText, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 40));
        }

        [Fact]
        public void TestInlineElementSticksWithSurroundingWord()
        {
            var inputText =
@"public class C {
    /// __________<c>__________ __________</c>__________
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// __________<c>__________
    /// __________</c>__________
    public void M() { }
}";

            AssertFormat(expectedText, inputText, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 20));
        }

        [Fact]
        public void TestInlineEmptyElementSticksWithSurroundingWord()
        {
            var inputText =
@"public class C {
    /// __________<see cref=""C""/>__________
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// __________<see cref=""C""/>__________
    public void M() { }
}";

            AssertFormat(expectedText, inputText, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 20));
        }
    }
}

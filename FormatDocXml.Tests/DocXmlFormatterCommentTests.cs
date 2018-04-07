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
    public class DocXmlFormatterCommentTests
    {
        [Fact]
        public void TestComment()
        {
            var inputText =
    @"public class C {
    /// Words and <!--words and--> words.
    public void M1() { }

    /// Words and
    /// <!--words
    /// and-->
    /// words.
    public void M2() { }
}";
            var expectedText =
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
            var inputText =
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
            var expectedText =
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
            var inputText =
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
            var expectedText =
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
            var inputText =
@"public class C {
    /// Words and words and <!--words and words and--> words.
    public void M() { }
}";
            var expectedText =
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
            var inputText =
@"public class C {
    /// __________<!--__________ __________-->__________
    public void M() { }
}";
            var expectedText =
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

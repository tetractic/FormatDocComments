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
    public class DocXmlFormatterMissingTokenTests
    {
        [Fact]
        public void TestBlockElementMissingEndTag()
        {
            var inputText =
@"public class C {
    /// <summary>Words and words.
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// <summary>
    /// Words and words.
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestBlockElementMissingStartTag()
        {
            var inputText =
@"public class C {
    /// Words and words.</summary>
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// Words and words.</summary>
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestBlockEmptyElementMissingSlash()
        {
            var inputText =
@"public class C {
    /// <include file=""file.xml"" path=""*"">
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// <include file=""file.xml"" path=""*"">
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestInlineElementMissingEndTag()
        {
            var inputText =
    @"public class C {
    /// Words and <c>words.
    public void M() { }
}";
            var expectedText =
    @"public class C {
    /// Words and <c>words.
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestInlineElementMissingStartTag()
        {
            var inputText =
    @"public class C {
    /// Words and words</c>.
    public void M() { }
}";
            var expectedText =
    @"public class C {
    /// Words and words</c>.
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestInlineEmptyElementMissingSlash()
        {
            var inputText =
    @"public class C {
    /// Words and <see cref=""M"">.
    public void M() { }
}";
            var expectedText =
    @"public class C {
    /// Words and <see cref=""M"">.
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }
    }
}

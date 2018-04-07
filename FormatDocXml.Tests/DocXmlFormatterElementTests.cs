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
    public class DocXmlFormatterElementTests
    {
        [Fact]
        public void TestElementInnerBreaks()
        {
            var inputText =
@"public class C {
    /// < exception
    /// cref = ""Exception"" >Words and words.</ exception >
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// <exception cref=""Exception"">Words and words.</exception>
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestEmptyElementInnerBreaks()
        {
            var inputText =
@"public class C {
    /// Words and < see
    /// cref = ""M"" />.
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// Words and <see cref=""M""/>.
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestNamespacedElement()
        {
            var inputText =
@"public class C {
    /// <ns:summary>Words and words.</ns:summary>
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// <ns:summary>Words and words.</ns:summary>
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestElementAttributeLineBreak()
        {
            var inputText =
@"public class C {
    /// <ns:tag attribute=""
/// ""/>
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// <ns:tag attribute=""
    /// ""/>
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }
    }
}

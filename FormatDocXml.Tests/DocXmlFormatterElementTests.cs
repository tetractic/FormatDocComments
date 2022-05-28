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
    public class DocXmlFormatterElementTests
    {
        [Fact]
        public void TestElementInnerBreaks()
        {
            string inputText =
@"public class C {
    /// < exception
    /// cref = ""Exception"" >Words and words.</ exception >
    public void M() { }
}";
            string expectedText =
@"public class C {
    /// <exception cref=""Exception"">Words and words.</exception>
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestEmptyElementInnerBreaks()
        {
            string inputText =
@"public class C {
    /// Words and < see
    /// cref = ""M"" />.
    public void M() { }
}";
            string expectedText =
@"public class C {
    /// Words and <see cref=""M""/>.
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestNamespacedElement()
        {
            string inputText =
@"public class C {
    /// <ns:summary>Words and words.</ns:summary>
    public void M() { }
}";
            string expectedText =
@"public class C {
    /// <ns:summary>Words and words.</ns:summary>
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestElementAttributeLineBreak()
        {
            string inputText =
@"public class C {
    /// <ns:tag attribute=""
/// ""/>
    public void M() { }
}";
            string expectedText =
@"public class C {
    /// <ns:tag attribute=""
    /// ""/>
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }
    }
}

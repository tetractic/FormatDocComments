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
        public void TestBlockElementMissingEndTagName()
        {
            var inputText =
@"public class C {
    /// <summary>Words and words.</>
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// <summary>
    /// Words and words.
    /// </>
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
        public void TestBlockElementMissingStartTagName()
        {
            var inputText =
@"public class C {
    /// <>Words and words.</summary>
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// <>
    /// Words and words.
    /// </summary>
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestBlockElementAttributeTruncated()
        {
            var inputText =
@"public class C {
    /// <include file
    public void M1() { }

    /// <include file=
    public void M2() { }

    /// <include file=""
    public void M3() { }

    /// <include file=""file.xml
    public void M4() { }

    /// <include file=""file.xml""
    public void M5() { }
}";
            var expectedText =
@"public class C {
    /// <include file
    public void M1() { }

    /// <include file=
    public void M2() { }

    /// <include file=""
    public void M3() { }

    /// <include file=""file.xml
    public void M4() { }

    /// <include file=""file.xml""
    public void M5() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestBlockElementCrefAttributeTruncated()
        {
            var inputText =
@"public class C {
    /// <exception cref
    public void M1() { }

    /// <exception cref=
    public void M2() { }

    /// <exception cref=""
    public void M3() { }

    /// <exception cref=""Exception
    public void M4() { }

    /// <exception cref=""Exception""
    public void M5() { }
}";
            var expectedText =
@"public class C {
    /// <exception cref
    public void M1() { }

    /// <exception cref=
    public void M2() { }

    /// <exception cref=""
    public void M3() { }

    /// <exception cref=""Exception
    public void M4() { }

    /// <exception cref=""Exception""
    public void M5() { }
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
        public void TestInlineElementMissingEndTagName()
        {
            var inputText =
    @"public class C {
    /// Words and <c>words</>.
    public void M() { }
}";
            var expectedText =
    @"public class C {
    /// Words and <c>words</>.
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
        public void TestInlineElementMissingStartTagName()
        {
            var inputText =
    @"public class C {
    /// Words and <>words</c>.
    public void M() { }
}";
            var expectedText =
    @"public class C {
    /// Words and <>words</c>.
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

        [Fact]
        public void TestInlineElementAttributeTruncated()
        {
            var inputText =
@"public class C {
    /// Words and <see langword
    public void M1() { }

    /// Words and <see langword=
    public void M2() { }

    /// Words and <see langword=""
    public void M3() { }

    /// Words and <see langword=""null
    public void M4() { }

    /// Words and <see langword=""null""
    public void M5() { }
}";
            var expectedText =
@"public class C {
    /// Words and <see langword
    public void M1() { }

    /// Words and <see langword=
    public void M2() { }

    /// Words and <see langword=""
    public void M3() { }

    /// Words and <see langword=""null
    public void M4() { }

    /// Words and <see langword=""null""
    public void M5() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestInlineElementCrefAttributeTruncated()
        {
            var inputText =
@"public class C {
    /// Words and <see cref
    public void M1() { }

    /// Words and <see cref=
    public void M2() { }

    /// Words and <see cref=""
    public void M3() { }

    /// Words and <see cref=""M4
    public void M4() { }

    /// Words and <see cref=""M5""
    public void M5() { }
}";
            var expectedText =
@"public class C {
    /// Words and <see cref
    public void M1() { }

    /// Words and <see cref=
    public void M2() { }

    /// Words and <see cref=""
    public void M3() { }

    /// Words and <see cref=""M4
    public void M4() { }

    /// Words and <see cref=""M5""
    public void M5() { }
}";

            AssertFormat(expectedText, inputText);
        }
    }
}

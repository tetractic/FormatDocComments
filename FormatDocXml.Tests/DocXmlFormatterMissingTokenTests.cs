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
            string inputText =
@"public class C {
    /// <summary>Words and words.
    public void M() { }
}";
            string expectedText =
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
            string inputText =
@"public class C {
    /// <summary>Words and words.</>
    public void M() { }
}";
            string expectedText =
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
            string inputText =
@"public class C {
    /// Words and words.</summary>
    public void M() { }
}";
            string expectedText =
@"public class C {
    /// Words and words.</summary>
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestBlockElementMissingStartTagName()
        {
            string inputText =
@"public class C {
    /// <>Words and words.</summary>
    public void M() { }
}";
            string expectedText =
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
            string inputText =
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
            string expectedText =
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
            string inputText =
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
            string expectedText =
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
            string inputText =
@"public class C {
    /// <include file=""file.xml"" path=""*"">
    public void M() { }
}";
            string expectedText =
@"public class C {
    /// <include file=""file.xml"" path=""*"">
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestInlineElementMissingEndTag()
        {
            string inputText =
    @"public class C {
    /// Words and <c>words.
    public void M() { }
}";
            string expectedText =
    @"public class C {
    /// Words and <c>words.
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestInlineElementMissingEndTagName()
        {
            string inputText =
    @"public class C {
    /// Words and <c>words</>.
    public void M() { }
}";
            string expectedText =
    @"public class C {
    /// Words and <c>words</>.
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestInlineElementMissingStartTag()
        {
            string inputText =
    @"public class C {
    /// Words and words</c>.
    public void M() { }
}";
            string expectedText =
    @"public class C {
    /// Words and words</c>.
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestInlineElementMissingStartTagName()
        {
            string inputText =
    @"public class C {
    /// Words and <>words</c>.
    public void M() { }
}";
            string expectedText =
    @"public class C {
    /// Words and <>words</c>.
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestInlineEmptyElementMissingSlash()
        {
            string inputText =
    @"public class C {
    /// Words and <see cref=""M"">.
    public void M() { }
}";
            string expectedText =
    @"public class C {
    /// Words and <see cref=""M"">.
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestInlineElementAttributeTruncated()
        {
            string inputText =
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
            string expectedText =
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
            string inputText =
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
            string expectedText =
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

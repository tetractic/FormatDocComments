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
    public class DocXmlFormatterSpanTests
    {
        [Fact]
        public void TestSpanWholeDocument()
        {
            var inputText =
@"/*[*//// <summary>This is C.</summary>
public class C {
    /// <summary>This is M1.</summary>
    public void M1() { }

    /// <summary>This is M2.</summary>
    public void M2() { }
}/*]*/";
            var expectedText =
@"/// <summary>
/// This is C.
/// </summary>
public class C {
    /// <summary>
    /// This is M1.
    /// </summary>
    public void M1() { }

    /// <summary>
    /// This is M2.
    /// </summary>
    public void M2() { }
}";

            var span = GetSpan(ref inputText);
            AssertFormat(expectedText, inputText, span);
        }

        [Fact]
        public void TestSpanInsideComment()
        {
            var inputText =
@"/// <summary>This is C.</summary>
public class C {
    /// <summary>/*[*//*]*/This is M1.</summary>
    public void M1() { }

    /// <summary>This is M2.</summary>
    public void M2() { }
}";
            var expectedText =
@"/// <summary>This is C.</summary>
public class C {
    /// <summary>
    /// This is M1.
    /// </summary>
    public void M1() { }

    /// <summary>This is M2.</summary>
    public void M2() { }
}";

            var span = GetSpan(ref inputText);
            AssertFormat(expectedText, inputText, span);
        }

        [Fact]
        public void TestSpanEncompassesComment()
        {
            var inputText =
@"/// <summary>This is C.</summary>
public class C /*[*/{
    /// <summary>This is M1.</summary>
    public/*]*/ void M1() { }

    /// <summary>This is M2.</summary>
    public void M2() { }
}";
            var expectedText =
@"/// <summary>This is C.</summary>
public class C {
    /// <summary>
    /// This is M1.
    /// </summary>
    public void M1() { }

    /// <summary>This is M2.</summary>
    public void M2() { }
}";

            var span = GetSpan(ref inputText);
            AssertFormat(expectedText, inputText, span);
        }

        [Fact]
        public void TestSpanOverlapsCommentStart()
        {
            var inputText =
@"/// <summary>This is C.</summary>
public class C /*[*/{
    /// <summary>/*]*/This is M1.</summary>
    public void M1() { }

    /// <summary>This is M2.</summary>
    public void M2() { }
}";
            var expectedText =
@"/// <summary>This is C.</summary>
public class C {
    /// <summary>
    /// This is M1.
    /// </summary>
    public void M1() { }

    /// <summary>This is M2.</summary>
    public void M2() { }
}";

            var span = GetSpan(ref inputText);
            AssertFormat(expectedText, inputText, span);
        }

        [Fact]
        public void TestSpanOverlapsCommentEnd()
        {
            var inputText =
@"/// <summary>This is C.</summary>
public class C {
    /// <summary>/*[*/This is M1.</summary>
    public/*]*/ void M1() { }

    /// <summary>This is M2.</summary>
    public void M2() { }
}";
            var expectedText =
@"/// <summary>This is C.</summary>
public class C {
    /// <summary>
    /// This is M1.
    /// </summary>
    public void M1() { }

    /// <summary>This is M2.</summary>
    public void M2() { }
}";

            var span = GetSpan(ref inputText);
            AssertFormat(expectedText, inputText, span);
        }

        [Fact]
        public void TestSpanOverlapsTwoComments()
        {
            var inputText =
@"/// <summary>This is C.</summary>
public class C {
    /// <summary>/*[*/This is M1.</summary>
    public void M1() { }

    /// <summary>/*]*/This is M2.</summary>
    public void M2() { }

    /// <summary>This is M3.</summary>
    public void M3() { }
}";
            var expectedText =
@"/// <summary>This is C.</summary>
public class C {
    /// <summary>
    /// This is M1.
    /// </summary>
    public void M1() { }

    /// <summary>
    /// This is M2.
    /// </summary>
    public void M2() { }

    /// <summary>This is M3.</summary>
    public void M3() { }
}";

            var span = GetSpan(ref inputText);
            AssertFormat(expectedText, inputText, span);
        }

        [Fact]
        public void TestSpanAtStartOfComment()
        {
            var inputText =
@"/// <summary>This is C.</summary>
public class C {
    /*[*//*]*//// <summary>This is M1.</summary>
    public void M1() { }

    /// <summary>This is M2.</summary>
    public void M2() { }
}";
            var expectedText =
@"/// <summary>This is C.</summary>
public class C {
    /// <summary>This is M1.</summary>
    public void M1() { }

    /// <summary>This is M2.</summary>
    public void M2() { }
}";

            var span = GetSpan(ref inputText);
            AssertFormat(expectedText, inputText, span);
        }

        [Fact]
        public void TestSpanAtEndOfComment()
        {
            var inputText =
@"/// <summary>This is C.</summary>
public class C {
    /// <summary>This is M1.</summary>
/*[*//*]*/    public void M1() { }

    /// <summary>This is M2.</summary>
    public void M2() { }
}";
            var expectedText =
@"/// <summary>This is C.</summary>
public class C {
    /// <summary>This is M1.</summary>
    public void M1() { }

    /// <summary>This is M2.</summary>
    public void M2() { }
}";

            var span = GetSpan(ref inputText);
            AssertFormat(expectedText, inputText, span);
        }
    }
}

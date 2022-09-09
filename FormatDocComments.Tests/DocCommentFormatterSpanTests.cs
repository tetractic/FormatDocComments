// Copyright 2018 Carl Reinke
//
// This file is part of a library that is licensed under the terms of the GNU
// Lesser General Public License Version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

using Xunit;
using static FormatDocComments.Tests.DocCommentFormatterTests;

namespace FormatDocComments.Tests
{
    public class DocCommentFormatterSpanTests
    {
        [Fact]
        public void TestSpanWholeDocument()
        {
            string inputText =
@"/*[*//// <summary>This is C.</summary>
public class C {
    /// <summary>This is M1.</summary>
    public void M1() { }

    /// <summary>This is M2.</summary>
    public void M2() { }
}/*]*/";
            string expectedText =
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
            string inputText =
@"/// <summary>This is C.</summary>
public class C {
    /// <summary>/*[*//*]*/This is M1.</summary>
    public void M1() { }

    /// <summary>This is M2.</summary>
    public void M2() { }
}";
            string expectedText =
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
            string inputText =
@"/// <summary>This is C.</summary>
public class C /*[*/{
    /// <summary>This is M1.</summary>
    public/*]*/ void M1() { }

    /// <summary>This is M2.</summary>
    public void M2() { }
}";
            string expectedText =
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
            string inputText =
@"/// <summary>This is C.</summary>
public class C /*[*/{
    /// <summary>/*]*/This is M1.</summary>
    public void M1() { }

    /// <summary>This is M2.</summary>
    public void M2() { }
}";
            string expectedText =
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
            string inputText =
@"/// <summary>This is C.</summary>
public class C {
    /// <summary>/*[*/This is M1.</summary>
    public/*]*/ void M1() { }

    /// <summary>This is M2.</summary>
    public void M2() { }
}";
            string expectedText =
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
            string inputText =
@"/// <summary>This is C.</summary>
public class C {
    /// <summary>/*[*/This is M1.</summary>
    public void M1() { }

    /// <summary>/*]*/This is M2.</summary>
    public void M2() { }

    /// <summary>This is M3.</summary>
    public void M3() { }
}";
            string expectedText =
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
            string inputText =
@"/// <summary>This is C.</summary>
public class C {
    /*[*//*]*//// <summary>This is M1.</summary>
    public void M1() { }

    /// <summary>This is M2.</summary>
    public void M2() { }
}";
            string expectedText =
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
            string inputText =
@"/// <summary>This is C.</summary>
public class C {
    /// <summary>This is M1.</summary>
/*[*//*]*/    public void M1() { }

    /// <summary>This is M2.</summary>
    public void M2() { }
}";
            string expectedText =
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

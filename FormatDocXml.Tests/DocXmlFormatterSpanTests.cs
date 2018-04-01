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

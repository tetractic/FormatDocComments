// Copyright 2018 Carl Reinke
//
// This file is part of a library that is licensed under the terms of GNU Lesser
// General Public License version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

using Xunit;
using static FormatDocComments.Tests.DocCommentFormatterTests;

namespace FormatDocComments.Tests
{
    public class DocCommentFormatterProcessingInstructionTests
    {
        [Fact]
        public void TestProcessingInstruction()
        {
            string inputText =
@"public class C {
    /// <?target and words  and words
    /// and words?>
    public void M() { }
}";
            string expectedText =
@"public class C {
    /// <?target and words  and words
    /// and words?>
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }
    }
}

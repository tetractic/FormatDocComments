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
    public class DocXmlFormatterWhitepaceTests
    {
        [Fact]
        public void TestEmpty()
        {
            var inputText =
@"public class C {
    ///
    public void M() { }
}";
            var expectedText = inputText;

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestSpace()
        {
            var inputText =
@"public class C {
    /// 
    public void M() { }
}";
            var expectedText = inputText;

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestTab()
        {
            var inputText =
@"public class C {
    ///" + "\t" + @"
    public void M() { }
}";
            var expectedText = inputText;

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestSpaceSpace()
        {
            var inputText =
@"public class C {
    ///  
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// 
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestTabSpace()
        {
            var inputText =
@"public class C {
    ///" + "\t" + @" 
    public void M() { }
}";
            var expectedText =
@"public class C {
    ///" + "\t" + @"
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestInternalLinePrefixEmpty()
        {
            var inputText =
@"public class C {
    ///<summary>
    ///Words and words.
    ///</summary>
    public void M() { }
}";
            var expectedText = inputText;

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestInternalLinePrefixMismatched()
        {
            var inputText =
@"public class C {
    /// <summary>
    ///" + "\t" + @"Words and words.
    /// </summary>
    public void M() { }
}";
            var expectedText =
@"public class C {
    ///<summary>
    ///Words and words.
    ///</summary>
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestLeadingSpace()
        {
            var inputText =
@"public class C {
    ///  Words and words.
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// Words and words.
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestTrailingSpace()
        {
            var inputText =
@"public class C {
    /// Words and words 
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// Words and words
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestLeadingLineBreak()
        {
            var inputText =
@"public class C {
    /// 
    /// Words and words.
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// Words and words.
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestTrailingLineBreak()
        {
            var inputText =
@"public class C {
    /// Words and words.
    /// 
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// Words and words.
    public void M() { }
}";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestSentenceSpaces()
        {
            var inputText =
@"public class C {
    /// An extra space is preserved after a sentence end.  Extra  spaces  inside
    /// sentences  are  not  preserved!   It's called ""sentence spacing.""  A
    /// study by Loh et al. published in 2002 indicates that it has no
    /// significant effect on readability...  I happen to prefer it anyway.
    public void M() { }
}";
            var wrapped80Text =
@"public class C {
    /// An extra space is preserved after a sentence end.  Extra spaces inside
    /// sentences are not preserved!  It's called ""sentence spacing.""  A study
    /// by Loh et al. published in 2002 indicates that it has no significant
    /// effect on readability...  I happen to prefer it anyway.
    public void M() { }
}";
            var wrapped60Text =
@"public class C {
    /// An extra space is preserved after a sentence end. 
    /// Extra spaces inside sentences are not preserved! 
    /// It's called ""sentence spacing.""  A study by Loh et
    /// al. published in 2002 indicates that it has no
    /// significant effect on readability...  I happen to
    /// prefer it anyway.
    public void M() { }
}";

            AssertFormat(wrapped80Text, inputText, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 80));
            AssertFormat(wrapped60Text, inputText, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 60));

            AssertFormat(wrapped80Text, wrapped80Text, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 80));
            AssertFormat(wrapped80Text, wrapped60Text, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 80));
            AssertFormat(wrapped60Text, wrapped80Text, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 60));
            AssertFormat(wrapped60Text, wrapped60Text, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 60));
        }
    }
}

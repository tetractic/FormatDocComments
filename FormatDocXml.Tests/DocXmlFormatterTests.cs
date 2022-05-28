// Copyright 2018 Carl Reinke
//
// This file is part of a library that is licensed under the terms of GNU Lesser
// General Public License version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;
using System;
using Xunit;

namespace FormatDocXml.Tests
{
    public class DocXmlFormatterTests
    {
        [Fact]
        public void TestBeginningOfDocument()
        {
            var inputText =
@"/// Words and words.
public class C { }";
            var expectedText = inputText;

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestEndOfDocument()
        {
            var inputText =
@"public class C { }
/// Words and words.
";
            var expectedText = inputText;

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestEndOfDocumentNoNewLine()
        {
            var inputText =
@"public class C { }
/// Words and words.";
            var expectedText =
@"public class C { }
/// Words and words.
";

            AssertFormat(expectedText, inputText);
        }

        [Fact]
        public void TestWrapColumn()
        {
            var inputText =
@"public class C {
    /// _ ______________________________
    /// _ _______________________________
    /// _ ______________________________
    public void M() { }
}";
            var expectedText =
@"public class C {
    /// _ ______________________________
    /// _
    /// _______________________________
    /// _ ______________________________
    public void M() { }
}";

            AssertFormat(expectedText, inputText, (config) => config
                .WithChangedOption(DocXmlFormattingOptions.WrapColumn, 40));
        }

        internal static void AssertFormat(string expectedText, string inputText, Func<OptionSet, OptionSet> updateOptions = null)
        {
            var workspace = new AdhocWorkspace();
            var solution = workspace.CurrentSolution;
            var project = solution.AddProject("projectName", "assemblyName", LanguageNames.CSharp);
            var sourceText = SourceText.From(inputText);
            var document = project.AddDocument("name.cs", sourceText);
            var options = document.Project.Solution.Workspace.Options;
            if (updateOptions != null)
                options = updateOptions(options);
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            var changes = DocXmlFormatter.FormatAsync(document, options).Result;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
            var newSourceText = sourceText.WithChanges(changes);
            var outputText = newSourceText.ToString();

            Assert.Equal(expectedText, outputText);
        }

        internal static void AssertFormat(string expectedText, string inputText, TextSpan span, Func<OptionSet, OptionSet> updateOptions = null)
        {
            var workspace = new AdhocWorkspace();
            var solution = workspace.CurrentSolution;
            var project = solution.AddProject("projectName", "assemblyName", LanguageNames.CSharp);
            var sourceText = SourceText.From(inputText);
            var document = project.AddDocument("name.cs", sourceText);
            var options = document.Project.Solution.Workspace.Options;
            if (updateOptions != null)
                options = updateOptions(options);
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            var changes = DocXmlFormatter.FormatAsync(document, span, options).Result;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
            var newSourceText = sourceText.WithChanges(changes);
            var outputText = newSourceText.ToString();

            Assert.Equal(expectedText, outputText);
        }

        internal static TextSpan GetSpan(ref string inputText, string startMarker = "/*[*/", string endMarker = "/*]*/")
        {
            var spanStart = inputText.IndexOf(startMarker);
            if (spanStart < 0)
                throw new ArgumentException("Span start marker not found.");
            inputText = inputText.Remove(spanStart, startMarker.Length);
            var spanEnd = inputText.IndexOf(endMarker);
            if (spanEnd < 0)
                throw new ArgumentException("Span end marker not found.");
            inputText = inputText.Remove(spanEnd, endMarker.Length);
            return TextSpan.FromBounds(spanStart, spanEnd);
        }
    }
}

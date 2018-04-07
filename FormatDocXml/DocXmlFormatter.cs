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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using static FormatDocXml.ColumnHelpers;

namespace FormatDocXml
{
    /// <summary>
    /// A formatter for XML documentation comments.
    /// </summary>
    public sealed class DocXmlFormatter
    {
        private const int _defaultWrapColumn = 80;

        private static readonly Dictionary<string, ElementFormatting> _elements = new Dictionary<string, ElementFormatting>()
        {
            ["c"] = ElementFormatting.Inline,
            ["paramref"] = ElementFormatting.Inline,
            ["see"] = ElementFormatting.Inline,
            ["typeparamref"] = ElementFormatting.Inline,

            ["code"] = ElementFormatting.Block | ElementFormatting.Preserve,
            ["example"] = ElementFormatting.Block,
            ["exception"] = ElementFormatting.Block | ElementFormatting.Snug | ElementFormatting.Indent,
            ["include"] = ElementFormatting.Block | ElementFormatting.Indent,
            ["list"] = ElementFormatting.Block | ElementFormatting.Indent,
            ["para"] = ElementFormatting.Block,
            ["param"] = ElementFormatting.Block | ElementFormatting.Snug | ElementFormatting.Indent,
            ["permission"] = ElementFormatting.Block | ElementFormatting.Snug | ElementFormatting.Indent,
            ["remarks"] = ElementFormatting.Block,
            ["returns"] = ElementFormatting.Block | ElementFormatting.Snug | ElementFormatting.Indent,
            ["seealso"] = ElementFormatting.Block | ElementFormatting.Snug | ElementFormatting.Indent,
            ["summary"] = ElementFormatting.Block,
            ["value"] = ElementFormatting.Block,
            ["typeparam"] = ElementFormatting.Block | ElementFormatting.Snug | ElementFormatting.Indent,

            ["listheader"] = ElementFormatting.Block | ElementFormatting.Indent,
            ["item"] = ElementFormatting.Block | ElementFormatting.Indent,
            ["term"] = ElementFormatting.Block | ElementFormatting.Snug | ElementFormatting.Indent,
            ["description"] = ElementFormatting.Block | ElementFormatting.Snug | ElementFormatting.Indent,
        };

        private readonly OptionSet _options;

        private readonly bool _useTabs;
        private readonly int _tabSize;
        private readonly int _indentSize;
        private readonly string _newLine;

        private readonly int _wrapColumn;

        private readonly List<TextChange> _changes = new List<TextChange>();

        private readonly List<TextChange> _wordChanges = new List<TextChange>();

        private readonly StringBuilder _stringBuilder = new StringBuilder();

        private SourceText _text;

        private int _column;

        private int _externalIndent;

        private string _internalLinePrefix;

        private int _preserve;

        private int _internalIndent;

        private int _breakStart;
        private int _wordStart;
        private int _wordEnd;

        private BreakMode _breakMode;

        private DocXmlFormatter(OptionSet options)
        {
            _options = options;

            _useTabs = _options.GetOption(FormattingOptions.UseTabs, LanguageNames.CSharp);
            _tabSize = _options.GetOption(FormattingOptions.TabSize, LanguageNames.CSharp);
            _indentSize = _options.GetOption(FormattingOptions.IndentationSize, LanguageNames.CSharp);
            _newLine = _options.GetOption(FormattingOptions.NewLine, LanguageNames.CSharp) ?? Environment.NewLine;

            _wrapColumn = _options.GetOption(DocXmlFormattingOptions.WrapColumn) ?? _defaultWrapColumn;
        }

        /// <summary>
        /// Determines the changes necessary to format the XML documentation comments in a specified
        /// document.
        /// </summary>
        /// <param name="document">The document to format the XML documentation comments in.</param>
        /// <param name="options">The options that control formatting behavior.  If
        ///     <paramref name="options"/> is <see langword="null"/> then the options from the
        ///     workspace are used.</param>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        /// <returns>The changes that will format the XML documentation comments.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="document"/> is
        ///     <see langword="null"/>.</exception>
        public static IList<TextChange> Format(Document document, OptionSet options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (options == null)
                options = document.Project.Solution.Workspace.Options;

            var formatter = new DocXmlFormatter(options);

            var rootNode = document.GetSyntaxRootAsync(cancellationToken).Result;

            foreach (var node in DocXmlFinder.FindNodes(rootNode))
                formatter.Format(node, cancellationToken);

            return formatter._changes;
        }

        /// <summary>
        /// Determines the changes necessary to format the XML documentation comments that overlap a
        /// specified span of a specified document.
        /// </summary>
        /// <param name="document">The document to format the XML documentation comments in.</param>
        /// <param name="span">The span to format in the document.  XML documentation comments that
        ///     overlap the span will be formatted.</param>
        /// <param name="options">The options that control formatting behavior.  If
        ///     <paramref name="options"/> is <see langword="null"/> then the options from the
        ///     workspace are used.</param>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        /// <returns>The changes that will format the XML documentation comments.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="document"/> is
        ///     <see langword="null"/>.</exception>
        public static IList<TextChange> Format(Document document, TextSpan span, OptionSet options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (options == null)
                options = document.Project.Solution.Workspace.Options;

            var formatter = new DocXmlFormatter(options);

            var rootNode = document.GetSyntaxRootAsync(cancellationToken).Result;

            foreach (var node in DocXmlFinder.FindNodes(rootNode, span))
                formatter.Format(node, cancellationToken);

            return formatter._changes;
        }

        /// <summary>
        /// Gets the changes necessary to format a specified documentation comment node.
        /// </summary>
        /// <param name="node">The documentation comment node to format.</param>
        /// <param name="options">The options that control formatting behavior.</param>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        /// <returns>The changes that will format the XML documentation comment.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is
        ///     <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="options"/> is
        ///     <see langword="null"/>.</exception>
        public static IList<TextChange> Format(DocumentationCommentTriviaSyntax node, OptionSet options, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var formatter = new DocXmlFormatter(options);
            formatter.Format(node, cancellationToken);
            return formatter._changes;
        }

        private static string GetInternalLinePrefix(DocumentationCommentTriviaSyntax node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.SingleLineDocumentationCommentTrivia:
                {
                    var firstToken = node.GetFirstToken();

                    char firstChar;

                    if (!TryGetFirstChar(firstToken, out firstChar))
                        return string.Empty;

                    if (!IsWhitespace(firstChar))
                        return string.Empty;

                    // Check for a matching whitespace character on every line.
                    foreach (var token in node.DescendantTokens())
                    {
                        if (token.Kind() == SyntaxKind.XmlTextLiteralNewLineToken)
                        {
                            var nextToken = token.GetNextToken();

                            // If the next token is beyond the end of the comment then the new
                            // line was the end of the comment.
                            if (!node.Span.Contains(nextToken.Span))
                                break;

                            if (!TryGetFirstChar(nextToken, out char c) || c != firstChar)
                                return string.Empty;
                        }
                    }

                    return firstChar.ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
                case SyntaxKind.MultiLineDocumentationCommentTrivia:
                {
                    string whitespace = null;

                    // Check for a matching whitespace prefix on every line after the first.
                    foreach (var token in node.DescendantTokens())
                    {
                        if (token.Kind() == SyntaxKind.XmlTextLiteralNewLineToken)
                        {
                            var nextToken = token.GetNextToken();

                            // If the next token is beyond the end of the comment then the new
                            // line was the end of the comment.
                            if (!node.Span.Contains(nextToken.Span))
                                break;

                            string text;

                            if (!TryGetLiteralText(nextToken, out text))
                                return string.Empty;

                            if (whitespace == null)
                            {
                                // The whitespace prefix is the leading whitespace of the text.
                                for (int i = 0; i < text.Length; ++i)
                                    if (!IsWhitespace(text[i]))
                                        text = text.Substring(0, i - 1);
                                whitespace = text;
                            }
                            else if (!text.StartsWith(whitespace, StringComparison.Ordinal))
                            {
                                // The new whitespace prefix is the common prefix of the text
                                // and the old whitespace prefix.
                                for (int i = 0; i < text.Length; ++i)
                                    if (text[i] != whitespace[i])
                                        text = text.Substring(0, i - 1);
                                whitespace = text;
                            }
                        }
                    }

                    return whitespace ?? string.Empty;
                }
                default:
                    throw new InvalidOperationException("Unreachable.");
            }

            bool TryGetFirstChar(SyntaxToken token, out char c)
            {
                string text;

                if (!TryGetLiteralText(token, out text) ||
                    text.Length < 1)
                {
                    c = default(char);
                    return false;
                }

                c = text[0];
                return true;
            }

            bool TryGetLiteralText(SyntaxToken token, out string text)
            {
                if (token.Kind() != SyntaxKind.XmlTextLiteralToken)
                {
                    text = default(string);
                    return false;
                }

                text = token.ToString();
                return true;
            }
        }

        private static bool IsWhitespace(char c)
        {
            switch (char.GetUnicodeCategory(c))
            {
                case System.Globalization.UnicodeCategory.SpaceSeparator:
                    return true;
                case System.Globalization.UnicodeCategory.Control:
                    return c == '\t' || c == '\v' || c == '\f';
                default:
                    return false;
            }
        }

        private static int GetColumnFromSourceTextWithChanges(SourceText text, TextSpan span, List<TextChange> changes, int column, int tabSize)
        {
            int position = span.Start;

            foreach (var change in changes)
            {
                Debug.Assert(change.Span.Start >= position);
                Debug.Assert(change.Span.End <= span.End);
                Debug.Assert(change.NewText != null);

                var newTextLength = change.NewText.Length;

                if (change.Span.Length == 0 && newTextLength == 0)
                    continue;

                if (position < change.Span.Start)
                    column = GetColumnFromText(text.ToString(TextSpan.FromBounds(position, change.Span.Start)), column, tabSize);

                if (newTextLength > 0)
                    column = GetColumnFromText(change.NewText, column, tabSize);

                position = change.Span.End;
            }

            if (position < span.End)
                column = GetColumnFromText(text.ToString(TextSpan.FromBounds(position, span.End)), column, tabSize);

            return column;
        }

        private void Format(DocumentationCommentTriviaSyntax node, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Only single line comments are supported.
            if (node.Kind() != SyntaxKind.SingleLineDocumentationCommentTrivia)
                return;

            _text = node.SyntaxTree.GetText(cancellationToken);

            _column = GetInitialColumn(node.GetFirstToken(), _tabSize, cancellationToken);
            _externalIndent = _column;

            _internalLinePrefix = GetInternalLinePrefix(node);
            _internalIndent = 0;

            _breakStart = node.FullSpan.Start;
            _wordStart = _breakStart;
            _wordEnd = _breakStart;
            _breakMode = BreakMode.FirstBreak;

            FormatXmlNodes(node.Content);
            BreakFinal(node.EndOfComment);
        }

        private void FormatXmlNodes(SyntaxList<XmlNodeSyntax> nodes)
        {
            foreach (var node in nodes)
                FormatXmlNode(node);
        }

        private void FormatXmlNode(XmlNodeSyntax node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.XmlElement:
                    FormatXmlElement((XmlElementSyntax)node);
                    break;
                case SyntaxKind.XmlEmptyElement:
                    FormatXmlEmptyElement((XmlEmptyElementSyntax)node);
                    break;
                case SyntaxKind.XmlText:
                    FormatXmlText((XmlTextSyntax)node);
                    break;
                case SyntaxKind.XmlCDataSection:
                    FormatXmlCDataSection((XmlCDataSectionSyntax)node);
                    break;
                case SyntaxKind.XmlComment:
                    FormatXmlComment((XmlCommentSyntax)node);
                    break;
                case SyntaxKind.XmlProcessingInstruction:
                    FormatXmlProcessingInstruction((XmlProcessingInstructionSyntax)node);
                    break;
                default:
                    FormatUnknown(node);
                    break;
            }
        }

        private void FormatXmlElement(XmlElementSyntax node)
        {
            var name = (!node.StartTag.Name.IsMissing ? node.StartTag.Name : node.EndTag.Name).ToString();
            ElementFormatting formatting;
            _elements.TryGetValue(name, out formatting);

            FormatXmlElementStartTag(node.StartTag, formatting);
            FormatXmlNodes(node.Content);
            FormatXmlElementEndTag(node.EndTag, formatting);
        }

        private void FormatXmlElementStartTag(XmlElementStartTagSyntax node, ElementFormatting formatting)
        {
            if (formatting.HasFlag(ElementFormatting.Block))
                Break(BreakMode.LineBreak);

            if (!node.IsMissing)
            {
                // TODO: Format insides of tag; queuing changes.
                AddWord(node.Span);
            }

            if (formatting.HasFlag(ElementFormatting.SnugStart))
                Break(BreakMode.SuppressWordBreak);
            else if (formatting.HasFlag(ElementFormatting.Block))
                Break(BreakMode.LineBreak);

            if (formatting.HasFlag(ElementFormatting.Indent))
                _internalIndent += _indentSize;

            if (formatting.HasFlag(ElementFormatting.Preserve))
                _preserve += 1;
        }

        private void FormatXmlElementEndTag(XmlElementEndTagSyntax node, ElementFormatting formatting)
        {
            if (formatting.HasFlag(ElementFormatting.SnugEnd))
                Break(BreakMode.SuppressWordBreak);
            else if (formatting.HasFlag(ElementFormatting.Block))
                Break(BreakMode.LineBreak);

            if (formatting.HasFlag(ElementFormatting.Indent) && !formatting.HasFlag(ElementFormatting.SnugEnd))
                _internalIndent -= _indentSize;

            if (!node.IsMissing)
            {
                // TODO: Format insides of tag; queuing changes.
                AddWord(node.Span);
            }

            if (formatting.HasFlag(ElementFormatting.Preserve))
            {
                _breakMode |= BreakMode.Preserve;
                _preserve -= 1;
            }

            if (formatting.HasFlag(ElementFormatting.Block))
                Break(BreakMode.LineBreak);

            if (formatting.HasFlag(ElementFormatting.Indent) && formatting.HasFlag(ElementFormatting.SnugEnd))
                _internalIndent -= _indentSize;
        }

        private void FormatXmlEmptyElement(XmlEmptyElementSyntax node)
        {
            var name = node.Name.ToString();
            ElementFormatting formatting;
            _elements.TryGetValue(name, out formatting);

            if (formatting.HasFlag(ElementFormatting.Block))
                Break(BreakMode.LineBreak);

            if (!node.IsMissing)
            {
                // TODO: Format insides of tag; queuing changes.
                AddWord(node.Span);
            }

            if (formatting.HasFlag(ElementFormatting.Block))
                Break(BreakMode.LineBreak);
        }

        private void FormatXmlText(XmlTextSyntax node)
        {
            FormatTextTokens(node.TextTokens);
        }

        private void FormatXmlCDataSection(XmlCDataSectionSyntax node)
        {
            AddWord(node.StartCDataToken.Span);

            FormatTextTokens(node.TextTokens);

            AddWord(node.EndCDataToken.Span);
        }

        private void FormatXmlComment(XmlCommentSyntax node)
        {
            AddWord(node.LessThanExclamationMinusMinusToken.Span);

            FormatTextTokens(node.TextTokens);

            AddWord(node.MinusMinusGreaterThanToken.Span);
        }

        private void FormatXmlProcessingInstruction(XmlProcessingInstructionSyntax node)
        {
            Break(BreakMode.LineBreak);

            AddWord(node.Span);

            Break(BreakMode.LineBreak);
        }

        private void FormatUnknown(XmlNodeSyntax node)
        {
            AddWord(node.Span);
        }

        private void FormatTextTokens(SyntaxTokenList textTokens)
        {
            foreach (var token in textTokens)
            {
                if (token.Kind() == SyntaxKind.XmlTextLiteralNewLineToken)
                    continue;

                var tokenText = token.Text;

                int wordStart = 0;
                var sentenceSpace = false;

                for (int i = 0; i < tokenText.Length; ++i)
                {
                    if (IsWhitespace(tokenText[i]))
                    {
                        // Preserve one extra space after sentence end.
                        if (sentenceSpace && (i + 1 < tokenText.Length ? IsWhitespace(tokenText[i + 1]) : token.GetNextToken().Kind() == SyntaxKind.XmlTextLiteralNewLineToken))
                        {
                            sentenceSpace = false;
                            continue;
                        }

                        if (wordStart < i)
                            AddWord(new TextSpan(token.SpanStart + wordStart, i - wordStart));

                        wordStart = i + 1;
                        sentenceSpace = false;
                    }
                    else if ("!.:?".IndexOf(tokenText[i]) >= 0)
                    {
                        sentenceSpace = true;
                    }
                    else if (char.IsLetterOrDigit(tokenText[i]))
                    {
                        sentenceSpace = false;
                    }
                }

                if (wordStart < tokenText.Length)
                    AddWord(new TextSpan(token.SpanStart + wordStart, tokenText.Length - wordStart));
            }
        }

        private void AddWord(TextSpan span)
        {
            Debug.Assert(span.Start >= _wordEnd);

            Debug.Assert(_breakStart <= _wordEnd);
            Debug.Assert(_wordStart <= _wordEnd);

            if (span.Length == 0)
                return;

            if (_breakStart == _wordEnd || span.Start > _wordEnd)
            {
                // This word span is separated from the existing word.
                Break();
                _wordStart = span.Start;
                _wordEnd = span.End;
            }
            else
            {
                // This word span extends the existing word.
                _wordEnd = span.End;
            }
        }

        private void Break(BreakMode nextBreakMode = BreakMode.None)
        {
            if (_breakStart == _wordEnd)
            {
                // No word since the last break.
                _breakMode |= nextBreakMode;
                return;
            }

            if (_preserve > 0)
                _breakMode |= BreakMode.Preserve;

            Debug.Assert(_breakStart <= _wordStart);
            Debug.Assert(_wordStart <= _wordEnd);

            // Calculate the length of the word.
            var wordNewTextLength = _wordEnd - _wordStart;
            foreach (var change in _wordChanges)
                wordNewTextLength += change.NewText.Length - change.Span.Length;

            if (_breakMode.HasFlag(BreakMode.Preserve))
            {
                // Keep the old break.
                var breakSpan = TextSpan.FromBounds(_breakStart, _wordStart);
                var breakText = GetText(breakSpan);

                // Recompute column after the break.
                _column = GetColumnFromText(breakText, _column, _tabSize);
            }
            else
            {
                // Construct the break.
                string breakNewText = BuildBreakText(wordNewTextLength);

                // Recompute column after the break.
                _column = GetColumnFromText(breakNewText, _column, _tabSize);

                // Add the break change.
                var breakSpan = TextSpan.FromBounds(_breakStart, _wordStart);
                var breakText = GetText(breakSpan);
                if (breakNewText != breakText)
                    _changes.Add(new TextChange(breakSpan, breakNewText));
            }

            // Recompute column after the word.
            _column = GetColumnFromSourceTextWithChanges(_text, TextSpan.FromBounds(_wordStart, _wordEnd), _wordChanges, _column, _tabSize);

            // Add the word changes.
            _changes.AddRange(_wordChanges);
            _wordChanges.Clear();

            // Prepare for next word.
            _breakStart = _wordEnd;
            _breakMode = nextBreakMode;
        }

        private void BreakFinal(SyntaxToken endOfComment)
        {
            Break();

            _wordStart = endOfComment.FullSpan.End;
            _wordEnd = _wordStart;

            Debug.Assert(_breakStart <= _wordEnd);

            Debug.Assert(_wordChanges.Count == 0);

            // Construct the break.
            string breakNewText = BuildFinalBreakText();

            // Recompute column after the break.
            _column = GetColumnFromText(breakNewText, _column, _tabSize);

            // Add the break change.
            var breakSpan = TextSpan.FromBounds(_breakStart, _wordStart);
            var breakText = GetText(breakSpan);
            if (breakNewText != breakText)
                _changes.Add(new TextChange(breakSpan, breakNewText));
        }

        private string BuildBreakText(int wordNewTextLength)
        {
            string breakNewText;

            if (_breakMode.HasFlag(BreakMode.FirstBreak))
            {
                breakNewText = _stringBuilder
                    .Clear()
                    .Append("///")
                    .Append(_internalLinePrefix)
                    .Append(' ', _internalIndent)
                    .ToString();
            }
            else
            {
                breakNewText = _breakMode.HasFlag(BreakMode.SuppressWordBreak)
                    ? string.Empty
                    : " ";

                // If the word will exceed the wrap column then the break is a line break.
                if (_column + breakNewText.Length + wordNewTextLength > _wrapColumn ||
                    _breakMode.HasFlag(BreakMode.LineBreak))
                {
                    _stringBuilder
                        .Clear()
                        .Append(_newLine);
                    if (_useTabs)
                        _stringBuilder
                            .Append('\t', _externalIndent / _tabSize)
                            .Append(' ', _externalIndent % _tabSize);
                    else
                        _stringBuilder
                            .Append(' ', _externalIndent);
                    breakNewText = _stringBuilder
                        .Append("///")
                        .Append(_internalLinePrefix)
                        .Append(' ', _internalIndent)
                        .ToString();
                }
            }

            return breakNewText;
        }

        private string BuildFinalBreakText()
        {
            string breakNewText;

            if (_breakMode.HasFlag(BreakMode.FirstBreak))
            {
                breakNewText = _stringBuilder
                    .Clear()
                    .Append("///")
                    .Append(_internalLinePrefix)
                    .Append(_newLine)
                    .ToString();
            }
            else
            {
                breakNewText = _newLine;
            }

            return breakNewText;
        }

        private string GetText(TextSpan span)
        {
            return _text.ToString(span);
        }

        /// <summary>
        /// The XML documentation comment element formatting control flags.
        /// </summary>
        [Flags]
        internal enum ElementFormatting
        {
            /// <summary>
            /// The tags of the element are not necessarily broken from the words before or after.
            /// </summary>
            Inline = 0,

            /// <summary>
            /// The tags of the element are broken from the words before and after using line
            /// breaks.
            /// </summary>
            Block = 1,

            /// <summary>
            /// Any break after the start tag of the element is suppressed.
            /// </summary>
            SnugStart = 2,

            /// <summary>
            /// Any break before the end tag of the element is suppressed.
            /// </summary>
            SnugEnd = 4,

            /// <summary>
            /// Any break after the start tag or before the end tag of the element is suppressed.
            /// </summary>
            Snug = 6,

            /// <summary>
            /// The content of the element is indented.
            /// </summary>
            Indent = 8,

            /// <summary>
            /// The breaks in the content of the element are not modified.
            /// </summary>
            Preserve = 16,
        }

        [Flags]
        private enum BreakMode
        {
            None = 0,
            FirstBreak = 1,
            LineBreak = 2,
            SuppressWordBreak = 4,
            Preserve = 8,
        }

        private sealed class DocXmlFinder : CSharpSyntaxWalker
        {
            private readonly int _start;
            private readonly int _end;

            private readonly List<DocumentationCommentTriviaSyntax> _nodes = new List<DocumentationCommentTriviaSyntax>();

            private DocXmlFinder(TextSpan selectionSpan)
                : base(SyntaxWalkerDepth.StructuredTrivia)
            {
                _start = selectionSpan.Start;
                _end = selectionSpan.End;
            }

            public static IEnumerable<DocumentationCommentTriviaSyntax> FindNodes(SyntaxNode rootNode)
            {
                // Find the DocXML nodes.
                var syntaxWalker = new DocXmlFinder(rootNode.FullSpan);
                syntaxWalker.Visit(rootNode);

                return syntaxWalker._nodes;
            }

            public static IEnumerable<DocumentationCommentTriviaSyntax> FindNodes(SyntaxNode node, TextSpan span)
            {
                // Find the node that encloses the selection span.
                var enclosingNode = node.FindNode(span, getInnermostNodeForTie: true);

                // Find the DocXML nodes that overlap the selection span.
                var syntaxWalker = new DocXmlFinder(span);
                syntaxWalker.Visit(enclosingNode);

                return syntaxWalker._nodes;
            }

            public override void Visit(SyntaxNode node)
            {
                if (node != null)
                {
                    // Skip nodes that don't overlap span.
                    var nodeFullSpan = node.FullSpan;
                    if (nodeFullSpan.Start >= _end || nodeFullSpan.End <= _start)
                        return;
                }

                base.Visit(node);
            }

            public override void VisitDocumentationCommentTrivia(DocumentationCommentTriviaSyntax node)
            {
                _nodes.Add(node);
            }
        }
    }
}

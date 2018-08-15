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

        private readonly Queue<TextChange> _queuedChanges = new Queue<TextChange>();

        private readonly StringBuilder _breakText = new StringBuilder();

        private readonly StringBuilder _stringBuilder = new StringBuilder();

        private SourceText _text;

        private int _column;

        private int _exteriorIndent;

        private string _interiorLinePrefix;

        private int _preserve;

        private int _interiorIndent;

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

        private static string GetInteriorLinePrefix(DocumentationCommentTriviaSyntax node)
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

        private static int GetColumnFromSourceTextWithChanges(SourceText text, TextSpan span, IEnumerable<TextChange> changes, int column, int tabSize)
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
            _exteriorIndent = _column;

            _interiorLinePrefix = GetInteriorLinePrefix(node);
            _interiorIndent = 0;

            _breakStart = node.FullSpan.Start;
            _wordStart = _breakStart;
            _wordEnd = _wordStart;
            _breakMode = BreakMode.FirstBreak;

            FormatXmlNodes(node.Content);
            BreakFinal(node.EndOfComment);
        }

        private void FormatXmlNodes(SyntaxList<XmlNodeSyntax> nodes)
        {
            foreach (var node in nodes)
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

            if (node.HasLeadingTrivia)
                AddTrivia(node.GetLeadingTrivia());

            FormatElementStartTag(node);

            if (formatting.HasFlag(ElementFormatting.Preserve))
                _preserve += 1;

            if (node.HasTrailingTrivia)
                AddTrivia(node.GetTrailingTrivia());

            if (formatting.HasFlag(ElementFormatting.SnugStart))
                Break(BreakMode.SuppressWordBreak);
            else if (formatting.HasFlag(ElementFormatting.Block))
                Break(BreakMode.LineBreak);

            if (formatting.HasFlag(ElementFormatting.Indent))
                _interiorIndent += _indentSize;
        }

        private void FormatXmlElementEndTag(XmlElementEndTagSyntax node, ElementFormatting formatting)
        {
            if (formatting.HasFlag(ElementFormatting.SnugEnd))
                Break(BreakMode.SuppressWordBreak);
            else if (formatting.HasFlag(ElementFormatting.Block))
                Break(BreakMode.LineBreak);

            // A snug-end element normally causes unindentation to happen after the end tag, but
            // there is a special case for when the content of a snug block element forced a line
            // break before the end tag (presumably because the content was a block element).
            // This makes a snug block element that contains a block element behave like a block
            // element.
            bool unsnug = formatting.HasFlag(ElementFormatting.Block | ElementFormatting.Snug) && _breakMode.HasFlag(BreakMode.LineBreak);
            bool unindentAfterTag = formatting.HasFlag(ElementFormatting.SnugEnd) && !unsnug;

            if (formatting.HasFlag(ElementFormatting.Indent) && !unindentAfterTag)
                _interiorIndent -= _indentSize;

            if (node.HasLeadingTrivia)
                AddTrivia(node.GetLeadingTrivia());

            if (formatting.HasFlag(ElementFormatting.Preserve))
                _preserve -= 1;

            FormatElementEndTag(node);

            if (node.HasTrailingTrivia)
                AddTrivia(node.GetTrailingTrivia());

            if (formatting.HasFlag(ElementFormatting.Block))
                Break(BreakMode.LineBreak);

            if (formatting.HasFlag(ElementFormatting.Indent) && unindentAfterTag)
                _interiorIndent -= _indentSize;
        }

        private void FormatXmlEmptyElement(XmlEmptyElementSyntax node)
        {
            var name = node.Name.ToString();
            ElementFormatting formatting;
            _elements.TryGetValue(name, out formatting);

            if (formatting.HasFlag(ElementFormatting.Block))
                Break(BreakMode.LineBreak);

            if (node.HasLeadingTrivia)
                AddTrivia(node.GetLeadingTrivia());

            FormatEmptyElementTag(node);

            if (node.HasTrailingTrivia)
                AddTrivia(node.GetTrailingTrivia());

            if (formatting.HasFlag(ElementFormatting.Block))
                Break(BreakMode.LineBreak);
        }

        private void FormatXmlText(XmlTextSyntax node)
        {
            FormatTextTokens(node.TextTokens);
        }

        private void FormatXmlCDataSection(XmlCDataSectionSyntax node)
        {
            AddWordAndTrivia(node.StartCDataToken);

            FormatTextTokens(node.TextTokens);

            AddWordAndTrivia(node.EndCDataToken);
        }

        private void FormatXmlComment(XmlCommentSyntax node)
        {
            AddWordAndTrivia(node.LessThanExclamationMinusMinusToken);

            FormatTextTokens(node.TextTokens);

            AddWordAndTrivia(node.MinusMinusGreaterThanToken);
        }

        private void FormatXmlProcessingInstruction(XmlProcessingInstructionSyntax node)
        {
            Break(BreakMode.LineBreak);

            AddWordAndTrivia(node);

            Break(BreakMode.LineBreak);
        }

        private void FormatUnknown(XmlNodeSyntax node)
        {
            Debug.Assert(false);

            AddWordAndTrivia(node);
        }

        private void FormatTextTokens(SyntaxTokenList textTokens)
        {
            foreach (var token in textTokens)
            {
                if (token.HasLeadingTrivia)
                    AddTrivia(token.LeadingTrivia);

                var tokenText = token.Text;

                int wordStart = 0;
                int breakStart = 0;
                var allowSentenceSpace = false;

                for (int i = 0; i < tokenText.Length; ++i)
                {
                    char c = tokenText[i];

                    var isSpace = IsWhitespace(c) || c == '\r' || c == '\n';

                    // Preserve one extra space after sentence end.
                    if (isSpace && allowSentenceSpace)
                    {
                        char nextC = i + 1 < tokenText.Length
                            ? tokenText[i + 1]
                            : token.Span.End < _text.Length
                                ? _text[token.Span.End]
                                : ' ';

                        if (IsWhitespace(nextC) || nextC == '\r' || nextC == '\n')
                        {
                            isSpace = false;
                            allowSentenceSpace = false;
                        }
                    }

                    if (isSpace)
                    {
                        if (wordStart < i)
                            AddWord(new TextSpan(token.SpanStart + wordStart, i - wordStart));

                        wordStart = i + 1;
                        allowSentenceSpace = false;
                    }
                    else
                    {
                        if (breakStart < i)
                            AddBreak(new TextSpan(token.SpanStart + breakStart, i - breakStart));

                        breakStart = i + 1;
                        if ("!.:?".IndexOf(c) >= 0)
                        {
                            allowSentenceSpace = true;
                        }
                        else if (char.IsLetterOrDigit(c))
                        {
                            allowSentenceSpace = false;
                        }
                    }
                }

                if (wordStart < tokenText.Length)
                    AddWord(new TextSpan(token.SpanStart + wordStart, tokenText.Length - wordStart));
                if (breakStart < tokenText.Length)
                    AddBreak(new TextSpan(token.SpanStart + breakStart, tokenText.Length - breakStart));

                if (token.HasTrailingTrivia)
                    AddTrivia(token.TrailingTrivia);
            }
        }

        private void FormatElementStartTag(XmlElementStartTagSyntax node)
        {
            if (node.ContainsSkippedText)
            {
                AddWord(node.Span);
            }
            else
            {
                AddWord(node.LessThanToken.Span);

                EnqueueTrailingTriviaChange(node.LessThanToken, string.Empty);

                EnqueueLeadingTriviaChange(node.Name, string.Empty);

                FormatTagName(node.Name);

                EnqueueTrailingTriviaChange(node.Name, string.Empty);

                foreach (var attribute in node.Attributes)
                {
                    EnqueueLeadingTriviaChange(attribute, " ");

                    FormatTagAttribute(attribute);

                    EnqueueTrailingTriviaChange(attribute, string.Empty);
                }

                EnqueueLeadingTriviaChange(node.GreaterThanToken, string.Empty);

                AddWord(node.GreaterThanToken.Span);
            }
        }

        private void FormatElementEndTag(XmlElementEndTagSyntax node)
        {
            if (node.ContainsSkippedText)
            {
                AddWord(node.Span);
            }
            else
            {
                AddWord(node.LessThanSlashToken.Span);

                EnqueueTrailingTriviaChange(node.LessThanSlashToken, string.Empty);

                EnqueueLeadingTriviaChange(node.Name, string.Empty);

                FormatTagName(node.Name);

                EnqueueTrailingTriviaChange(node.Name, string.Empty);

                EnqueueLeadingTriviaChange(node.GreaterThanToken, string.Empty);

                AddWord(node.GreaterThanToken.Span);
            }
        }

        private void FormatEmptyElementTag(XmlEmptyElementSyntax node)
        {
            if (node.ContainsSkippedText)
            {
                AddWord(node.Span);
            }
            else
            {
                AddWord(node.LessThanToken.Span);

                EnqueueTrailingTriviaChange(node.LessThanToken, string.Empty);

                EnqueueLeadingTriviaChange(node.Name, string.Empty);

                FormatTagName(node.Name);

                EnqueueTrailingTriviaChange(node.Name, string.Empty);

                foreach (var attribute in node.Attributes)
                {
                    EnqueueLeadingTriviaChange(attribute, " ");

                    FormatTagAttribute(attribute);

                    EnqueueTrailingTriviaChange(attribute, string.Empty);
                }

                EnqueueLeadingTriviaChange(node.SlashGreaterThanToken, string.Empty);

                AddWord(node.SlashGreaterThanToken.Span);
            }
        }

        private void FormatTagName(XmlNameSyntax node)
        {
            if (node.Prefix != null)
            {
                FormatTagNamePrefix(node.Prefix);

                EnqueueTrailingTriviaChange(node.Prefix, string.Empty);

                EnqueueLeadingTriviaChange(node.LocalName, string.Empty);
            }

            AddWord(node.LocalName.Span);
        }

        private void FormatTagNamePrefix(XmlPrefixSyntax node)
        {
            AddWord(node.Prefix.Span);

            EnqueueTrailingTriviaChange(node.Prefix, string.Empty);

            EnqueueLeadingTriviaChange(node.ColonToken, string.Empty);

            AddWord(node.ColonToken.Span);
        }

        private void FormatTagAttribute(XmlAttributeSyntax node)
        {
            FormatTagName(node.Name);

            EnqueueTrailingTriviaChange(node.Name, string.Empty);

            EnqueueLeadingTriviaChange(node.EqualsToken, string.Empty);

            AddWord(node.EqualsToken.Span);

            EnqueueTrailingTriviaChange(node.EqualsToken, string.Empty);

            EnqueueLeadingTriviaChange(node.StartQuoteToken, string.Empty);

            AddWord(node.StartQuoteToken.Span);

            switch (node.Kind())
            {
                case SyntaxKind.XmlCrefAttribute:
                    FormatTagCrefAttribute((XmlCrefAttributeSyntax)node);
                    break;
                case SyntaxKind.XmlNameAttribute:
                    FormatTagNameAttribute((XmlNameAttributeSyntax)node);
                    break;
                case SyntaxKind.XmlTextAttribute:
                    FormatTagTextAttribute((XmlTextAttributeSyntax)node);
                    break;
                default:
                    FormatTagUnknownAttribute(node);
                    break;
            }

            AddWord(node.EndQuoteToken.Span);
        }

        private void FormatTagCrefAttribute(XmlCrefAttributeSyntax node)
        {
            EnqueueTrailingTriviaChange(node.StartQuoteToken, string.Empty);

            EnqueueLeadingTriviaChange(node.Cref, string.Empty);

            AddWord(node.Cref.Span);

            EnqueueTrailingTriviaChange(node.Cref, string.Empty);

            EnqueueLeadingTriviaChange(node.EndQuoteToken, string.Empty);
        }

        private void FormatTagNameAttribute(XmlNameAttributeSyntax node)
        {
            EnqueueTrailingTriviaChange(node.StartQuoteToken, string.Empty);

            EnqueueLeadingTriviaChange(node.Identifier, string.Empty);

            AddWord(node.Identifier.Span);

            EnqueueTrailingTriviaChange(node.Identifier, string.Empty);

            EnqueueLeadingTriviaChange(node.EndQuoteToken, string.Empty);
        }

        private void FormatTagTextAttribute(XmlTextAttributeSyntax node)
        {
            if (node.StartQuoteToken.HasTrailingTrivia)
                AddTrivia(node.StartQuoteToken.TrailingTrivia);

            foreach (var token in node.DescendantTokens(TextSpan.FromBounds(node.StartQuoteToken.Span.End, node.EndQuoteToken.Span.Start)))
            {
                if (token.HasLeadingTrivia)
                    AddTrivia(token.LeadingTrivia);

                switch (token.Kind())
                {
                    case SyntaxKind.XmlTextLiteralNewLineToken:
                        Break(BreakMode.Preserve);
                        AddBreakAndTrivia(token);
                        break;
                    default:
                        AddWord(token.Span);
                        break;
                }

                if (token.HasTrailingTrivia)
                    AddTrivia(token.TrailingTrivia);
            }

            if (node.EndQuoteToken.HasLeadingTrivia)
                AddTrivia(node.EndQuoteToken.LeadingTrivia);
        }

        private void FormatTagUnknownAttribute(XmlAttributeSyntax node)
        {
            Debug.Assert(false);

            AddWord(TextSpan.FromBounds(node.StartQuoteToken.Span.End, node.EndQuoteToken.Span.Start));
        }

        private void EnqueueLeadingTriviaChange(SyntaxToken token, string newText)
        {
            if (token.HasLeadingTrivia)
                EnqueueChange(token.LeadingTrivia.Span, newText);
        }

        private void EnqueueTrailingTriviaChange(SyntaxToken token, string newText)
        {
            if (token.HasTrailingTrivia)
                EnqueueChange(token.TrailingTrivia.Span, newText);
        }

        private void EnqueueLeadingTriviaChange(CSharpSyntaxNode node, string newText)
        {
            if (node.HasLeadingTrivia)
                EnqueueChange(node.GetLeadingTrivia().Span, newText);
        }

        private void EnqueueTrailingTriviaChange(CSharpSyntaxNode node, string newText)
        {
            if (node.HasTrailingTrivia)
                EnqueueChange(node.GetTrailingTrivia().Span, newText);
        }

        private void EnqueueChange(TextSpan span, string newText)
        {
            var text = GetText(span);
            if (newText != text)
                _queuedChanges.Enqueue(new TextChange(span, newText));
        }

        private void AddTrivia(SyntaxTriviaList trivia)
        {
            foreach (var trivium in trivia)
            {
                switch (trivium.Kind())
                {
                    case SyntaxKind.DocumentationCommentExteriorTrivia:
                        break;
                    case SyntaxKind.EndOfLineTrivia:
                    case SyntaxKind.WhitespaceTrivia:
                        AddBreakAndTrivia(trivium.Token);
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }
            }
        }

        private void AddWordAndTrivia(SyntaxToken token)
        {
            if (token.HasLeadingTrivia)
                AddTrivia(token.LeadingTrivia);

            AddWord(token.Span);

            if (token.HasTrailingTrivia)
                AddTrivia(token.TrailingTrivia);
        }

        private void AddWordAndTrivia(CSharpSyntaxNode node)
        {
            if (node.HasLeadingTrivia)
                AddTrivia(node.GetLeadingTrivia());

            AddWord(node.Span);

            if (node.HasTrailingTrivia)
                AddTrivia(node.GetTrailingTrivia());
        }

        private void AddWord(TextSpan span)
        {
            Debug.Assert(span.Start >= _wordEnd);

            Debug.Assert(_breakStart <= _wordStart);
            Debug.Assert(_wordStart <= _wordEnd);

            if (_wordStart == _wordEnd)
            {
                // There is no existing word.
                _wordStart = span.Start;
                _wordEnd = span.End;
            }
            else
            {
                // This word span extends the existing word.
                _wordEnd = span.End;
            }
        }

        private void AddBreakAndTrivia(SyntaxToken token)
        {
            if (token.HasLeadingTrivia)
                AddTrivia(token.LeadingTrivia);

            AddBreak(token.Span, token.ToString());

            if (token.HasTrailingTrivia)
                AddTrivia(token.TrailingTrivia);
        }

        private void AddBreak(TextSpan span, string text = null)
        {
            Debug.Assert(span.Start >= _wordEnd);

            Debug.Assert(_breakStart <= _wordStart);
            Debug.Assert(_wordStart <= _wordEnd);

            if (span.Length == 0)
                return;

            if (_wordStart != _wordEnd)
                Break();

            _wordStart = span.End;
            _wordEnd = _wordStart;

            Debug.Assert(text == null || text == GetText(span));

            _breakText.Append(text ?? GetText(span));
        }

        private void Break(BreakMode nextBreakMode = BreakMode.None)
        {
            Debug.Assert(_breakStart <= _wordStart);
            Debug.Assert(_wordStart <= _wordEnd);

            if (_preserve > 0)
                nextBreakMode |= BreakMode.Preserve;

            // Defer break if there has been no word since the last break.
            if (_wordStart == _wordEnd)
            {
                _breakMode |= nextBreakMode;
                return;
            }

            Debug.Assert(_breakStart <= _wordStart);
            Debug.Assert(_wordStart <= _wordEnd);

            // Dequeue changes applicable to the word.
            while (_queuedChanges.Count > 0)
            {
                var change = _queuedChanges.Peek();
                if (change.Span.Start >= _wordEnd)
                    break;
                Debug.Assert(change.Span.Start >= _wordStart);
                Debug.Assert(change.Span.End <= _wordEnd);
                _wordChanges.Add(_queuedChanges.Dequeue());
            }

            // Calculate the length of the word.
            var wordNewTextLength = _wordEnd - _wordStart;
            foreach (var change in _wordChanges)
                wordNewTextLength += change.NewText.Length - change.Span.Length;

            // Construct the break.
            string breakNewText = BuildBreakText(wordNewTextLength);

            // Recompute column after the break.
            _column = GetColumnFromText(breakNewText, _column, _tabSize);

            // Add the break change.
            var breakSpan = TextSpan.FromBounds(_breakStart, _wordStart);
            var breakText = GetText(breakSpan);
            if (breakNewText != breakText)
                _changes.Add(new TextChange(breakSpan, breakNewText));

            // Recompute column after the word.
            _column = GetColumnFromSourceTextWithChanges(_text, TextSpan.FromBounds(_wordStart, _wordEnd), _wordChanges, _column, _tabSize);

            // Add the word changes.
            _changes.AddRange(_wordChanges);
            _wordChanges.Clear();

            // Prepare for next word.
            _breakText.Clear();
            _breakStart = _wordEnd;
            _wordStart = _breakStart;
            _breakMode = nextBreakMode;
        }

        private void BreakFinal(SyntaxToken endOfComment)
        {
            Break();

            _wordStart = endOfComment.Span.End;
            _wordEnd = _wordStart;

            Debug.Assert(_breakStart <= _wordEnd);

            Debug.Assert(_queuedChanges.Count == 0);

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
                    .Append(_interiorLinePrefix)
                    .Append(' ', _interiorIndent)
                    .ToString();
            }
            else if (_breakMode.HasFlag(BreakMode.Preserve))
            {
                _stringBuilder.Clear();
                for (int i = 0; i < _breakText.Length; ++i)
                {
                    char c = _breakText[i];
                    switch (c)
                    {
                        case '\r':
                            break;
                        case '\n':
                            _stringBuilder
                                .Append(_newLine)
                                .AppendIndent(_exteriorIndent, _useTabs, _tabSize)
                                .Append("///");
                            break;
                        default:
                            _stringBuilder.Append(c);
                            break;
                    }
                }
                breakNewText = _stringBuilder.ToString();
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
                    breakNewText = _stringBuilder
                        .Clear()
                        .Append(_newLine)
                        .AppendIndent(_exteriorIndent, _useTabs, _tabSize)
                        .Append("///")
                        .Append(_interiorLinePrefix)
                        .Append(' ', _interiorIndent)
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
                    .Append(_interiorLinePrefix)
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
            /// Suppresses any word break and any self-induced line break after the start tag of the
            /// element.
            /// </summary>
            SnugStart = 2,

            /// <summary>
            /// Suppresses any word break and any self-induced line break before the end tag of the
            /// element.
            /// </summary>
            SnugEnd = 4,

            /// <summary>
            /// Suppresses any word breaks and any self-induced line breaks after the start tag and
            /// before the end tag of the element.
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

﻿// Copyright 2018 Carl Reinke
//
// This file is part of a library that is licensed under the terms of the GNU
// Lesser General Public License Version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

using Microsoft.CodeAnalysis;
using System;
using System.Threading;

namespace FormatDocComments
{
    /// <summary>
    /// Provides helpers for determining the column in a text editor.
    /// </summary>
    internal static class ColumnHelpers
    {
        /// <summary>
        /// Gets the column of the start of the full span of the specified token.
        /// </summary>
        /// <param name="token">The token to determine the initial column of.</param>
        /// <param name="tabSize">The size of the tab character in columns.</param>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        /// <returns>The column of the start of the full span of <paramref name="token"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="token"/> is not contained in a
        ///     <see cref="SyntaxTree"/>.</exception>
        public static int GetInitialColumn(SyntaxToken token, int tabSize, CancellationToken cancellationToken = default)
        {
            var syntaxTree = token.SyntaxTree;
            if (syntaxTree == null)
                throw new ArgumentException("Token must be contained in a tree.", nameof(token));
            var text = syntaxTree.GetText(cancellationToken);
            int start = token.FullSpan.Start;
            var line = text.Lines.GetLineFromPosition(start);
            return ConvertTabsToSpaces(line.ToString(), 0, start - line.Start, tabSize);
        }

        /// <summary>
        /// Gets the column of the end of the specified text.
        /// </summary>
        /// <param name="text">The text to determine the end column of.</param>
        /// <param name="initialColumn">The column of the start of <paramref name="text"/>.</param>
        /// <param name="tabSize">The size of the tab character in columns.</param>
        /// <returns>The column of the end of <paramref name="text"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is
        ///     <see langword="null"/>.</exception>
        public static int GetColumnFromText(string text, int initialColumn, int tabSize)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            int beginIndex = 0;
            int lastNewLineIndex = text.LastIndexOf('\n');
            if (lastNewLineIndex >= 0)
            {
                beginIndex = lastNewLineIndex + 1;
                initialColumn = 0;
            }
            return ConvertTabsToSpaces(text, beginIndex, text.Length, tabSize, initialColumn);
        }

        private static int ConvertTabsToSpaces(string text, int beginIndex, int endIndex, int tabSize, int initialColumn = 0)
        {
            int column = initialColumn;
            for (int i = beginIndex; i < endIndex; ++i)
                column += text[i] == '\t' ? tabSize - column % tabSize : 1;
            return column;
        }
    }
}

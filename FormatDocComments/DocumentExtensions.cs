// Copyright 2018 Carl Reinke
//
// This file is part of a library that is licensed under the terms of GNU Lesser
// General Public License version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FormatDocComments
{
    /// <summary>
    /// Provides extensions for <see cref="Document"/>.
    /// </summary>
    internal static class DocumentExtensions
    {
        /// <summary>
        /// Applies specified text changes to the document and returns the new document.
        /// </summary>
        /// <param name="document">The document that the changes will be applied to.</param>
        /// <param name="textChanges">The changes to apply to the document.</param>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        /// <returns>The new document with the text changes applied.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="document"/> is
        ///     <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="textChanges"/> is
        ///     <see langword="null"/>.</exception>
        public static async Task<Document> ApplyTextChangesAsync(this Document document, IEnumerable<TextChange> textChanges, CancellationToken cancellationToken = default)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            if (textChanges == null)
                throw new ArgumentNullException(nameof(textChanges));

            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var newText = text.WithChanges(textChanges);

            var newDocument = document.WithText(newText);

            var workspace = document.Project.Solution.Workspace;
            if (workspace.TryApplyChanges(newDocument.Project.Solution))
                return newDocument;

            return document;
        }
    }
}

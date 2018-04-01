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
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Threading;

namespace FormatDocXml
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
        public static Document ApplyTextChanges(this Document document, IEnumerable<TextChange> textChanges, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            if (textChanges == null)
                throw new ArgumentNullException(nameof(textChanges));

            var text = document.GetTextAsync(cancellationToken).Result;
            var newText = text.WithChanges(textChanges);

            var newDocument = document.WithText(newText);

            var workspace = document.Project.Solution.Workspace;
            if (workspace.TryApplyChanges(newDocument.Project.Solution))
                return newDocument;

            return document;
        }
    }
}

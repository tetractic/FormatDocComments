// Copyright 2022 Carl Reinke
//
// This file is part of a library that is licensed under the terms of the GNU
// Lesser General Public License Version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.CodingConventions;
using Microsoft.VisualStudio.Threading;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace FormatDocComments
{
    [Export]
    internal sealed class FormatDocCommentsService
    {
        private readonly JoinableTaskContext _joinableTaskContext;

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

        [Import(AllowDefault = true)]
        private readonly ICodingConventionsManager _codingConventionsManager;

#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

        [ImportingConstructor]
        public FormatDocCommentsService(JoinableTaskContext joinableTaskContext)
        {
            _joinableTaskContext = joinableTaskContext;
        }

        public async Task<Document> FormatDocCommentsInDocumentAsync(Document document, CancellationToken cancellationToken)
        {
            var options = document.Project.Solution.Workspace.Options;
            if (!options.GetOption(DocCommentFormattingOptions.WrapColumn).HasValue)
                options = options.WithChangedOption(DocCommentFormattingOptions.WrapColumn, await GetGuideColumnAsync(document.FilePath, cancellationToken).ConfigureAwait(false));

            var changes = await DocCommentFormatter.FormatAsync(document, options, cancellationToken).ConfigureAwait(false);

            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            return document.WithText(text.WithChanges(changes));
        }

        public async Task<Document> FormatDocCommentsInSelectionAsync(Document document, TextSpan selectionSpan, CancellationToken cancellationToken)
        {
            var options = document.Project.Solution.Workspace.Options;
            if (!options.GetOption(DocCommentFormattingOptions.WrapColumn).HasValue)
                options = options.WithChangedOption(DocCommentFormattingOptions.WrapColumn, await GetGuideColumnAsync(document.FilePath, cancellationToken).ConfigureAwait(false));

            var changes = await DocCommentFormatter.FormatAsync(document, selectionSpan, options, cancellationToken).ConfigureAwait(false);

            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            return document.WithText(text.WithChanges(changes));
        }

        private static int? GetMaxLineLength(ICodingConventionContext codingConventionContext)
        {
            if (codingConventionContext.CurrentConventions.TryGetConventionValue("max_line_length", out string maxLineLengthString) &&
                int.TryParse(maxLineLengthString, out int maxLineLength) && maxLineLength > 0)
            {
                return maxLineLength;
            }

            return null;
        }

        private static IEnumerable<int> GetGuideColumns(ICodingConventionContext codingConventionContext)
        {
            if (codingConventionContext.CurrentConventions.TryGetConventionValue("guidelines", out string guidelinesString))
            {
                foreach (string guidelineString in guidelinesString.Split(','))
                {
                    int length;
                    for (length = 0; length < guidelineString.Length; ++length)
                    {
                        if (guidelineString[length] != ' ')
                        {
                            length = guidelineString.IndexOf(' ', length);
                            if (length < 0)
                                length = guidelineString.Length;
                            break;
                        }
                    }
                    string columnString = guidelineString.Substring(0, length);

                    if (int.TryParse(columnString, out int column) && column > 0)
                        yield return column;
                }
            }
        }

        private async Task<int?> GetGuideColumnAsync(string filePath, CancellationToken cancellationToken)
        {
            if (_codingConventionsManager != null && filePath != null)
            {
                var codingConventionContext = await _codingConventionsManager.GetConventionContextAsync(filePath, cancellationToken).ConfigureAwait(false);
                if (codingConventionContext != null)
                {
                    int? column = GetMaxLineLength(codingConventionContext) ??
                                  GetGuideColumns(codingConventionContext).Max();
                    if (column.HasValue)
                        return column.Value;
                }
            }

            await _joinableTaskContext.Factory.SwitchToMainThreadAsync(cancellationToken);

            return TextEditorGuidesSettings.GetGuideColumns().Max();
        }
    }
}

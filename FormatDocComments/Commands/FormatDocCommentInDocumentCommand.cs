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
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Editor.Commanding;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace FormatDocComments.Commands
{
    [Export(typeof(CommandBindingDefinition))]
    [CommandBinding(PackageIds.CommandSetGuidString, PackageIds.FormatDocCommentsInDocumentCommandId, typeof(FormatDocCommentInDocumentCommandArgs))]

    [Export(typeof(ICommandHandler))]
    [Name(nameof(FormatDocCommentInDocumentCommand))]
    [ContentType("CSharp")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal sealed class FormatDocCommentInDocumentCommand : ICommandHandler<FormatDocCommentInDocumentCommandArgs>
    {
        private readonly JoinableTaskContext _joinableTaskContext;

        [Import(AllowDefault = true)]
        private readonly ICodingConventionsManager _codingConventionsManager;

        [ImportingConstructor]
        public FormatDocCommentInDocumentCommand(JoinableTaskContext joinableTaskContext)
        {
            _joinableTaskContext = joinableTaskContext;
        }

        public string DisplayName => "Format Documentation Comments in Document";

        public bool ExecuteCommand(FormatDocCommentInDocumentCommandArgs args, CommandExecutionContext executionContext)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            if (executionContext == null)
                throw new ArgumentNullException(nameof(executionContext));

            var textView = args.TextView;

            var snapshot = textView.TextSnapshot;

            var textBuffer = textView.TextBuffer;
            if (textBuffer.EditInProgress)
                return false;

            var document = snapshot.GetOpenDocumentInCurrentContextWithChanges();
            if (document == null)
                return false;

            _ = _joinableTaskContext.Factory.RunAsync(() => FormatDocCommentsInDocumentAsync(document, executionContext.OperationContext.UserCancellationToken));

            return true;
        }

        public CommandState GetCommandState(FormatDocCommentInDocumentCommandArgs args)
        {
            return CommandState.Available;
        }

        private async Task FormatDocCommentsInDocumentAsync(Document document, CancellationToken cancellationToken)
        {
            var options = document.Project.Solution.Workspace.Options;
            if (!options.GetOption(DocCommentFormattingOptions.WrapColumn).HasValue)
                options = options.WithChangedOption(DocCommentFormattingOptions.WrapColumn, await GetGuideColumnAsync(document.FilePath, cancellationToken).ConfigureAwait(false));

            var changes = await DocCommentFormatter.FormatAsync(document, options, cancellationToken).ConfigureAwait(false);
            _ = await document.ApplyTextChangesAsync(changes, cancellationToken).ConfigureAwait(false);
        }

        private async Task<int?> GetGuideColumnAsync(string filePath, CancellationToken cancellationToken)
        {
            if (_codingConventionsManager != null && filePath != null)
            {
                var codingConventionContext = await _codingConventionsManager.GetConventionContextAsync(filePath, cancellationToken).ConfigureAwait(false);
                if (codingConventionContext != null)
                {
                    int? column = CodingConventionSettings.GetMaxLineLength(codingConventionContext) ??
                                  CodingConventionSettings.GetGuideColumns(codingConventionContext).Max();
                    if (column.HasValue)
                        return column.Value;
                }
            }

            await _joinableTaskContext.Factory.SwitchToMainThreadAsync(cancellationToken);

            return TextEditorGuidesSettings.GetGuideColumns().Max();
        }
    }
}

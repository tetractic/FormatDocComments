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
    [CommandBinding(PackageIds.CommandSetGuidString, PackageIds.FormatDocCommentsInSelectionCommandId, typeof(FormatDocCommentInSelectionCommandArgs))]

    [Export(typeof(ICommandHandler))]
    [Name(nameof(FormatDocCommentInSelectionCommand))]
    [ContentType("CSharp")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal sealed class FormatDocCommentInSelectionCommand : ICommandHandler<FormatDocCommentInSelectionCommandArgs>
    {
        private readonly JoinableTaskContext _joinableTaskContext;

        [ImportingConstructor]
        public FormatDocCommentInSelectionCommand(JoinableTaskContext joinableTaskContext)
        {
            _joinableTaskContext = joinableTaskContext;
        }

        public string DisplayName => "Format Documentation Comments in Selection";

        public bool ExecuteCommand(FormatDocCommentInSelectionCommandArgs args, CommandExecutionContext executionContext)
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

            var selection = textView.Selection;

            if (selection.Mode != TextSelectionMode.Stream)
                return false;

            var startPosition = selection.Start.Position;
            var endPosition = selection.End.Position;

            if (startPosition == endPosition)
            {
                // Extend the caret to a selection.

                var line = textView.GetTextViewLineContainingBufferPosition(startPosition);

                if (startPosition != line.Start)
                    startPosition -= 1;
                if (endPosition != line.End)
                    endPosition += 1;
            }

            var selectionSpan = TextSpan.FromBounds(startPosition, endPosition);

            var document = snapshot.GetOpenDocumentInCurrentContextWithChanges();
            if (document == null)
                return false;

            _ = _joinableTaskContext.Factory.RunAsync(() => FormatDocCommentsInSelectionAsync(document, selectionSpan, executionContext.OperationContext.UserCancellationToken));

            return true;
        }

        public CommandState GetCommandState(FormatDocCommentInSelectionCommandArgs args)
        {
            return CommandState.Available;
        }

        private async Task FormatDocCommentsInSelectionAsync(Document document, TextSpan selectionSpan, CancellationToken cancellationToken)
        {
            var options = document.Project.Solution.Workspace.Options;
            if (!options.GetOption(DocCommentFormattingOptions.WrapColumn).HasValue)
                options = options.WithChangedOption(DocCommentFormattingOptions.WrapColumn, await GetGuideColumnAsync(cancellationToken).ConfigureAwait(false));

            var changes = await DocCommentFormatter.FormatAsync(document, selectionSpan, options, cancellationToken).ConfigureAwait(false);
            _ = await document.ApplyTextChangesAsync(changes, cancellationToken).ConfigureAwait(false);
        }

        private async Task<int?> GetGuideColumnAsync(CancellationToken cancellationToken)
        {
            await _joinableTaskContext.Factory.SwitchToMainThreadAsync(cancellationToken);

            return TextEditorGuidesSettings.GetGuideColumns().Max();
        }
    }
}

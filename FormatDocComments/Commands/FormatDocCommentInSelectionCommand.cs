// Copyright 2022 Carl Reinke
//
// This file is part of a library that is licensed under the terms of the GNU
// Lesser General Public License Version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Editor.Commanding;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;

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
        private readonly FormatDocCommentsService _formatDocCommentsService;
        private readonly JoinableTaskContext _joinableTaskContext;

        [ImportingConstructor]
        public FormatDocCommentInSelectionCommand(FormatDocCommentsService formatDocCommentsService, JoinableTaskContext joinableTaskContext)
        {
            _formatDocCommentsService = formatDocCommentsService;
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

            _ = _joinableTaskContext.Factory.RunAsync(async () =>
            {
                document = await _formatDocCommentsService.FormatDocCommentsInSelectionAsync(document, selectionSpan, executionContext.OperationContext.UserCancellationToken).ConfigureAwait(true);

                var solution = document.Project.Solution;
                return solution.Workspace.TryApplyChanges(solution);
            });

            return true;
        }

        public CommandState GetCommandState(FormatDocCommentInSelectionCommandArgs args)
        {
            return CommandState.Available;
        }
    }
}

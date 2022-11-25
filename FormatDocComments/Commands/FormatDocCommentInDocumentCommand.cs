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
    [CommandBinding(PackageIds.CommandSetGuidString, PackageIds.FormatDocCommentsInDocumentCommandId, typeof(FormatDocCommentInDocumentCommandArgs))]

    [Export(typeof(ICommandHandler))]
    [Name(nameof(FormatDocCommentInDocumentCommand))]
    [ContentType("CSharp")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal sealed class FormatDocCommentInDocumentCommand : ICommandHandler<FormatDocCommentInDocumentCommandArgs>
    {
        private readonly FormatDocCommentsService _formatDocCommentsService;
        private readonly JoinableTaskContext _joinableTaskContext;

        [ImportingConstructor]
        public FormatDocCommentInDocumentCommand(FormatDocCommentsService formatDocCommentsService, JoinableTaskContext joinableTaskContext)
        {
            _formatDocCommentsService = formatDocCommentsService;
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

            _ = _joinableTaskContext.Factory.RunAsync(async () =>
            {
                document = await _formatDocCommentsService.FormatDocCommentsInDocumentAsync(document, executionContext.OperationContext.UserCancellationToken).ConfigureAwait(true);

                var solution = document.Project.Solution;
                return solution.Workspace.TryApplyChanges(solution);
            });

            return true;
        }

        public CommandState GetCommandState(FormatDocCommentInDocumentCommandArgs args)
        {
            return CommandState.Available;
        }
    }
}

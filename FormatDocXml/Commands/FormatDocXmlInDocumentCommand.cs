//
// Copyright (C) 2018-2019  Carl Reinke
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

namespace FormatDocXml.Commands
{
    [Export(typeof(CommandBindingDefinition))]
    [CommandBinding(PackageIds.CommandSetGuidString, PackageIds.FormatDocXmlInDocumentCommandId, typeof(FormatDocXmlInDocumentCommandArgs))]

    [Export(typeof(ICommandHandler))]
    [Name(nameof(FormatDocXmlInDocumentCommand))]
    [ContentType("CSharp")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal sealed class FormatDocXmlInDocumentCommand : ICommandHandler<FormatDocXmlInDocumentCommandArgs>
    {
        private readonly JoinableTaskContext _joinableTaskContext;

        [ImportingConstructor]
        public FormatDocXmlInDocumentCommand(JoinableTaskContext joinableTaskContext)
        {
            _joinableTaskContext = joinableTaskContext;
        }

        public string DisplayName => "Format Documentation XML in Document";

        public bool ExecuteCommand(FormatDocXmlInDocumentCommandArgs args, CommandExecutionContext executionContext)
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

            _joinableTaskContext.Factory.RunAsync(() => FormatDocXmlInDocumentAsync(document, executionContext.OperationContext.UserCancellationToken));

            return true;
        }

        public CommandState GetCommandState(FormatDocXmlInDocumentCommandArgs args)
        {
            return CommandState.Available;
        }

        private async Task FormatDocXmlInDocumentAsync(Document document, CancellationToken cancellationToken)
        {
            var options = document.Project.Solution.Workspace.Options;
            if (!options.GetOption(DocXmlFormattingOptions.WrapColumn).HasValue)
                options = options.WithChangedOption(DocXmlFormattingOptions.WrapColumn, await GetGuideColumnAsync(cancellationToken).ConfigureAwait(false));

            var changes = await DocXmlFormatter.FormatAsync(document, options, cancellationToken).ConfigureAwait(false);
            await document.ApplyTextChangesAsync(changes, cancellationToken).ConfigureAwait(false);
        }

        private async Task<int?> GetGuideColumnAsync(CancellationToken cancellationToken)
        {
            await _joinableTaskContext.Factory.SwitchToMainThreadAsync(cancellationToken);

            return TextEditorGuidesSettings.GetGuideColumns().Max();
        }
    }
}

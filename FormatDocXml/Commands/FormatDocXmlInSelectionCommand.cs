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
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Editor.Commanding;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace FormatDocXml.Commands
{
    [Export(typeof(CommandBindingDefinition))]
    [CommandBinding(PackageIds.CommandSetGuidString, PackageIds.FormatDocXmlInSelectionCommandId, typeof(FormatDocXmlInSelectionCommandArgs))]

    [Export(typeof(ICommandHandler))]
    [Name(nameof(FormatDocXmlInSelectionCommand))]
    [ContentType("CSharp")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal sealed class FormatDocXmlInSelectionCommand : ICommandHandler<FormatDocXmlInSelectionCommandArgs>
    {
        public string DisplayName => "Format Documentation XML in Selection";

        public bool ExecuteCommand(FormatDocXmlInSelectionCommandArgs args, CommandExecutionContext executionContext)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            if (executionContext == null)
                throw new ArgumentNullException(nameof(executionContext));

            FormatDocXmlInSelectionAsync(args.TextView, executionContext.OperationContext.UserCancellationToken).Wait();

            return true;
        }

        public CommandState GetCommandState(FormatDocXmlInSelectionCommandArgs args)
        {
            return CommandState.Available;
        }

        private static async Task FormatDocXmlInSelectionAsync(ITextView textView, CancellationToken cancellationToken)
        {
            var snapshot = textView.TextSnapshot;
            var selection = textView.Selection;

            if (selection.Mode != TextSelectionMode.Stream)
                return;

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
                return;

            var options = document.Project.Solution.Workspace.Options;
            if (!options.GetOption(DocXmlFormattingOptions.WrapColumn).HasValue)
                options = options.WithChangedOption(DocXmlFormattingOptions.WrapColumn, GetGuideColumn());

            var changes = DocXmlFormatter.Format(document, selectionSpan, options, cancellationToken);
            await document.ApplyTextChangesAsync(changes, cancellationToken).ConfigureAwait(true);
        }

        private static int? GetGuideColumn()
        {
            return TextEditorGuidesSettings.GetGuideColumns().Max();
        }
    }
}

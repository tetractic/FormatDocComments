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
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using TextSpan = Microsoft.CodeAnalysis.Text.TextSpan;

namespace FormatDocXml
{
    /// <summary>
    /// The XML documentation comment formatting command set.
    /// </summary>
    internal sealed class FormatDocXmlCommandSet
    {
        /// <summary>
        /// The "Format Doc Comments in Selection" command ID.
        /// </summary>
        public const int FormatDocXmlInSelectionCommandId = 0x0100;

        /// <summary>
        /// The "Format Doc Comments in Document" command ID.
        /// </summary>
        public const int FormatDocXmlInDocumentCommandId = 0x0101;

        /// <summary>
        /// The command set GUID.
        /// </summary>
        public static readonly Guid CommandSet = new Guid("ed2fe6df-2ac7-4a91-b899-baed23255208");

        private readonly Package _package;

        private readonly IVsTextManager _textManager;

        private readonly IVsEditorAdaptersFactoryService _vsEditorAdaptersFactory;

        private FormatDocXmlCommandSet(Package package)
        {
            if (package == null)
                throw new ArgumentNullException(nameof(package));

            _package = package;

            if (ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            {
                {
                    var formatDocXmlInSelectionCommandId = new CommandID(CommandSet, FormatDocXmlInSelectionCommandId);
                    var formatDocXmlInSelectionCommand = new OleMenuCommand(
                        invokeHandler: FormatDocXmlInSelectionCommandExecute,
                        changeHandler: null,
                        beforeQueryStatus: FormatDocXmlCommandQueryStatus,
                        id: formatDocXmlInSelectionCommandId);
                    commandService.AddCommand(formatDocXmlInSelectionCommand);
                }
                {
                    var formatDocXmlInDocumentCommandId = new CommandID(CommandSet, FormatDocXmlInDocumentCommandId);
                    var formatDocXmlInDocumentCommand = new OleMenuCommand(
                        invokeHandler: FormatDocXmlInDocumentCommandExecute,
                        changeHandler: null,
                        beforeQueryStatus: FormatDocXmlCommandQueryStatus,
                        id: formatDocXmlInDocumentCommandId);
                    commandService.AddCommand(formatDocXmlInDocumentCommand);
                }
            }

            var componentModel = (IComponentModel)ServiceProvider.GetService(typeof(SComponentModel));

            _textManager = (IVsTextManager)ServiceProvider.GetService(typeof(SVsTextManager));

            _vsEditorAdaptersFactory = componentModel.GetService<IVsEditorAdaptersFactoryService>();
        }

        private void FormatDocXmlCommandQueryStatus(object sender, EventArgs e)
        {
            if (sender is OleMenuCommand command)
            {
                // `GetActiveView` apparently ignores `fMustHaveFocus` if `pBuffer` is null, so call
                // it a second time with the buffer from the view it returned the first time to find
                // out if the view actually has focus.
                if (_textManager.GetActiveView(1, null, out IVsTextView vsTextView) == VSConstants.S_OK &&
                    vsTextView.GetBuffer(out IVsTextLines vsTextLines) == VSConstants.S_OK &&
                    _textManager.GetActiveView(1, vsTextLines, out vsTextView) == VSConstants.S_OK &&
                    vsTextLines.GetLanguageServiceID(out Guid languageServiceId) == VSConstants.S_OK &&
                    languageServiceId == LanguageServiceIds.CSharp)
                {
                    command.Enabled = true;
                    command.Supported = true;
                    command.Visible = true;
                }
                else
                {
                    command.Enabled = false;
                    command.Supported = false;
                    command.Visible = false;
                }
            }
        }

        private void FormatDocXmlInSelectionCommandExecute(object sender, EventArgs e)
        {
            if (sender is OleMenuCommand command)
            {
                if (_textManager.GetActiveView(1, null, out IVsTextView vsTextView) == VSConstants.S_OK)
                {
                    var textView = _vsEditorAdaptersFactory.GetWpfTextView(vsTextView);
                    FormatDocXmlInSelection(textView);
                }
            }
        }

        private void FormatDocXmlInDocumentCommandExecute(object sender, EventArgs e)
        {
            if (sender is OleMenuCommand command)
            {
                if (_textManager.GetActiveView(1, null, out IVsTextView vsTextView) == VSConstants.S_OK)
                {
                    var textView = _vsEditorAdaptersFactory.GetWpfTextView(vsTextView);
                    FormatDocXmlInDocument(textView);
                }
            }
        }

        /// <summary>
        /// Gets the instance of the command set.
        /// </summary>
        public static FormatDocXmlCommandSet Instance { get; private set; }

        private IServiceProvider ServiceProvider => _package;

        /// <summary>
        /// Initializes the singleton instance of the command set.
        /// </summary>
        /// <param name="package">The package that provides the command set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="package"/> is
        ///     <see langword="null"/>.</exception>
        public static void Initialize(Package package)
        {
            if (package == null)
                throw new ArgumentNullException(nameof(package));

            Instance = new FormatDocXmlCommandSet(package);
        }

        private static void FormatDocXmlInSelection(ITextView textView)
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

            var changes = DocXmlFormatter.Format(document, selectionSpan, options);
            document.ApplyTextChanges(changes);
        }

        private static void FormatDocXmlInDocument(ITextView textView)
        {
            var snapshot = textView.TextSnapshot;

            var textBuffer = textView.TextBuffer;
            if (textBuffer.EditInProgress)
                return;

            var document = snapshot.GetOpenDocumentInCurrentContextWithChanges();
            if (document == null)
                return;

            var options = document.Project.Solution.Workspace.Options;
            if (!options.GetOption(DocXmlFormattingOptions.WrapColumn).HasValue)
                options = options.WithChangedOption(DocXmlFormattingOptions.WrapColumn, GetGuideColumn());

            var changes = DocXmlFormatter.Format(document, options);
            document.ApplyTextChanges(changes);
        }

        private static int? GetGuideColumn()
        {
            var guideColumns = new List<int>(TextEditorGuideSettings.Instance.GuideColumns);
            if (guideColumns.Count == 0)
                return null;
            guideColumns.Sort();
            return guideColumns[guideColumns.Count - 1];
        }
    }
}

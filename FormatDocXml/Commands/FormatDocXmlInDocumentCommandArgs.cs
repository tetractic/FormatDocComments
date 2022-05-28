// Copyright 2018 Carl Reinke
//
// This file is part of a library that is licensed under the terms of GNU Lesser
// General Public License version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding;

namespace FormatDocXml.Commands
{
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
    internal sealed class FormatDocXmlInDocumentCommandArgs : EditorCommandArgs
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
    {
        public FormatDocXmlInDocumentCommandArgs(ITextView textView, ITextBuffer textBuffer)
            : base(textView, textBuffer)
        {
        }
    }
}

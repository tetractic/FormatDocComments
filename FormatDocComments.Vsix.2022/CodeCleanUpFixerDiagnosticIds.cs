// Copyright 2022 Carl Reinke
//
// This file is part of a library that is licensed under the terms of the GNU
// Lesser General Public License Version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

using Microsoft.VisualStudio.Language.CodeCleanUp;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace FormatDocComments
{
    internal static class CodeCleanUpFixerDiagnosticIds
    {
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

        [Export]
        [Name(CodeCleanUpFixer.FormatDocCommentsFixId)]
        [FixId(CodeCleanUpFixer.FormatDocCommentsFixId)]
        [ConfigurationKey("unused")]
        [LocalizedName(typeof(CodeCleanUpFixerResources), nameof(CodeCleanUpFixerResources.FormatDocComments))]
        [ExportMetadata("EnableByDefault", true)]
        [ContentType("CSharp")]
        public static readonly FixIdDefinition FormatDocComments;

#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
    }
}

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
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace FormatDocComments
{
    [Export(typeof(ICodeCleanUpFixerProvider))]
    [AppliesToProject("CSharp")]
    [ContentType("CSharp")]
    internal sealed class CodeCleanUpFixerProvider : ICodeCleanUpFixerProvider
    {
        private readonly CodeCleanUpFixer _codeCleanUpFixer;

        [ImportingConstructor]
        public CodeCleanUpFixerProvider(CodeCleanUpFixer codeCleanUpFixer)
        {
            _codeCleanUpFixer = codeCleanUpFixer;
        }

        public IReadOnlyCollection<ICodeCleanUpFixer> GetFixers()
        {
            return new[] { _codeCleanUpFixer };
        }

        public IReadOnlyCollection<ICodeCleanUpFixer> GetFixers(IContentType contentType)
        {
            if (!contentType.IsOfType("CSharp"))
                return Array.Empty<ICodeCleanUpFixer>();

            return GetFixers();
        }
    }
}

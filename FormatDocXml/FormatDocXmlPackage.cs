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
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;

namespace FormatDocXml
{
    /// <summary>
    /// The package exposed by this assembly.
    /// </summary>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(UIContextGuids.SolutionExists)]
    [ProvideAutoLoad(UIContextGuids.CodeWindow)]
    [Guid(PackageGuidString)]
    public sealed class FormatDocXmlPackage : Package
    {
        /// <summary>
        /// The package GUID string.
        /// </summary>
        public const string PackageGuidString = "8288a158-430f-4bc7-9502-0716acc4f964";

        /// <summary>
        /// Initializes a new instance of the <see cref="FormatDocXmlPackage"/> class.
        /// </summary>
        public FormatDocXmlPackage()
        {
            // Initialization that does not require Visual Studio services can go here.
        }

        /// <summary>
        /// Initializes the package.
        /// </summary>
        protected override void Initialize()
        {
            // Initialization that requires Visual Studio services goes here.

            TextEditorGuideSettings.Initialize(this);

            FormatDocXmlCommandSet.Initialize(this);

            base.Initialize();
        }
    }
}

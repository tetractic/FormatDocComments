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
using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace FormatDocXml
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.1")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageIds.PackageGuidString)]
    [ProvideBindingPath]
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
    internal sealed class FormatDocXmlPackage : AsyncPackage
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
    {
    }
}

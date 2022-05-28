// Copyright 2018 Carl Reinke
//
// This file is part of a library that is licensed under the terms of GNU Lesser
// General Public License version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace FormatDocComments
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageIds.PackageGuidString)]
    [ProvideBindingPath]
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
    internal sealed class FormatDocCommentsPackage : AsyncPackage
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
    {
    }
}

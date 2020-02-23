//
// Copyright (C) 2019  Carl Reinke
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
using System;

namespace FormatDocXml
{
    internal static class PackageIds
    {
        public const string PackageGuidString = "8288a158-430f-4bc7-9502-0716acc4f964";
        public const string CommandSetGuidString = "ed2fe6df-2ac7-4a91-b899-baed23255208";

        public static readonly Guid PackageGuid = new Guid(PackageGuidString);
        public static readonly Guid CommandSetGuid = new Guid(CommandSetGuidString);

        public const int FormatDocXmlInSelectionCommandId = 0x0100;
        public const int FormatDocXmlInDocumentCommandId = 0x0101;
    }
}

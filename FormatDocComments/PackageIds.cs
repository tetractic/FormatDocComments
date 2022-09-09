// Copyright 2018 Carl Reinke
//
// This file is part of a library that is licensed under the terms of the GNU
// Lesser General Public License Version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

using System;

namespace FormatDocComments
{
    internal static class PackageIds
    {
        public const string PackageGuidString = "8288a158-430f-4bc7-9502-0716acc4f964";
        public const string CommandSetGuidString = "ed2fe6df-2ac7-4a91-b899-baed23255208";

        public static readonly Guid PackageGuid = new Guid(PackageGuidString);
        public static readonly Guid CommandSetGuid = new Guid(CommandSetGuidString);

        public const int FormatDocCommentsInSelectionCommandId = 0x0100;
        public const int FormatDocCommentsInDocumentCommandId = 0x0101;
    }
}

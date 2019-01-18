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
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using System;
using System.Collections.Generic;

namespace FormatDocXml
{
    /// <summary>
    /// The settings for the text editor guide lines.
    /// </summary>
    /// <remarks>
    /// These settings are editable using the <c>VisualStudioProductTeam.EditorGuidelines</c> or
    /// <c>PaulHarrington.EditorGuidelines</c> extensions.
    /// </remarks>
    internal static class TextEditorGuidesSettings
    {
        /// <summary>
        /// Gets the columns of the guide lines.
        /// </summary>
        public static IEnumerable<int> GetGuideColumns()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var settingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);

            SettingsStore userSettings = settingsManager.GetReadOnlySettingsStore(SettingsScope.UserSettings);

            return GetGuideColumns(userSettings);
        }

        private static IEnumerable<int> GetGuideColumns(SettingsStore userSettings)
        {
            var guides = userSettings.GetString("Text Editor", "Guides", string.Empty).Trim();

            if (string.IsNullOrEmpty(guides) || !guides.StartsWith("RGB(", StringComparison.Ordinal))
                yield break;

            int index = guides.IndexOf(')', 4);
            if (index < 0)
                yield break;

            foreach (var s in guides.Substring(index + 1).Split(','))
                if (int.TryParse(s, out int column) && column >= 0)
                    yield return column;
        }
    }
}

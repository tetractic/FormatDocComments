// Copyright 2018 Carl Reinke
//
// This file is part of a library that is licensed under the terms of GNU Lesser
// General Public License version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using System;
using System.Collections.Generic;

namespace FormatDocComments
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
            string guides = userSettings.GetString("Text Editor", "Guides", string.Empty).Trim();

            if (string.IsNullOrEmpty(guides) || !guides.StartsWith("RGB(", StringComparison.Ordinal))
                yield break;

            int index = guides.IndexOf(')', 4);
            if (index < 0)
                yield break;

            foreach (string s in guides.Substring(index + 1).Split(','))
                if (int.TryParse(s, out int column) && column >= 0)
                    yield return column;
        }
    }
}

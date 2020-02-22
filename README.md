# Format Doc Comments
A Visual Studio extension that formats and word wraps C# XML documentation
comments.

[![Build status](https://ci.appveyor.com/api/projects/status/tj19gcp3on1426wc/branch/master?svg=true)](https://ci.appveyor.com/project/carlreinke/formatdocxml/branch/master)

## Getting Started

Install the extension from the [Visual Studio Marketplace].

![Menu screenshot](images/menu-screenshot.png)

The extension adds two new commands to the *Edit* › *Advanced* menu:
* *Format Doc Comments in Selection* formats the XML documentation comments that
  overlap the selection.  If nothing is selected then it formats the comment
  under the caret.
* *Format Doc Comments in Document* formats all the XML documentation comments
  in the document.

## Configuration

The formatting style is currently non-configurable.

The word wrap column can be configured by setting a text editor guideline using
the [Editor Guidelines] extension.  If no guidelines are set then it defaults to
80 columns.


[Visual Studio Marketplace]: https://marketplace.visualstudio.com/items?itemName=carlreinke.FormatDocXml
[Editor Guidelines]: https://marketplace.visualstudio.com/items?itemName=PaulHarrington.EditorGuidelines

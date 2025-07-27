# BazthalLib UI Library - README
[![NuGet](https://img.shields.io/nuget/v/BazthalLib.svg)](https://www.nuget.org/packages/BazthalLib)
[![Downloads](https://img.shields.io/nuget/dt/BazthalLib.svg)](https://www.nuget.org/packages/BazthalLib)
[![License](https://img.shields.io/github/license/Bazthal/BazthalLib)](https://github.com/Bazthal/BazthalLib/blob/master/LICENSE)
[![MP3 Player Sample](https://img.shields.io/badge/sample-MP3%20Player-blue)](https://github.com/Bazthal/MP3PlayerV2)

`BazthalLib` is a custom C# UI framework designed for themable Windows Forms apps.  
All components follow a unified, configurable color system with reusable styling.
Library was built alongside projects using it so everything was added to fill a need

## Key Features

### Unified Theme Engine
- Centralized `ThemeColors.cs` defines all color roles.
- Live propagation of theme updates to all registered controls.
- JSON support via `ThemeColorsJsonConverter.cs` for saving/loading themes.

### Themable Controls
A full suite of UI elements with built-in theme awareness:

Category	Included Controls
Buttons	ThemableButton, ThemableToolStripButton, ThemableRadioButton
Inputs	ThemableTextBox, ThemableComboBox, ThemableCheckBox, ThemableNumericUpDown
Lists	ThemableListBox, ThemableOptionListBox
Panels & Grouping	ThemablePanel, ThemableGroupBox, ThemableTabControlBase, ThemableTabControlHeader
ToolStrips	ThemableToolStrip, ThemableStatusStrip, ThemableToolStripTextBox, ThemableToolStripProgressBar, etc.
Misc	ThemableProgressBar, ThemableTrackBar, ThemableColorPickerDialog, ThemableProcessingDialog, ThemableMessageBox

All components implement or inherit from `IThemableControl`.

## Tinted Image Rendering
- `TintedImageRenderer.cs` enables dynamic icon tinting based on accent or foreground color.
- Integrates with buttons for adaptive shape, focus borders, and scaling tied to image layout.

## Utility Classes
- DebugUtils.cs	Debug logging utilities
- MouseHook.cs	Global mouse input handling
- NativeMethods.cs	P/Invoke support for Win32 APIs
- EyedropperOverlay.cs	Live screen color picker with multi-monitor support

## Smart Design Patterns
- `NotifyingItemCollection.cs`: A custom observable collection with change tracking
- `ThemeManager`: Central orchestrator of theming logic and control registration

## Ideal For
- Custom-themed Windows Forms applications
- Stream overlays, dashboards, and utilities
- Apps needing runtime theme switching or user-defined color customization

## Project Structure
Folder	Description
Controls	All themable UI components
Configuration	Theme persistence and JSON handling
UI	Theming engine and rendering tools
Systems	File handling, execution, networking logic
Resources	Assets for Color Picker and Theme Selector components

## Color Picker Preview
- Compact and expanded versions of the built-in color picker

| Compacted | Expanded |
|-----------|----------|
|![Color Picker Dialog](https://i.imgur.com/xciltI9.png) | ![Color Picker Dialog - Expanded](https://i.imgur.com/YdhbYpL.png)|


## Sample App: [MP3PlayerV2](https://github.com/Bazthal/MP3PlayerV2)
- This MP3 player UI was built entirely with BazthalLib:

| Light / Dark | Theme | HotPink Theme|
|--------------|-------|--------------|
| ![Light / Dark](https://i.imgur.com/qCo4YNu.png) | ![Main](https://i.imgur.com/V34L196.png) | ![HotPink](https://i.imgur.com/4ZK0z4b.png)|
| Settings | Processing Dialog |
![Settings](https://i.imgur.com/bJfe3tr.png) | ![Loading](https://i.imgur.com/Xeb9Fe3.png)|

# BazthalLib UI Library — README

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

![Color Picker Dialog](https://imgur.com/xciltI9.png)
![Color Picker Dialog - Expanded](https://imgur.com/YdhbYpL.png)


## Sample App: MP3 Player UI
- This MP3 player UI was built entirely with BazthalLib:

![Light / Dark](https://imgur.com/qCo4YNu.png)
![Main UI](https://imgur.com/V34L196.png)
![Main UI Hot Pink](https://imgur.com/4ZK0z4b.png)
![Settings Menu](https://imgur.com/bJfe3tr.png)

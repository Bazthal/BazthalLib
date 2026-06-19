# BazthalLib Changelog

## 1.2.6 .NET10
- Added .NET10 LTS to target framework
  - 

## 1.2.5

### Message Box Enhancements
- Added a `ShowAt` method to allow specifying a location for centering the message box.
- Enabled system sound playback for different icon types (Info, Warning, Error, Question).
- Updated to tint icons using `TintedImageRenderer` for consistent theming.
- Replaced overwrite confirmation messagebox to use `ThemableMessageBox` allowing for consistent theming.

### Themable Controls Improvements
- Introduced TintedImage support to `ThemablePictureBox`.
  - Tinted Images will use SizeMode along with AutoScaleImage to determine rendering size.
- `ThemablePictureBox` replaced the `WndProc` override with an `OnPaint` override.
- Added `ShowDisabledState` to `ThemableTextbox` to allow textbox to keep the visuals on for readonly.
  - Default is `true` to maintain existing behavior.
  - Same property added to `ThemableRichTextBox`, `ThemableMaskedTextBox` and `ThemableToolStripTextBox`.
- Added an option to resize tinted images to match button dimensions in `ThemableButton`.
  - The same property is exposed on `ThemableToolStripButton`.

### Logging and Debugging
- Updated `DebugUtils.Log` to use `Trace` instead of `Debug`, enabling console output in release builds of the library.
- Added `LogLevel` enum for log severity:
  - Info, Warning, Error, Critical, Debug, Obsolete.
- Added `LogCategory` enum for message categories:
  - General, Network, UI, Database, Performance, Security, Configuration, Serialization, Forms, Theme, System.
- Added `LogName` enum for log sources:
  - Initialization, Connection, UserAction, Query, Update, Deletion.
- Added optional overload to `Log` method to accept `LogCategory` and `LogName` parameters.
- Updated `DebugUtils.Log` and `DebugUtils.LogIf` to accept a `LogLevel` parameter.
- Added `LogObsoleteUsage` to log warnings for obsolete API usage.
- Changed `includeTimestamp` to default to `true` in `DebugUtils.Log` and `DebugUtils.LogIf`.
  - `DebugUtils.LogIf` now delegates to `Log` internally for consistent behavior.

### WinForms API Changes
- Changed `WinForms.OpenForm` to a generic method to support forms that require constructor arguments.
- The original `WinForms.OpenForm` overload has been marked obsolete (non-breaking) and will be removed in a future release.
- `WinForms.CloseForm` is now a more generic method as well with similar obsolescence handling for the original overload.

### Image Rendering
- `TintedImageRenderer` Now supports downscaling tinted images via the `ImageQuality` enum for improved performance and quality control.

### Serialization and JSON
- Added a JSON converter that can parse inline JSON strings embedded within another JSON value.
  - Moved the other converters to the `Extensibility.Serialization` namespace.
  - The original converters in the previous namespace have been marked obsolete and will be removed in a future update.
  - `ThemeColorSetter` has been updated to use the new namespace.

### System Theme Detection
- System Theme now attempts to read the accent color from the registry key `Software\Microsoft\Windows\DWM`.
  - If that registry key is unavailable or unreadable, the accent color falls back to `Color.DodgerBlue`.

## 1.1.5
- Added item reordering to `ThemableListBox`, supporting both single and multi-selection. Reordering can be performed using mouse drag or by pressing Ctrl + Arrow Up/Down keys.
- Clicking on the track of `ThemableScrollBar` now moves the thumb toward the cursor by `LargeChange`.
  - Holding down the mouse button will repeat this action until the thumb reaches the cursor.
- Introduced `FilesDroppedEventArgs` and `ItemsReorderedEventArgs` classes for handling file drop and item reorder events.

## 1.1.4
- Multi-selection support in list controls:
  - Added `HashSet<int>` to track selected indices.
  - Introduced `EnableMultiSelect` property in `ThemableListBox`.
    - Property is hidden in `ThemableOptionsListBox` to avoid design-time confusion.
  - Enhanced selection logic in `OnKeyDown` and `OnMouseDown` for multi-selection.
  - Updated `DrawItems` to visually highlight multiple selected items.
- Type-based search implemented in `ThemableListBox` for faster navigation.
- Improved design-time experience in `TabControlNameConverter.cs` (`GetStandardValues` method).
- Drag-to-scroll support added in `ThemableScrollBar`:
  - `ThemableListBox` now uses the scroll bar directly, unlocking all features.
  - Focus rectangle now wraps the content section.
- Overload added to `SetProgress` in `ThemableProcessingDialog.cs` for flexible UI updates.
- New `LimitedStack<T>` in `LimitedStack.cs`: thread-safe stack with fixed capacity.
- Backup changes in `Files.cs`:
  - Backups now copy files instead of moving them.
  - Added new `MigrationBackUp` method.
- Hid `Text` property in `ThemableScrollBar.cs` and `ThemableComboBox.cs` to prevent unintended usage.
- Updated version numbers for all modified controls.
- Mouse wheel improvements:
  - `ThemableNumericUpDown`: increments/decrements value.
  - `ThemableTrackBar`: adjusts value by `ScrollStep` increments (not `Thumbsize`).

## 1.1.3
- Streamlined backup file search in `Files.cs`.
- Fixed hexadecimal color parsing in `ThemeColorsJsonConverter.cs`.
- Enhanced `README.md` with NuGet badges and updated sample app link to [`MP3PlayerV2`](https://github.com/Bazthal/MP3PlayerV2).
- Added `DebugMode` check in `DebugUtils.cs` for logging.
- Added cancel button and functionality in `ThemableProcessingDialog.cs`.
- Refactored drawing logic in `ThemableScrollBarRenderer.cs`.

## 1.1.2
- Added `EnsureValidSize` method in `ThemableTrackBar` and `ThemableScrollBar` for size validation..
- Refactored `OnResize` method to streamline logic.
- Refactored file handling in `Execution.cs` and `Files.cs`.
- Optimized `DownloadFile` method in `Data.cs`.
- Updated image layout in `README.md`:
  - Used a table for better alignment
  - Removed outdated image assets.

## 1.1.1
- Switched all README images to Imgur-hosted URLs due to NuGet rendering limitations.

## 1.1.0

- Removed `System.Management` dependency.
- Added exclusions for `packages/` folder during compilation.
- Refactored `SysInfo.cs`:
  - Replaced WMI with `Registry` and `DriveInfo`.
  - Improved `GetCPUInfo`, `GetMemoryInfo`, and `GetStorageInfo`.
  - Added support for logical processor and memory status retrieval.
- Updated `README.md` to use image links compatible with NuGet.

## 1.0.0 Initial release
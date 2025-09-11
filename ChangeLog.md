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
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
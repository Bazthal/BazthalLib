## 1.1.2
- Added `EnsureValidSize` method in `ThemableTrackBar` and `ThemableScrollBar` for size validation..
- Refactored `OnResize` method to streamline logic.
- Refactored file handling in `Execution.cs` and `Files.cs`.
- Simplified `DownloadFile` method in `Data.cs`
- Changed the images again still didn't show up
  - Placed the images in a table to allign them better
  - Removed outdated image assets.

## 1.1.1
 - Changed the images to hosted on imgur due to NuGet not showing the images

## 1.1.0

- Removed `System.Management` package references.
- Added entries to exclude `packages` directory files from compilation.
- Updated `README.md` to use Imgur links for images.
- Refactored `SysInfo.cs` to improve functionality and readability:
  - Replaced `System.Management` with `Registry` and `DriveInfo`.
  - Enhanced `GetCPUInfo`, `GetMemoryInfo`, and `GetStorageInfo` methods.
  - Added new methods and structures for logical processor information and memory status.

## 1.0.0 Initial release
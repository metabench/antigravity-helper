# Windows Runtime Implementation Guide

This document explains how to enable full OCR functionality when running ConfirmScout on Windows.

## Current Status

The application is designed to work on Windows 11 (or Windows 10 build 22621+) but is built to be cross-platform compatible. The OCR functionality requires Windows Runtime (WinRT) APIs which are only available when running on Windows.

## OCR Implementation

### Build-Time vs Runtime

- **Build-Time**: The project can be built on any platform (Linux, macOS, Windows) by using conditional compilation and runtime checks
- **Runtime**: Full OCR functionality is only available when running on Windows due to WinRT dependencies

### Enabling Full OCR on Windows

When running on Windows, the application will automatically detect the platform and attempt to use Windows.Media.Ocr. However, for optimal results, you should:

1. **Ensure Windows.Media.Ocr is available**:
   - Windows 10 version 1903 or later
   - Windows 11 (all versions)

2. **Install language packs**:
   - Open Settings > Time & Language > Language
   - Add English (United States) if not already installed
   - Ensure OCR language support is included

3. **Update the OcrService.cs implementation**:
   - Replace the stub implementation with the code from `OcrServiceWindows.cs.txt`
   - This requires uncommenting the Windows-specific code and ensuring proper using directives

### Full Implementation Steps

To enable full OCR functionality:

1. Open `Services/OcrService.cs`
2. Replace the `ProcessImageWithWindowsOcr` method with the full implementation from `OcrServiceWindows.cs.txt`
3. Add the necessary using directives:
   ```csharp
   using System.Runtime.InteropServices.WindowsRuntime;
   using Windows.Graphics.Imaging;
   using Windows.Media.Ocr;
   using Windows.Storage.Streams;
   ```
4. Rebuild the project on Windows

### Alternative: Use Tesseract OCR

For cross-platform OCR support, you could integrate Tesseract:

1. Add NuGet package: `Tesseract` or `IronOcr`
2. Replace the OcrService implementation
3. Note: Tesseract requires trained data files and may have different accuracy

## Testing OCR

To test OCR functionality:

1. Build and run the application on Windows
2. Select a window with visible "Confirm", "Continue", or "Approve" buttons
3. Start monitoring
4. Check the log output for OCR results
5. Verify that bounding boxes appear over detected text

## Troubleshooting

### OCR Not Working

1. **Check logs**: Look in `%AppData%/ConfirmScout/Logs/` for error messages
2. **Verify Windows version**: Must be Windows 10 1903+ or Windows 11
3. **Check language packs**: English language pack must be installed
4. **Test with clear text**: Use high-contrast, large fonts initially

### Poor Detection Accuracy

1. **Increase FPS**: Higher capture rate improves stability detection
2. **Adjust DPI**: Ensure the application is DPI-aware
3. **Use clear fonts**: OCR works best with sans-serif fonts and good contrast
4. **Check window scaling**: High DPI scaling may affect bounding box coordinates

## Performance Considerations

- **CPU Usage**: OCR is CPU-intensive; lower FPS reduces load
- **Memory**: Each frame is temporarily stored; monitor memory usage
- **Latency**: OCR processing adds 50-200ms per frame depending on resolution

## Security Notes

- OCR processes window content, which may contain sensitive information
- Logs may contain detected text
- Screen captures are temporary and not saved to disk
- All processing is local; no network communication

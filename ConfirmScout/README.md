# ConfirmScout - Windows 11 WPF .NET 8 Application

A Windows 11 WPF application that monitors windows for confirmation buttons (Confirm/Continue/Approve) using OCR, and provides keyboard shortcuts for automated interaction.

## Features

### Core Functionality
- **Window Selection**: Pick any target window by its handle (HWND)
- **Screen Capture**: Captures window content at 2-5 FPS using Windows GDI
- **OCR Detection**: Performs OCR on captured frames to detect "Confirm", "Continue", and "Approve" buttons
- **Stability Check**: Requires 2-frame stability before highlighting detected text
- **Visual Feedback**: Always-on-top overlay window draws bounding boxes around detected text
- **Audio Feedback**: Beep sound when stable detection is found
- **Logging**: Comprehensive logging to application data folder and UI

### Hotkey Controls

#### Ctrl+Alt+Enter - Conditional Click
- Clicks the detected button ONLY if:
  - A stable match is still present (2-frame stability)
  - The target window is currently in the foreground
  - Cooldown period has elapsed (1 second between clicks)
- **No automatic clicking** - requires manual hotkey press

#### Ctrl+Alt+S - Scroll Search
- Performs stepwise scrolling (maximum 30 steps)
- Runs OCR after each scroll step
- Stops automatically when target text is found
- Press **Esc** to cancel the search
- Highlights detected text but does not auto-click

### Optional Features
- **Cursor Movement**: Optional setting to move cursor to detected text location
- **Configurable FPS**: Adjust capture rate from 2-5 FPS

## Requirements

- Windows 11 (or Windows 10 version 22621+)
- .NET 8 Runtime

## Building

### Prerequisites
- .NET 8 SDK
- Windows operating system (required for runtime OCR features)

### Build Commands
```bash
cd ConfirmScout
dotnet restore
dotnet build
```

### Run the Application
```bash
dotnet run
```

Or build and run the executable:
```bash
dotnet publish -c Release
cd bin/Release/net8.0-windows10.0.22621.0/publish
./ConfirmScout.exe
```

## Project Structure

```
ConfirmScout/
├── Models/
│   ├── DetectedText.cs          # Represents detected text with bounding box
│   └── StableDetection.cs       # Represents stable detection after 2-frame check
├── Services/
│   ├── CaptureService.cs        # Window capture using GDI
│   ├── OcrService.cs            # OCR processing (Windows.Media.Ocr)
│   ├── StabilityTracker.cs      # 2-frame stability verification
│   ├── HotkeyService.cs         # Global hotkey registration
│   ├── MouseController.cs       # Mouse movement and clicking with cooldown
│   ├── WindowPicker.cs          # Window enumeration and selection
│   └── Logger.cs                # File and debug logging
├── Windows/
│   ├── OverlayWindow.xaml       # Always-on-top overlay for highlighting
│   └── OverlayWindow.xaml.cs    # Overlay window code-behind
├── MainWindow.xaml              # Main application UI
├── MainWindow.xaml.cs           # Main application logic
├── App.xaml                     # Application resources
└── ConfirmScout.csproj          # Project file
```

## Usage

1. **Launch the Application**
   - Run ConfirmScout.exe

2. **Select Target Window**
   - Click "Refresh" to populate the window list
   - Select a window from the dropdown
   - Click "Select Window"

3. **Configure Settings**
   - Adjust FPS slider (2-5 FPS recommended)
   - Enable/disable cursor movement option

4. **Start Monitoring**
   - Click "Start Monitoring"
   - The overlay window will appear over the target window
   - When "Confirm", "Continue", or "Approve" text is detected and stable for 2 frames:
     - Red bounding box appears on the overlay
     - Beep sound plays
     - Text is logged in the application

5. **Use Hotkeys**
   - **Ctrl+Alt+Enter**: Click the detected button (if conditions are met)
   - **Ctrl+Alt+S**: Start scroll search (press Esc to cancel)

6. **Stop Monitoring**
   - Click "Stop Monitoring" to stop the capture and close the overlay

## Implementation Notes

### OCR Integration
The application uses `Windows.Media.Ocr` for text recognition. This requires:
- Windows 10 version 1903 or later
- English language pack installed
- The OCR engine runs asynchronously to avoid blocking the UI

### Capture Method
Uses GDI-based window capture via `BitBlt` for compatibility. For production use on Windows 11, consider upgrading to Windows.Graphics.Capture API for better performance and DPI awareness.

### Safety Features
1. **No Auto-Click**: The application never clicks automatically
2. **Cooldown Timer**: 1-second cooldown between clicks
3. **Foreground Check**: Clicks only work when target window is in foreground
4. **Stability Requirement**: Requires 2 consecutive frames with consistent detection
5. **Manual Activation**: All actions require explicit hotkey press

### Logging
Logs are stored in:
```
%AppData%/ConfirmScout/Logs/log_YYYYMMDD_HHMMSS.txt
```

## Known Limitations

1. **Cross-Platform Build**: While the code can be compiled on Linux (for CI/CD), the Windows-specific APIs (OCR, GDI capture) require Windows at runtime
2. **OCR Accuracy**: Detection accuracy depends on font size, clarity, and contrast
3. **DPI Scaling**: May require adjustment for high-DPI displays
4. **Window Focus**: Some applications may block or interfere with hotkey registration

## Security Considerations

- The application uses P/Invoke for Windows API calls (user32.dll, gdi32.dll)
- Global hotkeys are registered and properly unregistered on shutdown
- No network communication or external data transmission
- Logs may contain sensitive information from captured windows

## License

This project is part of the antigravity-helper repository.

## Technical Details

### Dependencies
- Microsoft.Windows.CsWinRT (2.0.7)
- Microsoft.Windows.SDK.Contracts (10.0.22621.2)
- System.Drawing.Common (8.0.0)
- SharpDX packages (for future Graphics Capture API integration)

### Target Framework
- .NET 8.0 (net8.0-windows10.0.22621.0)
- Platform: x64
- UI Framework: WPF

### Key Technologies
- WPF for UI
- Windows.Media.Ocr for text recognition
- Win32 API for window management and input
- GDI for screen capture
- Global hotkey registration

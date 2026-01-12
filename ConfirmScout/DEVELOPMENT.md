# Development Guide

## Prerequisites

- .NET 8 SDK
- Windows 10 version 22621 or later (for full functionality)
- Visual Studio 2022 or Visual Studio Code with C# extension
- Git

## Getting Started

### Clone and Build

```bash
git clone https://github.com/metabench/antigravity-helper.git
cd antigravity-helper/ConfirmScout
dotnet restore
dotnet build
```

### Run in Development

```bash
dotnet run
```

Or open the solution in Visual Studio:
```bash
start ConfirmScout.sln
```

## Project Structure

```
ConfirmScout/
├── Models/              # Data models
├── Services/            # Business logic services
├── Windows/             # Additional windows (Overlay)
├── MainWindow.xaml      # Main UI
├── MainWindow.xaml.cs   # Main UI logic
└── App.xaml            # Application entry point
```

## Key Technologies

- **WPF**: Windows Presentation Foundation for UI
- **Windows.Media.Ocr**: OCR engine (WinRT)
- **System.Drawing**: Image processing
- **Win32 API**: Window management, input simulation

## Building

### Debug Build
```bash
dotnet build --configuration Debug
```

### Release Build
```bash
dotnet build --configuration Release
```

### Publish Standalone
```bash
dotnet publish -c Release -r win-x64 --self-contained
```

## Testing

### Manual Testing Checklist

1. **Window Selection**
   - [ ] Refresh button populates list
   - [ ] Select window works
   - [ ] Window title displays correctly

2. **Capture**
   - [ ] Start monitoring activates capture
   - [ ] FPS slider changes capture rate
   - [ ] Stop monitoring releases resources

3. **Detection**
   - [ ] OCR detects "Confirm" button
   - [ ] OCR detects "Continue" button
   - [ ] OCR detects "Approve" button
   - [ ] Stability requires 2 frames
   - [ ] Bounding box appears correctly

4. **Overlay**
   - [ ] Overlay appears over target window
   - [ ] Overlay moves with window
   - [ ] Overlay is always on top
   - [ ] Overlay is click-through

5. **Hotkeys**
   - [ ] Ctrl+Alt+Enter registered
   - [ ] Ctrl+Alt+S registered
   - [ ] Hotkeys work when app is backgrounded
   - [ ] Click blocks when window not foreground

6. **Mouse Control**
   - [ ] Cursor moves to detection
   - [ ] Click happens at center of bounding box
   - [ ] Cooldown prevents rapid clicks
   - [ ] Scroll search scrolls stepwise

7. **Logging**
   - [ ] Log file created in AppData
   - [ ] Detections logged
   - [ ] Actions logged
   - [ ] Errors logged

## Code Style

### Naming Conventions
- Classes: PascalCase
- Methods: PascalCase
- Properties: PascalCase
- Private fields: _camelCase
- Local variables: camelCase

### Comments
- XML documentation for public APIs
- Inline comments for complex logic
- TODO comments for future work

### Error Handling
- Try-catch in all service methods
- Log all exceptions
- Graceful degradation when possible

## Common Development Tasks

### Adding a New Target Word

1. Edit `Services/OcrService.cs`
2. Add word to `_targetWords` array
3. Rebuild and test

```csharp
private readonly string[] _targetWords = { "Confirm", "Continue", "Approve", "OK" };
```

### Changing FPS Range

1. Edit `MainWindow.xaml`
2. Update Slider Minimum and Maximum
3. Adjust TickFrequency as needed

```xml
<Slider x:Name="FpsSlider" Minimum="1" Maximum="10" Value="2"/>
```

### Modifying Stability Requirements

1. Edit `Services/StabilityTracker.cs`
2. Change `RequiredFrames` constant
3. Adjust `PositionTolerance` if needed

```csharp
private const int RequiredFrames = 3; // Changed from 2
private const double PositionTolerance = 15.0; // Changed from 10.0
```

### Adding a New Hotkey

1. Edit `Services/HotkeyService.cs`
2. Add new hotkey ID and virtual key constant
3. Register in `RegisterHotkeys()` method
4. Handle in `WndProc()` method
5. Add event and raise it

```csharp
private const int HOTKEY_NEW_ID = 3;
private const uint VK_N = 0x4E;

public event EventHandler? NewHotkeyPressed;

// In RegisterHotkeys()
RegisterHotKey(_windowHandle, HOTKEY_NEW_ID, MOD_CONTROL | MOD_ALT, VK_N);

// In WndProc()
else if (id == HOTKEY_NEW_ID)
{
    NewHotkeyPressed?.Invoke(this, EventArgs.Empty);
}
```

## Debugging

### Enable Verbose Logging

Set breakpoints in:
- `CaptureService.CaptureFrame()` - Frame capture
- `OcrService.ProcessImageAsync()` - OCR processing
- `StabilityTracker.ProcessDetections()` - Stability logic
- `MainWindow.OnClickHotkeyPressed()` - Click handling

### Common Debug Points

1. **OCR Not Working**: Check `OcrService` initialization and Windows version
2. **Wrong Bounding Box**: Debug coordinate conversion in `MouseController`
3. **Hotkeys Not Working**: Verify registration in `HotkeyService`
4. **Capture Issues**: Check window handle validity and GDI calls

### Debug Output

View debug output in:
- Visual Studio Output window (Debug category)
- Log files in `%AppData%/ConfirmScout/Logs/`

## Performance Profiling

### Measure Frame Capture Time
Add timing code in `CaptureService.CaptureFrame()`:

```csharp
var sw = System.Diagnostics.Stopwatch.StartNew();
// ... capture code ...
sw.Stop();
_logger.Log($"Frame capture took {sw.ElapsedMilliseconds}ms");
```

### Measure OCR Time
Add timing code in `OcrService.ProcessImageAsync()`:

```csharp
var sw = System.Diagnostics.Stopwatch.StartNew();
// ... OCR code ...
sw.Stop();
_logger.Log($"OCR processing took {sw.ElapsedMilliseconds}ms");
```

## Known Issues

1. **Cross-Platform Build**: CsWinRT tools require Windows, but build succeeds with warnings
2. **DPI Scaling**: Coordinates may be off on high-DPI displays
3. **Some Windows**: Certain protected windows cannot be captured (e.g., UAC dialogs)

## Contributing

### Code Review Checklist

- [ ] Code follows style guidelines
- [ ] All methods have error handling
- [ ] Logging added for important actions
- [ ] No hardcoded paths or values
- [ ] Memory leaks prevented (Dispose pattern)
- [ ] Thread safety considered
- [ ] Documentation updated

### Git Workflow

1. Create feature branch: `git checkout -b feature/my-feature`
2. Make changes and commit: `git commit -m "Add feature"`
3. Push branch: `git push origin feature/my-feature`
4. Create pull request

## Release Process

1. Update version in `ConfirmScout.csproj`
2. Build release: `dotnet publish -c Release`
3. Test release build thoroughly
4. Tag release: `git tag v1.0.0`
5. Push tag: `git push --tags`
6. Create GitHub release with binaries

## Resources

- [WPF Documentation](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
- [Windows.Media.Ocr API](https://docs.microsoft.com/en-us/uwp/api/windows.media.ocr)
- [Win32 API Reference](https://docs.microsoft.com/en-us/windows/win32/api/)
- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/core/)

## Support

For issues and questions:
- Check existing documentation (README, ARCHITECTURE, USER_GUIDE)
- Review log files for errors
- Create GitHub issue with reproduction steps

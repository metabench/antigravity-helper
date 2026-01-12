# Architecture Design - ConfirmScout

## Overview

ConfirmScout is a Windows 11 WPF application that monitors target windows for specific confirmation buttons using OCR technology and provides hotkey-based interaction capabilities.

## System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      MainWindow (UI)                         │
│  - Window selection                                          │
│  - Configuration settings                                    │
│  - Start/Stop controls                                       │
│  - Status display and logging                                │
└────────┬────────────────────────────────────────────────────┘
         │
         ├─> Services Layer
         │   ├─> CaptureService      : Window screen capture
         │   ├─> OcrService           : Text recognition
         │   ├─> StabilityTracker     : Multi-frame validation
         │   ├─> HotkeyService        : Global hotkey registration
         │   ├─> MouseController      : Cursor/click control
         │   ├─> WindowPicker         : Window enumeration
         │   └─> Logger              : File and debug logging
         │
         └─> Windows Layer
             └─> OverlayWindow        : Always-on-top highlighting
```

## Component Details

### 1. MainWindow (UI Layer)

**Responsibilities:**
- User interface for application control
- Window selection and configuration
- Display status and logs
- Coordinate service initialization and lifecycle

**Key Features:**
- WPF-based modern UI
- Real-time log display
- FPS slider (2-5 FPS)
- Optional cursor movement toggle

### 2. CaptureService

**Responsibilities:**
- Capture window content at specified FPS
- Convert captured frames to byte arrays
- Emit FrameCaptured events

**Implementation:**
- Uses Win32 GDI (BitBlt) for window capture
- Timer-based frame capture
- PNG format conversion

**Future Enhancement:**
- Migrate to Windows.Graphics.Capture API for better performance
- Support for DPI scaling

### 3. OcrService

**Responsibilities:**
- Process captured images with OCR
- Detect target words (Confirm, Continue, Approve)
- Extract bounding box coordinates
- Return detection results with confidence scores

**Implementation:**
- Platform detection (Windows vs. non-Windows)
- Windows.Media.Ocr integration (runtime)
- Case-insensitive word matching

**Configuration:**
- Target words: ["Confirm", "Continue", "Approve"]
- Language: English (en-US)
- Confidence: 0.95 (default, as Windows OCR doesn't provide per-word confidence)

### 4. StabilityTracker

**Responsibilities:**
- Track detections across multiple frames
- Validate position consistency
- Emit events for stable detections (2+ frames)

**Algorithm:**
1. Receive detections from OCR
2. Compare with previous frame detections
3. Check position similarity (within 10px tolerance)
4. Increment frame count for consistent detections
5. Emit StableDetectionFound when count >= 2
6. Remove stale detections

**Parameters:**
- Required frames: 2
- Position tolerance: 10 pixels
- Timeout: Automatic (based on frame presence)

### 5. HotkeyService

**Responsibilities:**
- Register global hotkeys
- Handle hotkey press events
- Clean up on disposal

**Hotkeys:**
- Ctrl+Alt+Enter (ID: 1) → Click action
- Ctrl+Alt+S (ID: 2) → Scroll search

**Implementation:**
- Win32 RegisterHotKey/UnregisterHotKey
- WndProc message hook
- Event-based notification

### 6. MouseController

**Responsibilities:**
- Move cursor to specified coordinates
- Perform mouse clicks with cooldown
- Scroll window content

**Safety Features:**
- 1-second click cooldown
- Client-to-screen coordinate conversion
- Error logging

**Methods:**
- MoveCursor(windowHandle, boundingBox)
- ClickAtPosition(windowHandle, boundingBox) → bool
- ScrollDown(windowHandle)

### 7. WindowPicker

**Responsibilities:**
- Enumerate all visible windows
- Get window titles and handles
- Check foreground window
- Bring window to foreground

**Implementation:**
- EnumWindows Win32 API
- Window visibility filtering
- Title extraction

### 8. OverlayWindow

**Responsibilities:**
- Display always-on-top transparent overlay
- Draw bounding boxes around detected text
- Position overlay to match target window

**Features:**
- Transparent background
- No window chrome
- Not hit-testable (click-through)
- Red bounding boxes with labels

### 9. Logger

**Responsibilities:**
- Write logs to file
- Debug output
- Thread-safe logging

**Log Location:**
- `%AppData%/ConfirmScout/Logs/log_YYYYMMDD_HHMMSS.txt`

**Log Types:**
- General messages
- Detections with confidence
- Actions (click, scroll, etc.)

## Data Flow

### Monitoring Flow
```
1. User selects window and starts monitoring
2. CaptureService captures window at FPS rate
3. Each frame → OcrService for text detection
4. Detections → StabilityTracker for validation
5. Stable detection → OverlayWindow for display
6. Beep sound + log entry + optional cursor move
```

### Click Hotkey Flow
```
1. User presses Ctrl+Alt+Enter
2. HotkeyService emits ClickHotkeyPressed event
3. MainWindow receives event
4. Verify window is foreground
5. Verify detection still stable
6. MouseController clicks if checks pass
7. Log action and update status
```

### Scroll Search Flow
```
1. User presses Ctrl+Alt+S
2. HotkeyService emits ScrollHotkeyPressed event
3. MainWindow starts scroll search loop
4. For each step (max 30):
   a. MouseController scrolls down
   b. Wait 500ms for OCR
   c. Check if target found
   d. Stop if found or Esc pressed
5. Log completion status
```

## Threading Model

- **UI Thread**: WPF UI, event handlers
- **Capture Thread**: Timer-based frame capture
- **OCR Thread**: Async OCR processing
- **Hotkey Thread**: System hotkey messages

**Synchronization:**
- Dispatcher.Invoke for UI updates from background threads
- Event-based communication between services
- Lock-based file logging

## Error Handling

- Try-catch blocks in all service methods
- Errors logged to file and debug output
- UI displays error status messages
- Graceful degradation (e.g., OCR unavailable)

## Security Considerations

1. **No Auto-Click**: Prevents unintended automation
2. **Cooldown Timer**: Prevents rapid clicking abuse
3. **Foreground Check**: Ensures user intent
4. **Stability Requirement**: Prevents false positives
5. **Local Processing**: No network communication
6. **Logging**: May contain sensitive window content

## Performance Characteristics

- **CPU**: Low to moderate (depends on FPS and window size)
- **Memory**: ~50-100 MB (base) + frame buffers
- **Disk I/O**: Minimal (logging only)
- **Latency**: 50-200ms per frame (OCR processing)

## Extensibility Points

1. **Target Words**: Add to `_targetWords` array in OcrService
2. **Detection Logic**: Modify StabilityTracker algorithm
3. **Hotkeys**: Add new hotkey registrations in HotkeyService
4. **Overlay Style**: Customize OverlayWindow appearance
5. **Capture Method**: Replace CaptureService implementation
6. **OCR Engine**: Swap OcrService for alternative OCR (e.g., Tesseract)

## Future Enhancements

1. **Configuration File**: Save/load settings
2. **Multiple Target Windows**: Monitor multiple windows simultaneously
3. **Custom Word Lists**: User-defined target words
4. **Advanced Filtering**: Confidence thresholds, word patterns
5. **Macro Recording**: Record and replay action sequences
6. **Statistics**: Track detection accuracy and performance
7. **DPI Awareness**: Better support for high-DPI displays
8. **Accessibility**: Screen reader support, keyboard navigation

# ConfirmScout User Guide

## Quick Start

1. **Launch the Application**
   - Double-click `ConfirmScout.exe`
   - The main window will appear

2. **Select a Target Window**
   - Click the "Refresh" button to populate the window list
   - Select the window you want to monitor from the dropdown
   - Click "Select Window" to confirm your choice

3. **Configure Settings**
   - Adjust the FPS slider (2-5 FPS recommended for most use cases)
     - Lower FPS = less CPU usage
     - Higher FPS = faster detection
   - Optionally enable "Move cursor to detected text" if you want automatic cursor positioning

4. **Start Monitoring**
   - Click "Start Monitoring"
   - An overlay window will appear over your target window
   - The application will begin scanning for "Confirm", "Continue", or "Approve" buttons

5. **Using Hotkeys**
   - **Ctrl+Alt+Enter** - Click the detected button (if safe to do so)
   - **Ctrl+Alt+S** - Start scrolling search (press Esc to cancel)

## Detailed Instructions

### Window Selection

The window list shows all visible windows with titles. Common windows include:
- Web browsers (Chrome, Edge, Firefox)
- Applications (File Explorer, Settings)
- Dialog boxes and installers

**Tips:**
- Refresh the list if your target window appears after launching ConfirmScout
- The window title shows what you're monitoring
- You can change the target window at any time by stopping monitoring and selecting a new window

### Monitoring Process

When monitoring is active:

1. **Screen Capture**: The application captures the target window at your selected FPS
2. **OCR Processing**: Each frame is analyzed for text matching "Confirm", "Continue", or "Approve"
3. **Stability Check**: The text must appear in the same location for 2 consecutive frames
4. **Visual Feedback**: Once stable, a red box appears around the detected text
5. **Audio Feedback**: A beep sound plays when detection is confirmed
6. **Logging**: All detections are logged in the application window and log file

### Understanding Detection

**Target Words:**
- "Confirm" (case-insensitive)
- "Continue" (case-insensitive)
- "Approve" (case-insensitive)

**Stability Requirement:**
- Text must appear in the same position for at least 2 frames
- Position tolerance: within 10 pixels
- This prevents false positives from transient text

**Visual Indicators:**
- Red bounding box on the overlay window
- Text label showing the detected word
- Status message in the main window

### Using Ctrl+Alt+Enter (Click Hotkey)

This hotkey allows you to click the detected button, but only if safe:

**Safety Checks:**
1. ✓ Monitoring must be active
2. ✓ Target window must be in the foreground (active)
3. ✓ Detection must still be stable (present for 2+ frames)
4. ✓ Cooldown period must have elapsed (1 second between clicks)

**If any check fails, the click is blocked and logged**

**Usage:**
1. Start monitoring the target window
2. Wait for a button to be detected (red box appears)
3. Ensure the target window is active (click on it if needed)
4. Press Ctrl+Alt+Enter
5. The cursor will move to the button and click it

### Using Ctrl+Alt+S (Scroll Search Hotkey)

This hotkey performs a stepwise scroll search to find buttons:

**Process:**
1. Press Ctrl+Alt+S while monitoring
2. The window scrolls down one step
3. OCR runs on the new view
4. If a target button is found, scrolling stops and the button is highlighted
5. If not found, scrolling continues (up to 30 steps)
6. Press Esc at any time to cancel the search

**Best Practices:**
- Use on long pages or lists where buttons might be below the visible area
- The scroll search does NOT auto-click - use Ctrl+Alt+Enter after finding
- Monitor the status bar to see progress (Step X/30)

### Log Monitoring

The log panel shows:
- Timestamp for each event
- Detection events with word and confidence
- Actions performed (click, scroll)
- Errors or warnings

**Log File Location:**
```
C:\Users\YourName\AppData\Roaming\ConfirmScout\Logs\log_YYYYMMDD_HHMMSS.txt
```

### Stopping Monitoring

Click "Stop Monitoring" to:
- Stop screen capture
- Close the overlay window
- Clear current detections
- Reset the system for a new monitoring session

## Tips and Best Practices

### For Best Detection Accuracy

1. **Font Size**: Larger fonts are easier to detect (14pt or larger recommended)
2. **Contrast**: High contrast between text and background works best
3. **Font Style**: Sans-serif fonts (Arial, Segoe UI) are most reliable
4. **Resolution**: Native resolution without scaling is ideal
5. **Window Focus**: Avoid overlapping windows that obscure the target

### For Performance

1. **Lower FPS**: Use 2 FPS for reduced CPU usage
2. **Smaller Windows**: Smaller capture areas process faster
3. **Close Logs**: The log panel can consume memory over time - clear it periodically

### For Safety

1. **Always Review**: Before clicking, visually confirm the red box is on the correct button
2. **Foreground Check**: The target window must be active to click
3. **Cooldown**: Wait 1 second between clicks
4. **Manual Control**: No automatic clicking - you control when actions happen

## Troubleshooting

### "OCR Not Available" or No Detections

**Cause**: Windows OCR engine not initialized
**Solution**:
1. Ensure you're running on Windows 10 1903+ or Windows 11
2. Install English (United States) language pack
3. Check the log file for detailed error messages

### Bounding Box in Wrong Location

**Cause**: DPI scaling or coordinate mismatch
**Solution**:
1. Try setting the application to "System (Enhanced)" DPI awareness
2. Restart the application
3. Check if the target window uses custom DPI scaling

### Hotkeys Not Working

**Cause**: Hotkey registration failed or another application uses the same hotkey
**Solution**:
1. Check the log for "Failed to register hotkey" messages
2. Close other applications that might use Ctrl+Alt+Enter or Ctrl+Alt+S
3. Restart ConfirmScout

### High CPU Usage

**Cause**: High FPS or large window
**Solution**:
1. Reduce FPS to 2
2. Resize the target window to a smaller size
3. Close unnecessary applications

### Clicks Not Registering

**Cause**: Foreground window check failing
**Solution**:
1. Click on the target window to bring it to the foreground
2. Keep the target window active (don't alt-tab away)
3. Check the log for "Click blocked" messages

## Advanced Usage

### Multiple Monitoring Sessions

You can monitor different windows sequentially:
1. Stop current monitoring
2. Select a new window
3. Start monitoring again

### Custom Logging

The log file in `%AppData%\ConfirmScout\Logs\` contains detailed information:
- All detection events with timestamps
- OCR processing status
- Click and scroll actions
- Error diagnostics

You can use these logs to:
- Debug detection issues
- Track application behavior
- Analyze performance

### Keyboard Navigation

- **Tab**: Move between UI controls
- **Enter**: Activate focused button
- **Esc**: Cancel scroll search (when active)

## Limitations

1. **Platform**: Windows only (Windows 10 1903+ or Windows 11)
2. **Language**: English text only
3. **Target Words**: Only "Confirm", "Continue", "Approve"
4. **Accuracy**: Depends on font, size, and contrast
5. **Performance**: Affected by window size and FPS setting

## Safety and Privacy

- **No Auto-Click**: The application never clicks automatically
- **Local Processing**: All OCR happens on your computer
- **No Network**: No internet connection required or used
- **Log Privacy**: Logs may contain detected text from windows
- **Control**: You have full control via hotkeys

## Getting Help

If you encounter issues:

1. **Check Logs**: Review the log file for error messages
2. **Verify Setup**: Ensure Windows version and language pack requirements
3. **Documentation**: Read WINDOWS_RUNTIME.md for detailed OCR setup
4. **Architecture**: See ARCHITECTURE.md for technical details

## Keyboard Shortcuts Reference

| Shortcut | Action |
|----------|--------|
| Ctrl+Alt+Enter | Click detected button (with safety checks) |
| Ctrl+Alt+S | Start scroll search |
| Esc | Cancel scroll search |

## Status Messages

| Message | Meaning |
|---------|---------|
| "Ready" | Application is ready, no monitoring active |
| "Monitoring started at X FPS" | Capture is active |
| "Detected: [Word]" | Stable detection found |
| "Clicked: [Word]" | Click action completed |
| "Scroll search active..." | Scroll search in progress |
| "Click blocked: ..." | Click prevented due to safety check |

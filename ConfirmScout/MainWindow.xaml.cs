using System.Windows;
using System.Windows.Interop;
using ConfirmScout.Services;
using ConfirmScout.Models;
using ConfirmScout.Windows;

namespace ConfirmScout;

public partial class MainWindow : Window
{
    private readonly Logger _logger;
    private CaptureService? _captureService;
    private OcrService? _ocrService;
    private StabilityTracker? _stabilityTracker;
    private HotkeyService? _hotkeyService;
    private MouseController? _mouseController;
    private OverlayWindow? _overlayWindow;
    
    private IntPtr _selectedWindow = IntPtr.Zero;
    private bool _isMonitoring = false;
    private StableDetection? _currentDetection;
    private bool _isScrollSearchActive = false;
    private int _scrollStepCount = 0;
    private const int MaxScrollSteps = 30;

    public MainWindow()
    {
        InitializeComponent();
        _logger = new Logger();
        _logger.Log("Application started");
        
        Loaded += MainWindow_Loaded;
        Closed += MainWindow_Closed;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        RefreshWindowList();
        
        // Initialize hotkey service
        var windowHandle = new WindowInteropHelper(this).Handle;
        _hotkeyService = new HotkeyService(windowHandle, _logger);
        _hotkeyService.ClickHotkeyPressed += OnClickHotkeyPressed;
        _hotkeyService.ScrollHotkeyPressed += OnScrollHotkeyPressed;

        // Initialize mouse controller
        _mouseController = new MouseController(_logger);
        
        // Handle Esc key for canceling scroll search
        KeyDown += (s, args) =>
        {
            if (args.Key == System.Windows.Input.Key.Escape && _isScrollSearchActive)
            {
                CancelScrollSearch();
            }
        };
    }

    private void RefreshWindowList()
    {
        var windows = WindowPicker.GetAllWindows();
        WindowListComboBox.ItemsSource = windows;
        if (windows.Count > 0)
        {
            WindowListComboBox.SelectedIndex = 0;
        }
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        RefreshWindowList();
        UpdateStatus("Window list refreshed");
    }

    private void SelectWindowButton_Click(object sender, RoutedEventArgs e)
    {
        if (WindowListComboBox.SelectedItem is WindowPicker.WindowInfo selectedWindow)
        {
            _selectedWindow = selectedWindow.Handle;
            UpdateStatus($"Selected window: {selectedWindow.Title}");
            _logger.Log($"Selected window: {selectedWindow.Title} (HWND: {_selectedWindow})");
        }
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedWindow == IntPtr.Zero)
        {
            MessageBox.Show("Please select a target window first.", "No Window Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        StartButton.IsEnabled = false;
        StopButton.IsEnabled = true;
        _isMonitoring = true;

        // Initialize services
        _captureService = new CaptureService(_logger);
        _ocrService = new OcrService(_logger);
        _stabilityTracker = new StabilityTracker(_logger);
        
        _stabilityTracker.StableDetectionFound += OnStableDetectionFound;
        _captureService.FrameCaptured += OnFrameCaptured;

        // Create overlay window
        _overlayWindow = new OverlayWindow();
        _overlayWindow.SetTargetWindow(_selectedWindow);
        _overlayWindow.Show();

        // Start capture
        int fps = (int)FpsSlider.Value;
        _captureService.StartCapture(_selectedWindow, fps);

        UpdateStatus($"Monitoring started at {fps} FPS");
        _logger.Log("Monitoring started");
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        StopMonitoring();
    }

    private void StopMonitoring()
    {
        if (!_isMonitoring)
            return;

        _isMonitoring = false;
        StartButton.IsEnabled = true;
        StopButton.IsEnabled = false;

        _captureService?.StopCapture();
        _captureService?.Dispose();
        _captureService = null;

        _overlayWindow?.Close();
        _overlayWindow = null;

        _stabilityTracker?.Clear();

        UpdateStatus("Monitoring stopped");
        _logger.Log("Monitoring stopped");
    }

    private async void OnFrameCaptured(object? sender, byte[] imageData)
    {
        if (!_isMonitoring || _ocrService == null || _stabilityTracker == null)
            return;

        try
        {
            // Get window dimensions for OCR
            var detections = await _ocrService.ProcessImageAsync(imageData, 0, 0);
            
            // Update stability tracker
            _stabilityTracker.ProcessDetections(detections);
        }
        catch (Exception ex)
        {
            _logger.Log($"Frame processing error: {ex.Message}");
        }
    }

    private void OnStableDetectionFound(object? sender, StableDetection detection)
    {
        _currentDetection = detection;
        
        // Update overlay
        _overlayWindow?.UpdatePosition();
        _overlayWindow?.DrawBoundingBox(detection.BoundingBox, detection.Text);

        // Play beep
        System.Media.SystemSounds.Beep.Play();

        // Move cursor if enabled
        if (MoveCursorCheckBox.IsChecked == true && _selectedWindow != IntPtr.Zero)
        {
            _mouseController?.MoveCursor(_selectedWindow, detection.BoundingBox);
        }

        Dispatcher.Invoke(() =>
        {
            UpdateStatus($"Detected: {detection.Text} (Confidence: {detection.Confidence:F2})");
            AppendLog($"Stable detection: {detection.Text} at {detection.BoundingBox}");
        });

        // If in scroll search mode, stop searching
        if (_isScrollSearchActive)
        {
            _logger.Log($"Scroll search found target: {detection.Text}");
            CancelScrollSearch();
        }
    }

    private void OnClickHotkeyPressed(object? sender, EventArgs e)
    {
        if (!_isMonitoring || _selectedWindow == IntPtr.Zero)
        {
            _logger.Log("Click hotkey ignored - not monitoring or no window selected");
            return;
        }

        // Check if window is foreground
        var foregroundWindow = WindowPicker.GetForegroundWindowHandle();
        if (foregroundWindow != _selectedWindow)
        {
            _logger.Log("Click hotkey ignored - target window is not foreground");
            Dispatcher.Invoke(() => AppendLog("Click blocked: Target window not in foreground"));
            return;
        }

        // Check if we have a current detection
        if (_currentDetection != null && _stabilityTracker != null)
        {
            // Verify detection is still present
            var stableDetection = _stabilityTracker.GetStableDetection(_currentDetection.Text);
            if (stableDetection != null)
            {
                _mouseController?.ClickAtPosition(_selectedWindow, stableDetection.BoundingBox);
                Dispatcher.Invoke(() =>
                {
                    AppendLog($"Clicked on '{stableDetection.Text}'");
                    UpdateStatus($"Clicked: {stableDetection.Text}");
                });
            }
            else
            {
                _logger.Log("Click blocked - detection no longer stable");
                Dispatcher.Invoke(() => AppendLog("Click blocked: Detection no longer present"));
            }
        }
        else
        {
            _logger.Log("Click hotkey ignored - no stable detection");
            Dispatcher.Invoke(() => AppendLog("No stable detection to click"));
        }
    }

    private void OnScrollHotkeyPressed(object? sender, EventArgs e)
    {
        if (!_isMonitoring || _selectedWindow == IntPtr.Zero)
        {
            _logger.Log("Scroll hotkey ignored - not monitoring or no window selected");
            return;
        }

        if (_isScrollSearchActive)
        {
            _logger.Log("Scroll search already active");
            return;
        }

        StartScrollSearch();
    }

    private async void StartScrollSearch()
    {
        _isScrollSearchActive = true;
        _scrollStepCount = 0;
        
        _logger.Log("Starting scroll search");
        Dispatcher.Invoke(() =>
        {
            AppendLog("Scroll search started (Press Esc to cancel)");
            UpdateStatus("Scroll search active...");
        });

        // Perform stepwise scrolling
        while (_isScrollSearchActive && _scrollStepCount < MaxScrollSteps)
        {
            _scrollStepCount++;
            _mouseController?.ScrollDown(_selectedWindow);
            
            Dispatcher.Invoke(() => UpdateStatus($"Scroll search: Step {_scrollStepCount}/{MaxScrollSteps}"));
            
            // Wait for a moment to allow OCR to process
            await Task.Delay(500);

            // Check if target found (will be handled by OnStableDetectionFound)
        }

        if (_isScrollSearchActive)
        {
            // Reached max steps without finding target
            CancelScrollSearch();
            Dispatcher.Invoke(() =>
            {
                AppendLog($"Scroll search completed - {MaxScrollSteps} steps, no target found");
                UpdateStatus("Scroll search completed - no target found");
            });
        }
    }

    private void CancelScrollSearch()
    {
        _isScrollSearchActive = false;
        _scrollStepCount = 0;
        _logger.Log("Scroll search canceled");
        Dispatcher.Invoke(() =>
        {
            AppendLog("Scroll search canceled");
            UpdateStatus("Scroll search canceled");
        });
    }

    private void UpdateStatus(string message)
    {
        StatusTextBlock.Text = message;
    }

    private void AppendLog(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        LogTextBlock.Text += $"[{timestamp}] {message}\n";
        
        // Auto-scroll to bottom
        if (LogTextBlock.Parent is System.Windows.Controls.ScrollViewer scrollViewer)
        {
            scrollViewer.ScrollToBottom();
        }
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        StopMonitoring();
        _hotkeyService?.Dispose();
        _logger.Log("Application closed");
    }
}

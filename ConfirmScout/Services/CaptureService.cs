using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ConfirmScout.Services;

public class CaptureService : IDisposable
{
    private readonly Logger _logger;
    private System.Threading.Timer? _captureTimer;
    private IntPtr _targetWindow;
    private bool _isCapturing;

    public event EventHandler<byte[]>? FrameCaptured;

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public CaptureService(Logger logger)
    {
        _logger = logger;
    }

    public void StartCapture(IntPtr windowHandle, int fps = 2)
    {
        if (_isCapturing)
        {
            StopCapture();
        }

        _targetWindow = windowHandle;
        _isCapturing = true;
        
        int interval = 1000 / fps;
        _captureTimer = new System.Threading.Timer(CaptureFrame, null, 0, interval);

        _logger.Log($"Capture started for window {windowHandle} at {fps} fps");
    }

    private void CaptureFrame(object? state)
    {
        if (!_isCapturing || _targetWindow == IntPtr.Zero)
            return;

        try
        {
            // Get window rectangle
            if (!GetWindowRect(_targetWindow, out RECT rect))
                return;

            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            if (width <= 0 || height <= 0)
                return;

            // Capture window using GDI
            IntPtr hWnd = _targetWindow;
            IntPtr hdcSrc = GetWindowDC(hWnd);
            
            using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                IntPtr hdcDest = graphics.GetHdc();
                BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, TernaryRasterOperations.SRCCOPY);
                graphics.ReleaseHdc(hdcDest);
            }

            ReleaseDC(hWnd, hdcSrc);

            // Convert bitmap to PNG byte array
            using var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            byte[] imageData = ms.ToArray();

            FrameCaptured?.Invoke(this, imageData);
        }
        catch (Exception ex)
        {
            _logger.Log($"Frame capture error: {ex.Message}");
        }
    }

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight,
        IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

    private enum TernaryRasterOperations : uint
    {
        SRCCOPY = 0x00CC0020
    }

    public void StopCapture()
    {
        _isCapturing = false;
        _captureTimer?.Dispose();
        _captureTimer = null;
        _logger.Log("Capture stopped");
    }

    public void Dispose()
    {
        StopCapture();
        GC.SuppressFinalize(this);
    }
}

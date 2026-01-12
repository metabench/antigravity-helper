using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ConfirmScout.Windows;

public partial class OverlayWindow : Window
{
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    private IntPtr _targetWindow;

    public OverlayWindow()
    {
        InitializeComponent();
    }

    public void SetTargetWindow(IntPtr windowHandle)
    {
        _targetWindow = windowHandle;
        UpdatePosition();
    }

    public void UpdatePosition()
    {
        if (_targetWindow == IntPtr.Zero)
            return;

        if (GetWindowRect(_targetWindow, out RECT rect))
        {
            Left = rect.Left;
            Top = rect.Top;
            Width = rect.Right - rect.Left;
            Height = rect.Bottom - rect.Top;
        }
    }

    public void DrawBoundingBox(Rect boundingBox, string text)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            // Clear previous drawings
            OverlayCanvas.Children.Clear();

            // Create rectangle
            var rectangle = new Rectangle
            {
                Stroke = Brushes.Red,
                StrokeThickness = 3,
                Width = boundingBox.Width,
                Height = boundingBox.Height
            };

            // Position the rectangle
            System.Windows.Controls.Canvas.SetLeft(rectangle, boundingBox.X);
            System.Windows.Controls.Canvas.SetTop(rectangle, boundingBox.Y);

            OverlayCanvas.Children.Add(rectangle);

            // Add label
            var label = new System.Windows.Controls.TextBlock
            {
                Text = text,
                Foreground = Brushes.Red,
                Background = Brushes.White,
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(2)
            };

            System.Windows.Controls.Canvas.SetLeft(label, boundingBox.X);
            System.Windows.Controls.Canvas.SetTop(label, boundingBox.Y - 20);

            OverlayCanvas.Children.Add(label);
        });
    }

    public void ClearOverlay()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            OverlayCanvas.Children.Clear();
        });
    }
}

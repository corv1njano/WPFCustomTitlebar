using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace CustomTitlebar1.Utilities
{
    public class WindowWrapper
    {
        private readonly Window _targetWindow;
        private readonly Thickness _baseBorder;

        private const int SM_CXPADDEDBORDER = 92;
        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        public WindowWrapper(Window targetWindow)
        {
            _targetWindow = targetWindow;
            _baseBorder = _targetWindow.BorderThickness;

            _targetWindow.StateChanged += OnStateChanged;
        }

        public void CloseWindow()
        {
            _targetWindow.Close();
        }

        public void MinimizeWindow()
        {
            _targetWindow.WindowState = WindowState.Minimized;
        }

        public void MaximizeOrRestoreWindow()
        {
            _targetWindow.WindowState = _targetWindow.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void OnStateChanged(object? sender, EventArgs e)
        {
            Thickness sysResize = SystemParameters.WindowResizeBorderThickness;

            double padded = GetPaddedBorder(_targetWindow);

            if (_targetWindow.WindowState == WindowState.Maximized)
            {
                _targetWindow.BorderThickness = new Thickness(
                    _baseBorder.Left + sysResize.Left + padded,
                    _baseBorder.Top + sysResize.Top + padded,
                    _baseBorder.Right + sysResize.Right + padded,
                    _baseBorder.Bottom + sysResize.Bottom + padded
                );
            }
            else
            {
                _targetWindow.BorderThickness = _baseBorder;
            }
        }

        private static double GetPaddedBorder(Visual visual)
        {
            var dpi = VisualTreeHelper.GetDpi(visual);
            int paddedPx = GetSystemMetrics(SM_CXPADDEDBORDER);

            return paddedPx / dpi.DpiScaleX;
        }
    }
}

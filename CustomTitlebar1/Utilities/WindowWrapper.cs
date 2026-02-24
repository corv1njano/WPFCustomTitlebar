using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shell;

namespace CustomTitlebar1.Utilities
{
    public class WindowWrapper
    {
        private readonly Window _targetWindow;
        private readonly Thickness _baseBorder;

        private const int SM_CXPADDEDBORDER = 92;
        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        /// <summary>
        /// Initializes a new instance of the WindowWrapper class for the specified window.
        /// </summary>
        /// <param name="targetWindow">The Window instance to be wrapped. Cannot be null.</param>
        public WindowWrapper(Window targetWindow)
        {
            _targetWindow = targetWindow;
            _baseBorder = targetWindow.BorderThickness;

            _targetWindow.StateChanged += OnStateChanged;
        }

        /// <summary>
        /// Closes the target window.
        /// </summary>
        public void CloseWindow()
        {
            _targetWindow.Close();
        }

        /// <summary>
        /// Minimizes the target window.
        /// </summary>
        public void MinimizeWindow()
        {
            _targetWindow.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Toggles the window state between maximized and normal for the target window.
        /// </summary>
        /// <remarks>If the window is currently maximized, calling this method restores it to its normal
        /// size. If the window is in any other state, it is maximized.</remarks>
        public void MaximizeOrRestoreWindow()
        {
            _targetWindow.WindowState = _targetWindow.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        /// <summary>
        /// Calculates the height of the title bar area as a GridLength value for layout purposes based on
        /// <see cref="WindowChrome.CaptionHeight"/> and <see cref="WindowChrome.ResizeBorderThickness"/>.Top of the target
        /// window.
        /// </summary>
        /// <returns>A GridLength representing the combined height of the window's caption and top resize border. Returns a
        /// GridLength of 0 if the window chrome is not available.</returns>
        /// <remarks>Can be applied to the <see cref="System.Windows.Controls.Grid"/> Height property.</remarks>
        public GridLength GetTitlebarGridHeight()
        {
            var chrome = WindowChrome.GetWindowChrome(_targetWindow);
            if (chrome == null) return new(0);

            return new GridLength(chrome.CaptionHeight + chrome.ResizeBorderThickness.Top, GridUnitType.Pixel);
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

using CustomTitlebar1.Utilities;
using System.Windows;
using System.Windows.Media;

namespace CustomTitlebar1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly WindowWrapper _wrapper;

        public MainWindow()
        {
            InitializeComponent();
            _wrapper = new WindowWrapper(this);

            StateChanged += MainWindow_StateChanged;
        }

        private void MainWindow_StateChanged(object? sender, EventArgs e)
        {
            MaximizeRestoreIcon.ImageSource = WindowState switch
            {
                WindowState.Maximized => (ImageSource)FindResource("WindowControl.Restore"),
                WindowState.Normal => (ImageSource)FindResource("WindowControl.Maximize"),
                _ => MaximizeRestoreIcon.ImageSource
            };
        }

        private void Button_Minimize(object sender, RoutedEventArgs e)
        {
            _wrapper.MinimizeWindow();
        }

        private void Button_MaximizeRestore(object sender, RoutedEventArgs e)
        {
            _wrapper.MaximizeOrRestoreWindow();
        }

        private void Button_Close(object sender, RoutedEventArgs e)
        {
            _wrapper.CloseWindow();
        }
    }
}

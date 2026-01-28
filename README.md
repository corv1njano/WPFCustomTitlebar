Response to: https://stackoverflow.com/questions/29391063/wpf-maximized-window-bigger-than-screen/79878335#79878335

---

GitHub repo for those who want to download and test it out.

Since there hasn't been a "proper" solution to this problem, I thought I'd share my answer. First of all, I don't like to reset all window properties with `AllowsTransparency="True"` and `WindowStyle="None"`. You will lose all native Windows API features for you window (smooth animations, titlebar behaviour, window states etc.). So, we just use WindowChrome. Add the following to your window XAML:

```
<WindowChrome.WindowChrome>
    <WindowChrome ResizeBorderThickness="8"
                  UseAeroCaptionButtons="False"
                  CaptionHeight="32"/>
</WindowChrome.WindowChrome>
```

- **ResizeBorderThickness** draws an invisible border around your window to drag for resizing (set to 0 if you don't want the ability to resize, in addition to setting your window property to ResizeMode="None")
- **UseAeroCaptionButtons** hides the default window controls buttons, we will use our own ones
- **CaptionHeight** is the height of your titlebar, keep in mind that the actual titlebar height will be ResizeBorderThickness + CaptionHeight, e.g for Grid based titlebar overlay (Height = 40)

I then created a helper class to implement the window resize movement (maximize/restore calculation) and window control logic (our own implementation of Minimize/Maximize/Restore/Close). This is the class (I have put it under my Utilities folder in my project files) [for those who actually care about why this works and not just copy the code, explanation down below]:

```
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace CustomTitlebar.Utilities
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
```

In code behind (yes, this is still MVVM-friendly) wrap this helper class around your window:

```
private readonly WindowWrapper _wrapper;

public MainWindow()
{
    InitializeComponent();
    _wrapper = new WindowWrapper(this);
}
```

Explanation on how and why this works (or at least it should):

- `_targetWindow` stores the window to apply the logic to (every window needs it's own wrapper)

- `_baseBorder` stores the default window state thickness, we need to extended it when maximizing, when going back to windowed state (WindowState = Normal) we need to restore the old thickness, stored in this variable

- `OnStateChanged()` gets called when window state changes (e.g. when window has been maximized/minimized/restored), this is where the magic happens: the window thickness when maximized gets calculated through this formular _baseBorder + SystemParamter for ResizeBorderThickness + DIP Padding

- `GetPaddedBorder(<target window>)` is a small helper method to convert windows pixels to WPF DIP (device independent pixels), this makes sure that the Thickness is calculated correctly depending on DPI, zoom (like 125%, 300% etc.) and multi monitor setup

- `SM_CXPADDEDBORDER = 92` is a Windows const (read here) for our border padding, sadly WPF doesn't know what that border padding is, so we gotta get it from the Windows UI-API (user32.dll) with GetSystemMetrics(<metric index here>)

I also implemented small helper methods for window control (`CloseWindow(), MinimizeWindow(), MaximizeOrRestoreWindow()`). Those are optional, but since we hide the default aero buttons we gotta use our own logic, which is very easy to implement. You can call them in code behind, e.g. like this (yes, this is still MVVM friendly):

```
private void Button_Minimize(object sender, RoutedEventArgs e)
{
    _wrapper.MinimizeWindow();
}
```

You can then overlay your own titlebar, which in my case is just a WPF grid. Here's an example code for that (you may need to replace the static resources with you own ones):

```
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="40"/> <!-- CaptionHeight + ResizeBorderThickness -->
        <RowDefinition Height="*"/>
    </Grid.RowDefinitions>

    <!-- titlebar -->
    <Grid Grid.Row="0" Background="{StaticResource Titlebar.Background}">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="*"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>
        <StackPanel Grid.Column="0" Orientation="Horizontal">
            <Image Source="/Assets/Images/app_logo.png"
                   Width="20"
                   Height="20"
                   Margin="12,0"
                   VerticalAlignment="Center"
                   RenderOptions.BitmapScalingMode="HighQuality"/>
            <TextBlock Text="Application Name"
                       FontFamily="Arial"
                       Foreground="{DynamicResource Font.Light}"
                       FontSize="15"
                       VerticalAlignment="Center"/>
        </StackPanel>

        <StackPanel Grid.Column="1" Orientation="Horizontal" HorizontalAlignment="Right">
            <StackPanel.Resources>
                <Style TargetType="Rectangle">
                    <Setter Property="Height" Value="10"/>
                    <Setter Property="Width" Value="10"/>
                    <Setter Property="Fill" Value="{DynamicResource Font.Light}"/>
                </Style>
            </StackPanel.Resources>
            
            <Button Style="{StaticResource Button.WindowControl}" Click="Button_Minimize">
                <Button.Content>
                    <Rectangle>
                        <Rectangle.OpacityMask>
                            <ImageBrush ImageSource="{StaticResource WindowControl.Minimize}" Stretch="Uniform"/>
                        </Rectangle.OpacityMask>
                    </Rectangle>
                </Button.Content>
            </Button>
            <Button Style="{StaticResource Button.WindowControl}" Click="Button_MaximizeRestore">
                <Button.Content>
                    <Rectangle>
                        <Rectangle.OpacityMask>
                            <ImageBrush x:Name="MaximizeRestoreIcon" ImageSource="{StaticResource WindowControl.Maximize}"/>
                        </Rectangle.OpacityMask>
                    </Rectangle>
                </Button.Content>
            </Button>
            <Button Style="{StaticResource Button.WindowControl.Close}" Click="Button_Close">
                <Button.Content>
                    <Rectangle>
                        <Rectangle.OpacityMask>
                            <ImageBrush ImageSource="{StaticResource WindowControl.Close}"/>
                        </Rectangle.OpacityMask>
                    </Rectangle>
                </Button.Content>
            </Button>
        </StackPanel>
    </Grid>

    <!-- content -->
    <Grid Grid.Row="1">
        
    </Grid>
</Grid>
```

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using System;
using System.Application.Services;
using System.Application.Settings;

namespace System.Application.UI.Views.Controls
{
    public class WallpaperControl : TemplatedControl
    {
        readonly INativeWindowApiService? windowApiService = INativeWindowApiService.Instance;

        Window? window;
        Window? ParentWindow;
        IntPtr _Handle;
        IntPtr _DwmHandle;
        public WallpaperControl()
        {
            //this.InitializeComponent();

            if (OperatingSystem2.IsWindows)
            {
                this.GetObservable(IsVisibleProperty)
                    .Subscribe(x =>
                    {
                        if (x)
                        {
                            if (window == null)
                            {
                                LayoutUpdated += EmptyControl_LayoutUpdated;
                                AttachedToVisualTree += EmptyControl_AttachedToVisualTree;
                                DetachedFromVisualTree += EmptyControl_DetachedFromVisualTree;
                                window = new Window
                                {
                                    Width = Bounds.Width,
                                    Height = Bounds.Height,
                                    Background = null,
                                    WindowStartupLocation = WindowStartupLocation.Manual,
                                    WindowState = WindowState.Normal,
                                    ExtendClientAreaToDecorationsHint = true,
                                    ExtendClientAreaTitleBarHeightHint = -1,
                                    ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome,
                                    SystemDecorations = SystemDecorations.Full,
                                    CanResize = false,
                                    TransparencyLevelHint = WindowTransparencyLevel.Transparent,
                                    ShowInTaskbar = false,
                                    Topmost = false,
                                    Focusable = false,
                                    IsEnabled = false,
                                    ShowActivated = false,
                                    IsTabStop = false,
                                    IsHitTestVisible = false,
                                };
                                window.GotFocus += Window_GotFocus;
                                if (Parent != null && VisualRoot != null)
                                    EmptyControl_AttachedToVisualTree(null, new VisualTreeAttachmentEventArgs(Parent, VisualRoot));
                            }
                        }
                        else
                        {
                            if (window != null)
                            {
                                if (Parent != null && VisualRoot != null)
                                    EmptyControl_DetachedFromVisualTree(null, new VisualTreeAttachmentEventArgs(Parent, VisualRoot));
                                LayoutUpdated -= EmptyControl_LayoutUpdated;
                                AttachedToVisualTree -= EmptyControl_AttachedToVisualTree;
                                DetachedFromVisualTree -= EmptyControl_DetachedFromVisualTree;
                                window = null;
                            }
                        }
                    });
            }
        }

        private void Window_GotFocus(object? sender, Avalonia.Input.GotFocusEventArgs e)
        {
            ParentWindow?.Focus();
        }

        private void EmptyControl_DetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (windowApiService == null) return;
            windowApiService.ReleaseBackground(_DwmHandle);
            ParentWindow!.PositionChanged -= Parent_PositionChanged;
            ParentWindow.Closing -= ParentWindow_Closing;
            ParentWindow.GotFocus -= ParentWindow_GotFocus;
            Close();
        }

        private void EmptyControl_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (windowApiService == null || window == null) return;
            ParentWindow = (Window)e.Root;
            ParentWindow.Topmost = true;
            Show();
            ParentWindow.Topmost = false;
            ParentWindow.PositionChanged += Parent_PositionChanged;
            ParentWindow.Closing += ParentWindow_Closing;
            ParentWindow.GotFocus += ParentWindow_GotFocus;
            ParentWindow.Opened += ParentWindow_Opened;
            _Handle = window.PlatformImpl.Handle.Handle;
            windowApiService.SetWindowPenetrate(_Handle);
            //windowApiService.SetParentWindow(_Handle, ParentWindow.PlatformImpl.Handle.Handle);
            _DwmHandle = windowApiService.SetDesktopBackgroundToWindow(
                _Handle, (int)window.Width, (int)window.Height);
        }

        private void ParentWindow_Opened(object? sender, EventArgs e)
        {
            if (window != null)
                Show();
        }

        private void ParentWindow_GotFocus(object? sender, Avalonia.Input.GotFocusEventArgs e)
        {
            if (window == null || ParentWindow == null) return;
            window.Topmost = true;
            window.Topmost = false;
            ParentWindow.Topmost = true;
            ParentWindow.Topmost = false;
        }

        private void ParentWindow_Closing(object? sender, ComponentModel.CancelEventArgs e)
        {
            window?.Hide();
        }

        private void Parent_PositionChanged(object? sender, PixelPointEventArgs e)
        {
            if (window == null) return;
            window.Position = this.PointToScreen(Bounds.Position);
        }

        private void EmptyControl_LayoutUpdated(object? sender, EventArgs e)
        {
            if (windowApiService == null || window == null) return;
            window.Position = this.PointToScreen(Bounds.Position);
            window.Width = Bounds.Width;
            window.Height = Bounds.Height;
            windowApiService.BackgroundUpdate(_DwmHandle, (int)window.Width, (int)window.Height);
            //NativeMethods.SetWindowPos(HWND, NativeMethods.HWND_TOPMOST, window.Position.X, window.Position.Y, (int)window.Width, (int)window.Height, 0);
        }

        public WindowState WindowState
        {
            get { return window?.WindowState ?? default; }
            set { if (window != null) window.WindowState = value; }
        }

        public void Show()
        {
            window?.Show();
        }

        public IntPtr Handle
        {
            get { return _Handle; }
        }

        public void Close()
        {
            window?.Close();
        }
    }
}

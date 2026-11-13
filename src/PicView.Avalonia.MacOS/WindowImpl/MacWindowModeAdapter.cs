using Avalonia.Threading;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.Interfaces;
using PicView.Avalonia.UI;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.Sizing;

namespace PicView.Avalonia.MacOS.WindowImpl;

/// <summary>
/// What macOS does natively when a window changes Window Mode.
/// </summary>
public sealed class MacWindowModeAdapter : IWindowModeAdapter
{
    /// <summary>
    /// macOS reports the window's state once as it opens, which is not a change the user made.
    /// </summary>
    public bool SkipsInitialNativeState => true;

    public void EnterNativeState(MainWindow window, WindowModeState state)
    {
        if (state.Mode is WindowMode.Fullscreen)
        {
            // Root cause of the full-screen white strip (found by reading the macOS Window.axaml
            // template, not by guessing): the window template paints a full-window white
            // Background, and the content is inset at the top by WindowDecorationMargin whenever
            // ExtendClientAreaToDecorationsHint is true. That top inset is exactly the title-bar
            // height and is normally filled by MacOSTitlebar. In full screen the toolbar is hidden,
            // so the inset exposes the white background and shows as a white bar. Collapsing the
            // extended client area removes the inset so the image content fills the whole window.
            window.ExtendClientAreaToDecorationsHint = false;
            WindowModeSwitcher.SetNativeState(window, state.Mode);
            return;
        }

        if (state.Mode is not WindowMode.Normal)
        {
            WindowModeSwitcher.SetNativeState(window, state.Mode);
            return;
        }

        // Disabling ExtendClientAreaToDecorationsHint and enabling it again fixes not being able to draw in title bar
        window.ExtendClientAreaToDecorationsHint = false;
        WindowModeSwitcher.SetNativeState(window, state.Mode);
        Dispatcher.UIThread.Post(() =>
        {
            window.ExtendClientAreaToDecorationsHint = true;
            if (state.AutoFit)
            {
                window.WindowFitter.FollowContent();
            }
        });
    }
}

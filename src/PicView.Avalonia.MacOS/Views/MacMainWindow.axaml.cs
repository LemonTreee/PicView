using Avalonia;
using Avalonia.Controls;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.Interfaces;
using PicView.Avalonia.MacOS.WindowImpl;
using PicView.Core.DebugTools;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Avalonia.MacOS.Views;

public partial class MacMainWindow : MainWindow
{
    public MacMainWindow() : this(null)
    {
    }

    /// <param name="firstTab">A tab moved out of another Main Window, for a window that opens with it.</param>
    public MacMainWindow(CoreViewModel? core, TabViewModel? firstTab = null)
    {
        core ??= Application.Current?.DataContext as CoreViewModel;
        if (core is null)
        {
            return;
        }
        var mainWindowViewModel = new MainWindowViewModel(core, core.Translation, core.GlobalSettings, core.GallerySettings, firstTab);
        DataContext = mainWindowViewModel;

        InitializeComponent();
        
        SharedBottomBar = BottomBar;
        SharedTitleBar = Titlebar;
        SharedMainView = MainView;
        UIHelper.Initialize(this);

        Loaded += delegate
        {
            if (DataContext is not MainWindowViewModel vm)
            {
                return;
            }
            
            // Keep the window borderless whenever the top toolbar is hidden. In full screen the
            // interface is hidden (see InterfaceVisibility.For) and macOS forces an opaque native
            // title bar, so going borderless there avoids the empty white strip at the top; the
            // transparent, extended title bar is re-applied by MacWindowModeAdapter on entering
            // full screen.
            void UpdateDecorations()
            {
                WindowDecorations = vm.IsFullscreen.Value || !vm.IsTopToolbarShown.Value
                    ? WindowDecorations.None
                    : WindowDecorations.Full;
            }

            Observable.EveryValueChanged(vm, x => x.IsTopToolbarShown.Value, FrameProvider)
                .Subscribe(_ => UpdateDecorations(), static result =>
                {
#if DEBUG
                    if (result is { IsFailure: true, Exception: not null })
                    {
                        DebugHelper.LogDebug(nameof(MacMainWindow), nameof(vm.IsTopToolbarShown), result.Exception);
                    }
#endif
                })
                .AddTo(Disposables);

            Observable.EveryValueChanged(vm, x => x.IsFullscreen.Value, FrameProvider)
                .Subscribe(_ => UpdateDecorations(), static result =>
                {
#if DEBUG
                    if (result is { IsFailure: true, Exception: not null })
                    {
                        DebugHelper.LogDebug(nameof(MacMainWindow), nameof(vm.IsFullscreen), result.Exception);
                    }
#endif
                })
                .AddTo(Disposables);
        };
    }

    protected override IWindowProvider CreateWindowProvider() => new MacWindowProvider();
}

using System.Windows;
using PocketLink.App.ViewModels;
using PocketLink.App.Views;
using PocketLink.Contracts.Interfaces;
using PocketLink.Infrastructure.Mock;
using PocketLink.Infrastructure.Sampling;

namespace PocketLink.App;

/// <summary>
/// 应用入口。ShutdownMode=OnExplicitShutdown 配合托盘图标，
/// 关闭主窗口不会退出进程，仅托盘菜单“退出”会调用 Shutdown。
/// </summary>
public partial class App : Application
{
    private MainWindow? _mainWindow;
    private TrayIconController? _trayController;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        IDeviceAdapter adapter = new MockDeviceAdapter(new MockDeviceAdapterOptions
        {
            Scenario = MockScenario.Normal,
        });
        var sampler = new DeviceSampler(adapter);
        var viewModel = new DashboardViewModel(sampler);

        _mainWindow = new MainWindow { DataContext = viewModel };

        _trayController = new TrayIconController(
            showWindow: () => ShowMainWindow(),
            hideWindow: () => _mainWindow?.Hide(),
            exitApplication: () => Shutdown());

        _mainWindow.Closing += (_, args) =>
        {
            args.Cancel = true;
            _mainWindow?.Hide();
        };

        ShowMainWindow();
        viewModel.StartSampling();
    }

    private void ShowMainWindow()
    {
        if (_mainWindow is null)
        {
            return;
        }

        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayController?.Dispose();
        base.OnExit(e);
    }
}

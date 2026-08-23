using System.Windows;
using H.NotifyIcon;

namespace PocketLink.App.Views;

/// <summary>
/// 托盘图标控制器：显示/隐藏主窗口、退出应用。使用系统默认图标占位，
/// 避免在无美术资源阶段引入额外二进制资产；后续可替换为正式 .ico。
/// </summary>
public sealed class TrayIconController : IDisposable
{
    private readonly TaskbarIcon _taskbarIcon;

    public TrayIconController(Action showWindow, Action hideWindow, Action exitApplication)
    {
        _taskbarIcon = new TaskbarIcon
        {
            ToolTipText = "PocketLink Monitor",
            Icon = System.Drawing.SystemIcons.Application,
        };

        var showItem = new System.Windows.Controls.MenuItem { Header = "显示窗口" };
        showItem.Click += (_, _) => showWindow();

        var hideItem = new System.Windows.Controls.MenuItem { Header = "隐藏窗口" };
        hideItem.Click += (_, _) => hideWindow();

        var exitItem = new System.Windows.Controls.MenuItem { Header = "退出" };
        exitItem.Click += (_, _) => exitApplication();

        var contextMenu = new System.Windows.Controls.ContextMenu();
        contextMenu.Items.Add(showItem);
        contextMenu.Items.Add(hideItem);
        contextMenu.Items.Add(new System.Windows.Controls.Separator());
        contextMenu.Items.Add(exitItem);

        _taskbarIcon.ContextMenu = contextMenu;
        _taskbarIcon.TrayLeftMouseUp += (_, _) => showWindow();
        _taskbarIcon.ForceCreate();
    }

    public void Dispose()
    {
        _taskbarIcon.Dispose();
    }
}

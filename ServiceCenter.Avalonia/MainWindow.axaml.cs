using Avalonia.Controls;
using ServiceCenter.Avalonia.Pages;

namespace ServiceCenter.Avalonia;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        MainControl.Content = new Login();
    }

    public void ShowShell(bool show)
    {
        RootGrid.ColumnDefinitions[0].Width = show ? new GridLength(230) : new GridLength(0);
        if (!show)
            return;
        Menu.Refresh();
        RoleText.Text = App.CurrentUser == null ? "" : App.CurrentUser.FullName;
    }
}

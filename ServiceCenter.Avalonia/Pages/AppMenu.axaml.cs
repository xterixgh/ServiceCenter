using Avalonia.Controls;

namespace ServiceCenter.Avalonia.Pages
{
    public partial class AppMenu : UserControl
    {
        public AppMenu()
        {
            InitializeComponent();
            Refresh();
        }

        public void Refresh()
        {
            string role = App.CurrentUser == null ? "" : App.CurrentUser.Role;
            bool office = role == "administrator" || role == "dispatcher";
            ClientsButton.IsVisible = office;
            DictButton.IsVisible = role == "administrator";
            EmployeesButton.IsVisible = role == "administrator";
        }

        private void Open(UserControl page)
        {
            if (TopLevel.GetTopLevel(this) is MainWindow window)
                window.MainControl.Content = page;
        }

        private void Tickets_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e) => Open(new TicketList());
        private void Clients_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e) => Open(new ClientsPage());
        private void Dict_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e) => Open(new DictionariesPage());
        private void Employees_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e) => Open(new EmployeesPage());
        private void Stats_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e) => Open(new StatisticsPage());

        private void Logout_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            App.CurrentUser = null;
            if (TopLevel.GetTopLevel(this) is MainWindow window)
            {
                window.ShowShell(false);
                window.MainControl.Content = new Login();
            }
        }
    }
}

using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using ServiceCenter.Data;

namespace ServiceCenter.Avalonia.Pages
{
    public partial class Login : UserControl
    {
        public Login()
        {
            InitializeComponent();
        }

        private async void LoginButton_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            string login = LoginBox.Text == null ? "" : LoginBox.Text.Trim();
            string password = PasswordBox.Text == null ? "" : PasswordBox.Text;
            if (login == "" || password == "")
            {
                ErrorText.Text = "Неверный логин или пароль";
                ErrorText.IsVisible = true;
                return;
            }

            var user = await App.DbContext.Users.FirstOrDefaultAsync(u => u.Login == login);
            if (user == null || !user.IsActive || !PasswordHasher.Verify(password, user.PasswordHash))
            {
                ErrorText.Text = "Неверный логин или пароль";
                ErrorText.IsVisible = true;
                return;
            }

            App.CurrentUser = user;
            if (TopLevel.GetTopLevel(this) is MainWindow window)
            {
                window.ShowShell(true);
                window.MainControl.Content = new TicketList();
            }
        }
    }
}

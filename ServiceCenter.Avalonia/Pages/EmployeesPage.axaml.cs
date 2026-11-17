using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using ServiceCenter.Data;
using ServiceCenter.Data.Models;
using System;
using System.Linq;

namespace ServiceCenter.Avalonia.Pages
{
    public partial class EmployeesPage : UserControl
    {
        public EmployeesPage()
        {
            InitializeComponent();
            RoleBox.ItemsSource = new string[] { "administrator", "dispatcher", "master" };
            Loaded += async (_, _) => await Reload();
        }

        private async System.Threading.Tasks.Task Reload()
        {
            var users = await App.DbContext.Users.OrderBy(u => u.FullName).ToListAsync();
            UserList.ItemsSource = users.Select(u => new EmployeeLine(u)).ToList();
        }

        private void UserList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (UserList.SelectedItem is not EmployeeLine line)
                return;
            LoginBox.Text = line.User.Login;
            NameBox.Text = line.User.FullName;
            PasswordBox.Text = "";
            RoleBox.SelectedItem = line.User.Role;
            ActiveBox.IsChecked = line.User.IsActive;
            ErrorText.IsVisible = false;
        }

        private void New_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            UserList.SelectedItem = null;
            LoginBox.Text = "";
            NameBox.Text = "";
            PasswordBox.Text = "";
            ActiveBox.IsChecked = true;
            ErrorText.IsVisible = false;
        }

        private async void Save_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoginBox.Text) || string.IsNullOrWhiteSpace(NameBox.Text) || RoleBox.SelectedItem is not string role)
            {
                ErrorText.Text = "Заполните логин, имя и роль";
                ErrorText.IsVisible = true;
                return;
            }

            string login = LoginBox.Text.Trim();
            EmployeeLine? selected = UserList.SelectedItem as EmployeeLine;
            bool taken = await App.DbContext.Users.AnyAsync(u => u.Login == login && (selected == null || u.Id != selected.User.Id));
            if (taken)
            {
                ErrorText.Text = "Такой логин уже занят";
                ErrorText.IsVisible = true;
                return;
            }

            if (selected == null && string.IsNullOrWhiteSpace(PasswordBox.Text))
            {
                ErrorText.Text = "Укажите пароль";
                ErrorText.IsVisible = true;
                return;
            }

            if (selected == null)
            {
                App.DbContext.Users.Add(new User
                {
                    Login = login,
                    FullName = NameBox.Text.Trim(),
                    PasswordHash = PasswordHasher.Hash(PasswordBox.Text ?? ""),
                    Role = role,
                    IsActive = ActiveBox.IsChecked == true,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                User user = selected.User;
                user.Login = login;
                user.FullName = NameBox.Text.Trim();
                user.Role = role;
                user.IsActive = ActiveBox.IsChecked == true;
                if (!string.IsNullOrWhiteSpace(PasswordBox.Text))
                    user.PasswordHash = PasswordHasher.Hash(PasswordBox.Text ?? "");
            }

            await App.DbContext.SaveChangesAsync();
            PasswordBox.Text = "";
            ErrorText.IsVisible = false;
            await Reload();
        }

        private sealed class EmployeeLine
        {
            public EmployeeLine(User user) => User = user;
            public User User { get; }
            public override string ToString()
            {
                string state = User.IsActive ? "" : ", отключён";
                return User.FullName + ", " + User.Login + ", " + TicketRules.RoleTitle(User.Role) + state;
            }
        }
    }
}

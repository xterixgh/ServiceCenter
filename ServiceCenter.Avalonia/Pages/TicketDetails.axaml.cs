using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using ServiceCenter.Data;
using ServiceCenter.Data.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceCenter.Avalonia.Pages
{
    public partial class TicketDetails : UserControl
    {
        private readonly int _id;
        private Ticket? _ticket;

        public TicketDetails(int id)
        {
            _id = id;
            InitializeComponent();
            Loaded += TicketDetails_Loaded;
        }

        private async void TicketDetails_Loaded(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            MasterBox.ItemsSource = await App.DbContext.Users
                .Where(u => u.Role == "master" && u.IsActive)
                .OrderBy(u => u.FullName)
                .ToListAsync();
            StatusBox.ItemsSource = await App.DbContext.TicketStatuses.OrderBy(s => s.Id).ToListAsync();
            string role = App.CurrentUser == null ? "" : App.CurrentUser.Role;
            AssignPanel.IsVisible = role is "administrator" or "dispatcher";
            await Reload();
        }

        private async Task Reload()
        {
            _ticket = await App.DbContext.Tickets
                .Include(t => t.Client)
                .Include(t => t.DeviceType)
                .Include(t => t.ServiceItem)
                .Include(t => t.TicketStatus)
                .Include(t => t.Master)
                .FirstOrDefaultAsync(t => t.Id == _id);
            if (_ticket == null || App.CurrentUser == null)
                return;
            if (App.CurrentUser.Role == "master" && _ticket.Master_Id != App.CurrentUser.Id)
            {
                ErrorText.Text = "Эта заявка назначена другому мастеру";
                ErrorText.IsVisible = true;
                return;
            }

            TitleText.Text = "Заявка № " + _ticket.Id;
            string master = _ticket.Master == null ? "не назначен" : _ticket.Master.FullName;
            string due = _ticket.DueDate == null ? "срок не задан" : _ticket.DueDate.Value.ToString("dd.MM.yyyy");
            string source = _ticket.Source == "site" ? "сайт" : "сотрудник";
            InfoText.Text = _ticket.Client.FullName + " · " + _ticket.DeviceType.Name + " · " + _ticket.ServiceItem.Name
                + " · " + _ticket.TicketStatus.Name + " · источник: " + source + " · мастер: " + master + " · " + due;
            FaultText.Text = _ticket.FaultText;

            var comments = await App.DbContext.Comments
                .Include(c => c.User)
                .Where(c => c.Ticket_Id == _id)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
            CommentItems.ItemsSource = comments.Select(c => c.CreatedAt.ToString("dd.MM.yyyy HH:mm") + " · " + c.User.FullName + ": " + c.Text).ToList();
        }

        private async void Assign_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_ticket == null || MasterBox.SelectedItem is not User master)
            {
                ErrorText.Text = "Выберите мастера";
                ErrorText.IsVisible = true;
                return;
            }
            _ticket.Master_Id = master.Id;
            await App.DbContext.SaveChangesAsync();
            ErrorText.IsVisible = false;
            await Reload();
        }

        private async void Status_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_ticket == null || App.CurrentUser == null || StatusBox.SelectedItem is not TicketStatus next)
            {
                ErrorText.Text = "Выберите статус";
                ErrorText.IsVisible = true;
                return;
            }
            if (!TicketRules.CanMove(App.CurrentUser.Role, _ticket.TicketStatus.Code, next.Code))
            {
                ErrorText.Text = "Такой переход статуса для вашей роли запрещён";
                ErrorText.IsVisible = true;
                return;
            }
            int oldId = _ticket.Status_Id;
            _ticket.Status_Id = next.Id;
            App.DbContext.StatusLogs.Add(new StatusLog
            {
                Ticket_Id = _ticket.Id,
                User_Id = App.CurrentUser.Id,
                OldStatus_Id = oldId,
                NewStatus_Id = next.Id,
                CreatedAt = DateTime.UtcNow
            });
            await App.DbContext.SaveChangesAsync();
            ErrorText.IsVisible = false;
            await Reload();
        }

        private async void Comment_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_ticket == null || App.CurrentUser == null || string.IsNullOrWhiteSpace(CommentBox.Text))
            {
                ErrorText.Text = "Введите текст комментария";
                ErrorText.IsVisible = true;
                return;
            }
            App.DbContext.Comments.Add(new TicketComment
            {
                Ticket_Id = _ticket.Id,
                User_Id = App.CurrentUser.Id,
                Text = CommentBox.Text.Trim(),
                CreatedAt = DateTime.UtcNow
            });
            await App.DbContext.SaveChangesAsync();
            CommentBox.Text = "";
            ErrorText.IsVisible = false;
            await Reload();
        }

        private void Back_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (TopLevel.GetTopLevel(this) is MainWindow window)
                window.MainControl.Content = new TicketList();
        }
    }
}

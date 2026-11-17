using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.EntityFrameworkCore;
using ServiceCenter.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceCenter.Avalonia.Pages
{
    public partial class TicketList : UserControl
    {
        private TicketRow? _selected;

        public TicketList()
        {
            InitializeComponent();
            Loaded += TicketList_Loaded;
        }

        private async void TicketList_Loaded(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (App.CurrentUser == null)
                return;
            UserInfoText.Text = App.CurrentUser.FullName + " · " + ServiceCenter.Data.TicketRules.RoleTitle(App.CurrentUser.Role);
            AddButton.IsVisible = App.CurrentUser.Role is "administrator" or "dispatcher";
            StatusBox.ItemsSource = await App.DbContext.TicketStatuses.OrderBy(s => s.Id).ToListAsync();
            await LoadAsync();
        }

        private async Task LoadAsync()
        {
            if (App.CurrentUser == null)
                return;

            var query = App.DbContext.Tickets
                .Include(t => t.Client)
                .Include(t => t.TicketStatus)
                .Include(t => t.Master)
                .AsQueryable();

            if (App.CurrentUser.Role == "master")
                query = query.Where(t => t.Master_Id == App.CurrentUser.Id);

            if (!string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                string text = SearchBox.Text.Trim().ToLower();
                query = query.Where(t => t.Client.FullName.ToLower().Contains(text) || t.FaultText.ToLower().Contains(text));
            }

            if (StatusBox.SelectedItem is TicketStatus status)
                query = query.Where(t => t.Status_Id == status.Id);

            List<Ticket> tickets = await query.OrderByDescending(t => t.Id).ToListAsync();
            List<TicketRow> rows = new List<TicketRow>();
            foreach (Ticket ticket in tickets)
            {
                rows.Add(new TicketRow
                {
                    Ticket = ticket,
                    Number = "№ " + ticket.Id,
                    ClientName = ticket.Client.FullName,
                    Fault = ticket.FaultText,
                    StatusName = ticket.TicketStatus.Name,
                    MasterName = ticket.Master == null ? "не назначен" : ticket.Master.FullName
                });
            }

            TicketItems.ItemsSource = rows;
            StatusText.Text = "Заявок: " + rows.Count;
            _selected = null;
        }

        private void Row_Pressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is not Border border || border.Tag is not TicketRow row)
                return;
            _selected = row;
            if (e.ClickCount == 2 && TopLevel.GetTopLevel(this) is MainWindow window)
                window.MainControl.Content = new TicketDetails(row.Ticket.Id);
        }

        private async void Filter_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e) => await LoadAsync();

        private async void Reset_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            SearchBox.Text = "";
            StatusBox.SelectedItem = null;
            await LoadAsync();
        }

        private void Add_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (TopLevel.GetTopLevel(this) is MainWindow window)
                window.MainControl.Content = new TicketEdit();
        }
    }

    public class TicketRow
    {
        public Ticket Ticket { get; set; } = null!;
        public string Number { get; set; } = "";
        public string ClientName { get; set; } = "";
        public string Fault { get; set; } = "";
        public string StatusName { get; set; } = "";
        public string MasterName { get; set; } = "";
    }
}

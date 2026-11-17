using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using ServiceCenter.Data.Models;
using System;
using System.Linq;

namespace ServiceCenter.Avalonia.Pages
{
    public partial class TicketEdit : UserControl
    {
        public TicketEdit()
        {
            InitializeComponent();
            Loaded += TicketEdit_Loaded;
        }

        private async void TicketEdit_Loaded(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            ClientBox.ItemsSource = await App.DbContext.Clients.OrderBy(c => c.FullName).ToListAsync();
            DeviceBox.ItemsSource = await App.DbContext.DeviceTypes.OrderBy(c => c.Name).ToListAsync();
            ServiceBox.ItemsSource = await App.DbContext.Services.OrderBy(c => c.Name).ToListAsync();
        }

        private async void Save_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (ClientBox.SelectedItem is not Client client ||
                DeviceBox.SelectedItem is not DeviceType device ||
                ServiceBox.SelectedItem is not ServiceItem service ||
                string.IsNullOrWhiteSpace(FaultBox.Text))
            {
                ErrorText.Text = "Заполните клиента, устройство, услугу и неисправность";
                ErrorText.IsVisible = true;
                return;
            }

            DateTime? due = null;
            if (!string.IsNullOrWhiteSpace(DueBox.Text))
            {
                if (!DateTime.TryParse(DueBox.Text, out DateTime parsed))
                {
                    ErrorText.Text = "Срок укажите как ГГГГ-ММ-ДД";
                    ErrorText.IsVisible = true;
                    return;
                }
                due = DateTime.SpecifyKind(parsed.Date, DateTimeKind.Utc);
            }

            TicketStatus status = await App.DbContext.TicketStatuses.FirstAsync(s => s.Code == "accepted");
            Ticket ticket = new Ticket
            {
                Client_Id = client.Id,
                DeviceType_Id = device.Id,
                Service_Id = service.Id,
                Status_Id = status.Id,
                FaultText = FaultBox.Text.Trim(),
                DueDate = due,
                CreatedAt = DateTime.UtcNow
            };
            App.DbContext.Tickets.Add(ticket);
            await App.DbContext.SaveChangesAsync();
            if (App.CurrentUser != null)
            {
                App.DbContext.StatusLogs.Add(new StatusLog
                {
                    Ticket_Id = ticket.Id,
                    User_Id = App.CurrentUser.Id,
                    NewStatus_Id = status.Id,
                    CreatedAt = DateTime.UtcNow
                });
                await App.DbContext.SaveChangesAsync();
            }

            if (TopLevel.GetTopLevel(this) is MainWindow window)
                window.MainControl.Content = new TicketDetails(ticket.Id);
        }

        private void Back_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (TopLevel.GetTopLevel(this) is MainWindow window)
                window.MainControl.Content = new TicketList();
        }
    }
}

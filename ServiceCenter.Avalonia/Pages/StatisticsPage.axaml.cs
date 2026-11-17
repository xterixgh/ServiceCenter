using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ServiceCenter.Avalonia.Pages
{
    public partial class StatisticsPage : UserControl
    {
        public StatisticsPage()
        {
            InitializeComponent();
            Loaded += StatisticsPage_Loaded;
        }

        private async void StatisticsPage_Loaded(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (App.CurrentUser == null)
                return;

            var query = App.DbContext.Tickets.Include(t => t.TicketStatus).Include(t => t.Master).AsQueryable();
            if (App.CurrentUser.Role == "master")
                query = query.Where(t => t.Master_Id == App.CurrentUser.Id);

            var tickets = await query.ToListAsync();
            int closed = tickets.Count(t => t.TicketStatus.Code == "closed");
            SummaryText.Text = "Заявок: " + tickets.Count + " · закрыто: " + closed;

            StatusItems.ItemsSource = tickets
                .GroupBy(t => t.TicketStatus.Name)
                .Select(g => g.Key + " — " + g.Count())
                .ToList();

            MasterItems.ItemsSource = tickets
                .GroupBy(t => t.Master == null ? "не назначен" : t.Master.FullName)
                .Select(g => g.Key + " — " + g.Count())
                .ToList();
        }
    }
}

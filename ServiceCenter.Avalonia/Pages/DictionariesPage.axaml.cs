using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using ServiceCenter.Data.Models;
using System.Linq;

namespace ServiceCenter.Avalonia.Pages
{
    public partial class DictionariesPage : UserControl
    {
        public DictionariesPage()
        {
            InitializeComponent();
            Loaded += async (_, _) => await Reload();
        }

        private async System.Threading.Tasks.Task Reload()
        {
            DeviceList.ItemsSource = await App.DbContext.DeviceTypes.OrderBy(x => x.Name).ToListAsync();
            ServiceList.ItemsSource = await App.DbContext.Services.OrderBy(x => x.Name).ToListAsync();
            StatusList.ItemsSource = await App.DbContext.TicketStatuses.OrderBy(x => x.Id).ToListAsync();
        }

        private async void AddDevice_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DeviceBox.Text))
                return;
            App.DbContext.DeviceTypes.Add(new DeviceType { Name = DeviceBox.Text.Trim() });
            await App.DbContext.SaveChangesAsync();
            DeviceBox.Text = "";
            await Reload();
        }

        private async void AddService_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ServiceBox.Text))
                return;
            App.DbContext.Services.Add(new ServiceItem { Name = ServiceBox.Text.Trim() });
            await App.DbContext.SaveChangesAsync();
            ServiceBox.Text = "";
            await Reload();
        }

        private async void DeleteDevice_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (DeviceList.SelectedItem is not DeviceType item)
                return;
            App.DbContext.DeviceTypes.Remove(item);
            try
            {
                await App.DbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                App.DbContext.Entry(item).State = EntityState.Unchanged;
            }
            await Reload();
        }

        private async void DeleteService_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (ServiceList.SelectedItem is not ServiceItem item)
                return;
            App.DbContext.Services.Remove(item);
            try
            {
                await App.DbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                App.DbContext.Entry(item).State = EntityState.Unchanged;
            }
            await Reload();
        }
    }
}

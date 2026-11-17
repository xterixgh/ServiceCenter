using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using ServiceCenter.Data.Models;
using System.Linq;

namespace ServiceCenter.Avalonia.Pages
{
    public partial class ClientsPage : UserControl
    {
        public ClientsPage()
        {
            InitializeComponent();
            Loaded += async (_, _) => await Reload();
        }

        private async System.Threading.Tasks.Task Reload()
        {
            ClientList.ItemsSource = await App.DbContext.Clients.OrderBy(c => c.FullName).ToListAsync();
        }

        private void ClientList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (ClientList.SelectedItem is not Client client)
                return;
            NameBox.Text = client.FullName;
            PhoneBox.Text = client.Phone;
            NoteBox.Text = client.Note;
            ErrorText.IsVisible = false;
        }

        private void New_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            ClientList.SelectedItem = null;
            NameBox.Text = "";
            PhoneBox.Text = "";
            NoteBox.Text = "";
            ErrorText.IsVisible = false;
        }

        private async void Save_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text) || string.IsNullOrWhiteSpace(PhoneBox.Text))
            {
                ErrorText.Text = "Укажите фамилию и телефон";
                ErrorText.IsVisible = true;
                return;
            }

            if (ClientList.SelectedItem is Client selected)
            {
                selected.FullName = NameBox.Text.Trim();
                selected.Phone = PhoneBox.Text.Trim();
                selected.Note = NoteBox.Text == null ? "" : NoteBox.Text.Trim();
            }
            else
            {
                App.DbContext.Clients.Add(new Client
                {
                    FullName = NameBox.Text.Trim(),
                    Phone = PhoneBox.Text.Trim(),
                    Note = NoteBox.Text == null ? "" : NoteBox.Text.Trim()
                });
            }

            await App.DbContext.SaveChangesAsync();
            NameBox.Text = "";
            PhoneBox.Text = "";
            NoteBox.Text = "";
            ErrorText.IsVisible = false;
            await Reload();
        }
    }
}

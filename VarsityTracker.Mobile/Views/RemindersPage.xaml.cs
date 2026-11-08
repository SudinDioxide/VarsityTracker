using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using VarsityTracker.Mobile.Models;
using VarsityTracker.Mobile.Services;

namespace VarsityTracker.Mobile.Views
{
    public partial class RemindersPage : ContentPage
    {
        private readonly MobileApiService _api = new MobileApiService();

        public RemindersPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadRemindersAsync();
        }

        private async void OnRefreshReminders(object sender, EventArgs e)
        {
            await LoadRemindersAsync();
            RemindersRefreshView.IsRefreshing = false;
        }

        private async Task LoadRemindersAsync()
        {
            int userId = StudentSession.Current.Profile.UserId;
            var list = await _api.GetRemindersAsync(userId);
            RemindersCollectionView.ItemsSource = null;
            RemindersCollectionView.ItemsSource = list;
        }

        private async void OnEditReminderClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is ReminderItem reminder)
            {
                string action = await DisplayActionSheet(
                    $"Change Alert Time for {reminder.FixtureTitle}",
                    "Cancel", null,
                    "15 minutes before kickoff",
                    "30 minutes before kickoff",
                    "60 minutes before kickoff");

                if (action == "Cancel" || string.IsNullOrEmpty(action)) return;

                int leadTime = action.StartsWith("15") ? 15 : action.StartsWith("30") ? 30 : 60;
                await _api.UpdateReminderAsync(reminder.ReminderId, leadTime);
                await LoadRemindersAsync();
                await DisplayAlert("Updated", $"Alert time changed to {leadTime} minutes before kickoff.", "OK");
            }
        }

        private async void OnDeleteReminderClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is ReminderItem reminder)
            {
                bool confirm = await DisplayAlert("Remove Alert", 
                    $"Are you sure you want to delete the reminder for {reminder.FixtureTitle}?", "Delete", "Cancel");

                if (confirm)
                {
                    await _api.DeleteReminderAsync(reminder.ReminderId);
                    await LoadRemindersAsync();
                }
            }
        }
    }
}


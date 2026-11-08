using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using VarsityTracker.Mobile.Models;
using VarsityTracker.Mobile.Services;

namespace VarsityTracker.Mobile.Views
{
    public partial class FeedbackPage : ContentPage
    {
        private readonly MobileApiService _api = new MobileApiService();

        public FeedbackPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadFeedbacksAsync();
        }

        private async void OnRefreshFeedbacks(object sender, EventArgs e)
        {
            await LoadFeedbacksAsync();
            FeedbackRefreshView.IsRefreshing = false;
        }

        private async Task LoadFeedbacksAsync()
        {
            int userId = StudentSession.Current.Profile.UserId;
            var list = await _api.GetFeedbacksAsync(userId);
            FeedbackCollectionView.ItemsSource = null;
            FeedbackCollectionView.ItemsSource = list;
        }

        // CREATE: Submit New Hazard Report
        private async void OnNewReportClicked(object sender, EventArgs e)
        {
            var venues = _api.GetPredefinedVenues();
            string[] venueNames = venues.Select(v => v.VenueName).ToArray();

            string selectedVenue = await DisplayActionSheet("Select Affected Facility", "Cancel", null, venueNames);
            if (string.IsNullOrEmpty(selectedVenue) || selectedVenue == "Cancel") return;

            var venue = venues.First(v => v.VenueName == selectedVenue);

            string subject = await DisplayPromptAsync("Hazard Subject", "e.g. Broken perimeter net, lighting failure", maxLength: 60);
            if (string.IsNullOrWhiteSpace(subject)) return;

            string details = await DisplayPromptAsync("Details & Location", "Describe the damage and exact court/zone:", maxLength: 200);
            if (string.IsNullOrWhiteSpace(details)) return;

            int userId = StudentSession.Current.Profile.UserId;
            await _api.CreateFeedbackAsync(userId, venue.VenueId, subject, details);
            await LoadFeedbacksAsync();

            await DisplayAlert("Report Submitted", 
                "Your safety defect ticket has been recorded. UTHM Sports Division will inspect the venue.", "OK");
        }

        // UPDATE: Edit Details
        private async void OnEditFeedbackClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is FeedbackItem item)
            {
                string newSubject = await DisplayPromptAsync("Edit Subject", "Update issue subject:", 
                    initialValue: item.Subject, maxLength: 60);
                if (string.IsNullOrWhiteSpace(newSubject)) return;

                string newDetails = await DisplayPromptAsync("Edit Description", "Update defect details / notes:", 
                    initialValue: item.Details, maxLength: 200);
                if (string.IsNullOrWhiteSpace(newDetails)) return;

                await _api.UpdateFeedbackAsync(item.FeedbackId, item.VenueId, newSubject, newDetails);
                await LoadFeedbacksAsync();
                await DisplayAlert("Updated", "Report details updated successfully.", "OK");
            }
        }

        // DELETE: Withdraw Ticket
        private async void OnDeleteFeedbackClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is FeedbackItem item)
            {
                bool confirm = await DisplayAlert("Withdraw Ticket", 
                    $"Are you sure you want to withdraw ticket: '{item.Subject}'?", "Withdraw", "Keep");

                if (confirm)
                {
                    await _api.DeleteFeedbackAsync(item.FeedbackId);
                    await LoadFeedbacksAsync();
                    await DisplayAlert("Withdrawn", "Hazard ticket has been withdrawn and removed.", "OK");
                }
            }
        }
    }
}


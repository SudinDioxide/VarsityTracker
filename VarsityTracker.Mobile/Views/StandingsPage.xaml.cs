using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using VarsityTracker.Mobile.Models;
using VarsityTracker.Mobile.Services;

namespace VarsityTracker.Mobile.Views
{
    public partial class StandingsPage : ContentPage
    {
        private readonly MobileApiService _apiService;

        public StandingsPage()
        {
            InitializeComponent();
            _apiService = new MobileApiService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadStandingsAsync();
        }

        private async Task LoadStandingsAsync()
        {
            try
            {
                var standings = await _apiService.GetStandingsAsync();
                StandingsCollectionView.ItemsSource = standings;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Standings", "Unable to load standings: " + ex.Message, "OK");
            }
            finally
            {
                StandingsRefreshView.IsRefreshing = false;
            }
        }

        private async void OnRefreshStandings(object sender, EventArgs e)
        {
            await LoadStandingsAsync();
        }
    }
}


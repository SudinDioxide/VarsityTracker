using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using VarsityTracker.Mobile.Models;
using VarsityTracker.Mobile.Services;

namespace VarsityTracker.Mobile.Views
{
    public partial class PlayersPage : ContentPage
    {
        private readonly MobileApiService _apiService;
        private List<PlayerDto> _allPlayers = new List<PlayerDto>();
        public ObservableCollection<PlayerDto> DisplayedPlayers { get; set; } = new ObservableCollection<PlayerDto>();

        private string _selectedFaculty = string.Empty;
        private string _searchKeyword = string.Empty;

        public PlayersPage()
        {
            InitializeComponent();
            _apiService = new MobileApiService();
            PlayersCollectionView.ItemsSource = DisplayedPlayers;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadPlayersAsync();
        }

        private async Task LoadPlayersAsync()
        {
            try
            {
                var players = await _apiService.GetPlayersAsync();
                _allPlayers = players ?? new List<PlayerDto>();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading players: {ex.Message}");
            }
            finally
            {
                PlayersRefreshView.IsRefreshing = false;
            }
        }

        private void ApplyFilter()
        {
            var filtered = _allPlayers.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(_selectedFaculty))
            {
                filtered = filtered.Where(p => string.Equals(p.FacultyCode, _selectedFaculty, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(_searchKeyword))
            {
                var q = _searchKeyword.Trim().ToLowerInvariant();
                filtered = filtered.Where(p =>
                    (p.FullName != null && p.FullName.ToLowerInvariant().Contains(q)) ||
                    (p.MatricNo != null && p.MatricNo.ToLowerInvariant().Contains(q)) ||
                    (p.PositionRole != null && p.PositionRole.ToLowerInvariant().Contains(q)) ||
                    (p.FacultyCode != null && p.FacultyCode.ToLowerInvariant().Contains(q))
                );
            }

            var resultList = filtered.OrderBy(p => p.FacultyCode).ThenBy(p => p.FullName).ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                DisplayedPlayers.Clear();
                foreach (var item in resultList)
                {
                    DisplayedPlayers.Add(item);
                }
            });
        }

        private async void OnRefreshPlayers(object sender, EventArgs e)
        {
            await LoadPlayersAsync();
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            _searchKeyword = e.NewTextValue ?? string.Empty;
            ApplyFilter();
        }

        private void OnFacultyFilterClicked(object sender, EventArgs e)
        {
            if (sender is Button clickedBtn)
            {
                _selectedFaculty = clickedBtn.CommandParameter?.ToString() ?? string.Empty;

                // Update visual styles of chip buttons in the horizontal layout
                if (clickedBtn.Parent is HorizontalStackLayout stack)
                {
                    foreach (var child in stack.Children)
                    {
                        if (child is Button btn)
                        {
                            bool isSelected = btn == clickedBtn;
                            btn.BackgroundColor = isSelected ? Color.FromArgb("#0D2B45") : Color.FromArgb("#E2E8F0");
                            btn.TextColor = isSelected ? Colors.White : Color.FromArgb("#334155");
                        }
                    }
                }

                ApplyFilter();
            }
        }
    }
}


using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using VarsityTracker.Mobile.Models;
using VarsityTracker.Mobile.Services;

namespace VarsityTracker.Mobile.Views
{
    public partial class MatchesPage : ContentPage
    {
        private readonly MobileApiService _api = new MobileApiService();
        private readonly ObservableCollection<MatchItem> _matches = new ObservableCollection<MatchItem>();
        private List<MatchItem> _masterMatches = new List<MatchItem>();
        private string _activeSportFilter = "All";
        private System.Threading.CancellationTokenSource? _syncCts;
        private bool _isSyncing = false;

        public MatchesPage()
        {
            InitializeComponent();
            MatchesCollectionView.ItemsSource = _matches;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            StartAutoSync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StopAutoSync();
        }

        private void StartAutoSync()
        {
            StopAutoSync();
            _syncCts = new System.Threading.CancellationTokenSource();
            var token = _syncCts.Token;

            Task.Run(async () =>
            {
                // Immediate initial pull
                await SyncMatchesAsync(isInitial: true);

                // Continuous loop: 1000ms when online for instant live updates, 2500ms when offline
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        int delay = MobileApiService.IsConnectedToBackend ? 1000 : 2500;
                        await Task.Delay(delay, token);
                        if (token.IsCancellationRequested) break;

                        await SyncMatchesAsync(isInitial: false);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[AutoSync Error] {ex.Message}");
                    }
                }
            }, token);
        }

        private void StopAutoSync()
        {
            if (_syncCts != null)
            {
                _syncCts.Cancel();
                _syncCts.Dispose();
                _syncCts = null;
            }
        }

        private async void OnRefreshMatches(object sender, EventArgs e)
        {
            await SyncMatchesAsync(isInitial: false);
            MatchesRefreshView.IsRefreshing = false;
        }

        private async Task SyncMatchesAsync(bool isInitial = false)
        {
            if (_isSyncing) return;
            _isSyncing = true;

            try
            {
                var latestList = await _api.GetMatchesAsync();
                if (latestList != null && latestList.Any())
                {
                    _masterMatches = latestList;

                    // ALWAYS MARSHAL TO UI MAIN THREAD FOR INSTANT UI REACTION
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        UpdateConnectionUI();
                        ApplyActiveFilter();
                    });
                }
            }
            catch (Exception ex)
            {
                if (isInitial)
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        UpdateConnectionUI();
                        await DisplayAlert("Sync Notice", "Unable to pull live feed from web server: " + ex.Message, "OK");
                    });
                }
            }
            finally
            {
                _isSyncing = false;
            }
        }

        private void UpdateConnectionUI()
        {
            if (MobileApiService.IsConnectedToBackend)
            {
                SyncStatusBorder.BackgroundColor = Color.FromArgb("#10B981");
                LblSyncStatus.Text = "● Live Online";
                OfflineNoticeBorder.IsVisible = false;
                LblHeaderSubtitle.Text = $"Connected live to {MobileApiService.ActiveServerUrl}. Auto-syncing real-time scores.";
            }
            else
            {
                SyncStatusBorder.BackgroundColor = Color.FromArgb("#F59E0B");
                LblSyncStatus.Text = "⚠️ Offline Mode";
                OfflineNoticeBorder.IsVisible = true;
                LblOfflineReason.Text = MobileApiService.LastSyncError ?? "Cannot reach web server at http://localhost:5299.";
                LblHeaderSubtitle.Text = "Serving local cached matches. Retrying server connection...";
            }
        }

        private async void OnRetryConnectionClicked(object? sender, EventArgs e)
        {
            MobileApiService._confirmedBaseUrl = null;
            await SyncMatchesAsync(isInitial: true);
            if (!MobileApiService.IsConnectedToBackend)
            {
                await DisplayAlert("Connection Status", "Could not reach web server.\n\n" + MobileApiService.DiagnosticLog, "OK");
            }
        }

        private async void OnDiagnosticClicked(object? sender, EventArgs e)
        {
            await DisplayAlert("Server Diagnostic Details", MobileApiService.DiagnosticLog, "OK");
        }

        private async void OnSetUrlClicked(object? sender, EventArgs e)
        {
            string current = MobileApiService.ActiveServerUrl ?? "http://localhost:5299";
            string? result = await DisplayPromptAsync("Custom Backend URL", 
                "Enter web server host and port:\n(e.g., http://localhost:5299 or http://10.0.2.2:5299)", 
                initialValue: current, 
                maxLength: 100, 
                keyboard: Keyboard.Url);

            if (!string.IsNullOrWhiteSpace(result))
            {
                MobileApiService.SetCustomUrl(result);
                await SyncMatchesAsync(isInitial: true);
            }
        }

        private void ApplyActiveFilter()
        {
            var filtered = _activeSportFilter == "All"
                ? _masterMatches
                : _masterMatches.Where(m => m.Sport != null && m.Sport.IndexOf(_activeSportFilter, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            // Check if collection structure or match IDs changed
            bool structureChanged = _matches.Count != filtered.Count ||
                !_matches.Select(m => m.MatchId).SequenceEqual(filtered.Select(m => m.MatchId));

            if (structureChanged)
            {
                _matches.Clear();
                foreach (var item in filtered)
                {
                    _matches.Add(item);
                }
            }
            else
            {
                for (int i = 0; i < filtered.Count; i++)
                {
                    var incoming = filtered[i];
                    var existing = _matches[i];
                    if (existing.HomeScore != incoming.HomeScore ||
                        existing.AwayScore != incoming.AwayScore ||
                        existing.Status != incoming.Status ||
                        existing.LatestEvent != incoming.LatestEvent ||
                        existing.Venue != incoming.Venue)
                    {
                        existing.HomeScore = incoming.HomeScore;
                        existing.AwayScore = incoming.AwayScore;
                        existing.Status = incoming.Status;
                        existing.LatestEvent = incoming.LatestEvent;
                        existing.Venue = incoming.Venue;
                        // Replace at index to guarantee WinUI 3 collection redraw
                        _matches[i] = incoming;
                    }
                }
            }
        }

        private void OnFilterSportClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is string sport)
            {
                _activeSportFilter = sport;

                var buttons = new[] { BtnFilterAll, BtnFilterFutsal, BtnFilterBadminton, BtnFilterBasketball, BtnFilterVolleyball, BtnFilterTakraw };
                foreach (var b in buttons)
                {
                    if (b != null)
                    {
                        b.BackgroundColor = Color.FromArgb("#E2E8F0");
                        b.TextColor = Color.FromArgb("#1E293B");
                    }
                }

                btn.BackgroundColor = Color.FromArgb("#0D2B45");
                btn.TextColor = Colors.White;

                ApplyActiveFilter();
            }
        }

        private async void OnViewMatchDetailsClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is MatchItem match)
            {
                await Navigation.PushAsync(new MatchDetailsPage(match.MatchId));
            }
        }

        private async void OnSetReminderClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is MatchItem match)
            {
                string action = await DisplayActionSheet(
                    $"Set Alert for {match.MatchTitle}",
                    "Cancel", null,
                    "15 minutes before kickoff",
                    "30 minutes before kickoff",
                    "60 minutes before kickoff");

                if (action == "Cancel" || string.IsNullOrEmpty(action)) return;

                int leadTime = action.StartsWith("15") ? 15 : action.StartsWith("30") ? 30 : 60;
                int userId = StudentSession.Current.Profile.UserId;

                bool success = await _api.CreateReminderAsync(userId, match.MatchId, leadTime);
                if (success)
                {
                    await DisplayAlert("Alert Scheduled", 
                        $"Reminder alert set for {match.MatchTitle} ({leadTime} min prior). View it under 'My Alerts' tab.", "OK");
                }
            }
        }

        private async void OnViewStandingsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//StandingsPage");
        }
    }
}


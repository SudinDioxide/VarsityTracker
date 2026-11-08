using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using VarsityTracker.Mobile.Models;
using VarsityTracker.Mobile.Services;

namespace VarsityTracker.Mobile.Views
{
    public partial class MatchDetailsPage : ContentPage
    {
        private readonly int _matchId;
        private readonly MobileApiService _api = new MobileApiService();
        private MatchDetailDto? _detail;
        private System.Threading.CancellationTokenSource? _syncCts;

        public MatchDetailsPage(int matchId)
        {
            InitializeComponent();
            _matchId = matchId;
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
                await LoadDetailsAsync(showSpinner: true);

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(1000, token);
                        if (token.IsCancellationRequested) break;

                        await LoadDetailsAsync(showSpinner: false);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DetailSync Error] {ex.Message}");
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

        private async void OnBackClicked(object? sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnRefreshDetail(object? sender, EventArgs e)
        {
            await LoadDetailsAsync(showSpinner: false);
            DetailRefreshView.IsRefreshing = false;
        }

        private async Task LoadDetailsAsync(bool showSpinner = true)
        {
            if (showSpinner)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LoadingSpinner.IsVisible = true;
                    LoadingSpinner.IsRunning = true;
                });
            }

            try
            {
                var d = await _api.GetMatchDetailAsync(_matchId);
                if (d != null)
                {
                    _detail = d;
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        RenderMatchDetails(d);
                    });
                }
            }
            catch (Exception ex)
            {
                if (showSpinner)
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await DisplayAlert("Error", "Unable to load match fixture details: " + ex.Message, "OK");
                    });
                }
            }
            finally
            {
                if (showSpinner)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        LoadingSpinner.IsVisible = false;
                        LoadingSpinner.IsRunning = false;
                    });
                }
            }
        }

        private void RenderMatchDetails(MatchDetailDto d)
        {
            LblHeaderTitle.Text = $"{d.HomeFaculty} vs {d.AwayFaculty}";
            LblStatus.Text = d.Status.ToUpper();
            StatusBadgeBorder.BackgroundColor = Color.FromArgb(d.StatusColor);

            LblSport.Text = $"{d.Sport} ({d.ScoringType})";
            LblDate.Text = d.FormattedDate;

            LblHomeCode.Text = d.HomeFaculty;
            LblHomeName.Text = d.HomeFacultyName;
            LblHomeScore.Text = d.Status == "Scheduled" ? "-" : d.HomeScore.ToString();
            LblHomeMgr.Text = $"Mgr: {d.HomeManager}";

            LblAwayCode.Text = d.AwayFaculty;
            LblAwayName.Text = d.AwayFacultyName;
            LblAwayScore.Text = d.Status == "Scheduled" ? "-" : d.AwayScore.ToString();
            LblAwayMgr.Text = $"Mgr: {d.AwayManager}";

            LblScoringType.Text = d.ScoringType;

            LblVenueName.Text = d.Venue;
            LblVenueLocation.Text = d.VenueLocation;
            LblScorekeeper.Text = d.Scorekeeper;

            // Render Periods
            PeriodScoresLayout.Children.Clear();
            if (d.Periods != null && d.Periods.Any())
            {
                foreach (var p in d.Periods)
                {
                    var pGrid = new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitionCollection
                        {
                            new ColumnDefinition { Width = GridLength.Star },
                            new ColumnDefinition { Width = GridLength.Auto },
                            new ColumnDefinition { Width = GridLength.Star }
                        }
                    };

                    pGrid.Add(new Label { Text = $"{d.HomeFaculty}: {p.HomePoints}", FontSize = 11, TextColor = Color.FromArgb("#0D6EFD"), HorizontalOptions = LayoutOptions.Start });
                    pGrid.Add(new Label { Text = $"Period {p.PeriodNumber}", FontSize = 10, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#64748B"), HorizontalOptions = LayoutOptions.Center }, 1);
                    pGrid.Add(new Label { Text = $"{d.AwayFaculty}: {p.AwayPoints}", FontSize = 11, TextColor = Color.FromArgb("#DC3545"), HorizontalOptions = LayoutOptions.End }, 2);

                    PeriodScoresLayout.Children.Add(new Border
                    {
                        BackgroundColor = Color.FromArgb("#F8FAFC"),
                        Stroke = Color.FromArgb("#E2E8F0"),
                        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 6 },
                        Padding = new Thickness(10, 6),
                        Content = pGrid
                    });
                }
            }
            else
            {
                PeriodScoresLayout.Children.Add(new Label { Text = "No period score breakdown recorded yet.", FontSize = 11, TextColor = Color.FromArgb("#64748B") });
            }

            // Render Events
            EventsListLayout.Children.Clear();
            if (d.Events != null && d.Events.Any())
            {
                LblEventCount.Text = $"{d.Events.Count} Events";
                foreach (var ev in d.Events)
                {
                    var evStack = new VerticalStackLayout { Spacing = 2 };
                    var evHeader = new HorizontalStackLayout { Spacing = 6 };
                    evHeader.Children.Add(new Label { Text = ev.Icon, FontSize = 12 });
                    evHeader.Children.Add(new Label { Text = ev.Minute, FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0D2B45") });
                    evHeader.Children.Add(new Label { Text = ev.EventType, FontSize = 10, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#64748B") });

                    evStack.Children.Add(evHeader);
                    evStack.Children.Add(new Label { Text = ev.Description, FontSize = 11, TextColor = Color.FromArgb("#1E293B") });
                    if (!string.IsNullOrEmpty(ev.PlayerName))
                    {
                        evStack.Children.Add(new Label { Text = $"Athlete: {ev.PlayerName}", FontSize = 10, TextColor = Color.FromArgb("#64748B"), FontAttributes = FontAttributes.Italic });
                    }

                    EventsListLayout.Children.Add(new Border
                    {
                        BackgroundColor = Color.FromArgb("#FFFBEB"),
                        Stroke = Color.FromArgb("#FCD34D"),
                        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 6 },
                        Padding = new Thickness(10, 6),
                        Content = evStack
                    });
                }
            }
            else
            {
                LblEventCount.Text = "0 Events";
                EventsListLayout.Children.Add(new Label { Text = "No official match commentary events logged yet.", FontSize = 11, TextColor = Color.FromArgb("#64748B") });
            }

            // Render Home Squad
            LblHomeRosterTitle.Text = $"{d.HomeFaculty} Squad ({d.HomePlayers.Count})";
            HomePlayersLayout.Children.Clear();
            if (d.HomePlayers != null && d.HomePlayers.Any())
            {
                foreach (var p in d.HomePlayers)
                {
                    var playerRow = new VerticalStackLayout { Spacing = 0 };
                    playerRow.Children.Add(new Label { Text = p.FullName, FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A") });
                    playerRow.Children.Add(new Label { Text = $"{p.MatricNo} • {p.PositionRole}", FontSize = 9, TextColor = Color.FromArgb("#64748B") });
                    HomePlayersLayout.Children.Add(playerRow);
                }
            }
            else
            {
                HomePlayersLayout.Children.Add(new Label { Text = "No squad registered.", FontSize = 10, TextColor = Color.FromArgb("#94A3B8") });
            }

            // Render Away Squad
            LblAwayRosterTitle.Text = $"{d.AwayFaculty} Squad ({d.AwayPlayers.Count})";
            AwayPlayersLayout.Children.Clear();
            if (d.AwayPlayers != null && d.AwayPlayers.Any())
            {
                foreach (var p in d.AwayPlayers)
                {
                    var playerRow = new VerticalStackLayout { Spacing = 0 };
                    playerRow.Children.Add(new Label { Text = p.FullName, FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A") });
                    playerRow.Children.Add(new Label { Text = $"{p.MatricNo} • {p.PositionRole}", FontSize = 9, TextColor = Color.FromArgb("#64748B") });
                    AwayPlayersLayout.Children.Add(playerRow);
                }
            }
            else
            {
                AwayPlayersLayout.Children.Add(new Label { Text = "No squad registered.", FontSize = 10, TextColor = Color.FromArgb("#94A3B8") });
            }
        }

        private async void OnSetAlertClicked(object? sender, EventArgs e)
        {
            if (_detail == null) return;

            string action = await DisplayActionSheet(
                $"Set Alert for {_detail.HomeFaculty} vs {_detail.AwayFaculty}",
                "Cancel",
                null,
                "🔔 15 minutes before kickoff",
                "🔔 30 minutes before kickoff",
                "🔔 1 hour before kickoff");

            if (string.IsNullOrEmpty(action) || action == "Cancel") return;

            int leadTime = 30;
            if (action.Contains("15")) leadTime = 15;
            else if (action.Contains("1 hour")) leadTime = 60;

            int userId = StudentSession.Current.Profile.UserId;
            bool ok = await _api.CreateReminderAsync(userId, _detail.MatchId, leadTime);

            if (ok)
            {
                await DisplayAlert("Alert Scheduled (CRUD #2)", $"Personal alert scheduled {leadTime} minutes before kickoff for {_detail.HomeFaculty} vs {_detail.AwayFaculty}.", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Could not save match alert.", "OK");
            }
        }
    }
}


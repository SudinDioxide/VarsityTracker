using Microsoft.Maui.Controls;
using VarsityTracker.Mobile.Views;

namespace VarsityTracker.Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(MatchDetailsPage), typeof(MatchDetailsPage));
            Routing.RegisterRoute(nameof(PlayersPage), typeof(PlayersPage));
        }
    }
}

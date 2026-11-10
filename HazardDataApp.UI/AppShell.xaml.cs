using HazardDataApp.UI.Views;

namespace HazardDataApp.UI;

public partial class AppShell : Shell
{
    public AppShell(HomePage home, AddArticlePage article, QuickStatsPage stats)
    {
        InitializeComponent();
        HomeContent.Content = home;
        ArticleContent.Content = article;
        StatsContent.Content = stats;
    }
}

using HazardDataApp.UI.Views;

namespace HazardDataApp.UI;

public partial class AppShell : Shell
{
    public AppShell(HomePage home, ArticlePage article, StatsPage stats)
    {
        InitializeComponent();
        HomeContent.Content = home;
        ArticleContent.Content = article;
        StatsContent.Content = stats;
    }
}

using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace HazardDataApp.UI.Views;

public partial class AddArticlePage : ContentPage
{
    public ObservableCollection<ArticleItem> Articles { get; } = new();

    public AddArticlePage()
    {
        InitializeComponent();

        // Example rows shown in the supplied prototype.
        Articles.Add(new ArticleItem("Winter Dangers", "#FFFFFF"));
        Articles.Add(new ArticleItem("Water Disasters", "#EEEEEE"));

        BindingContext = this;
    }

    private async void OnAddArticleClicked(object sender, EventArgs e)
    {
        string header = HeaderEntry.Text?.Trim() ?? string.Empty;
        string body = BodyEditor.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(header))
        {
            await DisplayAlert("Missing article name",
                "Please enter an article header before adding the article.", "OK");
            return;
        }

        Articles.Add(new ArticleItem(header, Articles.Count % 2 == 0 ? "#FFFFFF" : "#EEEEEE"));

        HeaderEntry.Text = string.Empty;
        BodyEditor.Text = string.Empty;

        await DisplayAlert("Article added",
            $"“{header}” was added to the article list.", "OK");
    }

    private async void OnOpenArticleClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is ArticleItem article)
        {
            await DisplayAlert("Open article", $"Open “{article.Name}”.", "OK");
            // Replace this alert with navigation to your article-detail page.
        }
    }

    private async void OnPreviewArticleClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is ArticleItem article)
        {
            await DisplayAlert("Article preview", article.Name, "Close");
        }
    }

    private async void OnDownloadArticleClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is ArticleItem article)
        {
            await DisplayAlert("Download", $"Download action for “{article.Name}”.", "OK");
            // Add your file-export/download implementation here.
        }
    }

    private async void OnDeleteArticleClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is ArticleItem article)
        {
            bool confirm = await DisplayAlert(
                "Delete article",
                $"Are you sure you want to delete “{article.Name}”?",
                "Delete",
                "Cancel");

            if (confirm)
                Articles.Remove(article);
        }
    }

    private async void OnHomeClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//main/home");
    }

    private async void OnQuickStatsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//main/quick-stats");
    }
}

public class ArticleItem
{
    public string Name { get; }
    public string RowColor { get; }

    public ArticleItem(string name, string rowColor)
    {
        Name = name;
        RowColor = rowColor;
    }
}

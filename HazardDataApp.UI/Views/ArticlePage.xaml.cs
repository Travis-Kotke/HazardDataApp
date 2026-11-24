
using System.Collections.ObjectModel;
using HazardDataApp.BusinessLogic.Models;
using HazardDataApp.BusinessLogic.Services;

namespace HazardDataApp.UI.Views;

public partial class ArticlePage : ContentPage
{
    private readonly IReportService reportService;
    private bool busy;

    public ObservableCollection<Article> Articles { get; } = new();

    public ArticlePage(IReportService reportService)
    {
        InitializeComponent();

        this.reportService = reportService;

        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadArticlesAsync();
    }

    private async Task LoadArticlesAsync()
    {
        if (busy) return;

        busy = true;

        try
        {
            var articles = await reportService.GetArticlesAsync();

            Articles.Clear();

            for (int i = 0; i < articles.Count; i++)
            {
                articles[i].RowColor =
                    i % 2 == 0 ? "#FFFFFF" : "#EEEEEE";

                Articles.Add(articles[i]);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Database Error",
                ex.Message,
                "OK");
        }
        finally
        {
            busy = false;
        }
    }

    private async void OnAddArticleClicked(object sender, EventArgs e)
    {
        string title = HeaderEntry.Text?.Trim() ?? "";
        string body = BodyEditor.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(title))
        {
            await DisplayAlert(
                "Missing title",
                "Please enter an article title.",
                "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            await DisplayAlert(
                "Missing content",
                "Please enter the article contents.",
                "OK");
            return;
        }

        try
        {
            busy = true;

            await reportService.AddArticleAsync(title, body);

            HeaderEntry.Text = "";
            BodyEditor.Text = "";

            await LoadArticlesAfterSaveAsync();

            await DisplayAlert(
                "Success",
                "The article was saved to Supabase as a draft.",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Save Failed", ex.Message, "OK");
        }
        finally
        {
            busy = false;
        }
    }

    private async Task LoadArticlesAfterSaveAsync()
    {
        var articles = await reportService.GetArticlesAsync();

        Articles.Clear();

        for (int i = 0; i < articles.Count; i++)
        {
            articles[i].RowColor =
                i % 2 == 0 ? "#FFFFFF" : "#EEEEEE";

            Articles.Add(articles[i]);
        }
    }

    private async void OnPublishArticleClicked(object sender, EventArgs e)
    {
        if (sender is not Button button ||
            button.CommandParameter is not Article article)
            return;

        try
        {
            await reportService.SetArticlePublishedAsync(
                article.Id, !article.IsPublished);

            await LoadArticlesAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Update Failed", ex.Message, "OK");
        }
    }

    private async void OnDeleteArticleClicked(object sender, EventArgs e)
    {
        if (sender is not Button button ||
            button.CommandParameter is not Article article)
            return;

        bool confirm = await DisplayAlert(
            "Delete Article",
            $"Are you sure you want to delete '{article.Title}'?",
            "Delete",
            "Cancel");

        if (!confirm) return;

         try
        {
            await reportService.DeleteArticleAsync(article.Id);

            await LoadArticlesAsync();

            await DisplayAlert(
                "Success",
                "Article deleted successfully.",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Delete Failed",
                ex.Message,
                "OK");
        }
    }

    private async void OnOpenArticleClicked(object sender, EventArgs e)
    {
        if (sender is not Button button ||
            button.CommandParameter is not Article article)
            return;

        await DisplayAlert(
            article.Title,
            article.Body,
            "Close");
    }

    private async void OnDownloadArticleClicked(object sender, EventArgs e)
    {
        if (sender is not Button button ||
            button.CommandParameter is not Article article)
            return;

        string fileName = $"Article_{article.Id}.txt";

        string filePath = Path.Combine(
            FileSystem.CacheDirectory, fileName);

        await File.WriteAllTextAsync(
            filePath,
            $"{article.Title}\n\n{article.Body}");

        await Share.Default.RequestAsync(
            new ShareFileRequest
            {
                Title = $"Download {article.Title}",
                File = new ShareFile(filePath)
            });
    }
}

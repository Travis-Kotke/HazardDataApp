namespace UWO_Safety_App;

//Made by Carson 
 public partial class HomePageAdmin : ContentPage
 {
 	public HomePageAdmin()
 	{
 		InitializeComponent();
 	}

    private async void UnresolvedReports_Clicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new UnresolvedIssuesPage());
    }

    private async void ResolvedReports_Clicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new ResolvedIssuesPage());
    }
 }
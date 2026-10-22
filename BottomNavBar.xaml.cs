namespace UWO_Safety_App;
//Made by Carson EW
public partial class BottomNavBar : ContentView
{
    public BottomNavBar()
    {
        InitializeComponent();
    }

    private async void Home_Clicked(object? sender, EventArgs e)
    {   
        //Once Log in works, Add if to check if user is logged in as admin or user and navigate to the correct page
        await NavigateTo(new HomePageUser());
    }

    private async void Info_Clicked(object? sender, EventArgs e)
    {
        await NavigateTo(new InfoPage());
    }

    private async void Report_Clicked(object? sender, EventArgs e)
    {
        await NavigateTo(new CameraScreen()); 
    }

    private async void Map_Clicked(object?  sender, EventArgs e)
    {
        // Replace with MapPage 
    }

    private async void Back_Clicked(object? sender, EventArgs e)
    {
        // Navigate back to the previous page
        if (Window != null && Window.Page is NavigationPage navigationPage
            && navigationPage.Navigation.NavigationStack.Count > 1)
        {
            await navigationPage.PopAsync();
        }
    }


    private async Task NavigateTo(Page page)
    {
        // Pushes onto the navigation stack so Back can return to the previous page.
        if (Window != null && Window.Page is NavigationPage navigationPage)
        {
            await navigationPage.PushAsync(page);
        }
    }
}
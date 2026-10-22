using Microsoft.Extensions.DependencyInjection;

namespace UWO_Safety_App;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

		private readonly Func<ContentPage>[] Pages =
	{
		() => new HomePageUser(),
		() => new HomePageAdmin(),
		() => new LoginPage(),
		() => new ForgotPasswordPage(),
		() => new MapViewPage(),
		() => new InfoPage(),
		() => new CameraScreen(),
		() => new ArticlePage(),
		() => new AddArticlePage(),
		() => new UserProblemSurvey(),
		() => new UnresolvedIssuesPage(),
		() => new ResolvedIssuesPage(),
		() => new IssueDetailPage(ReportData.All[0]),

	};
	private const int pageToShow = 5; // Change this index to show a different page
	protected override Window CreateWindow(IActivationState? activationState)
	{
		// Wrapped in a NavigationPage so pages can be pushed and the Back button can pop them.
		return new Window(new NavigationPage(Pages[pageToShow]()));
	}
}

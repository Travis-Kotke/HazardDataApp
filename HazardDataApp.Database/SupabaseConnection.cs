namespace HazardDataApp.Database;

// Database owns the only client. Lazy<Task> makes concurrent reads share initialization.
internal sealed class SupabaseConnection
{
    private readonly Lazy<Task<Supabase.Client>> client = new(InitializeAsync);

    public Task<Supabase.Client> GetClientAsync() => client.Value;

    private static async Task<Supabase.Client> InitializeAsync()
    {
        if (SupabaseConfig.Url.Contains("YOUR_PROJECT") ||
            SupabaseConfig.PublishableKey == "YOUR_PUBLISHABLE_OR_ANON_KEY")
            throw new InvalidOperationException("Configure HazardDataApp.Database/SupabaseConfig.cs first.");

        var client = new Supabase.Client(SupabaseConfig.Url, SupabaseConfig.PublishableKey,
            new Supabase.SupabaseOptions { AutoConnectRealtime = false });
        await client.InitializeAsync();
        return client;
    }
}

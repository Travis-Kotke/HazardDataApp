namespace HazardDataApp.Database;

// Optional: copy to SupabaseConfig.cs here to override the shared public defaults.
// Replace these placeholders with your alternate project's public client values.
// Never use a service_role or sb_secret_ key in a mobile app.
internal static class SupabaseConfig
{
    public const string Url = "https://YOUR_PROJECT.supabase.co";
    public const string PublishableKey = "YOUR_PUBLISHABLE_OR_ANON_KEY";
}

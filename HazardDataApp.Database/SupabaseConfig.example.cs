namespace HazardDataApp.Database;

// Copy to SupabaseConfig.cs in this folder and replace these public client values.
// Never use a service_role or sb_secret_ key in a mobile app.
internal static class SupabaseConfig
{
    public const string Url = "https://YOUR_PROJECT.supabase.co";
    public const string PublishableKey = "YOUR_PUBLISHABLE_OR_ANON_KEY";
}

namespace HazardDataApp.Database;

// Shared PUBLIC client settings so teammates can clone and run without setup.
// Access is controlled by Supabase policies, not by keeping this key secret.
// Never put a service_role or sb_secret_ key here.
internal static class SupabaseConfig
{
    public const string Url = "https://stxvtrxjkamdocnewtme.supabase.co";
    public const string PublishableKey = "sb_publishable_3UWEDHEMf9nEBXw6x_T5Lw_fD_zByMt";
}

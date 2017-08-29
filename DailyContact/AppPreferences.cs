
namespace DailyContact
{
    using System;
    using Android.Content;
    using Android.Preferences;

    public class AppPreferences
    {
        private static string PREFERENCEACCESSKEY = "PREFERENCE_ACCESS_KEY";

        private ISharedPreferences MSharedPrefs;
        private ISharedPreferencesEditor MPrefsEditor;
        private Context MContext;

        public AppPreferences(Context context)
        {
            this.MContext = context;
            this.MSharedPrefs = PreferenceManager.GetDefaultSharedPreferences(this.MContext);
            this.MPrefsEditor = this.MSharedPrefs.Edit();
        }

        public void SaveAccessKey(string key)
        {
            this.MPrefsEditor.PutString(PREFERENCEACCESSKEY, key);
            this.MPrefsEditor.Commit();
        }

        public string GetAccessKey()
        {
            return this.MSharedPrefs.GetString(PREFERENCEACCESSKEY, string.Empty);
        }
    }
}
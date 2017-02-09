using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Plugin.Settings.Abstractions;
using Plugin.Settings;

namespace SendSMS
{
    public static class Settings
    {
        private static ISettings AppSettings
        {
            get { return CrossSettings.Current; }
        }
        private const string PhoneNumbersKey = "phonenumbers_key";
        private static readonly string PhoneNumbersDefault = "5554";

        public static string PhoneNumberText
        {
            get { return AppSettings.GetValueOrDefault<string>(PhoneNumbersKey, PhoneNumbersDefault); }
            set { AppSettings.AddOrUpdateValue<string>(PhoneNumbersKey, value); }
        }
    }
}
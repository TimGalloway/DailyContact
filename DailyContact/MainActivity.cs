//-----------------------------------------------------------------------
// <copyright file="MainActivity.cs" company="GallowayConsulting">
//     Company copyright tag.
// </copyright>
//-----------------------------------------------------------------------
namespace DailyContact
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Android.App;
    using Android.Content;
    using Android.Locations;
    using Android.OS;
    using Android.Telephony;
    using Android.Util;
    using Android.Widget;

    /// <summary>
    /// Main Activity Class
    /// </summary>
    [Activity(Label = "DailyContact", MainLauncher = true, Icon = "@drawable/PSC4wd")]
    public class MainActivity : Activity, ILocationListener
    {
        /// <summary>
        /// Current Location of the device
        /// </summary>
        Location currentLocation;

        /// <summary>
        /// Physical address of the device
        /// </summary>
        TextView addressText;

        /// <summary>
        /// Location manager class
        /// </summary>
        LocationManager locationManager;

        /// <summary>
        /// String representing the location provider
        /// </summary>
        string locationProvider;

        /// <summary>
        /// object that is the latitude long text
        /// </summary>
        TextView latLongText;

        /// <summary>
        /// Method that is run when location changes
        /// </summary>
        /// <param name="location">The location of the device</param>
        public void OnLocationChanged(Location location)
        {
            this.currentLocation = location;
            if (this.currentLocation == null)
            {
                this.latLongText.Text = "Cannot find current location";
            }
            else
            {
                this.latLongText.Text = "Lat: " + this.currentLocation.Latitude.ToString() + ": Long: " + this.currentLocation.Longitude.ToString();

                // Get our button from the layout resource,
                // and attach an event to it
                Button sendSMS = FindViewById<Button>(Resource.Id.btnSendSMS);
                Button getLocation = FindViewById<Button>(Resource.Id.btnLocation);
                sendSMS.Enabled = true;
                getLocation.Enabled = true;
            }
        }

        public void OnProviderDisabled(string provider)
        {
        }

        public void OnProviderEnabled(string provider)
        {
        }

        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
        }
        
        protected override void OnResume()
        {
            base.OnResume();
            this.locationManager.RequestLocationUpdates(this.locationProvider, 0, 0, this);
        }

        protected override void OnPause()
        {
            base.OnPause();
            this.locationManager.RemoveUpdates(this);
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            this.SetContentView(Resource.Layout.Main);

            // Set AppPreferences object
            Context context = Android.App.Application.Context;
            AppPreferences ap = new AppPreferences(context);

            // Get current phone numbers 
            string phonenumbers = ap.GetAccessKey();

            // Get our button from the layout resource,
            // and attach an event to it
            Button bSendSMS = FindViewById<Button>(Resource.Id.btnSendSMS);
            Button getLocation = FindViewById<Button>(Resource.Id.btnLocation);
            EditText phoneNumbers = FindViewById<EditText>(Resource.Id.txtPhoneNumbers);

            phoneNumbers.Text = phonenumbers;

            // Get the text fields from the layout
            this.addressText = this.FindViewById<TextView>(Resource.Id.txtLocation);
            this.latLongText = this.FindViewById<TextView>(Resource.Id.txtLatLong);

            // Disable buttons until location is found
            bSendSMS.Enabled = false;
            getLocation.Enabled = false;

            this.InitializeLocationManager();

            bSendSMS.Click += delegate {
                var _PhoneNumbersText = FindViewById<TextView>(Resource.Id.txtPhoneNumbers);
                var _SituationRB = FindViewById<RadioGroup>(Resource.Id.radioGroup1);
                var _SituationButton = FindViewById<RadioButton>(_SituationRB.CheckedRadioButtonId);
                var _Comments = FindViewById<TextView>(Resource.Id.txtComments);

                // fetch the Sms Manager
                SmsManager sms = SmsManager.Default;

                String _SMSString = "Current Lat/Long: " + currentLocation.Latitude.ToString() + " / " + currentLocation.Longitude.ToString() +
                                    "\r\n\r\nComments: " + _Comments.Text +
                                    "\r\n\r\nSituation: " + _SituationButton.Text;

                // split it between any commas, stripping whitespace afterwards
                string userInput = _PhoneNumbersText.Text.ToString();
                string[] numbers = userInput.Split(',');

                foreach (string number in numbers)
                {
                    sms.SendTextMessage(number, null, _SMSString, null, null);
                }

                ap.SaveAccessKey(userInput);

                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.SetTitle("Alert");
                builder.SetMessage("Message(s) sent");
                builder.SetCancelable(false);
                builder.SetPositiveButton("OK", delegate { this.Finish(); });
                builder.Show();
            };

            getLocation.Click += delegate
            {
                this.BtnLocation_OnClick();
            };
        }

        void InitializeLocationManager()
        {
            this.locationManager = (LocationManager)GetSystemService(LocationService);
            Criteria criteriaForLocationService = new Criteria
            {
                Accuracy = Accuracy.Fine
            };
            IList<string> acceptableLocationProviders = this.locationManager.GetProviders(criteriaForLocationService, true);

            if (acceptableLocationProviders.Any())
            {
                this.locationProvider = acceptableLocationProviders.First();
            }
            else
            {
                this.locationProvider = string.Empty;
            }

            Log.Debug("TAG", "Using " + this.locationProvider + ".");
        }

        async Task<Address> ReverseGeocodeCurrentLocation()
        {
            Geocoder geocoder = new Geocoder(this);
            IList<Address> addressList =
                await geocoder.GetFromLocationAsync(this.currentLocation.Latitude, this.currentLocation.Longitude, 10);

            Address address = addressList.FirstOrDefault();
            return address;
        }

        async void BtnLocation_OnClick()
        {
            if (this.currentLocation == null)
            {
                this.addressText.Text = "Can't determine the current address. Try again in a few minutes.";
                return;
            }

            Address address = await this.ReverseGeocodeCurrentLocation();
            this.DisplayAddress(address);
        }

        void DisplayAddress(Address address)
        {
            if (address != null)
            {
                StringBuilder deviceAddress = new StringBuilder();
                for (int i = 0; i < address.MaxAddressLineIndex; i++)
                {
                    deviceAddress.AppendLine(address.GetAddressLine(i));
                }

                // Remove the last comma from the end of the address.
                this.addressText.Text = deviceAddress.ToString();
            }
            else
            {
                this.addressText.Text = "Unable to determine the address. Try again in a few minutes.";
            }
        }
    }
}

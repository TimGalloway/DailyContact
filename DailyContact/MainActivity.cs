using System;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Android.Telephony;
using Android.Locations;
using System.Collections.Generic;
using System.Linq;
using Android.Util;
using System.Text;

namespace DailyContact
{
    [Activity(Label = "DailyContact", MainLauncher = true, Icon = "@drawable/PSC4wd")]
    public class MainActivity : Activity, ILocationListener
    {
        Location _currentLocation;

        public void OnLocationChanged(Location location)
        {
            _currentLocation = location;
            if (_currentLocation == null)
            {
                _LatLongText.Text = "Cannot find current location";
            }else
            {
                _LatLongText.Text = "Lat: " +_currentLocation.Latitude.ToString() + ": Long: " + _currentLocation.Longitude.ToString();

                // Get our button from the layout resource,
                // and attach an event to it
                Button bSendSMS = FindViewById<Button>(Resource.Id.btnSendSMS);
                Button bGetLocation = FindViewById<Button>(Resource.Id.btnLocation);
                bSendSMS.Enabled = true;
                bGetLocation.Enabled = true;
            }
        }

        public void OnProviderDisabled(string provider) { }

        public void OnProviderEnabled(string provider) { }

        public void OnStatusChanged(string provider, Availability status, Bundle extras) { }
        
        //static readonly string TAG = "X:" + typeof(Activity1).Name;
        TextView _addressText;
        LocationManager _locationManager;

        string _locationProvider;
        TextView _LatLongText;

        void InitializeLocationManager()
        {
            _locationManager = (LocationManager)GetSystemService(LocationService);
            Criteria criteriaForLocationService = new Criteria
            {
                Accuracy = Accuracy.Fine
            };
            IList<string> acceptableLocationProviders = _locationManager.GetProviders(criteriaForLocationService, true);

            if (acceptableLocationProviders.Any())
            {
                _locationProvider = acceptableLocationProviders.First();
            }
            else
            {
                _locationProvider = string.Empty;
            }
            Log.Debug("TAG", "Using " + _locationProvider + ".");
        }

        protected override void OnResume()
        {
            base.OnResume();
            _locationManager.RequestLocationUpdates(_locationProvider, 0, 0, this);
        }

        protected override void OnPause()
        {
            base.OnPause();
            _locationManager.RemoveUpdates(this);
        }

        void DisplayAddress(Address address)
        {
            if (address != null)
            {
                StringBuilder deviceAddress = new StringBuilder();
                deviceAddress.AppendLine(address.GetAddressLine(0));
                _addressText.Text = deviceAddress.ToString();
            }
            else
            {
                _addressText.Text = "Unable to determine the address. Try again in a few minutes.";
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Set AppPreferences object
            Context mContext = Android.App.Application.Context;
            AppPreferences ap = new AppPreferences(mContext);

            // Get current phone numbers 
            string _phonenumbers = ap.GetAccessKey();

            // Get our button from the layout resource,
            // and attach an event to it
            Button bSendSMS = FindViewById<Button>(Resource.Id.btnSendSMS);
            Button bGetLocation = FindViewById<Button>(Resource.Id.btnLocation);
            EditText tPhoneNumbers = FindViewById<EditText>(Resource.Id.txtPhoneNumbers);

            tPhoneNumbers.Text = _phonenumbers;

            // Get the text fields from the layout
            _addressText = FindViewById<TextView>(Resource.Id.txtLocation);
            _LatLongText = FindViewById<TextView>(Resource.Id.txtLatLong);

            //Disable buttons until location is found
            bSendSMS.Enabled = false;
            bGetLocation.Enabled = false;

            InitializeLocationManager();

            bSendSMS.Click += delegate {
                var _PhoneNumbersText = FindViewById<TextView>(Resource.Id.txtPhoneNumbers);
                var _SituationRB = FindViewById<RadioGroup>(Resource.Id.radioGroup1);
                var _SituationButton = FindViewById<RadioButton>(_SituationRB.CheckedRadioButtonId);

                // fetch the Sms Manager
                SmsManager sms = SmsManager.Default;

                String _SMSString = "Current Lat/Long: " + _currentLocation.Latitude.ToString() + " / " + _currentLocation.Longitude.ToString() +
                                    "\r\n\r\nLocation: " + _addressText.Text +
                                    "\r\n\r\nSituation: " + _SituationButton.Text;

                // split it between any commas, stripping whitespace afterwards
                String userInput = _PhoneNumbersText.Text.ToString();
                String[] numbers = userInput.Split(',');

                foreach (string number in numbers)
                {
                    sms.SendTextMessage(number, null, _SMSString, null, null);
                }

                ap.SaveAccessKey(userInput);

                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.SetTitle("Alert");
                builder.SetMessage("Message(s) sent");
                builder.SetCancelable(false);
                builder.SetPositiveButton("OK", delegate { Finish(); });
                builder.Show();
            };

            bGetLocation.Click += async (sender, e) =>
            {
                var geo = new Geocoder(this);
                var addresses = await geo.GetFromLocationAsync(_currentLocation.Latitude, _currentLocation.Longitude,1);

                if (addresses.Any())
                {
                    DisplayAddress(addresses.FirstOrDefault());
                }
                else
                {
                    _addressText.Text = e.ToString();
                }
            };
        }
    }
}


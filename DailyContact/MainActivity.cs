﻿using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Telephony;
using Android.Locations;
using System.Collections.Generic;
using System.Linq;
using Android.Util;
using System.Threading.Tasks;
using System.Text;
using SendSMS;

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

        async Task<Address> ReverseGeocodeCurrentLocation()
        {
            Geocoder geocoder = new Geocoder(this);
            IList<Address> addressList =
                await geocoder.GetFromLocationAsync(_currentLocation.Latitude, _currentLocation.Longitude, 10);

            Address address = addressList.FirstOrDefault();
            return address;
        }

        async void btnLocation_OnClick()
        {
            if (_currentLocation == null)
            {
                _addressText.Text = "Can't determine the current address. Try again in a few minutes.";
                return;
            }

            Address address = await ReverseGeocodeCurrentLocation();
            DisplayAddress(address);
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

            // Get our button from the layout resource,
            // and attach an event to it
            Button bSendSMS = FindViewById<Button>(Resource.Id.btnSendSMS);
            Button bGetLocation = FindViewById<Button>(Resource.Id.btnLocation);

            // Get the text fields from the layout
            _addressText = FindViewById<TextView>(Resource.Id.txtLocation);
            _LatLongText = FindViewById<TextView>(Resource.Id.txtLatLong);

            //Disable buttons until location is found
            bSendSMS.Enabled = false;
            bGetLocation.Enabled = false;

            InitializeLocationManager();


            bSendSMS.Click += delegate {
                SmsManager.Default.SendTextMessage("0403048129", null,
"Hello from Xamarin.Android", null, null);
                bSendSMS.Text = string.Format("Sent"); };

            bGetLocation.Click += delegate
            {
                btnLocation_OnClick();
            };

        }
    }
}


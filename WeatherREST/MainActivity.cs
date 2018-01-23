using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Locations;
using Android.OS;
using Android.Util;
using Android.Widget;

namespace WeatherREST
{
   
   [Activity(Label = "WeatherREST", MainLauncher = true)]
    public class MainActivity : Activity, ILocationListener
    {
        static readonly string TAG = "X:" + typeof(MainActivity).Name;
        Location _currentLocation;
        LocationManager _locationManager;
        String _locationProvider;
        TextView locationTextView;
        Button getWeatherButton;
        EditText latitude;
        EditText longitude;
        public async void OnLocationChanged(Location location)
        {
            _currentLocation = location;
            if (_currentLocation == null)
            {
                latitude.Text = "Waiting for location. Try again in a short while.";
                longitude.Text = "Waiting for location. Try again in a short while.";
            }
            else
            {
                //coordinatesTextView.Text = string.Format("{0:f6},{1:f6}", _currentLocation.Latitude, _currentLocation.Longitude);
                latitude.Text = _currentLocation.Latitude.ToString();
                longitude.Text = _currentLocation.Longitude.ToString();
                Address address = await ReverseGeocodeCurrentLocation();
                DisplayAddress(address);
            }
        }
        public void OnProviderDisabled(string provider) { }
        public void OnProviderEnabled(string provider) { }
        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
            Log.Debug(TAG, "{0}, {1}", provider, status);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);

            locationTextView = FindViewById<TextView>(Resource.Id.locationTextView);
            FindViewById<Button>(Resource.Id.getLoacationButton).Click += AddressButton_OnClick;

            latitude = FindViewById<EditText>(Resource.Id.latText);
            longitude = FindViewById<EditText>(Resource.Id.longText);
            getWeatherButton = FindViewById<Button>(Resource.Id.getWeatherButton);

            getWeatherButton.Click += async (sender, e) =>
            {
                if(_currentLocation != null)
                {
                    Console.Out.WriteLine("getWeatherButton.Click");
                    string url = "http://api.openweathermap.org/data/2.5/weather?lat=" +
                                 _currentLocation.Latitude +
                                 "&lon=" +
                                 _currentLocation.Longitude +
                                 "&appid=" +
                                 "e40777858c3faf09d51e3f692f6b6c8f&units=metric";

                    Console.Out.Write(url);
                    // Fetch the weather information asynchronously, 
                    // parse the results, then update the screen:
                    JsonValue json = await FetchWeatherAsync(url);
                    //Address address = await getLocation();
                    ParseAndDisplay(json);
                }
                else
                {
                    latitude.Text = "Waiting for location. Try again in a short while.";
                    longitude.Text = "Waiting for location. Try again in a short while.";
                }
                
            };
            InitializeLocationManager();
        }
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
            Log.Debug(TAG, "Using " + _locationProvider + ".");
        }
        protected override void OnResume()
        {
            base.OnResume();
            Log.Debug(TAG, "Called on  OnResume() moethod.");
            _locationManager.RequestLocationUpdates(_locationProvider, 0, 0, this);
            Log.Debug(TAG, "Listening for location updates using " + _locationProvider + ".");
        }
        protected override void OnPause()
        {
            base.OnPause();
            _locationManager.RemoveUpdates(this);
            Log.Debug(TAG, "No longer listening for location updates.");
        }
        

         async void AddressButton_OnClick(object sender, EventArgs eventArgs)
        {
            Console.Out.WriteLine(" AddressButton_OnClick");
            if (_currentLocation == null)
            {
                locationTextView.Text = "Can't determine the current address. Try again in a few minutes.";
                return;
            }

            Address address = await ReverseGeocodeCurrentLocation();
            DisplayAddress(address);
        }

        private async Task<JsonValue> FetchWeatherAsync(string url)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
            request.ContentType = "application/json";
            request.Method = "GET";

            using (WebResponse response = await request.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    JsonValue jsonDoc = await Task.Run(() => JsonObject.Load(stream));
                    Console.Out.WriteLine("Start printing json response");
                    Console.Out.WriteLine("Response: {0}", jsonDoc.ToString());
                    return jsonDoc;
                }
            }
        }

        private void ParseAndDisplay(JsonValue json)
        {
            TextView location = FindViewById<TextView>(Resource.Id.locationText);
            TextView temperature = FindViewById<TextView>(Resource.Id.tempText);
            TextView humidity = FindViewById<TextView>(Resource.Id.humidText);
            TextView conditions = FindViewById<TextView>(Resource.Id.condText);
            JsonValue locationJson = null;
            JsonValue mainValue = json["main"];
            JsonValue weatherValue = null;
            try
            {
                locationJson = json["name"];
                //Console.Out.WriteLine("mainValue Alo" + mainValue["temp"]);
            }
            catch (Exception e)
            {
                location.Text = "Nie mozna pobrać!";
            }
            if (location != null)
            {
                temperature.Text = mainValue["temp"].ToString();
                humidity.Text = mainValue["humidity"].ToString();
                location.Text = locationJson;
                weatherValue = json["weather"];
                conditions.Text = weatherValue.ToString();
                JsonArray weatherArray = (JsonArray)json["weather"];
                JsonValue weather = weatherArray[0];
                conditions.Text = weather["description"];
            }
        }

        private async Task<Address> ReverseGeocodeCurrentLocation()
        {
            Geocoder geocoder = new Geocoder(this);
            IList<Address> addressList =
                await geocoder.GetFromLocationAsync(_currentLocation.Latitude, _currentLocation.Longitude, 10);

            Address address = addressList.FirstOrDefault();
            Console.Out.WriteLine(address.ToString());
            return address;

        }
        

        
        void DisplayAddress(Address address)
        {
            Log.Debug(TAG, "Downloading address.");
            if (address != null)
            {
                address.GetAddressLine(0);
                StringBuilder deviceAddress = new StringBuilder();
                Console.Out.WriteLine("awlo {0}", address.MaxAddressLineIndex);
                for (int i = 0; i <= address.MaxAddressLineIndex; i++)
                {
                    deviceAddress.AppendLine(address.GetAddressLine(i));
                    Console.Out.WriteLine("awlo");
                }         
                locationTextView.Text = deviceAddress.ToString();
                Console.Out.WriteLine(deviceAddress.ToString());
            }
            else
            {
                locationTextView.Text = "Unable to determine the address. Try again in a few minutes.";
            }
        }




    }
}


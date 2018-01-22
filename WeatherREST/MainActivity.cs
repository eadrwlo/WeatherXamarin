using Android.App;
using Android.Widget;
using Android.OS;
using System;
using System.Json;
using System.Net;
using System.IO;
using System.Threading.Tasks;
namespace WeatherREST
{
    [Activity(Label = "WeatherREST", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            EditText latitude = FindViewById<EditText>(Resource.Id.latText);
            EditText longitude = FindViewById<EditText>(Resource.Id.longText);
            Button getWeatherButton = FindViewById<Button>(Resource.Id.getWeatherButton);

            getWeatherButton.Click += async (sender, e) => {

                string url = "http://api.openweathermap.org/data/2.5/weather?lat=" +
                             latitude.Text +
                             "&lon=" +
                             longitude.Text + 
                             "&appid=" +
                             "e40777858c3faf09d51e3f692f6b6c8f&units=metric";
                              
                Console.Out.Write(url);

                // Fetch the weather information asynchronously, 
                // parse the results, then update the screen:
                JsonValue json = await FetchWeatherAsync(url);
                ParseAndDisplay (json);
            };
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

        private void ParseAndDisplay (JsonValue json)
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
    }
}


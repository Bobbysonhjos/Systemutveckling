using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using SmartApp.MVVM.Models;

namespace SmartApp.Services
{

    internal interface IWeatherService
    {
        public Task<WeatherResponse> GetWeatherDataAsync(string uri = "https://api.openweathermap.org/data/2.5/weather?lat=59.1881139&lon=18.1140349&appid=746d2b2d71322bde500d8c165bf0e8d6");
    }

    internal class WeatherService : IWeatherService
    {
        public async Task<WeatherResponse> GetWeatherDataAsync(string uri = "https://api.openweathermap.org/data/2.5/weather?q=Stockholm&appid=8d9e3e2ec1ee988fa772836aa854765a")
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetFromJsonAsync<WeatherApiResponse>(uri);
                return new WeatherResponse
                {
                    Temperature = (int)response!.main.temp,
                    Humidity = response.main.humidity,

                    WeatherCondition = response.weather[0].main
                };
            }
            catch { }
            return null!;
        }
    }
}

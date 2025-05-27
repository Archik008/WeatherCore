// WeatherImporter.cs 
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WeatherCore;

public class WeatherImporter
{
    private readonly HttpClient _http = new();
    private WeatherService w_service;

    private Dictionary<string, string> cities = new Dictionary<string, string>()
    {
        {"Алматы", "Almaty"},
        {"Астана", "Astana" }
    };

    public WeatherImporter(IDbContextFactory<AppDbContext> contextFactory)
    {
        w_service = new WeatherService(contextFactory);
    }

    public async Task<WeatherRoot> FetchAndWriteWeatherAsync(string cityName)
    {
        string eng_city = cities[cityName];
        var geoUrl = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(eng_city)}&count=1";
        var geoJson = await _http.GetStringAsync(geoUrl);
        var geo = JsonSerializer.Deserialize<GeoCodingResponse>(geoJson);
        if (geo?.Results == null || geo.Results.Count == 0) return null;

        var location = geo.Results[0];
        double latitude = location.Latitude;
        double longitude = location.Longitude;

        string latStr = latitude.ToString(CultureInfo.InvariantCulture);
        string lonStr = longitude.ToString(CultureInfo.InvariantCulture);

        var forecastUrl = $"https://api.open-meteo.com/v1/forecast?latitude={latStr}&longitude={lonStr}&hourly=temperature_2m,relative_humidity_2m,uv_index,weathercode,windspeed_10m&timezone=auto";
        var airUrl = $"https://air-quality-api.open-meteo.com/v1/air-quality?latitude={latStr}&longitude={lonStr}&hourly=pm10,pm2_5,us_aqi&timezone=auto";

        var forecastJson = await _http.GetStringAsync(forecastUrl);
        var airJson = await _http.GetStringAsync(airUrl);

        var forecast = JsonSerializer.Deserialize<OpenMeteoResponse>(forecastJson);
        var air = JsonSerializer.Deserialize<OpenMeteoAirQualityResponse>(airJson);

        if (forecast?.Hourly?.Time == null || air?.Hourly?.Time == null)
            return null;

        var airByTime = air.Hourly.Time
            .Select((t, i) => new
            {
                Time = DateTime.Parse(t),
                Aqi = air.Hourly.UsaAqi[i],
            })
            .ToDictionary(x => x.Time, x => x);

        var hourlyData = forecast.Hourly.Time.Select((t, i) => new
        {
            Time = DateTime.Parse(t),
            Temp = forecast.Hourly.Temperature[i] ?? -1,
            Humidity = forecast.Hourly.Humidity[i] ?? -1,
            Uv = forecast.Hourly.UvIndex[i] ?? -1,
            Wind = forecast.Hourly.WindSpeed[i] ?? -1,
            Code = forecast.Hourly.WeatherCode[i] ?? -1,
            AQI = airByTime.TryGetValue(DateTime.Parse(t), out var aqi) ? aqi : null
        }).ToList();

        var grouped = hourlyData.GroupBy(x => x.Time.Date);
        var forecastDays = new List<ForecastDay>();

        foreach (var dayGroup in grouped)
        {
            var dayWeather = new DayWeather
            {
                Day = dayGroup.Key,
                Temp_min = dayGroup.Min(x => x.Temp),
                Temp_max = dayGroup.Max(x => x.Temp),
                Humility = dayGroup.Average(x => x.Humidity),
                Uv = dayGroup.Average(x => x.Uv),
                Wind = dayGroup.Average(x => x.Wind),
                Aqi = (int)(dayGroup.Where(x => x.AQI?.Aqi != null).Average(x => x.AQI.Aqi) ?? -1)
            };

            var savedDay = await w_service.AddDayWeatherAsync(cityName, dayWeather);

            var hourlyWeathers = new List<HourlyWeather>();
            foreach (var h in dayGroup)
            {
                var hourWeather = new HourWeather
                {
                    hour = h.Time,
                    cur_temp = h.Temp,
                    condition = TranslateWeatherCode(h.Code)
                };

                await w_service.AddHourWeatherAsync(savedDay.Id, hourWeather);

                hourlyWeathers.Add(new HourlyWeather
                {
                    Time = h.Time.ToString("yyyy-MM-ddTHH:mm"),
                    TempC = h.Temp,
                    Humidity = h.Humidity,
                    WindKph = h.Wind,
                    Uv = h.Uv,
                    Condition = new Condition { Text = TranslateWeatherCode(h.Code) }
                });
            }

            forecastDays.Add(new ForecastDay
            {
                Date = dayGroup.Key.ToString("yyyy-MM-dd"),
                Day = new ForecastDayDetails
                {
                    MaxtempC = dayGroup.Max(x => x.Temp),
                    MintempC = dayGroup.Min(x => x.Temp),
                    AvgtempC = dayGroup.Average(x => x.Temp),
                    Avghumidity = dayGroup.Average(x => x.Humidity),
                    Uv = dayGroup.Average(x => x.Uv),
                    AirQuality = new AirQuality
                    {
                        UsEpaIndex = (int?)dayGroup.Where(x => x.AQI?.Aqi != null).Average(x => x.AQI.Aqi)
                    }
                },
                Hour = hourlyWeathers.ToArray()
            });
        }

        var current = hourlyData.First();

        return new WeatherRoot
        {
            Location = new Location
            {
                Name = cityName,
                Region = "",
                Country = ""
            },
            Forecast = new Forecast
            {
                ForecastDay = forecastDays.ToArray()
            }
        };
    }
    private string TranslateWeatherCode(int code)
    {
        return code switch
        {
            0 => "Ясно",
            1 or 2 => "Малооблачно",
            3 => "Пасмурно",
            45 or 48 => "Туман",
            51 or 53 or 55 => "Мелкий дождь",
            61 or 63 or 65 => "Дождь",
            71 or 73 or 75 => "Снег",
            80 or 81 or 82 => "Ливень",
            _ => "Неизвестно"
        };
    }

    public async Task<CurrentWeather> GetCurrentWeatherAsync(string cityName)
    {
        if (!cities.TryGetValue(cityName, out var englishCityName))
            englishCityName = cityName; // если не найден — используем как есть

        // Получаем координаты города
        var geoUrl = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(englishCityName)}&count=1";
        var geoJson = await _http.GetStringAsync(geoUrl);
        var geo = JsonSerializer.Deserialize<GeoCodingResponse>(geoJson);

        if (geo?.Results == null || geo.Results.Count == 0)
            return null;

        var location = geo.Results[0];
        var latStr = location.Latitude.ToString(CultureInfo.InvariantCulture);
        var lonStr = location.Longitude.ToString(CultureInfo.InvariantCulture);

        // Получаем текущую погоду
        var forecastUrl = $"https://api.open-meteo.com/v1/forecast?latitude={latStr}&longitude={lonStr}&current=temperature_2m,weathercode,uv_index,relative_humidity_2m,windspeed_10m&timezone=auto";
        var airUrl = $"https://air-quality-api.open-meteo.com/v1/air-quality?latitude={latStr}&longitude={lonStr}&current=us_aqi&timezone=auto";

        var forecastJson = await _http.GetStringAsync(forecastUrl);
        var airJson = await _http.GetStringAsync(airUrl);

        var forecastData = JsonSerializer.Deserialize<OpenMeteoCurrentResponse>(forecastJson);
        var airData = JsonSerializer.Deserialize<OpenMeteoAirCurrentResponse>(airJson);

        var current = forecastData?.Current;
        var air = airData?.Current;

        if (current == null)
            return null;

        return new CurrentWeather
        {
            Humidity = current.RelativeHumidity ?? -1,
            WindKph = current.WindSpeed ?? -1,
            Uv = current.UvIndex ?? -1,
            AirQuality = new AirQuality
            {
                UsEpaIndex = air?.UsaAqi ?? -1
            },
            // Добавь свойство "Condition" в CurrentWeather, если нужно
            Condition = new Condition
            {
                Text = TranslateWeatherCode(current.WeatherCode ?? -1)
            }
        };
    }

}

public class CurrentWeather
{
    [JsonPropertyName("humidity")]
    public int Humidity { get; set; }

    [JsonPropertyName("wind_kph")]
    public double WindKph { get; set; }

    [JsonPropertyName("uv")]
    public double Uv { get; set; }

    [JsonPropertyName("air_quality")]
    public AirQuality AirQuality { get; set; }

    [JsonPropertyName("condition")]
    public Condition Condition { get; set; } // <-- Добавь, если не было
}


public class GeoCodingResponse
{
    [JsonPropertyName("results")]
    public List<GeoResult> Results { get; set; }
}

public class GeoResult
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }
}

public class OpenMeteoResponse
{
    [JsonPropertyName("hourly")]
    public OpenMeteoHourly Hourly { get; set; }
}


public class OpenMeteoHourly
{
    [JsonPropertyName("time")]
    public List<string> Time { get; set; }

    [JsonPropertyName("temperature_2m")]
    public List<double?> Temperature { get; set; }

    [JsonPropertyName("relative_humidity_2m")]
    public List<int?> Humidity { get; set; }

    [JsonPropertyName("uv_index")]
    public List<double?> UvIndex { get; set; }

    [JsonPropertyName("weathercode")]
    public List<int?> WeatherCode { get; set; }

    [JsonPropertyName("windspeed_10m")]
    public List<double?> WindSpeed { get; set; }
}

public class OpenMeteoAirQualityResponse
{
    [JsonPropertyName("hourly")]
    public OpenMeteoAirHourly Hourly { get; set; }
}

public class OpenMeteoAirHourly
{
    [JsonPropertyName("time")]
    public List<string> Time { get; set; }

    [JsonPropertyName("us_aqi")]
    public List<int?> UsaAqi { get; set; }
}

public class OpenMeteoCurrentResponse
{
    [JsonPropertyName("current")]
    public OpenMeteoCurrentWeather Current { get; set; }
}

public class OpenMeteoCurrentWeather
{
    [JsonPropertyName("temperature_2m")]
    public double? Temperature { get; set; }

    [JsonPropertyName("weathercode")]
    public int? WeatherCode { get; set; }

    [JsonPropertyName("uv_index")]
    public double? UvIndex { get; set; }

    [JsonPropertyName("relative_humidity_2m")]
    public int? RelativeHumidity { get; set; }

    [JsonPropertyName("windspeed_10m")]
    public double? WindSpeed { get; set; }
}

public class OpenMeteoAirCurrentResponse
{
    [JsonPropertyName("current")]
    public OpenMeteoAirCurrent Current { get; set; }
}

public class OpenMeteoAirCurrent
{
    [JsonPropertyName("us_aqi")]
    public int? UsaAqi { get; set; }
}

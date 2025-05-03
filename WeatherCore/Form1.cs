using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using ScottPlot.WinForms;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using ScottPlot.Statistics;
using MySql.Data.MySqlClient;
using System.Data;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using OpenTK.Graphics.OpenGL;

namespace WeatherCore
{
    public partial class Form1 : Form
    {
        private List<ChartDataPoint> dataPoints = new List<ChartDataPoint>();
        private const int yAxisWidth = 40;
        private const int paddingLeft = 50; // Увеличил отступ слева для оси Y и подписей
        private const int paddingTop = 20;
        private const int paddingRight = 10;
        private const int paddingBottom = 40; // Отступ снизу для меток

        private readonly IWeatherService _weatherService;
        private readonly AppDbContext _appDbContext;

        public Form1(IWeatherService weatherService, AppDbContext context)
        {
            InitializeComponent();
            _weatherService = weatherService;
            _appDbContext = context;
        }

        private static Color background_color = ColorTranslator.FromHtml("#4A90E2");
        private static Color section_color = ColorTranslator.FromHtml("#093860");
        private static Color panel_color = ColorTranslator.FromHtml("#00457E");
        private string selected_city;
        private static Dictionary<string, string> cities = new()
        {
            {"Алматы", "Almaty"},
            {"Астана", "Astana"},
        };

        private DateTime selected_date;

        private static string location_city = cities.TryGetValue("Алматы", out var en) ? en : "Алматы";

        // Пробовал в int возвести, все равно в графике десятичные числа у дней получаются;(
        private double[] days = Enumerable.Range(1, 7).Select(i => (double)i).ToArray();
         
        private FontFamily family;
        public int CornerRadius { get; set; } = 20;

        private void setCity(string city)
        {
            selectedCity.Text = city;
            location_city = city;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            // Задаем фон для элементов
            BackColor = background_color;
            panel1.BackColor = panel_color;
            SetBackColorRecursive(panel1, panel_color);

            // Задаем Combobox как список
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.DrawMode = DrawMode.OwnerDrawFixed;

            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.FlowDirection = FlowDirection.LeftToRight;
            flowLayoutPanel1.WrapContents = false; // чтобы элементы не переносились на следующую строку

            PrivateFontCollection fontCollection = new PrivateFontCollection();
            fontCollection.AddFontFile("Pixeled.ttf"); // файл шрифта
            FontFamily font_family = fontCollection.Families[0];
            family = font_family;

            // Добавляем 30 кнопок (UserControl2)

            // Создаём шрифт и используем далее

            Font font = new Font(family, 17);
            Font = font;

            var bigger_font = new Font(family, 25);

            Control[] labels = { label10, label9, label17, label13, label19, label5, label6, label8, label11, label15, label16, label12 };
            SetFont(labels, bigger_font);

            var biggest_font = new Font(family, 40);
            Control[] biggest_labels = { label14, label7, label17 };
            SetFont(biggest_labels, biggest_font);

            // Белый цвет 
            FormsPlot[] plots = { formsPlot2 };
            foreach (var plot in plots)
            {
                plot.Plot.Axes.Color(ScottPlot.Color.FromHtml("#fdfdfd"));
                plot.Plot.Axes.ContinuouslyAutoscale = true;
            }

            button2.BackColor = Color.White;

            comboBox1.SelectedIndex = 0;

            await UpdateFirstInfo();
        }

        private void SetFont(Control[] labels, Font font)
        {
            foreach (var label in labels)
            {
                label.Font = font;
            }
        }

        private async Task<WeatherRoot> ReturnWeatherObject()
        {
            var manager = new WeatherAPIManager(_weatherService, _appDbContext);
            var get_city = await _weatherService.AddOrGetCityAsync(location_city);
            var old_weather = await manager.GetWeatherByCityIdAndDateRangeOrFetchAsync(get_city.Id, DateTime.Now.Date);
            if (old_weather != null)
            {
                return old_weather;
            }
            var weather = await manager.RunAsync(location_city);
            return weather;
        }

        private void SetUV(ForecastDay day)
        {
            List<double> uvs = new List<double>();
            foreach (var hour in day.Hour)
            {
                uvs.Add(hour.Uv);
            }
            double min = uvs.Min();
            double max = uvs.Max();

            label17.Text = $"{min:F1}/{max:F1}";

            string uvDescription;

            if (max < 3)
            {
                uvDescription = "Низкий";
            }
            else if (max < 6)
            {
                uvDescription = "Умеренный";
            }
            else if (max < 8)
            {
                uvDescription = "Высокий";
            }
            else if (max < 11)
            {
                uvDescription = "Очень высокий";
            }
            else
            {
                uvDescription = "Экстремальный";
            }

            label16.Text = uvDescription;
        }

        private async Task<ForecastDay> ReturnCurDayData()
        {
            var weather = await ReturnWeatherObject();
            if (weather != null)
            {
                var today = weather.Forecast.ForecastDay.FirstOrDefault();
                return today;
            }
            return null;
        }

        private async Task UpdateFirstInfo()
        {
            var weather = await ReturnWeatherObject();
            var today = await ReturnCurDayData();

            if (today?.Hour != null)
            {
                flowLayoutPanel1.Controls.Clear();
                foreach (var hour in today.Hour)
                {
                    DateTime time = DateTime.Parse(hour.Time);
                    string str_time = $"{time:HH}:00";
                    string degrees = $"{Convert.ToInt32(hour.TempC)}°C";
                    UserControl2 my_panel = new UserControl2(str_time, degrees);
                    my_panel.font = new Font(family, 15);
                    flowLayoutPanel1.Controls.Add(my_panel);
                }

                LoadDescription();
                UpdateHumility(weather.Current);
            }
        }

        private List<double> AddTemperatures(ForecastDay[] today)
        {
            List<double> temperatures = new List<double>();

            foreach (var dayObj in today)
            {
                if (dayObj?.Day != null)
                {
                    temperatures.Add(dayObj.Day.AvgtempC);
                }
            }
            return temperatures;
        }

        private async void LoadDescription()
        {
            var translator = new ConditionTranslator();
            await translator.LoadConditionsAsync("codes.json");

            int conditionCode = 1183; // например, Light Rain
            bool isNight = DateTime.Now.Hour < 6 || DateTime.Now.Hour > 20;

            var cur_day = await ReturnCurDayData();
            var cur_day_obj = cur_day.Hour.FirstOrDefault();
            string original = cur_day_obj.Condition.Text.Trim();
            string translated = translator.GetRussianConditionText(original, isNight);

            cloud_desc.Text = translated;

            min_max_degs.Text = $"{cur_day.Day.MintempC}/{cur_day.Day.MaxtempC}°C";

            var weather_obj = await ReturnWeatherObject();
            var cur_weather = weather_obj.Current;

            string air_quelity = LoadAqi(cur_weather);

            label3.Text = air_quelity;
        }

        private string LoadAqi(CurrentWeather cur_day)
        {
            AirQualityGauge gauge = new AirQualityGauge();
            gauge.AQI = cur_day.AirQuality.UsEpaIndex;
            panel3.Controls.Add(gauge);
            int aqi = gauge.AQI;
            string aqiDescription;

            if (aqi <= 50)
            {
                aqiDescription = "Хорошее качество воздуха.";
            }
            else if (aqi <= 100)
            {
                aqiDescription = "Удовлетворительное качество воздуха.";
            }
            else if (aqi <= 150)
            {
                aqiDescription = "Вредно для чувствительных групп.";
            }
            else if (aqi <= 200)
            {
                aqiDescription = "Вредно для здоровья.";
            }
            else if (aqi <= 300)
            {
                aqiDescription = "Очень вредно.";
            }
            else
            {
                aqiDescription = "Опасное качество воздуха.";
            }

            label19.Text = aqiDescription;
            return aqiDescription;
        }

        private void LoadWind(CurrentWeather cur_weather)
        {
            label7.Text = $"{cur_weather.WindKph:F1} м/с";

            string windDescription;

            if (cur_weather.WindKph < 1)
            {
                windDescription = "Штиль";
            }
            else if (cur_weather.WindKph < 5)
            {
                windDescription = "Слабый";
            }
            else if (cur_weather.WindKph < 10)
            {
                windDescription = "Умеренный";
            }
            else if (cur_weather.WindKph < 15)
            {
                windDescription = "Сильный";
            }
            else
            {
                windDescription = "Очень сильный";
            }

            label6.Text = windDescription;
        }

        private void UpdateHumility(CurrentWeather cur_day)
        {
            var avg_humuility = cur_day.Humidity;
            label14.Text = $"{avg_humuility}%";

            string humidityDescription;

            if (avg_humuility < 30)
            {
                humidityDescription = "Слишком сухо.";
            }
            else if (avg_humuility >= 30 && avg_humuility <= 60)
            {
                humidityDescription = "Нормальная.";
            }
            else if (avg_humuility > 60 && avg_humuility <= 80)
            {
                humidityDescription = "Высокая.";
            }
            else
            {
                humidityDescription = "Очень высокая.";
            }

            label13.Text = humidityDescription;
        }

        private async void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            object selected = comboBox1.SelectedItem;
            string selected_city = selected.ToString().Trim();
            setCity(selected_city);
            await UpdateFirstInfo();
        }
        private void SetBackColorRecursive(Control parent, Color color, Font font = null)
        {
            foreach (Control child in parent.Controls)
            {
                child.BackColor = color;
                if (font != null)
                {
                    child.Font = font;
                }
                SetBackColorRecursive(child, color);
            }
        }

        private void tabPage1_Paint(object sender, PaintEventArgs e)
        {
            Control tab = (Control)sender;
            using (Pen pen = new Pen(panel_color, 2)) // цвет и толщина границы
            {
                Rectangle rect = new Rectangle(0, 0, tab.Width - 1, tab.Height - 1);
                e.Graphics.DrawRectangle(pen, rect);
            }
        }

        private void comboBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index >= 0)
            {
                e.DrawBackground(); // Рисуем фоновую часть
                e.Graphics.DrawString(comboBox1.Items[e.Index].ToString(), e.Font, Brushes.WhiteSmoke, e.Bounds);
                e.Dispose();
            }
        }

        private void setGraphic(FormsPlot plot, double[] x, double[] y)
        {
            plot.Plot.Clear();
            plot.Plot.Add.Scatter(x, y);
            plot.Refresh();
        }

        // Тоже chatgpt оптимизировал
        private async void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            var cur_day_weather = await ReturnCurDayData();
            var weather = await ReturnWeatherObject();

            switch (tabControl1.SelectedIndex)
            {
                case 0:
                    UpdateHumility(weather.Current);
                    break;

                case 1:
                    var weather_data = await ReturnWeatherObject();
                    double[] temperatures = AddTemperatures(weather_data.Forecast.ForecastDay).ToArray();
                    label10.Text = $"{temperatures.Average():F1}°C";
                    setGraphic(formsPlot2, days, temperatures);
                    break;

                case 2:
                    LoadAqi(weather.Current);
                    break;

                case 3:
                    LoadWind(weather.Current);
                    break;

                case 4:
                    SetUV(cur_day_weather);
                    break;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (FormSelectDay form = new FormSelectDay())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    DateTime selected_Date = form.SelectedDate;
                    selected_date = selected_Date;
                }
            }
        }
    }
    public class ChartDataPoint
    {
        public int Value { get; set; }
        public string Label { get; set; }

        public ChartDataPoint(int value, string label)
        {
            Value = value;
            Label = label;
        }
    }

    // Здесь начинается код от chagpt чеcтно говоря, потому что я понятия не имел какова структура)
    public class WeatherAPIManager
    {
        private const string apiKey = "aa65767110d44c4993c113415252904";
        private const string filePath = "weather.json";
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly AppDbContext _context;

        private IWeatherService weatherService;

        public WeatherAPIManager(IWeatherService weatherService, AppDbContext context)
        {
            weatherService = weatherService;
            _context = context;
        }

        public async Task<WeatherRoot> RunAsync(string city)
        {
            var weather_data = await ReadWeatherFromFileAsync(filePath);
            var cur_loc_city = weather_data.Location.Name;

            if ((!File.Exists(filePath) || File.GetLastWriteTimeUtc(filePath).Date < DateTime.UtcNow.Date) && cur_loc_city != city)
            {
                Debug.WriteLine("Файл не найден, устарел или запрошен новый город. Загружаю новый...");
                await FetchAndSaveWeatherAsync(city);
                weather_data = await ReadWeatherFromFileAsync(filePath);
            }
            else
            {
                Debug.WriteLine("Файл актуален. Использую локальные данные.");
            }

            var translator = new ConditionTranslator();

            //Debug.WriteLine($"Влажность: {weather_data.Current.Humidity}%");
            //Debug.WriteLine($"Ветер: {weather_data.Current.WindKph / 3.6:F1} м/с");
            //Debug.WriteLine($"UV индекс: {weather_data.Current.Uv}");
            //Debug.WriteLine($"Качество воздуха (AQI): {weather_data.Current.AirQuality.UsEpaIndex}");

            var get_city = await weatherService.AddOrGetCityAsync(cur_loc_city);

            if (get_city != null)
            {
                return weather_data;
            }

            Debug.WriteLine("\n📆 Прогноз на 7 дней:");
            foreach (var day in weather_data.Forecast.ForecastDay)
            {
                var day_date = day.Date;

                double[] min_max_uv_day = getUvMinMax(day);

                Debug.WriteLine($"\n📅 {day.Date}: {day.Day.MaxtempC}°C / {day.Day.MintempC}°C, AQI: {day.Day.AirQuality?.UsEpaIndex}, UV: {day.Day.Uv}, Влажность: {day.Day.Avghumidity}%");

                Debug.WriteLine($"🕒 Почасовой прогноз:");

                double average_winds = day.Hour.Select(h => h.WindKph / 3.6).Average();
                double[] uvs = day.Hour.Select(h => h.Uv).ToArray();
                double min_v = uvs.Min();
                double max_v = uvs.Max();

                var day_db = await weatherService.AddDayWeatherAsync(get_city.Name, new DayWeather { Day = DateTime.Parse(day_date), Humility = day.Day.Avghumidity, Wind = average_winds, Uv_min = min_v, Uv_max = max_v, Aqi = day.Day.AirQuality.UsEpaIndex, Temp_min = day.Day.MintempC, Temp_max = day.Day.MaxtempC, CityId = get_city.Id});

                foreach (var hour in day.Hour)
                {
                    double windMs = hour.WindKph / 3.6;
                    Debug.WriteLine($"{hour.Time} | 🌡️ {hour.TempC}°C | 💧 {hour.Humidity}% | 🌬️ {windMs:F1} м/с | 🔆 UV: {hour.Uv} | ☁️ {hour.Condition?.Text}");
                    await weatherService.AddHourWeatherAsync(day_db.Id, new HourWeather { cur_temp = hour.TempC, condition = translator.GetRussianConditionText(hour.Condition.Text), DayId = day_db.Id, hour = DateTime.Parse(hour.Time)});
                }
            }
            return weather_data;
        }

        public async Task<WeatherRoot> GetWeatherByCityIdAndDateRangeOrFetchAsync(int cityId, DateTime startDate)
        {
            var city = await _context.Cities.FirstOrDefaultAsync(c => c.Id == cityId);
            if (city == null)
                return null;

            var forecastDays = await _context.Days
                .Include(d => d.hourlyWeathers)
                .Where(d => d.CityId == cityId && d.Day >= startDate.Date && d.Day < startDate.Date.AddDays(7))
                .OrderBy(d => d.Day)
                .ToListAsync();

            if (forecastDays.Count < 7)
            {
                // Недостаточно данных — загружаем и добавляем
                var weatherRoot = await RunAsync(city.Name);
                return weatherRoot;
            }

            var forecast = forecastDays.Select(dayWeather =>
            {
                return new ForecastDay
                {
                    Date = dayWeather.Day.ToString("yyyy-MM-dd"),
                    Day = new ForecastDayDetails
                    {
                        AvgtempC = (dayWeather.Temp_min + dayWeather.Temp_max) / 2,
                        MaxtempC = dayWeather.Temp_max,
                        MintempC = dayWeather.Temp_min,
                        Avghumidity = dayWeather.Humility,
                        Uv = dayWeather.Uv_max,
                        AirQuality = new AirQuality
                        {
                            UsEpaIndex = dayWeather.Aqi
                        }
                    },
                    Hour = dayWeather.hourlyWeathers.Select(h => new HourlyWeather
                    {
                        Time = h.hour.ToString("yyyy-MM-dd HH:mm"),
                        TempC = h.cur_temp,
                        Humidity = (int)dayWeather.Humility,
                        WindKph = dayWeather.Wind,
                        Uv = dayWeather.Uv_max,
                        Condition = new Condition
                        {
                            Text = h.condition
                        }
                    }).ToArray()
                };
            }).ToArray();

            return new WeatherRoot
            {
                Location = new Location
                {
                    Name = city.Name
                },
                Forecast = new Forecast
                {
                    ForecastDay = forecast
                }
            };
        }


        private double[] getUvMinMax(ForecastDay day)
        {
            List<double> uvs = new List<double>();
            foreach (var hour in day.Hour)
            {
                uvs.Add(hour.Uv);
            }
            double min = uvs.Min();
            double max = uvs.Max();
            return new double[] { min, max };
        }


        private async Task FetchAndSaveWeatherAsync(string city)
        {
            string url = $"http://api.weatherapi.com/v1/forecast.json?key={apiKey}&q={city}&days=7&aqi=yes&alerts=no";
            try
            {
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                await File.WriteAllTextAsync(filePath, json);
                Debug.WriteLine("Данные успешно загружены и сохранены.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении погоды", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<WeatherRoot> ReadWeatherFromFileAsync(string file)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var weather = JsonSerializer.Deserialize<WeatherRoot>(json);
                return weather;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при чтении файла", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine(ex.Message);
                return null;
            }
        }
    }


    public class WeatherRoot
    {
        [JsonPropertyName("current")]
        public CurrentWeather Current { get; set; }

        [JsonPropertyName("location")]
        public Location Location { get; set; }

        [JsonPropertyName("forecast")]
        public Forecast Forecast { get; set; }
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
    }

    public class Forecast
    {
        [JsonPropertyName("forecastday")]
        public ForecastDay[] ForecastDay { get; set; }
    }

    public class ForecastDay
    {
        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("day")]
        public ForecastDayDetails Day { get; set; }

        [JsonPropertyName("hour")]
        public HourlyWeather[] Hour { get; set; }
    }

    public class HourlyWeather
    {
        [JsonPropertyName("time")]
        public string Time { get; set; }

        [JsonPropertyName("temp_c")]
        public double TempC { get; set; }

        [JsonPropertyName("humidity")]
        public int Humidity { get; set; }

        [JsonPropertyName("wind_kph")]
        public double WindKph { get; set; }

        [JsonPropertyName("uv")]
        public double Uv { get; set; }

        [JsonPropertyName("condition")]
        public Condition Condition { get; set; }
    }

    public class Condition
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }

    public class ForecastDayDetails
    {
        [JsonPropertyName("maxtemp_c")]
        public double MaxtempC { get; set; }

        [JsonPropertyName("mintemp_c")]
        public double MintempC { get; set; }

        [JsonPropertyName("avghumidity")]
        public double Avghumidity { get; set; }

        [JsonPropertyName("avgtemp_c")]
        public double AvgtempC { get; set; } // <-- добавлено

        [JsonPropertyName("uv")]
        public double Uv { get; set; }

        [JsonPropertyName("air_quality")]
        public AirQuality AirQuality { get; set; }
    }

    public class AirQuality
    {
        [JsonPropertyName("us-epa-index")]
        public int UsEpaIndex { get; set; }
    }

    public class Location
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("region")]
        public string Region { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }
    }


    public class WeatherCondition
    {
        public int code { get; set; }
        public string day { get; set; }
        public string night { get; set; }
        public int icon { get; set; }
        public List<LanguageTranslation> languages { get; set; }
    }

    public class LanguageTranslation
    {
        public string lang_name { get; set; }
        public string lang_iso { get; set; }
        public string day_text { get; set; }
        public string night_text { get; set; }
    }

    public class ConditionTranslator
    {
        private List<WeatherCondition> _conditions;

        public async Task LoadConditionsAsync(string filePath)
        {
            string json = await File.ReadAllTextAsync(filePath);
            _conditions = JsonSerializer.Deserialize<List<WeatherCondition>>(json);
        }

        public string GetRussianConditionText(string condition_text, bool is_night = false)
        {
            foreach (var item in _conditions)
            {
                var ru_lang = item.languages.FirstOrDefault(l => l.lang_iso == "ru");
                if (item.day == condition_text)
                {
                    return ru_lang.day_text;
                }
                else if (item.night == condition_text)
                {
                    return ru_lang.night_text;
                }
            }
            return null;
        }

    }

    public class AppDbContext : DbContext
    {
        public DbSet<City> Cities { get; set; }
        public DbSet<DayWeather> Days { get; set; }
        public DbSet<HourWeather> Hours { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseMySql("server=localhost;database=weathercore_db;user=root;password=12345",
                new MySqlServerVersion(new Version(8, 0, 34)));
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<City>()
            .HasMany(c => c.Weather)  // Один город может иметь несколько прогнозов
            .WithOne(w => w.City)     // Каждый прогноз относится к одному городу
            .HasForeignKey(w => w.CityId); // Явное указание на внешний ключ

            modelBuilder.Entity<DayWeather>()
            .HasMany(d => d.hourlyWeathers)
            .WithOne(h => h.Day)
            .HasForeignKey(w => w.DayId);
        }
    }

    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<DayWeather> Weather { get; set; }
    }

    public class DayWeather
    {
        public int Id { get; set; }
        public DateTime Day { get; set; }
        public double Humility {  get; set; }
        public double Wind {  get; set; }
        public double Uv_min { get; set; }
        public double Uv_max { get; set; }
        public int Aqi { get; set; }
        public double Temp_min { get; set; }
        public double Temp_max { get; set; }
        public int CityId { get; set; }

        public City City { get; set; }
        public List<HourWeather> hourlyWeathers { get; set; }
    }

    public class HourWeather
    {
        public int Id { get; set; }
        public DateTime hour { get; set; }
        public string condition { get; set; }
        public double cur_temp { get; set; }

        public int DayId { get; set; }
        public DayWeather Day { get; set; }
    }

    public interface ICityRepository
    {
        Task<List<City>> GetAllAsync();
        Task<City?> GetByIdAsync(int id);
        Task AddAsync(City city);
    }

    public interface IDayWeatherRepository
    {
        Task<List<DayWeather>> GetAllByCityIdAsync(int cityId);
        Task<DayWeather?> GetByIdAsync(int id);
        Task AddAsync(DayWeather weather);
    }

    public interface IHourWeatherRepository
    {
        Task<List<HourWeather>> GetAllByDayIdAsync(int dayId);
        Task<HourWeather?> GetByIdAsync(int id);
        Task AddAsync(HourWeather hourWeather);
    }


    public class CityRepository : ICityRepository
    {
        private readonly AppDbContext _context;

        public CityRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<City>> GetAllAsync()
        {
            return await _context.Cities
                .Include(c => c.Weather)
                .ToListAsync();
        }

        public async Task<City?> GetByIdAsync(int id)
        {
            return await _context.Cities
                .Include(c => c.Weather)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(City city)
        {
            _context.Cities.Add(city);
            await _context.SaveChangesAsync();
        }
    }

    public class DayWeatherRepository : IDayWeatherRepository
    {
        private readonly AppDbContext _context;

        public DayWeatherRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<DayWeather>> GetAllByCityIdAsync(int cityId)
        {
            return await _context.Days
                .Where(w => w.CityId == cityId)
                .ToListAsync();
        }

        public async Task<DayWeather?> GetByIdAsync(int id)
        {
            return await _context.Days
                .Include(w => w.City)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task AddAsync(DayWeather weather)
        {
            _context.Days.Add(weather);
            await _context.SaveChangesAsync();
        }
    }

    public class HourWeatherRepository : IHourWeatherRepository
    {
        private readonly AppDbContext _context;

        public HourWeatherRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<HourWeather>> GetAllByDayIdAsync(int dayId)
        {
            return await _context.Hours
                .Where(h => h.DayId == dayId)
                .ToListAsync();
        }

        public async Task<HourWeather?> GetByIdAsync(int id)
        {
            return await _context.Hours
                .Include(h => h.Day)
                .FirstOrDefaultAsync(h => h.Id == id);
        }

        public async Task AddAsync(HourWeather hourWeather)
        {
            _context.Hours.Add(hourWeather);
            await _context.SaveChangesAsync();
        }
    }


    public interface IWeatherService
    {
        Task<City> AddOrGetCityAsync(string cityName);
        Task<DayWeather> AddDayWeatherAsync(string cityName, DayWeather dayWeather);
        Task<List<DayWeather>> GetForecastByCityAsync(string cityName);
        Task AddHourWeatherAsync(int dayId, HourWeather hourWeather);
        Task<List<HourWeather>> GetHourlyForecastAsync(int dayId);
    }


    public class WeatherService : IWeatherService
    {
        private readonly ICityRepository _cityRepo;
        private readonly IDayWeatherRepository _weatherRepo;
        private readonly IHourWeatherRepository _hourRepo;

        public WeatherService(ICityRepository cityRepo, IDayWeatherRepository weatherRepo)
        {
            _cityRepo = cityRepo;
            _weatherRepo = weatherRepo;
        }

        public async Task<City> AddOrGetCityAsync(string cityName)
        {
            var cities = await _cityRepo.GetAllAsync();
            var city = cities.FirstOrDefault(c => c.Name.ToLower() == cityName.ToLower());

            if (city == null)
            {
                city = new City { Name = cityName };
                await _cityRepo.AddAsync(city);
            }

            return city;
        }

        public async Task<DayWeather> AddDayWeatherAsync(string cityName, DayWeather dayWeather)
        {
            var city = await AddOrGetCityAsync(cityName);
            dayWeather.CityId = city.Id;

            await _weatherRepo.AddAsync(dayWeather);
            return dayWeather;
        }

        public async Task AddHourWeatherAsync(int dayId, HourWeather hourWeather)
        {
            hourWeather.DayId = dayId;
            await _hourRepo.AddAsync(hourWeather);
        }

        public async Task<List<DayWeather>> GetForecastByCityAsync(string cityName)
        {
            var cities = await _cityRepo.GetAllAsync();
            var city = cities.FirstOrDefault(c => c.Name.ToLower() == cityName.ToLower());

            if (city == null)
                return new List<DayWeather>();

            return await _weatherRepo.GetAllByCityIdAsync(city.Id);
        }

        public async Task<List<HourWeather>> GetHourlyForecastAsync(int dayId)
        {
            return await _hourRepo.GetAllByDayIdAsync(dayId);
        }
    }
}
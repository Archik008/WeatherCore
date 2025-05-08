using Microsoft.EntityFrameworkCore;
using ScottPlot.WinForms;
using System.Data;
using System.Diagnostics;
using System.Drawing.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

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

        private const string filePath = "weather.json";

        private readonly WeatherService _weatherService;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public Form1(IDbContextFactory<AppDbContext> contextFactory, WeatherService weatherService)
        {
            InitializeComponent();
            _contextFactory = contextFactory;
            _weatherService = weatherService;
        }

        private static Color background_color = ColorTranslator.FromHtml("#4A90E2");
        private static Color section_color = ColorTranslator.FromHtml("#093860");
        private static Color panel_color = ColorTranslator.FromHtml("#00457E");
        private string selected_city;

        private DateTime selected_date = DateTime.Now.Date;

        private static string location_city = "Алматы";

        // Пробовал в int возвести, все равно в графике десятичные числа у дней получаются;(
        private double[] daily_hours = Enumerable.Range(0, 24).Select(i => (double)i).ToArray();

        private FontFamily family;
        public int CornerRadius { get; set; } = 20;

        private bool is_running = false;

        private void setCity(string city)
        {
            selectedCity.Text = city;
            location_city = city;
        }

        private readonly SemaphoreSlim _initSemaphore = new SemaphoreSlim(1, 1);
        private bool _isInitialized = false;

        private async Task LoadFormAsync()
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

        private async void Form1_Load(object sender, EventArgs e)
        {
            await LoadFormAsync();
        }

        private void SetFont(Control[] labels, Font font)
        {
            foreach (var label in labels)
            {
                label.Font = font;
            }
        }

        public string TruncateText(string text, int maxLength = 16)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength).Trim() + "...";
        }

        private async Task<WeatherRoot> ReturnWeatherObject()
        {
            await _initSemaphore.WaitAsync();
            try
            {
                var manager = new WeatherAPIManager(_contextFactory, _weatherService);
                var get_city = await _weatherService.AddOrGetCityAsync(location_city);
                var old_weather = await manager.GetWeatherByCityAndDayAsync(get_city.Name, selected_date);
                var weather = await manager.RunAsync(get_city.Name, selected_date);

                if (get_city == null || old_weather == null)
                {
                    return weather;
                }
                else
                {
                    return old_weather;
                }
            }
            finally
            {
                _initSemaphore.Release();
            }

        }

        private async Task SetUV()
        {
            var weather_obj = await ReturnWeatherObject();
            double[] uvs = weather_obj.Forecast.ForecastDay.FirstOrDefault().Hour.Select(obj => obj.Uv).ToArray();
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
            var today = await ReturnCurDayData();

            if (today?.Hour != null)
            {
                flowLayoutPanel1.Controls.Clear();
                var translator = new ConditionTranslator();
                await translator.LoadConditionsAsync("codes.json");

                foreach (var hour in today.Hour)
                {
                    DateTime time = DateTime.Parse(hour.Time);
                    string str_time = $"{time:HH}:00";
                    string degrees = $"{Convert.ToInt32(hour.TempC)}°C";

                    Debug.WriteLine($"Условия: {hour.Condition.Text}");

                    var my_panel = new UserControl2(str_time, degrees, hour.Condition.Text);
                    my_panel.font = new Font(family, 15);
                    flowLayoutPanel1.Controls.Add(my_panel);
                }

                await LoadDescription();
                await UpdateHumility();
            }
        }


        private List<double> AddTemperatures(HourlyWeather[] hours)
        {
            List<double> temperatures = new List<double>();

            foreach (var hour in hours)
            {
                temperatures.Add(hour.TempC);
            }
            return temperatures;
        }

        private async Task LoadDescription()
        {
            var translator = new ConditionTranslator();
            await translator.LoadConditionsAsync("codes.json");

            bool isNight = DateTime.Now.Hour < 6 || DateTime.Now.Hour > 20;

            var cur_day = await ReturnCurDayData();
            var cur_day_obj = cur_day.Hour.FirstOrDefault();
            string translated = cur_day_obj.Condition.Text.Trim();

            cloud_desc.Text = TruncateText(translated, 30);
            ToolTip tooltip = new ToolTip();
            tooltip.SetToolTip(cloud_desc, translated);

            min_max_degs.Text = $"{cur_day.Day.MintempC}/{cur_day.Day.MaxtempC}°C";

            string air_quality = await LoadAqi();

            label3.Text = air_quality;
        }

        private async Task<string> LoadAqi()
        {
            var cur_day = await ReturnCurDayData();
            AirQualityGauge gauge = new AirQualityGauge();
            gauge.AQI = cur_day.Day.AirQuality.UsEpaIndex;
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

        private async Task LoadWind()
        {
            var cur_day_weather = await ReturnCurDayData();
            var windKphAvg = cur_day_weather.Hour.Average(h => h.WindKph);
            label7.Text = $"{windKphAvg:F1} м/с";

            string windDescription;

            if (windKphAvg < 1)
            {
                windDescription = "Штиль";
            }
            else if (windKphAvg < 5)
            {
                windDescription = "Слабый";
            }
            else if (windKphAvg < 10)
            {
                windDescription = "Умеренный";
            }
            else if (windKphAvg < 15)
            {
                windDescription = "Сильный";
            }
            else
            {
                windDescription = "Очень сильный";
            }

            label6.Text = windDescription;
        }

        private async Task UpdateHumility()
        {
            var cur_day_data = await ReturnCurDayData();
            var avg_humuility = cur_day_data.Day.Avghumidity;
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
            selected_date = DateTime.Now;
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
                    UpdateHumility();
                    break;

                case 1:
                    var weather_data = await ReturnWeatherObject();
                    double[] temperatures = AddTemperatures(weather_data.Forecast.ForecastDay.FirstOrDefault().Hour).ToArray();
                    label10.Text = $"{temperatures.Average():F1}°C";
                    setGraphic(formsPlot2, daily_hours, temperatures);
                    break;

                case 2:
                    await LoadAqi();
                    break;

                case 3:
                    await LoadWind();
                    break;

                case 4:
                    await SetUV();
                    break;
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            var minMaxDays = await _weatherService.GetMaxMinDate();
            using (FormSelectDay form = new FormSelectDay(minMaxDays[0], minMaxDays[1]))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    DateTime selected_Date = form.SelectedDate;
                    selected_date = selected_Date;
                    var weather_manager = new WeatherAPIManager(_contextFactory, _weatherService);
                    await weather_manager.RunAsync(location_city, selected_date);
                    await UpdateFirstInfo();
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
    public class WeatherAPIManager
    {
        private const string apiKey = "aa65767110d44c4993c113415252904";
        private const string filePath = "weather.json";
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly AppDbContext _context;
        private WeatherService _weatherService;

        private static Dictionary<string, string> weather_cities_dict = new Dictionary<string, string>() { { "Алматы", "Almaty" }, { "Астана", "Astana" } };

        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        public WeatherAPIManager(IDbContextFactory<AppDbContext> contextFactory, WeatherService weatherService)
        {
            _contextFactory = contextFactory;
            _weatherService = weatherService;
        }
        public async Task<WeatherRoot> RunAsync(string city, DateTime? start_date = null)
        {
            var weather_data = await ReadWeatherFromFileAsync(filePath);
            string cur_loc_city;
            if (weather_data == null)
            {
                cur_loc_city = city;
            }
            else
            {
                cur_loc_city = weather_data.Location.Name;
            }

            WeatherRoot weather_obj;
            if (start_date.HasValue)
            {
                weather_obj = await GetWeatherByCityAndDayAsync(city, start_date.Value.Date);
            }
            else
            {
                var cur_date = DateTime.UtcNow;
                weather_obj = await GetWeatherByCityAndDayAsync(city, cur_date.Date);
            }
            if (weather_obj != null)
            {
                Debug.WriteLine("Использование из базы данных успешно");
                return weather_obj;
            }

            if (weather_obj == null || File.GetLastWriteTimeUtc(filePath).Date < DateTime.UtcNow.Date || cur_loc_city != city)
            {
                await FetchAndSaveWeatherAsync(city);
                Debug.WriteLine("Файл не найден, устарел или запрошен новый город. Загружаю новый...");
                weather_data = await ReadWeatherFromFileAsync(filePath);
                return await WriteWeatherData(weather_data, city);
            }

            var is_city_exists = await _weatherService.IsCityExists(city);

            if (is_city_exists)
            {
                return weather_data;
            }

            return await WriteWeatherData(weather_data, city);
        }

        public async Task<WeatherRoot> WriteWeatherData(WeatherRoot weather_data, string cur_loc_city)
        {
            var translator = new ConditionTranslator();
            await translator.LoadConditionsAsync("codes.json");


            var get_city = await _weatherService.AddOrGetCityAsync(cur_loc_city);

            foreach (var day in weather_data.Forecast.ForecastDay)
            {
                var day_date = day.Date;

                double[] min_max_uv_day = getUvMinMax(day);

                //Debug.WriteLine($"\n📅 {day.Date}: {day.Day.MaxtempC}°C / {day.Day.MintempC}°C, AQI: {day.Day.AirQuality?.UsEpaIndex}, UV: {day.Day.Uv}, Влажность: {day.Day.Avghumidity}%");

                //Debug.WriteLine($"🕒 Почасовой прогноз:");

                double average_winds = day.Hour.Select(h => h.WindKph / 3.6).Average();
                double[] uvs = day.Hour.Select(h => h.Uv).ToArray();
                double min_v = uvs.Min();
                double max_v = uvs.Max();

                var day_db = await _weatherService.AddDayWeatherAsync(get_city.Name, new DayWeather { Day = DateTime.Parse(day_date), Humility = day.Day.Avghumidity, Wind = average_winds, Uv = day.Day.Uv, Aqi = day.Day.AirQuality.UsEpaIndex, Temp_min = day.Day.MintempC, Temp_max = day.Day.MaxtempC, CityId = get_city.Id });

                foreach (var hour in day.Hour)
                {
                    double windMs = hour.WindKph / 3.6;
                    string translated_text = translator.GetRussianConditionText(hour.Condition?.Text);
                    if (translated_text == null)
                    {
                        continue;
                    }
                    //Debug.WriteLine($"{hour.Time} | 🌡️ {hour.TempC}°C | 💧 {hour.Humidity}% | 🌬️ {windMs:F1} м/с | 🔆 UV: {hour.Uv} | ☁️ {hour.Condition?.Text}");
                    await _weatherService.AddHourWeatherAsync(day_db.Id, new HourWeather { cur_temp = hour.TempC, condition = translated_text, DayId = day_db.Id, hour = DateTime.Parse(hour.Time) });
                }
            }
            return weather_data;
        }

        public async Task<WeatherRoot> GetWeatherByCityAndDayAsync(string cityName, DateTime currentDate)
        {
            using var context = _contextFactory.CreateDbContext();

            var city = await context.Cities
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Name == cityName);

            if (city == null)
                return null;

            var day = await context.Days
                .Where(d => d.CityId == city.Id && d.Day.Date == currentDate)
                .Include(d => d.hourlyWeathers)
                .FirstOrDefaultAsync();

            if (day == null)
                return null;

            var forecastDay = new ForecastDay
            {
                Date = day.Day.ToString("yyyy-MM-dd"),
                Day = new ForecastDayDetails
                {
                    AvgtempC = Math.Round((day.Temp_min + day.Temp_max) / 2, 1),
                    MaxtempC = day.Temp_max,
                    MintempC = day.Temp_min,
                    Avghumidity = day.Humility,
                    Uv = day.Uv,
                    AirQuality = new AirQuality
                    {
                        UsEpaIndex = day.Aqi
                    }
                },
                Hour = day.hourlyWeathers.Select(h => new HourlyWeather
                {
                    Time = h.hour.ToString("yyyy-MM-dd HH:mm"),
                    TempC = h.cur_temp,
                    Humidity = (int)day.Humility,
                    WindKph = day.Wind,
                    Uv = day.Uv,
                    Condition = new Condition
                    {
                        Text = h.condition
                    }
                }).ToArray()
            };

            return new WeatherRoot
            {
                Location = new Location { Name = city.Name },
                Forecast = new Forecast { ForecastDay = new[] { forecastDay } }
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
            catch 
            {
                return;
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

        public string GetRussianConditionText(string condition_text = null, bool is_night = false)
        {
            if (condition_text == null)
            {
                return null;
            }
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
               .HasMany(c => c.Weather)
               .WithOne(w => w.City)
               .HasForeignKey(w => w.CityId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DayWeather>()
                .HasMany(d => d.hourlyWeathers)
                .WithOne(h => h.Day)
                .HasForeignKey(w => w.DayId)
                .OnDelete(DeleteBehavior.Cascade);
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
        public double Humility { get; set; }
        public double Wind { get; set; }
        public double Uv {  get; set; }
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


    public class WeatherService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public WeatherService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<City> AddOrGetCityAsync(string cityName)
        {
            using var context = _contextFactory.CreateDbContext();

            var city = await context.Cities
                .FirstOrDefaultAsync(c => c.Name.ToLower() == cityName.ToLower());

            if (city == null)
            {
                city = new City { Name = cityName };
                context.Cities.Add(city);
                await context.SaveChangesAsync();
            }

            return city;
        }

        public async Task<bool> IsCityExists(string cityName)
        {
            using var context = _contextFactory.CreateDbContext();
            var city = await context.Cities
                .FirstOrDefaultAsync(c => c.Name.ToLower() == cityName.ToLower());
            if (city == null)
            {
                return false;
            }
            return true;
        }

        public async Task<DateTime[]> GetMaxMinDate()
        {
            using var context = _contextFactory.CreateDbContext();

            var minDate = await context.Days.MinAsync(d => (DateTime?)d.Day);
            var maxDate = await context.Days.MaxAsync(d => (DateTime?)d.Day);

            // Если таблица пуста — возвращаем массив с сегодняшней датой
            if (minDate == null || maxDate == null)
            {
                var today = DateTime.Today;
                return new[] { today, today };
            }

            return new[] { minDate.Value, maxDate.Value };
        }

        public async Task<DayWeather> AddDayWeatherAsync(string cityName, DayWeather dayWeather)
        {
            using var context = _contextFactory.CreateDbContext();

            var city = await AddOrGetCityAsync(cityName);
            dayWeather.CityId = city.Id;

            var existingDay = await context.Days.FirstOrDefaultAsync(d => d.Day == dayWeather.Day && d.CityId == city.Id);

            if (existingDay != null)
            {
                return existingDay; // ✅ Возвращаем уже существующий, с валидным Id
            }

            context.Days.Add(dayWeather);
            await context.SaveChangesAsync();
            return dayWeather; // ✅ Теперь у него есть Id
        }

        public async Task<HourWeather> AddHourWeatherAsync(int dayId, HourWeather hourWeather)
        {
            hourWeather.DayId = dayId;
            using var context = _contextFactory.CreateDbContext();
            context.Hours.Add(hourWeather);
            await context.SaveChangesAsync();
            return hourWeather;
        }

        public async Task<List<DayWeather>> GetForecastByCityAsync(string cityName)
        {
            using var context = _contextFactory.CreateDbContext();

            var city = await context.Cities
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Name.ToLower() == cityName.ToLower());

            if (city == null)
                return new List<DayWeather>();

            return await context.Days
                .Where(dw => dw.CityId == city.Id)
                .ToListAsync();
        }

        public async Task<List<HourWeather>> GetHourlyForecastAsync(int dayId)
        {
            using var context = _contextFactory.CreateDbContext();

            return await context.Hours
                .Where(hw => hw.DayId == dayId)
                .ToListAsync();
        }
    }
}


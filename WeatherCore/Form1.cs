using Microsoft.EntityFrameworkCore;
using OpenTK.Graphics.ES11;
using ScottPlot.WinForms;
using System.Data;
using System.Diagnostics;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;

namespace WeatherCore
{
    public partial class Form1 : Form
    {

        private readonly WeatherService _weatherService;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public Form1(IDbContextFactory<AppDbContext> contextFactory, WeatherService weatherService)
        {
            InitializeComponent();
            _contextFactory = contextFactory;
            _weatherService = weatherService;
            notifyIcon1.Icon = new Icon(ResourcePathHelper.GetPath("Resources/app_icon.ico")); // путь к твоей иконке
            notifyIcon1.Visible = true;
            notifyIcon1.Text = "WeatherCore запущен";
        }

        private static Color background_color = ColorTranslator.FromHtml("#4A90E2");
        private static Color panel_color = ColorTranslator.FromHtml("#00457E");

        private DateTime selected_date = DateTime.Now.Date;

        private static string location_city = "Алматы";

        // Пробовал в int возвести, все равно в графике десятичные числа у дней получаются;(
        private double[] daily_hours = Enumerable.Range(0, 24).Select(i => (double)i).ToArray();

        private FontFamily pixeled_family;
        public int CornerRadius { get; set; } = 20;

        private void setCity(string city)
        {
            selectedCity.Text = city;
            location_city = city;
        }

        private readonly SemaphoreSlim _initSemaphore = new SemaphoreSlim(1, 1);

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
            fontCollection.AddFontFile(ResourcePathHelper.GetPath("Resources/Pixeled.ttf")); // файл шрифта
            FontFamily family = fontCollection.Families[0];
            // Создаём шрифт и используем далее
            pixeled_family = family;

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

            await _initSemaphore.WaitAsync();
            await UpdateFirstInfo();
        }

        private bool _formLoaded = false;
        private async void Form1_Load(object sender, EventArgs e)
        {
            if (_formLoaded) return;
            _formLoaded = true;
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

        private async Task SetUV()
        {
            var weather_obj = await ReturnCurDayData();
            double[] uvs = weather_obj.Hour.Select(obj => obj.Uv).ToArray();
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
                var today = weather.Forecast.ForecastDay.FirstOrDefault(d => DateTime.Parse(d.Date) == selected_date.Date);
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

                foreach (var hour in today.Hour)
                {
                    DateTime time = DateTime.Parse(hour.Time);
                    string str_time = $"{time:HH}:00";
                    string degrees = $"{Convert.ToInt32(hour.TempC)}°C";

                    bool is_night = time.Hour > 20 || time.Hour < 6;

                    var my_panel = new UserControl2(str_time, degrees, hour.Condition.Text, is_night);
                    my_panel.font = new Font(pixeled_family, 15);
                    flowLayoutPanel1.Controls.Add(my_panel);
                }

                await LoadDescription();
                await updateTabPage();
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
            var cur_day = await ReturnCurDayData();
            var cur_day_obj = cur_day.Hour.FirstOrDefault(h => DateTime.Parse(h.Time).Hour == selected_date.Hour);

            min_max_degs.Text = $"{cur_day.Day.MintempC}/{cur_day.Day.MaxtempC}°C";

            var importer = new WeatherImporter(_contextFactory);

            var cur_weather = await importer.GetCurrentWeatherAsync(location_city);

            cloud_desc.Text = cur_weather.Condition.Text;

            var get_aqi_text = await LoadAqi(cur_weather.AirQuality.UsEpaIndex);

            string aqi_text = "Качество воздуха: " + get_aqi_text;

            label3.Text = aqi_text;

            notifyIcon1.BalloonTipTitle = "Погода обновлена";
            notifyIcon1.BalloonTipText = "Теперь доступны свежие данные!";
            notifyIcon1.ShowBalloonTip(3000); // 3 секунды
        }

        private async Task<string> LoadAqi(int? usEpaIndex)
        {
            panel3.Controls.Clear();
            AirQualityGauge gauge = new AirQualityGauge(usEpaIndex);
            panel3.Controls.Add(gauge);
            int aqi = gauge.AQI;

            string aqiDescription;

            if (aqi == -1)
            {
                aqiDescription = "Неизвестно";
            }
            else if (aqi <= 50)
            {
                aqiDescription = "Хорошее";
            }
            else if (aqi <= 100)
            {
                aqiDescription = "Удовлетворительное";
            }
            else if (aqi <= 150)
            {
                aqiDescription = "Чувствительное";
            }
            else if (aqi <= 200)
            {
                aqiDescription = "Вредное";
            }
            else if (aqi <= 300)
            {
                aqiDescription = "Очень вредное";
            }
            else
            {
                aqiDescription = "Опасное";
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
            label14.Text = $"{avg_humuility:F1}%";

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

        private async Task updateTabPage()
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
                    var cur_day = await ReturnCurDayData();
                    await LoadAqi(cur_day.Day.AirQuality?.UsEpaIndex);
                    break;

                case 3:
                    await LoadWind();
                    break;

                case 4:
                    await SetUV();
                    break;
            }
        }

        // Тоже chatgpt оптимизировал
        private async void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            await updateTabPage();
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            var minMaxDays = await _weatherService.GetMaxMinDate(location_city);
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

        private async void timer1_Tick_1(object sender, EventArgs e)
        {
            label3.Text = "Качество воздуха: Загрузка...";
            await LoadDescription();
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
        private WeatherService _weatherService;

        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        public WeatherAPIManager(IDbContextFactory<AppDbContext> contextFactory, WeatherService weatherService)
        {
            _contextFactory = contextFactory;
            _weatherService = weatherService;
        }
        public async Task<WeatherRoot> RunAsync(string city, DateTime? start_date = null)
        {
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
                return weather_obj;
            }

            var importer = new WeatherImporter(_contextFactory);
            await _weatherService.AddOrGetCityAsync(city);
            var new_weather = await importer.FetchAndWriteWeatherAsync(city);
            return new_weather;
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
    }


    public class WeatherRoot
    {
        [JsonPropertyName("location")]
        public Location Location { get; set; }

        [JsonPropertyName("forecast")]
        public Forecast Forecast { get; set; }
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
        public int? UsEpaIndex { get; set; }
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

        public async Task<DateTime[]> GetMaxMinDate(string city_name)
        {
            using var context = _contextFactory.CreateDbContext();

            var cityDays = context.Days
                    .Include(d => d.City)
                    .Where(d => d.City.Name.ToLower() == city_name.ToLower());

            var minDate = await cityDays.MinAsync(d => (DateTime?)d.Day);
            var maxDate = await cityDays.MaxAsync(d => (DateTime?)d.Day);

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

            var city = await context.Cities
                .FirstOrDefaultAsync(c => c.Name.ToLower() == cityName.ToLower());

            if (city == null)
            {
                city = new City { Name = cityName };
                context.Cities.Add(city);
                await context.SaveChangesAsync();
            }

            dayWeather.CityId = city.Id;

            var existingDay = await context.Days
                .FirstOrDefaultAsync(d => d.Day.Date == dayWeather.Day.Date && d.CityId == city.Id);

            if (existingDay != null)
                return existingDay;

            context.Days.Add(dayWeather);
            await context.SaveChangesAsync();
            return dayWeather;
        }


        public async Task<HourWeather> AddHourWeatherAsync(int dayId, HourWeather hourWeather)
        {
            hourWeather.DayId = dayId;
            using var context = _contextFactory.CreateDbContext();
            var existing_hour = await context.Hours.FirstOrDefaultAsync(h => h.DayId == dayId && h.hour.Hour == hourWeather.hour.Hour);
            if (existing_hour != null)
            {
                return existing_hour;
            }
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


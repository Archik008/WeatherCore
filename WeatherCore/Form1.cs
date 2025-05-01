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

        public Form1()
        {
            InitializeComponent();
        }

        private static Color background_color = ColorTranslator.FromHtml("#4A90E2");
        private static Color section_color = ColorTranslator.FromHtml("#093860");
        private static Color panel_color = ColorTranslator.FromHtml("#00457E");
        private string selected_city;
        private static readonly Dictionary<string, string> ConditionTranslations = new()
        {
            { "Sunny", "Ясно" },
            { "Clear", "Безоблачно" },
            { "Cloudy", "Облачно" },
            { "Patchy rain nearby", "Местами дождь поблизости" },
            { "Patchy light rain in area with thunder", "Лёгкий дождь с грозой" },
            { "Thundery outbreaks in nearby", "Грозовые очаги поблизости" }
        };
        private static Dictionary<string, string> cities = new()
        {
            {"Алматы", "Almaty"},
            {"Астана", "Astana"},
            {"Шымкент", "Shymkent"}
        };

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
            var manager = new WeatherManager();
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
            Debug.WriteLine("Test commit");
            return null;
        }

        private async Task UpdateFirstInfo()
        {
            var weather = await ReturnWeatherObject();
            var today = await ReturnCurDayData();

            if (today?.Hour != null)
            {
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

            //string translated = translator.GetRussianConditionText(conditionCode, isNight);
            //Console.WriteLine($"Погода: {translated}");

            var cur_day = await ReturnCurDayData();
            var cur_day_obj = cur_day.Hour.FirstOrDefault();
            string original = cur_day_obj.Condition.Text.Trim();
            string translated = ConditionTranslations.TryGetValue(original, out var ru) ? ru : original;

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
                    DateTime selectedDate = form.SelectedDate;
                    MessageBox.Show($"Вы выбрали: {selectedDate.ToShortDateString()}");
                }
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

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
    public class WeatherManager
    {
        private const string apiKey = "aa65767110d44c4993c113415252904";
        private const string filePath = "weather.json";
        private static readonly HttpClient httpClient = new HttpClient();

        public async Task<WeatherRoot> RunAsync(string city)
        {
            var weather_data = await ReadWeatherFromFileAsync(filePath);

            //bool is_same_city = weather_data?.Location?.Name?.ToLower() == city.ToLower();

            if (!(File.Exists(filePath) ||
                File.GetLastWriteTimeUtc(filePath).Date < DateTime.UtcNow.Date))
            {
                Debug.WriteLine("Файл не найден, устарел или запрошен новый город. Загружаю новый...");
                await FetchAndSaveWeatherAsync(city);
            }
            else
            {
                Debug.WriteLine("Файл актуален. Использую локальные данные.");
            }

            return await ReadWeatherFromFileAsync(filePath);
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

                //Debug.WriteLine($"Влажность: {weather.Current.Humidity}%");
                //Debug.WriteLine($"Ветер: {weather.Current.WindKph / 3.6:F1} м/с");
                //Debug.WriteLine($"UV индекс: {weather.Current.Uv}");
                //Debug.WriteLine($"Качество воздуха (AQI): {weather.Current.AirQuality.UsEpaIndex}");

                //Debug.WriteLine("\n📆 Прогноз на 7 дней:");
                //foreach (var day in weather.Forecast.ForecastDay)
                //{
                //    Debug.WriteLine($"\n📅 {day.Date}: {day.Day.MaxtempC}°C / {day.Day.MintempC}°C, AQI: {day.Day.AirQuality?.UsEpaIndex}, UV: {day.Day.Uv}, Влажность: {day.Day.Avghumidity}%");

                //    Debug.WriteLine($"🕒 Почасовой прогноз:");
                //    foreach (var hour in day.Hour)
                //    {
                //        double windMs = hour.WindKph / 3.6;
                //        Debug.WriteLine($"{hour.Time} | 🌡️ {hour.TempC}°C | 💧 {hour.Humidity}% | 🌬️ {windMs:F1} м/с | 🔆 UV: {hour.Uv} | ☁️ {hour.Condition?.Text}");
                //    }
                //}
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

        public void GetRussianConditionText(string condition_text)
        {
            
            //foreach (var item in _conditions)
            //{
            //    var ru_lang = item.languages.FirstOrDefault(l => l.lang_iso == "ru");
            //    if (ru_lang != null)
            //    {
                    
            //    }
            //}
            //var condition = _conditions?.FirstOrDefault(c => c.code == code);
            //var ru = condition?.languages?.FirstOrDefault(l => l.lang_iso == "ru");
            // не работает переделывай
            //?
        }
    }
}


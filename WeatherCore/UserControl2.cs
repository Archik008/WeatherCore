using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WeatherCore
{
    public partial class UserControl2 : UserControl
    {
        private string hours;
        private string degrees;

        // Вот полученное условие
        private string Condition;
        public Font font { get; set; }
        public Control label_time;

        private bool is_night;
        public UserControl2(string hours, string degrees, string condition, bool is_night)
        {
            InitializeComponent();
            this.hours = hours;
            this.degrees = degrees;
            Condition = condition;
            label_time = label1;
            var hour = DateTime.Now.Hour;
            this.is_night = is_night;
        }

        private void UserControl2_Load(object sender, EventArgs e)
        {
            if (font == null)
            {
                return;
            }

            label1.Font = font;
            label1.Text = hours;
            label3.Font = font;
            label3.Text = degrees;

            string filename = "Ожидается гроза.png"; // значение по умолчанию
            // Установка названия файла в зависимости от условия
            switch (Condition)
            {
                case "Солнечно":
                case "Ясно":
                    if (is_night)
                    {
                        filename = "Полумесяц.png";
                    }
                    else
                    {
                        filename = "Солнце.png";
                    }
                    break;
                case "Переменная облачность":
                case "Облачно":
                case "Пасмурно":
                case "Дымка":
                case "Туман":
                case "Переохлажденный туман":
                    filename = "Облачно.png";
                    break;
                case "Местами дождь":
                case "Местами небольшой дождь":
                case "Небольшой дождь":
                case "Временами умеренный дождь":
                case "Умеренный дождь":
                case "Слабый переохлажденный дождь":
                case "Местами слабая морось":
                case "Слабая морось":
                case "Замерзающая морось":
                case "Местами замерзающая морось":
                    filename = "Дождь.png";
                    break;
                case "Сильный дождь":
                case "Временами сильный дождь":
                case "Сильная замерзающая морось":
                case "Умеренный или сильный переохлажденный дождь":
                case "Умеренный или сильный ливневый дождь":
                case "Сильные ливни":
                    filename = "Сильный дождь.png";
                    break;
                case "Местами дождь со снегом":
                case "Небольшой дождь со снегом":
                case "Умеренный или сильный дождь со снегом":
                case "Небольшой ливневый дождь со снегом":
                case "Умеренные или сильные ливневые дожди со снегом":
                    filename = "Дождь начинается.png";
                    break;
                case "Местами снег":
                case "Местами небольшой снег":
                case "Небольшой снег":
                case "Умеренный снег":
                case "Местами умеренный снег":
                case "Местами сильный снег":
                case "Сильный снег":
                case "Умеренный или сильный снег":
                    filename = "Снег.png";
                    break;
                case "Метель":
                case "Поземок":
                case "Снегопад":
                    filename = "Снегопад.png";
                    break;
                case "Местами грозы":
                case "В отдельных районах местами небольшой дождь с грозой":
                case "В отдельных районах умеренный или сильный дождь с грозой":
                case "В отдельных районах местами небольшой снег с грозой":
                case "В отдельных районах умеренный или сильный снег с грозой":
                    filename = "Гроза.png";
                    break;
            }
            pictureBox1.Image = Image.FromFile(ResourcePathHelper.GetPath("Resources/" + filename));
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}

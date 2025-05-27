using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WeatherCore
{
    public partial class AirQualityGauge : UserControl
    {
        private AirQualityGaugeElement gauge;

        public AirQualityGauge(int? aqi)
        {
            InitializeComponent();

            gauge = new AirQualityGaugeElement();

            if (aqi != null)
            {
                gauge.AQI = (int)aqi;
            }

            gauge.Location = new Point(10, 10);
            gauge.Size = new Size(200, 200);
            this.Controls.Add(gauge);
            Debug.WriteLine($"Aqi in contructor: {aqi}");

        }

        // Свойство для внешнего доступа к AQI
        public int AQI
        {
            get => gauge.AQI;
            set => gauge.AQI = value;
        }
    }


    // Я тут во всю chatgpt использовал и сам чуть чуть поправил чтобы он нарисовал мне спидометр
    public class AirQualityGaugeElement : Control
    {
        private int aqi = -1;

        public int AQI
        {
            get => aqi;
            set
            {
                if (aqi != value)
                {
                    aqi = value;
                    Invalidate(); // Триггерим перерисовку
                    Update();
                }
                else
                {
                    Debug.WriteLine("AQI set called, but value didn't change.");
                }
            }
        }

        public AirQualityGaugeElement()
        {
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Debug.WriteLine($"Aqi in paint: {AQI}");

            Rectangle rect = new Rectangle(10, 10, Width - 20, Height - 20);
            float startAngle = 135;
            float sweepAngle = 270;

            // Рисуем шкалу
            var aqiRanges = new[]
            {
            (Max: 50, Color: Color.FromArgb(0, 228, 0)),
            (Max: 100, Color: Color.FromArgb(255, 255, 0)),
            (Max: 150, Color: Color.FromArgb(255, 126, 0)),
            (Max: 200, Color: Color.FromArgb(255, 0, 0)),
            (Max: 300, Color: Color.FromArgb(143, 63, 151)),
            (Max: 400, Color: Color.FromArgb(126, 0, 35)),
            (Max: 500, Color: Color.FromArgb(115, 0, 0))
        };

            int previousMax = 0;
            foreach (var (max, color) in aqiRanges)
            {
                float range = max - previousMax;
                float anglePerAqi = sweepAngle / 500f;
                float currentSweep = range * anglePerAqi;
                using (Pen pen = new Pen(color, 20))
                {
                    g.DrawArc(pen, rect, startAngle, currentSweep);
                }
                startAngle += currentSweep;
                previousMax = max;
            }

            Point center = new Point(Width / 2, Height / 2);

            if (AQI >= 0)
            {
                float needleLength = Width / 2 - 30;
                float aqiAngle = 135 + (AQI * sweepAngle / 500f);
                PointF needleEnd = new PointF(
                    center.X + needleLength * (float)Math.Cos(aqiAngle * Math.PI / 180),
                    center.Y + needleLength * (float)Math.Sin(aqiAngle * Math.PI / 180)
                );

                // Стрелка
                using (Pen needlePen = new Pen(Color.Black, 4))
                {
                    g.DrawLine(needlePen, center, needleEnd);
                }
            }

            // Текст
            using (Font font = new Font("Segoe UI", 16, FontStyle.Bold))
            using (Brush textBrush = new SolidBrush(AQI >= 0 ? GetColorForAQI(AQI) : Color.Gray))
            {
                string text = AQI >= 0 ? $"AQI: {AQI}" : "";
                SizeF textSize = g.MeasureString(text, font);
                g.DrawString(text, font, textBrush, center.X - textSize.Width / 2, center.Y - textSize.Height / 2 + 60);
            }

            e.Dispose();
        }

        private Color GetColorForAQI(int aqi)
        {
            if (aqi <= 50 && aqi > 0) return Color.FromArgb(0, 228, 0);
            if (aqi <= 100) return Color.FromArgb(255, 255, 0);
            if (aqi <= 150) return Color.FromArgb(255, 126, 0);
            if (aqi <= 200) return Color.FromArgb(255, 0, 0);
            if (aqi <= 300) return Color.FromArgb(143, 63, 151);
            if (aqi <= 400) return Color.FromArgb(126, 0, 35);
            return Color.FromArgb(115, 0, 0);
        }
    }


}

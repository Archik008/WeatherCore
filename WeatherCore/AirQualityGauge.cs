using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WeatherCore
{
    public partial class AirQualityGauge : UserControl
    {
        private AirQualityGaugeElement gauge;

        public AirQualityGauge()
        {
            InitializeComponent();
            gauge = new AirQualityGaugeElement();
            gauge.Location = new Point(10, 10);
            gauge.Size = new Size(200, 200);
            this.Controls.Add(gauge);
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
        private int aqi = 0;

        public int AQI
        {
            get => aqi;
            set
            {
                aqi = Math.Clamp(value, 0, 500);
                Invalidate();
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

            Rectangle rect = new Rectangle(10, 10, Width - 20, Height - 20);
            float startAngle = 135;
            float sweepAngle = 270;

            // AQI диапазоны и цвета по стандарту EPA
            var aqiRanges = new[]
            {
            (Max: 50, Color: Color.FromArgb(0, 228, 0)),        // Good
            (Max: 100, Color: Color.FromArgb(255, 255, 0)),     // Moderate
            (Max: 150, Color: Color.FromArgb(255, 126, 0)),     // Unhealthy for Sensitive Groups
            (Max: 200, Color: Color.FromArgb(255, 0, 0)),       // Unhealthy
            (Max: 300, Color: Color.FromArgb(143, 63, 151)),    // Very Unhealthy
            (Max: 400, Color: Color.FromArgb(126, 0, 35)),      // Hazardous
            (Max: 500, Color: Color.FromArgb(115, 0, 0))        // Very Hazardous
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

            // Центр и угол для стрелки
            Point center = new Point(Width / 2, Height / 2);
            float needleLength = Width / 2 - 30;
            float aqiAngle = 135 + (AQI * sweepAngle / 500f);
            PointF needleEnd = new PointF(
                center.X + needleLength * (float)Math.Cos(aqiAngle * Math.PI / 180),
                center.Y + needleLength * (float)Math.Sin(aqiAngle * Math.PI / 180)
            );

            // Рисуем стрелку
            using (Pen needlePen = new Pen(Color.Black, 4))
            {
                g.DrawLine(needlePen, center, needleEnd);
            }

            // Текст
            using (Font font = new Font("Segoe UI", 16, FontStyle.Bold))
            using (Brush textBrush = new SolidBrush(GetColorForAQI(AQI)))
            {
                string text = $"AQI: {AQI}";
                SizeF textSize = g.MeasureString(text, font);
                g.DrawString(text, font, textBrush, center.X - textSize.Width / 2, center.Y - textSize.Height / 2 + 60);
            }
            e.Dispose();
        }

        private Color GetColorForAQI(int aqi)
        {
            if (aqi <= 50) return Color.FromArgb(0, 228, 0);         // Good
            if (aqi <= 100) return Color.FromArgb(255, 255, 0);      // Moderate
            if (aqi <= 150) return Color.FromArgb(255, 126, 0);      // Unhealthy for Sensitive Groups
            if (aqi <= 200) return Color.FromArgb(255, 0, 0);        // Unhealthy
            if (aqi <= 300) return Color.FromArgb(143, 63, 151);     // Very Unhealthy
            if (aqi <= 400) return Color.FromArgb(126, 0, 35);       // Hazardous
            return Color.FromArgb(115, 0, 0);                         // Very Hazardous
        }
    }

}

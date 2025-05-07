using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WeatherCore
{
    public partial class FormSelectDay : Form
    {
        public DateTime SelectedDate { get; private set; }
        private DateTime _start_date, _end_date;
        public FormSelectDay(DateTime start_date, DateTime end_date)
        {
            _start_date = start_date;
            _end_date = end_date;
            InitializeComponent();
        }

        private void FormSelectDay_Load(object sender, EventArgs e)
        {
            PrivateFontCollection fontCollection = new PrivateFontCollection();
            fontCollection.AddFontFile("Pixeled.ttf"); // файл шрифта
            FontFamily font_family = fontCollection.Families[0];
            Font font = new Font(font_family, 17);
            Font = font;

            DateTime today = _start_date;
            DateTime endOfWeek = _end_date;
            dateTimePicker1.MinDate = today;
            dateTimePicker1.MaxDate = endOfWeek;
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            SelectedDate = dateTimePicker1.Value;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
        public UserControl2(string hours, string degrees, string condition)
        {
            InitializeComponent();
            this.hours = hours;
            this.degrees = degrees;
            this.Condition = condition;
            label_time = label1;
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

            // Тут задаешь у picturebox картинку в зависимости от условия
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}

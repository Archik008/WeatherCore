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
        public Font font { get; set; }
        public UserControl2(string hours, string degrees)
        {
            InitializeComponent();
            this.hours = hours;
            this.degrees = degrees;
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
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}

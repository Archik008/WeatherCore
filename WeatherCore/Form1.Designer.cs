﻿namespace WeatherCore
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            panel1 = new Panel();
            panel2 = new Panel();
            label3 = new Label();
            min_max_degs = new Label();
            cloud_desc = new Label();
            button2 = new Button();
            flowLayoutPanel1 = new FlowLayoutPanel();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            pictureBox2 = new PictureBox();
            label14 = new Label();
            label13 = new Label();
            label12 = new Label();
            tabPage2 = new TabPage();
            formsPlot2 = new ScottPlot.WinForms.FormsPlot();
            label11 = new Label();
            label10 = new Label();
            label9 = new Label();
            label8 = new Label();
            tabPage3 = new TabPage();
            label19 = new Label();
            panel3 = new Panel();
            tabPage4 = new TabPage();
            pictureBox1 = new PictureBox();
            label7 = new Label();
            label6 = new Label();
            label5 = new Label();
            tabPage6 = new TabPage();
            label17 = new Label();
            label16 = new Label();
            label15 = new Label();
            selectedCity = new Label();
            label1 = new Label();
            label2 = new Label();
            comboBox1 = new ComboBox();
            contextMenuStrip1 = new ContextMenuStrip(components);
            timer1 = new System.Windows.Forms.Timer(components);
            notifyIcon1 = new NotifyIcon(components);
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            tabPage2.SuspendLayout();
            tabPage3.SuspendLayout();
            tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            tabPage6.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.Highlight;
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(panel2);
            panel1.Controls.Add(flowLayoutPanel1);
            panel1.Controls.Add(tabControl1);
            panel1.Controls.Add(selectedCity);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(comboBox1);
            panel1.Location = new Point(11, 12);
            panel1.Margin = new Padding(3, 4, 3, 4);
            panel1.Name = "panel1";
            panel1.Size = new Size(776, 426);
            panel1.TabIndex = 4;
            // 
            // panel2
            // 
            panel2.Controls.Add(label3);
            panel2.Controls.Add(min_max_degs);
            panel2.Controls.Add(cloud_desc);
            panel2.Controls.Add(button2);
            panel2.Location = new Point(246, 4);
            panel2.Margin = new Padding(3, 4, 3, 4);
            panel2.Name = "panel2";
            panel2.Size = new Size(525, 69);
            panel2.TabIndex = 6;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.ForeColor = SystemColors.ControlLightLight;
            label3.Location = new Point(211, 40);
            label3.Name = "label3";
            label3.Size = new Size(206, 20);
            label3.TabIndex = 6;
            label3.Text = "Качество воздуха: Загрузка...";
            // 
            // min_max_degs
            // 
            min_max_degs.AutoSize = true;
            min_max_degs.ForeColor = SystemColors.ControlLightLight;
            min_max_degs.Location = new Point(251, 20);
            min_max_degs.Name = "min_max_degs";
            min_max_degs.Size = new Size(78, 20);
            min_max_degs.TabIndex = 5;
            min_max_degs.Text = "Загрузка...";
            // 
            // cloud_desc
            // 
            cloud_desc.AutoSize = true;
            cloud_desc.ForeColor = SystemColors.ControlLightLight;
            cloud_desc.Location = new Point(251, 0);
            cloud_desc.Name = "cloud_desc";
            cloud_desc.Size = new Size(78, 20);
            cloud_desc.TabIndex = 4;
            cloud_desc.Text = "Загрузка...";
            // 
            // button2
            // 
            button2.Location = new Point(45, 13);
            button2.Margin = new Padding(3, 4, 3, 4);
            button2.Name = "button2";
            button2.Size = new Size(160, 38);
            button2.TabIndex = 3;
            button2.Text = "Другие дни";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.BackColor = Color.Transparent;
            flowLayoutPanel1.Location = new Point(4, 76);
            flowLayoutPanel1.Margin = new Padding(3, 4, 3, 4);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(768, 104);
            flowLayoutPanel1.TabIndex = 5;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Controls.Add(tabPage6);
            tabControl1.ItemSize = new Size(100, 25);
            tabControl1.Location = new Point(3, 184);
            tabControl1.Margin = new Padding(3, 4, 3, 4);
            tabControl1.Multiline = true;
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(773, 241);
            tabControl1.TabIndex = 4;
            tabControl1.SelectedIndexChanged += tabControl1_SelectedIndexChanged;
            // 
            // tabPage1
            // 
            tabPage1.BackColor = SystemColors.ActiveCaption;
            tabPage1.Controls.Add(pictureBox2);
            tabPage1.Controls.Add(label14);
            tabPage1.Controls.Add(label13);
            tabPage1.Controls.Add(label12);
            tabPage1.ForeColor = SystemColors.ControlLightLight;
            tabPage1.Location = new Point(4, 29);
            tabPage1.Margin = new Padding(3, 4, 3, 4);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3, 4, 3, 4);
            tabPage1.Size = new Size(765, 208);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Влажность";
            tabPage1.Paint += tabPage1_Paint;
            // 
            // pictureBox2
            // 
            pictureBox2.Image = (Image)resources.GetObject("pictureBox2.Image");
            pictureBox2.Location = new Point(370, 6);
            pictureBox2.Margin = new Padding(3, 4, 3, 4);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(111, 161);
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.TabIndex = 3;
            pictureBox2.TabStop = false;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Font = new Font("Segoe UI", 36F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label14.Location = new Point(239, 86);
            label14.Name = "label14";
            label14.Size = new Size(74, 81);
            label14.TabIndex = 2;
            label14.Text = "...";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(34, 49);
            label13.Name = "label13";
            label13.Size = new Size(78, 20);
            label13.TabIndex = 1;
            label13.Text = "Загрузка...";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(34, 18);
            label12.Name = "label12";
            label12.Size = new Size(87, 20);
            label12.TabIndex = 0;
            label12.Text = "Влажность:";
            // 
            // tabPage2
            // 
            tabPage2.BackColor = SystemColors.ActiveCaption;
            tabPage2.Controls.Add(formsPlot2);
            tabPage2.Controls.Add(label11);
            tabPage2.Controls.Add(label10);
            tabPage2.Controls.Add(label9);
            tabPage2.Controls.Add(label8);
            tabPage2.ForeColor = SystemColors.ControlLightLight;
            tabPage2.Location = new Point(4, 29);
            tabPage2.Margin = new Padding(3, 4, 3, 4);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3, 4, 3, 4);
            tabPage2.Size = new Size(765, 208);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Температура";
            // 
            // formsPlot2
            // 
            formsPlot2.DisplayScale = 1.25F;
            formsPlot2.Location = new Point(321, 6);
            formsPlot2.Margin = new Padding(3, 4, 3, 4);
            formsPlot2.Name = "formsPlot2";
            formsPlot2.Size = new Size(391, 199);
            formsPlot2.TabIndex = 5;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(21, 27);
            label11.Name = "label11";
            label11.Size = new Size(166, 20);
            label11.TabIndex = 4;
            label11.Text = "Суточная температура";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Segoe UI", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label10.Location = new Point(239, 78);
            label10.Name = "label10";
            label10.Size = new Size(29, 31);
            label10.TabIndex = 3;
            label10.Text = "...";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.ForeColor = SystemColors.ControlLightLight;
            label9.Location = new Point(86, 139);
            label9.Name = "label9";
            label9.Size = new Size(100, 20);
            label9.TabIndex = 1;
            label9.Text = "Приемлемая";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.ForeColor = SystemColors.ControlLightLight;
            label8.Location = new Point(7, 78);
            label8.Name = "label8";
            label8.Size = new Size(164, 20);
            label8.TabIndex = 0;
            label8.Text = "Средняя температура:";
            // 
            // tabPage3
            // 
            tabPage3.BackColor = SystemColors.ActiveCaption;
            tabPage3.Controls.Add(label19);
            tabPage3.Controls.Add(panel3);
            tabPage3.ForeColor = SystemColors.ControlLightLight;
            tabPage3.Location = new Point(4, 29);
            tabPage3.Margin = new Padding(3, 4, 3, 4);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(3, 4, 3, 4);
            tabPage3.Size = new Size(765, 208);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Качество воздуха";
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Location = new Point(182, 81);
            label19.Name = "label19";
            label19.Size = new Size(78, 20);
            label19.TabIndex = 2;
            label19.Text = "Загрузка...";
            // 
            // panel3
            // 
            panel3.Location = new Point(437, 29);
            panel3.Margin = new Padding(3, 4, 3, 4);
            panel3.Name = "panel3";
            panel3.Size = new Size(296, 151);
            panel3.TabIndex = 1;
            // 
            // tabPage4
            // 
            tabPage4.BackColor = SystemColors.ActiveCaption;
            tabPage4.Controls.Add(pictureBox1);
            tabPage4.Controls.Add(label7);
            tabPage4.Controls.Add(label6);
            tabPage4.Controls.Add(label5);
            tabPage4.Location = new Point(4, 29);
            tabPage4.Margin = new Padding(3, 4, 3, 4);
            tabPage4.Name = "tabPage4";
            tabPage4.Padding = new Padding(3, 4, 3, 4);
            tabPage4.Size = new Size(765, 208);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "Ветер";
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(395, 64);
            pictureBox1.Margin = new Padding(3, 4, 3, 4);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(139, 104);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 3;
            pictureBox1.TabStop = false;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.BackColor = SystemColors.ActiveCaption;
            label7.Font = new Font("Segoe UI", 16.2F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label7.ForeColor = SystemColors.ControlLightLight;
            label7.Location = new Point(222, 95);
            label7.Name = "label7";
            label7.Size = new Size(35, 38);
            label7.TabIndex = 2;
            label7.Text = "...";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.ForeColor = SystemColors.ControlLightLight;
            label6.Location = new Point(33, 53);
            label6.Name = "label6";
            label6.Size = new Size(63, 20);
            label6.TabIndex = 1;
            label6.Text = "Слабый";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.ForeColor = SystemColors.ControlLightLight;
            label5.Location = new Point(33, 21);
            label5.Name = "label5";
            label5.Size = new Size(52, 20);
            label5.TabIndex = 0;
            label5.Text = "Ветер:";
            // 
            // tabPage6
            // 
            tabPage6.BackColor = SystemColors.ActiveCaption;
            tabPage6.Controls.Add(label17);
            tabPage6.Controls.Add(label16);
            tabPage6.Controls.Add(label15);
            tabPage6.ForeColor = SystemColors.ControlLightLight;
            tabPage6.Location = new Point(4, 29);
            tabPage6.Margin = new Padding(3, 4, 3, 4);
            tabPage6.Name = "tabPage6";
            tabPage6.Padding = new Padding(3, 4, 3, 4);
            tabPage6.Size = new Size(765, 208);
            tabPage6.TabIndex = 5;
            tabPage6.Text = "УФ";
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Font = new Font("Segoe UI", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label17.Location = new Point(328, 93);
            label17.Name = "label17";
            label17.Size = new Size(29, 31);
            label17.TabIndex = 11;
            label17.Text = "...";
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Location = new Point(408, 60);
            label16.Name = "label16";
            label16.Size = new Size(78, 20);
            label16.TabIndex = 10;
            label16.Text = "Загрузка...";
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new Point(251, 60);
            label15.Name = "label15";
            label15.Size = new Size(110, 20);
            label15.TabIndex = 9;
            label15.Text = "Ультрафиолет:";
            // 
            // selectedCity
            // 
            selectedCity.AutoSize = true;
            selectedCity.ForeColor = SystemColors.HighlightText;
            selectedCity.Location = new Point(64, 16);
            selectedCity.Name = "selectedCity";
            selectedCity.Size = new Size(129, 20);
            selectedCity.TabIndex = 3;
            selectedCity.Text = "город не выбран";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.ForeColor = SystemColors.HighlightText;
            label1.Location = new Point(14, 16);
            label1.Name = "label1";
            label1.Size = new Size(54, 20);
            label1.TabIndex = 0;
            label1.Text = "Город:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.DeepSkyBlue;
            label2.ForeColor = SystemColors.HighlightText;
            label2.Location = new Point(14, 44);
            label2.Name = "label2";
            label2.Size = new Size(114, 20);
            label2.TabIndex = 2;
            label2.Text = "Выбрать город";
            // 
            // comboBox1
            // 
            comboBox1.BackColor = SystemColors.Menu;
            comboBox1.ForeColor = SystemColors.InfoText;
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "Алматы", "Астана" });
            comboBox1.Location = new Point(134, 40);
            comboBox1.Margin = new Padding(3, 4, 3, 4);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(106, 28);
            comboBox1.TabIndex = 1;
            comboBox1.Tag = "";
            comboBox1.DrawItem += comboBox1_DrawItem;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new Size(20, 20);
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(61, 4);
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 120000;
            timer1.Tick += timer1_Tick_1;
            // 
            // notifyIcon1
            // 
            notifyIcon1.Text = "notifyIcon1";
            notifyIcon1.Visible = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.LightSkyBlue;
            ClientSize = new Size(800, 449);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 4, 3, 4);
            Name = "Form1";
            Text = "WeatherCore";
            Load += Form1_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            tabPage3.ResumeLayout(false);
            tabPage3.PerformLayout();
            tabPage4.ResumeLayout(false);
            tabPage4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            tabPage6.ResumeLayout(false);
            tabPage6.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Label selectedCity;
        private Label label1;
        private Label label2;
        private ComboBox comboBox1;
        private FlowLayoutPanel flowLayoutPanel1;
        private Panel panel2;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private PictureBox pictureBox2;
        private Label label14;
        private Label label13;
        private Label label12;
        private TabPage tabPage2;
        private ScottPlot.WinForms.FormsPlot formsPlot2;
        private Label label11;
        private Label label10;
        private Label label9;
        private Label label8;
        private TabPage tabPage3;
        private TabPage tabPage4;
        private PictureBox pictureBox1;
        private Label label7;
        private Label label6;
        private Label label5;
        private TabPage tabPage6;
        private Label label17;
        private Label label16;
        private Label label15;
        private Panel panel3;
        private Label label19;
        private Button button2;
        private Label cloud_desc;
        private Label label3;
        private Label min_max_degs;
        private ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.Timer timer1;
        private NotifyIcon notifyIcon1;
    }
}

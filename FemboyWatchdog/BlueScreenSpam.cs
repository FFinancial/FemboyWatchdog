using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FemboyWatchdog
{
    public partial class BlueScreenSpam : Form
    {
        private Timer _t;

        public BlueScreenSpam()
        {
            InitializeComponent();

            Cursor.Hide();
            pictureBox1.Size = new Size(Width, Height);

            Random r = new Random();
            if (r.Next(20) == 0)
            {
                pictureBox1.Image = global::FemboyWatchdog.Properties.Resources.monikabsod;
            }

            _t = new Timer();
            _t.Interval = 5000;
            _t.Tick += new EventHandler(_t_Tick);
        }

        private void _t_Tick(object sender, EventArgs e)
        {
            Dispose();
        }

        private void BlueScreenSpam_Shown(object sender, EventArgs e)
        {
            _t.Start();
        }
    }
}

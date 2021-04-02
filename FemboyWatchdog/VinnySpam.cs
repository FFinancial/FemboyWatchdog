using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace FemboyWatchdog
{
    public partial class VinnySpam : Form
    {
        private System.Windows.Forms.Timer _t;

        public VinnySpam()
        {
            InitializeComponent();

            _t = new System.Windows.Forms.Timer();
            _t.Interval = 500;
            _t.Tick += new EventHandler(_t_Tick);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void VinnySpam_Shown(object sender, EventArgs e)
        {
            _t.Start();
        }

        private void _t_Tick(object sender, EventArgs e)
        {
            // vinny bye bye
            Dispose();
        }
    }
}

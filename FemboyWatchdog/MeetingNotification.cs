using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Media;
using System.Text;
using System.Windows.Forms;

namespace FemboyWatchdog
{
    public partial class MeetingNotification : Form
    {
        private Timer _flasher;
        private int _timesFlashed;

        public MeetingNotification()
        {
            InitializeComponent();

            _flasher = new Timer();
            _flasher.Interval = 500;
            _flasher.Tick += new EventHandler(_flasher_Tick);

            _timesFlashed = 0;
        }

        private void _flasher_Tick(object sender, EventArgs e)
        {
            if (++_timesFlashed > 10)
            {
                Close();
                return;
            }

            if (Visible)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        private void MeetingNotification_Load(object sender, EventArgs e)
        {
            _flasher.Start();
            SoundPlayer snd = new SoundPlayer(@"media\klaxon.wav");
            snd.Play();
        }
    }
}

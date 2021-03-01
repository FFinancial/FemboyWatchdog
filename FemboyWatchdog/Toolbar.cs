using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FemboyWatchdog
{
    public partial class Toolbar : Form
    {
        private MeetingProvider meetings;

        public Toolbar()
        {
            InitializeComponent();
            Location = new Point((Screen.PrimaryScreen.Bounds.Width - Width) / 2, 0);

            meetings = new MeetingProvider();
            meetings.MeetingStarted += new EventHandler<MeetingProvider.MeetingInfo>(meetings_MeetingStarted);
            meetings.StartChecking();
        }

        private void meetings_MeetingStarted(object sender, EventArgs e)
        {
            MeetingNotification alert = new MeetingNotification();
            alert.Show();
        }

        private void callAMeetingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            meetings.StartMeeting();
        }

        private void menuStrip1_MouseEnter(object sender, EventArgs e)
        {
            Focus();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox dialog = new AboutBox();
            dialog.ShowDialog();
        }
    }
}

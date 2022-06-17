using Microsoft.VisualBasic;
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
        private MemoProvider memos;
        private Timer _labelTimer;
        private int _memoIndex;

        public Toolbar()
        {
            InitializeComponent();
            Location = new Point((Screen.PrimaryScreen.Bounds.Width - Width) / 2, 0);

            meetings = new MeetingProvider();
            meetings.MeetingStarted += new EventHandler<MeetingProvider.MeetingInfo>(meetings_MeetingStarted);
            meetings.StartChecking();

            memos = new MemoProvider();
            memos.NewMemo += new EventHandler<MemoProvider.Memo>(memos_NewMemo);
            memos.StartChecking();

            _memoIndex = 0;

            _labelTimer = new Timer();
            _labelTimer.Interval = 10000;
            _labelTimer.Tick += new EventHandler(_labelTimer_Tick);
            _labelTimer.Start();

            memoLabel.Hide();
        }

        private void _labelTimer_Tick(object sender, EventArgs e)
        {
            if (memos.Memos.Count == 0)
            {
                memoLabel.Hide();
            }
            else
            {
                memoLabel.Text = memos.Memos[_memoIndex++ % memos.Memos.Count].Message;
                memoLabel.Show();
            }
        }

        private void memos_NewMemo(object sender, MemoProvider.Memo m)
        {
            MessageBox.Show(m.Message, "NEW COMPANY MEMO", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void postMemoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string message = Interaction.InputBox("Enter a messsage:", "Post a Memo");
            if (message.Length > 0)
                memos.PostNewMemo(message);
        }

        private void viewPastMemosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MemoViewer dialog = new MemoViewer(memos.Memos);
            dialog.Show();
        }
    }
}

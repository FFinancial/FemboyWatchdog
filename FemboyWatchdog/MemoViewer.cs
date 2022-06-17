using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FemboyWatchdog
{
    public partial class MemoViewer : Form
    {
        private List<MemoProvider.Memo> _memos;

        public MemoViewer(List<MemoProvider.Memo> memos)
        {
            InitializeComponent();

            _memos = memos;
            memosList.View = View.Details;
            memosList.Columns.Add("Date", 150);
            memosList.Columns.Add("Message", 300);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MemoViewer_Load(object sender, EventArgs e)
        {
            foreach (MemoProvider.Memo memo in _memos)
            {
                string[] columns = { memo.Time.ToString(), memo.Message };
                ListViewItem li = new ListViewItem(columns);
                memosList.Items.Add(li);
            }
        }
    }
}

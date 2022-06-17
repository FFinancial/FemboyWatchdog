using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace FemboyWatchdog
{
    public class MemoProvider
    {
        private WebClient _memoChecker;
        private WebClient _memoCreator;
        private Timer _timer;
        public List<Memo> Memos { get; private set; }
        private DateTime _lastReceivedMemoTime;
        private double _checkInterval;
        public bool Checking { get; private set; }

        public event EventHandler<Memo> NewMemo;

        public class Memo : EventArgs
        {
            public DateTime Time;
            public string Message;
        }

        internal class MemoError : EventArgs
        {
            public string Error;
        }

        public MemoProvider(double interval = 5.0)
        {
            _memoChecker = new WebClient();
            _memoChecker.DownloadStringCompleted += new DownloadStringCompletedEventHandler(_memoChecker_DownloadStringCompleted);

            _memoCreator = new WebClient();
            _memoCreator.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            _memoCreator.UploadStringCompleted += new UploadStringCompletedEventHandler(_memoCreator_UploadStringCompleted);

            Memos = new List<Memo>();

            _lastReceivedMemoTime = DateTime.UtcNow;
            _checkInterval = interval;
            Checking = false;
        }

        private void _memoCreator_UploadStringCompleted(object sender, UploadStringCompletedEventArgs e)
        {
            // apparently headers get unset if you reuse the WebClient
            _memoCreator.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

            if (e.Cancelled || e.Error != null)
                return;

            try
            {
                Memos = JsonConvert.DeserializeObject<List<Memo>>(e.Result);
            }
            catch (JsonSerializationException err)
            {
                try
                {
                    MemoError response = JsonConvert.DeserializeObject<MemoError>(e.Result);
                    MessageBox.Show(response.Error, "Error Sending Memo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (JsonSerializationException err2)
                {
                    MessageBox.Show("Unknown deserialization error", "Error Sending Memo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public void PostNewMemo(string message)
        {
            _memoCreator.UploadStringAsync(
                new Uri(Properties.Settings.Default.FemboyApiBaseUrl + "memo"),
                string.Format("token={0}&message={1}", Properties.Settings.Default.FemboyApiToken, Uri.EscapeDataString(message))
            );

        }

        public void StartChecking()
        {
            if (Checking)
                return;

            _timer_Tick(this, null);

            _timer = new Timer();
            _timer.Interval = Convert.ToInt32(_checkInterval * 1000);
            _timer.Tick += new EventHandler(_timer_Tick);
            _timer.Start();

            Checking = true;
        }

        private bool FindNewMemos(Memo m)
        {
            return m.Time > _lastReceivedMemoTime;
        }

        private void _memoChecker_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Cancelled || e.Error != null)
                return;

            try
            {
                Memos = JsonConvert.DeserializeObject<List<Memo>>(e.Result);

                foreach (Memo m in Memos.FindAll(FindNewMemos))
                {
                    _lastReceivedMemoTime = m.Time;
                    OnNewMemo(m);
                }
            }
            catch (JsonSerializationException err) { }
        }

        protected virtual void OnNewMemo(Memo m)
        {
            EventHandler<Memo> handler = NewMemo;
            if (handler != null)
            {
                handler(this, m);
            }
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            if (!_memoChecker.IsBusy)
            {
                try
                {
                    _memoChecker.DownloadStringAsync(new Uri(
                        string.Format(
                            "{0}memo/?token={1}",
                            Properties.Settings.Default.FemboyApiBaseUrl,
                            Properties.Settings.Default.FemboyApiToken
                        )
                    ));
                }
                catch (WebException err) { }
            }
        }
    }
}

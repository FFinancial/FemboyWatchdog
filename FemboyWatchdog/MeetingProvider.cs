using System;
using System.Net;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace FemboyWatchdog
{
    class MeetingProvider
    {
        private WebClient _meetingChecker;
        private WebClient _meetingCreator;
        private Timer _timer;
        private MeetingInfo _meetInfo;
        private DateTime _lastMeetingResponseTime = DateTime.MinValue;
        private double _checkInterval;
        private const double _DATETIME_EPSILON = 30.0;
        public bool Checking { get; private set; }

        public event EventHandler<MeetingInfo> MeetingStarted;

        internal class MeetingInfo : EventArgs
        {
            public DateTime LastMeetingTime;
            public string Error;
        }

        public MeetingProvider(double interval = 5.0)
        {
            _meetingChecker = new WebClient();
            _meetingChecker.DownloadStringCompleted += new DownloadStringCompletedEventHandler(_meetingChecker_DownloadStringCompleted);

            _meetingCreator = new WebClient();
            _meetingCreator.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            _meetingCreator.UploadStringCompleted += new UploadStringCompletedEventHandler(_meetingCreator_UploadStringCompleted);

            _checkInterval = interval;
            Checking = false;
        }

        private void _meetingCreator_UploadStringCompleted(object sender, UploadStringCompletedEventArgs e)
        {
            // apparently headers get unset if you reuse the WebClient
            _meetingCreator.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

            if (e.Cancelled || e.Error != null)
                return;

            try
            {
                MeetingInfo response = JsonConvert.DeserializeObject<MeetingInfo>(e.Result);

                if (response.Error != null)
                    return;

                _meetInfo = response;
            }
            catch (JsonSerializationException err) { }
        }

        public void StartMeeting()
        {
            _meetingCreator.UploadStringAsync(
                new Uri(Properties.Settings.Default.FemboyApiBaseUrl + "meet"),
                "token=" + Properties.Settings.Default.FemboyApiToken
            );
        }

        public void StartChecking()
        {
            if (Checking)
                return;

            _timer = new Timer();
            _timer.Interval = Convert.ToInt32(_checkInterval * 1000);
            _timer.Tick += new EventHandler(_timer_Tick);
            _timer.Start();

            Checking = true;
        }

        private void _meetingChecker_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Cancelled || e.Error != null)
                return;

            try
            {
                MeetingInfo response = JsonConvert.DeserializeObject<MeetingInfo>(e.Result);

                if (response.Error != null)
                    return;

                _meetInfo = response;

                // if a meeting is happened long ago, we don't care about it.
                // also, avoid reacting twice to the same meeting.
                if (Math.Abs((DateTime.UtcNow - _meetInfo.LastMeetingTime).TotalSeconds) <= _DATETIME_EPSILON
                    && (DateTime.UtcNow - _lastMeetingResponseTime).TotalSeconds >= _checkInterval)
                {
                    _lastMeetingResponseTime = _meetInfo.LastMeetingTime;
                    OnMeetingStarted(_meetInfo);
                }
            }
            catch (JsonSerializationException err) { }
        }

        protected virtual void OnMeetingStarted(MeetingInfo e)
        {
            EventHandler<MeetingInfo> handler = MeetingStarted;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            if (!_meetingChecker.IsBusy)
            {
                try
                {
                    _meetingChecker.DownloadStringAsync(new Uri(
                        string.Format(
                            "{0}meet/?token={1}",
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

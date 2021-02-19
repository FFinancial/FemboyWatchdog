using System;
using System.Net;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace FemboyWatchdog
{
    class LocationTracker
    {
        private WebClient _wcIp;
        private WebClient _wcLocation;
        private Timer _timer;

        public IPLocationData LocationData { get; private set; }

        internal class IPLocationData
        {
            public double latitude;
            public double longitude;
            public string region_name;
            public string city;
            public string country_name;
        }


        public LocationTracker(double interval = 15.0)
        {
            _wcIp = new WebClient();
            _wcLocation = new WebClient();
            _wcIp.DownloadStringCompleted += new DownloadStringCompletedEventHandler(ip_DownloadStringCompleted);
            _wcLocation.DownloadStringCompleted += new DownloadStringCompletedEventHandler(loc_DownloadStringCompleted);

            // get location for the first time
            GetLocationFromIp();

            _timer = new Timer();
            _timer.Interval = Convert.ToInt32(interval * 1000);
            _timer.Tick += new EventHandler(_timer_Tick);
            _timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            // i would use GeoCoordinateWatcher, but it is broken
            GetLocationFromIp();
        }

        private void GetLocationFromIp()
        {
            try
            {
                if (!_wcIp.IsBusy)
                    _wcIp.DownloadStringAsync(new Uri("http://ipv4.icanhazip.com"));
            }
            catch (WebException err)
            {
                // fail silently
            }
        }

        private void loc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Cancelled || e.Error != null)
                return;

            try
            {
                LocationData = JsonConvert.DeserializeObject<IPLocationData>(e.Result);
            }
            catch (JsonSerializationException err) { }
        }

        void ip_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Cancelled || e.Error != null)
                return;

            try
            {
                if (!_wcLocation.IsBusy)
                    _wcLocation.DownloadStringAsync(new Uri(
                        string.Format(
                            "http://api.ipstack.com/{0}?access_key={1}&fields=latitude,longitude,region_name,country_name,city",
                            e.Result,
                            "31d59064bb7ef00a9daff4794b73118c"
                        )
                    ));
            }
            catch (WebException err) { }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace FemboyWatchdog
{
    class SpamProvider
    {
        private Timer _nextSpam;
        private Random _random;
        private List<MethodInfo> _spamCallbacks;
        
        // between 2-10 minutes
        private const int MIN_DELAY = 120000;
        private const int MAX_DELAY = 600000;

        public SpamProvider()
        {
            _random = new Random();
            _nextSpam = new Timer();
            _nextSpam.Interval = _random.Next(MIN_DELAY, MAX_DELAY);
            _nextSpam.Tick += new EventHandler(_nextSpam_Tick);
            _nextSpam.Start();

            _spamCallbacks = new List<MethodInfo>();
            foreach (MethodInfo method in typeof(SpamProvider).GetMethods())
            {
                if (method.Name.StartsWith("SpamCallback_"))
                    _spamCallbacks.Add(method);
            }
        }

        private void _nextSpam_Tick(object sender, EventArgs e)
        {
            _nextSpam.Interval = _random.Next(MIN_DELAY, MAX_DELAY);
            MethodInfo chosenAction = _spamCallbacks[_random.Next(_spamCallbacks.Count)];
            chosenAction.Invoke(this, null);
        }

        public void SpamCallback_Vinny()
        {
            int count = _random.Next(20);
            List<Form> vinnies = new List<Form>();
            for (int i = 0; i < count; ++i)
            {
                Form vinny = new VinnySpam();
                vinnies.Add(vinny);
                vinny.Show();
            }
        }

        public void SpamCallback_BSOD()
        {
            // yes, this is supposed to block
            System.Threading.Thread.Sleep(3000);
            Form bsod = new BlueScreenSpam();
            bsod.Show();
        }
    }
}

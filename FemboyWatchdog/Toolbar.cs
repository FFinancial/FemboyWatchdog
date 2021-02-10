using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Media;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Text;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing.Imaging;
using System.IO;

namespace FemboyWatchdog
{
    public partial class Toolbar : Form
    {
        private DateTime clockIn = DateTime.Now;

        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoDevice;
        private VideoCapabilities[] videoCapabilities;

        private Timer labelBlinkTimer;
        private Timer ttsTimer;
        private Timer mousemvmtTimer;
        private int mousemvmtCount = 0;
        /* mouse movements come in as a stream, so we'll say a mouse movement is "over" by waiting
           a certain time with no movement */
        private Timer lastmousemvmtTimer;

        private SpeechSynthesizer ss;

        private SolidBrush brush = new SolidBrush(Color.Red);
        private Font font = new Font(FontFamily.GenericSansSerif, 24);

        private GlobalHooks.LowLevelMouseProc _proc;
        private static IntPtr _hookID = IntPtr.Zero;


        public Toolbar()
        {
            _proc = OnMouseMessage;
            GlobalHooks.SetHook(_proc);
            InitializeComponent();

            ss = new SpeechSynthesizer();

            ttsTimer = new Timer();
            ttsTimer.Interval = 15000;
            labelBlinkTimer = new Timer();
            labelBlinkTimer.Interval = 1000;
            mousemvmtTimer = new Timer();
            mousemvmtTimer.Interval = 60000;
            mousemvmtTimer.Start();
            lastmousemvmtTimer = new Timer();
            lastmousemvmtTimer.Interval = 500;

            // hook events
            vsp.NewFrame += new AForge.Controls.VideoSourcePlayer.NewFrameHandler(vsp_NewFrame);
            ttsTimer.Tick += new EventHandler(ttsTimer_Tick);
            labelBlinkTimer.Tick += new EventHandler(labelBlinkTimer_Tick);
            mousemvmtTimer.Tick += new EventHandler(mousemoveTimer_Tick);
            lastmousemvmtTimer.Tick += new EventHandler(lastmousemvmtTimer_Tick);

            OpenCamera();
        }

        void lastmousemvmtTimer_Tick(object sender, EventArgs e)
        {
            ++mousemvmtCount;
            lastmousemvmtTimer.Stop();
        }

        void mousemoveTimer_Tick(object sender, EventArgs e)
        {
            if (mousemvmtCount < 5)
            {
                SoundPlayer snd = new SoundPlayer(@"media\beep.wav");
                snd.Play();
                DialogResult answer = MessageBox.Show(
                    this,
                    string.Format("WORKER: Your productivity ({0}) is below 5 mouse movements/minute. Are you sleeping?", mousemvmtCount),
                    "EMPLOYEE ALERT",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Exclamation
                );
                if (answer == DialogResult.Yes)
                {
                    MessageBox.Show(
                        this,
                        "Expect a formal termination letter from human resources soon.",
                        "YOU'RE FIRED!",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
            }
            mousemvmtCount = 0;
        }

        private IntPtr OnMouseMessage(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (GlobalHooks.MouseMessages)wParam == GlobalHooks.MouseMessages.WM_MOUSEMOVE)
            {
                if (!lastmousemvmtTimer.Enabled)
                    lastmousemvmtTimer.Start();
            }
            return GlobalHooks.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private void OpenCamera()
        {
            try
            {
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (videoDevices.Count != 0)
                {
                    videoDevice = new VideoCaptureDevice(videoDevices[0].MonikerString);
                }
                else
                {
                    MessageBox.Show(this, "Camera device not found! Get a webcam!", "Error initializing camera", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(-2);
                    return;
                }
                videoCapabilities = videoDevice.VideoCapabilities;
                if (videoCapabilities.Length == 0)
                {
                    MessageBox.Show(this, "Camera does not support video capture! Get a better webcam!", "Error initializing camera", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(-3);
                    return;
                }

                // adjust VSP's resolution so as to not stretch the picture
                double aspectRatio = 1.0 * videoCapabilities[0].FrameSize.Width / videoCapabilities[0].FrameSize.Height;
                vsp.Size = new System.Drawing.Size(Convert.ToInt32(aspectRatio * 144), 144);

                OpenVideoSource(videoDevice);
            }
            catch (Exception err)
            {
                MessageBox.Show(this, err.ToString(), "Error initializing camera", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
        }

        void labelBlinkTimer_Tick(object sender, EventArgs e)
        {
            this.label2.Visible = !this.label2.Visible;
        }

        void ttsTimer_Tick(object sender, EventArgs e)
        {
            ss.SpeakAsync("You are being monitored");
        }

        public void OpenVideoSource(IVideoSource source)
        {
            try
            {
                // set busy cursor
                this.Cursor = Cursors.WaitCursor;
                // stop current video source
                CloseCurrentVideoSource();
                // start new video source
                vsp.VideoSource = source;
                vsp.Start();
                this.Cursor = Cursors.Default;
                ttsTimer.Start();
                labelBlinkTimer.Start();
            }
            catch { }
        }

        public void CloseCurrentVideoSource()
        {
            try
            {
                if (vsp.VideoSource != null)
                {
                    vsp.SignalToStop();
                    // wait ~ 3 seconds
                    for (int i = 0; i < 30; i++)
                    {
                        if (!vsp.IsRunning)
                            break;
                        System.Threading.Thread.Sleep(100);
                    }
                    if (vsp.IsRunning)
                    {
                        vsp.Stop();
                    }
                    vsp.VideoSource = null;
                    labelBlinkTimer.Stop();
                }
            }
            catch { }
        }

        private void vsp_NewFrame(object sender, ref Bitmap image)
        {
            try
            {
                DateTime now = DateTime.Now;
                Graphics g = Graphics.FromImage(image);
                // paint current time
                g.DrawString(now.ToString(), font, brush, new Point(5, 5));
                g.DrawString("Clock in time: " + clockIn.ToString(), font, brush, new Point(5, image.Height - font.Height - 5));
            }
            catch
            { }
        }
    }
}

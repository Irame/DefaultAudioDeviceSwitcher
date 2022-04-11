using System.Diagnostics;

namespace DefaultAudioDeviceSwitcher
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new MyApplicationContext());
        }
    }

    enum DeviceKind { Headset, Speaker }

    class MyApplicationContext : ApplicationContext
    {
        private NotifyIcon _trayIcon;

        private Settings _settings;

        private DeviceKind _activeDevice;
        public DeviceKind ActiveDevice { 
            get => _activeDevice; 
            set {
                if (_activeDevice == value)
                    return;
                
                _activeDevice = value;

                if (_activeDevice == DeviceKind.Headset)
                {
                    Process.Start(_settings.NirCmdPath, $"setdefaultsounddevice \"{_settings.HeadsetName}\"");
                    if (_settings.ChangeCommunicationDevice)
                        Process.Start(_settings.NirCmdPath, $"setdefaultsounddevice \"{_settings.HeadsetName}\" 2");
                    _trayIcon.Icon = new Icon("Icons/Headset.ico");
                }
                else
                {
                    Process.Start(_settings.NirCmdPath, $"setdefaultsounddevice \"{_settings.SpeakerName}\"");
                    if (_settings.ChangeCommunicationDevice)
                        Process.Start(_settings.NirCmdPath, $"setdefaultsounddevice \"{_settings.SpeakerName}\" 2");
                    _trayIcon.Icon = new Icon("Icons/Speaker.ico");
                }
            }
        }

        public MyApplicationContext()
        {
            ActiveDevice = DeviceKind.Headset;

            _settings = new Settings("DefaultAudioDeviceSwitcher.json");

            var contextMenuStrip = new ContextMenuStrip();

            contextMenuStrip.Items.Add("Config", null, OpenConfig);
            contextMenuStrip.Items.Add("Exit", null, Exit);

            _trayIcon = new NotifyIcon();
            _trayIcon.Icon = new Icon("Icons/Headset.ico");
            _trayIcon.Visible = true;
            _trayIcon.ContextMenuStrip = contextMenuStrip;
            _trayIcon.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (ActiveDevice == DeviceKind.Headset)
                        ActiveDevice = DeviceKind.Speaker;
                    else
                        ActiveDevice = DeviceKind.Headset;
                }
            };
        }

        void Exit(object? sender, EventArgs e)
        {
            _trayIcon.Visible = false;

            Application.Exit();
        }

        void OpenConfig(object? sender, EventArgs e)
        {
            var configForm = new ConfigForm(_settings);

            configForm.ShowDialog();
        }
    }
}
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

                while (!File.Exists(_settings.NirCmdPath))
                    if (!ConfigError("NirCmd not found!")) return;

                if (_activeDevice == DeviceKind.Headset)
                {
                    while (string.IsNullOrWhiteSpace(_settings.HeadsetName))
                        if (!ConfigError("No headset configured!")) return;

                    Process.Start(_settings.NirCmdPath, $"setdefaultsounddevice \"{_settings.HeadsetName}\"");
                    if (_settings.ChangeCommunicationDevice)
                        Process.Start(_settings.NirCmdPath, $"setdefaultsounddevice \"{_settings.HeadsetName}\" 2");
                    _trayIcon.Icon = Properties.Resources.Headset;
                }
                else
                {
                    while (string.IsNullOrWhiteSpace(_settings.HeadsetName))
                        if (!ConfigError("No speaker configured!")) return;

                    Process.Start(_settings.NirCmdPath, $"setdefaultsounddevice \"{_settings.SpeakerName}\"");
                    if (_settings.ChangeCommunicationDevice)
                        Process.Start(_settings.NirCmdPath, $"setdefaultsounddevice \"{_settings.SpeakerName}\" 2");
                    _trayIcon.Icon = Properties.Resources.Speaker;
                }

                _activeDevice = value;
            }
        }

        public MyApplicationContext()
        {
            ActiveDevice = DeviceKind.Headset;

            var appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            var settingsPath = Path.Combine(appdataPath, "DefaultAudioDeviceSwitcher", "config.json");

            _settings = new Settings(settingsPath);

            var contextMenuStrip = new ContextMenuStrip();

            contextMenuStrip.Items.Add("Config", null, OpenConfig);
            contextMenuStrip.Items.Add("Exit", null, Exit);

            _trayIcon = new NotifyIcon();
            _trayIcon.Icon = Properties.Resources.Headset;
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

        bool ConfigError(string message)
        {
            var msgResult = MessageBox.Show(message + "\n\nWould you like to configure it?", "DefaultAudioDeviceSwitcher", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

            if (msgResult == DialogResult.Yes)
            {
                new ConfigForm(_settings).ShowDialog();
                return true;
            }
            
            return false;
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
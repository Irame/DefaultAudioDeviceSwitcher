using CoreAudioApi;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
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
        private NotificationClient _notificationClient;

        private NotifyIcon _trayIcon;

        private Settings _settings;

        private ConfigForm? _configForm;

        private DeviceKind? _activeDevice;
        public DeviceKind? ActiveDevice { 
            get => _activeDevice; 
            set {
                _activeDevice = value;

                switch (_activeDevice)
                {
                    case DeviceKind.Headset:
                        _trayIcon.Icon = Properties.Resources.Headset;
                        break;
                    case DeviceKind.Speaker:
                        _trayIcon.Icon = Properties.Resources.Speaker;
                        break;
                    default:
                        _trayIcon.Icon = Properties.Resources.Questionmark;
                        break;
                }
            }
        }

        public HashSet<string> AvailableDevices => new MMDeviceEnumerator()
            .EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
            .Select(x => x.ID)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet();

        public MyApplicationContext()
        {
            var appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            var settingsPath = Path.Combine(appdataPath, "DefaultAudioDeviceSwitcher", "config.json");

            _settings = new Settings(settingsPath);
            _settings.SettingsSaved += ConfigChanged;

            var contextMenuStrip = new ContextMenuStrip();

            contextMenuStrip.Items.Add("Config", null, OpenConfig);
            contextMenuStrip.Items.Add("Exit", null, Exit);

            _trayIcon = new NotifyIcon();
            _trayIcon.Visible = true;
            _trayIcon.ContextMenuStrip = contextMenuStrip;
            _trayIcon.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    var availableDevices = AvailableDevices;

                    if (_activeDevice == DeviceKind.Speaker)
                    {
                        while (!availableDevices.Contains(_settings.HeadsetId))
                            if (!ConfigError("No headset configured!")) return;

                        SetDefaultDevice(_settings.HeadsetId);
                    }
                    else
                    {
                        while (!availableDevices.Contains(_settings.SpeakerId))
                            if (!ConfigError("No speaker configured!")) return;

                        SetDefaultDevice(_settings.SpeakerId);
                    }
                }
            };

            _notificationClient = new NotificationClient();
            _notificationClient.DefaultDeviceChanged += DefaultDeviceChanged;

            SetActiveDevice(GetDefaultDeviceId());
        }

        void ConfigChanged(object? sender, EventArgs e)
        {
            if (_activeDevice == DeviceKind.Headset)
            {
                if (AvailableDevices.Contains(_settings.HeadsetId))
                    SetDefaultDevice(_settings.HeadsetId);
            }
            else if (_activeDevice == DeviceKind.Speaker)
            {
                if (AvailableDevices.Contains(_settings.SpeakerId))
                    SetDefaultDevice(_settings.SpeakerId);
            }
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
            if (_configForm == null)
            {
                _configForm = new ConfigForm(_settings);
                _configForm.ShowDialog();
                _configForm = null;
            }
        }

        void DefaultDeviceChanged(object? sender, DefaultDeviceChangedArgs e)
        {
            if (e.Flow == DataFlow.Render)
            {
                if (e.Role == Role.Console || e.Role == Role.Multimedia)
                {
                    SetActiveDevice(e.DefaultDeviceId);
                }
            }
        }

        void SetActiveDevice(string defaultDevice)
        {
            if (defaultDevice == _settings.HeadsetId)
                ActiveDevice = DeviceKind.Headset;
            else if (defaultDevice == _settings.SpeakerId)
                ActiveDevice = DeviceKind.Speaker;
            else
                ActiveDevice = null;
        }

        string GetDefaultDeviceId()
        {
            return new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Console).ID;
        }

        void SetDefaultDevice(string id)
        {
            var policyConfigClient = new PolicyConfigClient();

            policyConfigClient.SetDefaultEndpoint(id, ERole.eConsole);
            policyConfigClient.SetDefaultEndpoint(id, ERole.eMultimedia);

            if (_settings.ChangeCommunicationDevice)
                policyConfigClient.SetDefaultEndpoint(id, ERole.eCommunications);
        }
    }

    public record DefaultDeviceChangedArgs(DataFlow Flow, Role Role, string DefaultDeviceId);

    class NotificationClient : IMMNotificationClient
    {
        public event EventHandler<DefaultDeviceChangedArgs>? DefaultDeviceChanged;

        public NotificationClient()
        {
            new MMDeviceEnumerator().RegisterEndpointNotificationCallback(this);
        }

        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
        {
            DefaultDeviceChanged?.Invoke(this, new DefaultDeviceChangedArgs(flow, role, defaultDeviceId));
        }

        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key) { }

        public void OnDeviceStateChanged(string deviceId, DeviceState newState) { }

        public void OnDeviceAdded(string pwstrDeviceId) { }

        public void OnDeviceRemoved(string deviceId) { }
    }
}
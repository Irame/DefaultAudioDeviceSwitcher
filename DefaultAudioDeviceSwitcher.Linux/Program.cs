using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DefaultAudioDeviceSwitcher.Linux
{
    public enum DeviceKind
    {
        Headset,
        Speaker
    }

    internal class SinkInfo
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public override string ToString() => string.IsNullOrEmpty(Description) ? Name : Description;
    }

    internal class Settings
    {
        private string _filePath = null!;

        public string? HeadsetName { get; set; }
        public string? SpeakerName { get; set; }

        public static Settings Load(string filePath)
        {
            Settings settings;
            if (File.Exists(filePath))
                settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(filePath)) ?? new Settings();
            else
                settings = new Settings();
            settings._filePath = filePath;

            return settings;
        }

        public void Save()
        {
            var json = JsonSerializer.Serialize(this);
            File.WriteAllText(_filePath, json);
        }
    }

    internal static class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                RunTrayApp();
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error starting tray app: " + ex);
                return 1;
            }
        }

        private static void RunTrayApp()
        {
            try
            {
                RunGui().Wait();
            }
            catch (AggregateException ae)
            {
                throw ae.Flatten();
            }
        }

        private static async Task RunGui()
        {
            Gtk.Application.Init();

            var configPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "DefaultAudioDeviceSwitcher"
            );

            Directory.CreateDirectory(configPath);

            var configFile = Path.Combine(configPath, "config.json");
            var settings = Settings.Load(configFile);

            var ctx = new TrayAppContext(settings);

            Gtk.Application.Run();

            await Task.CompletedTask;
        }
    }

    class TrayAppContext
    {
        private readonly Settings _settings;
        private DeviceKind? _activeDevice;

        private Gtk.Menu _menu;
        private Gtk.MenuItem _configItem;
        private Gtk.MenuItem _exitItem;

        private CancellationTokenSource _pulseWatcherCts = new();

        public DeviceKind? ActiveDevice
        {
            get => _activeDevice;
            set
            {
                _activeDevice = value;
                UpdateIndicator();
            }
        }

        private IntPtr _indicator = IntPtr.Zero;

        public TrayAppContext(Settings settings)
        {
            _settings = settings;

            _indicator = CreateTrayIcon();

            // Start background watcher
            StartPulseAudioWatcher();

            DetectCurrentDevice();
        }

        private static bool IsInsideFlatpak => File.Exists("/.flatpak-info");

        private IntPtr CreateTrayIcon()
        {
            var indicator = AppIndicatorNative.app_indicator_new(
                "default-audio-device-switcher",
                "question",
                AppIndicatorCategory.Hardware);

            AppIndicatorNative.app_indicator_set_status(indicator, AppIndicatorStatus.Active);
            AppIndicatorNative.app_indicator_set_title(indicator, "Default Audio Device Switcher");

            // Build menu
            _menu = new Gtk.Menu();

            // Add a "Switch" item for left-click equivalent
            var switchItem = new Gtk.MenuItem("Switch Audio Device");
            switchItem.Activated += (s, e) => SwitchDevice();
            _menu.Append(switchItem);

            _configItem = new Gtk.MenuItem("Config");
            _configItem.Activated += (s, e) => ShowConfig();
            _menu.Append(_configItem);

            _exitItem = new Gtk.MenuItem("Exit");
            _exitItem.Activated += (s, e) => Exit();
            _menu.Append(_exitItem);

            _menu.ShowAll();

            AppIndicatorNative.app_indicator_set_secondary_activate_target(indicator, switchItem.Handle);
            AppIndicatorNative.app_indicator_set_menu(indicator, _menu.Handle);

            return indicator;
        }

        // ------------------------------------------------------
        // 1. Menu Handlers
        // ------------------------------------------------------
        private void SwitchDevice()
        {
            Console.WriteLine("Left-click: switching audio device");

            if (_activeDevice == DeviceKind.Headset)
                SwitchTo(DeviceKind.Speaker);
            else if (_activeDevice == DeviceKind.Speaker)
                SwitchTo(DeviceKind.Headset);
            else
                SwitchTo(DeviceKind.Headset);
        }

        private void ShowConfig()
        {
            var sinks = GetSinks();
            if (sinks == null)
                return;

            var window = new ConfigWindow(_settings, sinks);
            window.Show();
        }

        private void Exit()
        {
            _pulseWatcherCts.Cancel();
            Gtk.Application.Quit();
        }

        // ------------------------------------------------------
        // 2. DEVICE SWITCHING
        // ------------------------------------------------------
        private void SwitchTo(DeviceKind target)
        {
            var sink = target switch
            {
                DeviceKind.Headset => _settings.HeadsetName,
                DeviceKind.Speaker => _settings.SpeakerName,
                _ => null
            };

            if (sink == null)
            {
                Console.WriteLine("Sink missing in config");
                return;
            }

            Console.WriteLine($"Switching to {sink} ({target})");

            SetDefaultSink(sink);
        }

        // ------------------------------------------------------
        // 3. DEFAULT SINK DETECTION
        // ------------------------------------------------------
        private void DetectCurrentDevice()
        {
            var defaultSink = GetDefaultSink();
            if (defaultSink == null)
            {
                ActiveDevice = null;
                return;
            }

            if (defaultSink == _settings.HeadsetName)
                ActiveDevice = DeviceKind.Headset;
            else if (defaultSink == _settings.SpeakerName)
                ActiveDevice = DeviceKind.Speaker;
            else
                ActiveDevice = null;
        }

        // ------------------------------------------------------
        // 4. UPDATE TRAY ICON
        // ------------------------------------------------------
        private void UpdateIndicator()
        {
            var iconName = _activeDevice switch
            {
                DeviceKind.Headset => "headset",
                DeviceKind.Speaker => "speaker",
                _ => "question"
            };

            if (_indicator != IntPtr.Zero)
            {
                AppIndicatorNative.app_indicator_set_icon(_indicator, iconName);
            }
        }

        // ------------------------------------------------------
        // 5. WATCH PULSEAUDIO FOR CHANGES
        // ------------------------------------------------------
        private void StartPulseAudioWatcher()
        {
            Task.Run(() =>
            {
                var psi = new ProcessStartInfo("pactl", "subscribe")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };

                using var process = Process.Start(psi);
                if (process == null) return;

                while (!_pulseWatcherCts.IsCancellationRequested)
                {
                    string? line = process.StandardOutput.ReadLine();
                    if (line == null) continue;

                    // Anything relating to sink or server might matter
                    if (line.Contains("on sink") || line.Contains("on server"))
                    {
                        Gtk.Application.Invoke((_, _) => { DetectCurrentDevice(); });
                    }
                }

                process.Kill();
            });
        }

        // ------------------------------------------------------
        // 6. PACTL HELPERS
        // ------------------------------------------------------
        private List<SinkInfo>? GetSinks()
        {
            var sinks = new List<SinkInfo>();

            var cmdOutput = Run("pactl", "--format=json list cards");
            var soundCards = JsonSerializer.Deserialize<List<SoundCard>>(cmdOutput);

            if (soundCards == null)
            {
                Console.WriteLine("Could not parse pactl output");
                return null;
            }

            foreach (var soundCard in soundCards)
            {
                if (!soundCard.Name.StartsWith("alsa_card."))
                    continue;

                var deviceSlug = soundCard.Name["alsa_card.".Length..];
                var deviceName = soundCard.Properties.GetValueOrDefault("device.nick")
                                 ?? soundCard.Properties.GetValueOrDefault("api.alsa.card.name");

                HashSet<string> processedSinks = [];

                foreach (var (profileName, profile) in soundCard.Profiles.OrderBy(x => x.Key.Contains("input:")))
                {
                    if (profile is { Available: true, Sinks: > 0 })
                    {
                        sinks.AddRange(profileName.Split('+')
                            .Where(x => x.StartsWith("output:"))
                            .Select(x => x["output:".Length..])
                            .Select(output => $"alsa_output.{deviceSlug}.{output}")
                            .Where(x => processedSinks.Add(x))
                            .Select(sinkName => new SinkInfo
                            {
                                Name = sinkName,
                                Description = $"{deviceName} - {profile.Description}"
                            })) ;
                    }
                }
            }

            return sinks;
        }

        private string? GetDefaultSink()
        {
            var infoStr = Run("pactl", "--format=json info");
            var info  = JsonSerializer.Deserialize<Dictionary<string, object>>(infoStr);
            return info?.GetValueOrDefault("default_sink_name")?.ToString();
        }

        private void SetDefaultSink(string sink)
        {
            Run("pactl", $"set-default-sink {sink}", useSpawn: true);

            var inputs = Run("pactl", "list short sink-inputs");
            foreach (var l in inputs.Split('\n'))
            {
                var parts = l.Split('\t', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                if (int.TryParse(parts[0], out var id))
                    Run("pactl", $"move-sink-input {id} {sink}", useSpawn: true);
            }
        }

        private string Run(string cmd, string args, bool useSpawn = false)
        {
            string actualCmd, actualArgs;

            if (IsInsideFlatpak && useSpawn)
            {
                actualCmd = "flatpak-spawn";
                actualArgs = $"--host {cmd} {args}";
            }
            else
            {
                actualCmd = cmd;
                actualArgs = args;
            }

            try
            {
                var psi = new ProcessStartInfo(actualCmd, actualArgs)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };

                using var p = Process.Start(psi);
                if (p == null) return "";

                string result = p.StandardOutput.ReadToEnd();
                p.WaitForExit(1000);
                return result;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error running {cmd}: {ex.Message}");
                return "";
            }
        }
    }

    class ConfigWindow : Gtk.Window
    {
        private readonly Settings _settings;
        private readonly List<SinkInfo> _sinks;
        private Gtk.ComboBox _headsetCombo;
        private Gtk.ComboBox _speakerCombo;

        public ConfigWindow(Settings settings, List<SinkInfo> sinks)
            : base(Gtk.WindowType.Toplevel)
        {
            _settings = settings;
            _sinks = sinks;

            Title = "Audio Device Configuration";
            DefaultWidth = 400;
            DefaultHeight = 200;
            WindowPosition = Gtk.WindowPosition.Center;
            DeleteEvent += (o, args) => Destroy();

            var vbox = new Gtk.VBox(false, 10) { MarginStart = 20, MarginEnd = 20, MarginTop = 20, MarginBottom = 20 };

            // Headset label and combo
            var headsetLabel = new Gtk.Label("Headset:") { Xalign = 0 };
            vbox.PackStart(headsetLabel, false, false, 0);

            _headsetCombo = new Gtk.ComboBox();
            var headsetStore = new Gtk.ListStore(typeof(string));
            headsetStore.AppendValues("(Not configured)");
            foreach (var sink in _sinks)
            {
                headsetStore.AppendValues(sink.ToString());
            }
            _headsetCombo.Model = headsetStore;
            var headsetCell = new Gtk.CellRendererText();
            _headsetCombo.PackStart(headsetCell, true);
            _headsetCombo.AddAttribute(headsetCell, "text", 0);

            // Set active item for headset
            int headsetIdx = 0;
            if (!string.IsNullOrEmpty(_settings.HeadsetName))
            {
                for (int i = 0; i < _sinks.Count; i++)
                {
                    if (_sinks[i].Name == _settings.HeadsetName)
                    {
                        headsetIdx = i + 1;
                        break;
                    }
                }
            }
            _headsetCombo.Active = headsetIdx;
            vbox.PackStart(_headsetCombo, false, false, 0);

            // Speaker label and combo
            var speakerLabel = new Gtk.Label("Speaker:") { Xalign = 0 };
            vbox.PackStart(speakerLabel, false, false, 0);

            _speakerCombo = new Gtk.ComboBox();
            var speakerStore = new Gtk.ListStore(typeof(string));
            speakerStore.AppendValues("(Not configured)");
            foreach (var sink in _sinks)
            {
                speakerStore.AppendValues(sink.ToString());
            }
            _speakerCombo.Model = speakerStore;
            var speakerCell = new Gtk.CellRendererText();
            _speakerCombo.PackStart(speakerCell, true);
            _speakerCombo.AddAttribute(speakerCell, "text", 0);

            // Set active item for speaker
            int speakerIdx = 0;
            if (!string.IsNullOrEmpty(_settings.SpeakerName))
            {
                for (int i = 0; i < _sinks.Count; i++)
                {
                    if (_sinks[i].Name == _settings.SpeakerName)
                    {
                        speakerIdx = i + 1;
                        break;
                    }
                }
            }
            _speakerCombo.Active = speakerIdx;
            vbox.PackStart(_speakerCombo, false, false, 0);

            // Buttons
            var hbox = new Gtk.HBox(true, 10) { MarginTop = 20 };
            var saveBtn = new Gtk.Button("Save");
            saveBtn.Clicked += OnSaveClicked;
            hbox.PackStart(saveBtn, true, true, 0);

            var cancelBtn = new Gtk.Button("Cancel");
            cancelBtn.Clicked += (o, args) => Destroy();
            hbox.PackStart(cancelBtn, true, true, 0);

            vbox.PackStart(hbox, false, false, 0);

            Add(vbox);
            ShowAll();
        }

        private void OnSaveClicked(object? sender, EventArgs e)
        {
            int headsetActive = _headsetCombo.Active;
            if (headsetActive > 0 && headsetActive <= _sinks.Count)
            {
                _settings.HeadsetName = _sinks[headsetActive - 1].Name;
            }
            else
            {
                _settings.HeadsetName = null;
            }

            int speakerActive = _speakerCombo.Active;
            if (speakerActive > 0 && speakerActive <= _sinks.Count)
            {
                _settings.SpeakerName = _sinks[speakerActive - 1].Name;
            }
            else
            {
                _settings.SpeakerName = null;
            }

            _settings.Save();
            Destroy();
        }
    }
}
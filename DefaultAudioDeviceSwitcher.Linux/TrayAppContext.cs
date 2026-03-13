using System.Diagnostics;
using System.Text.Json;

namespace DefaultAudioDeviceSwitcher.Linux;

public enum DeviceKind
{
    Headset,
    Speaker
}

class TrayAppContext
{
    private readonly Settings _settings;

    private Gtk.Menu? _menu;

    private readonly CancellationTokenSource _pulseWatcherCts = new();

    private DeviceKind? ActiveDevice
    {
        get;
        set
        {
            field = value;
            UpdateIndicator();
        }
    }

    private readonly IntPtr _indicator;

    public TrayAppContext(Settings settings)
    {
        _settings = settings;
        _settings.OnSave += DetectCurrentDevice; 

        _indicator = CreateTrayIcon();

        // Start background watcher
        Pactl.WatchForChanges(DetectCurrentDevice, _pulseWatcherCts.Token);

        DetectCurrentDevice();
    }

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
        switchItem.Activated += (_, _) => SwitchDevice();
        _menu.Append(switchItem);

        var configItem = new Gtk.MenuItem("Config");
        configItem.Activated += (_, _) => ShowConfig();
        _menu.Append(configItem);

        var exitItem = new Gtk.MenuItem("Exit");
        exitItem.Activated += (_, _) => Exit();
        _menu.Append(exitItem);

        _menu.ShowAll();

        AppIndicatorNative.app_indicator_set_secondary_activate_target(indicator, switchItem.Handle);
        AppIndicatorNative.app_indicator_set_menu(indicator, _menu.Handle);

        return indicator;
    }

    private void SwitchDevice()
    {
        Console.WriteLine("Switching audio device");

        if (ActiveDevice == DeviceKind.Headset)
            SwitchTo(DeviceKind.Speaker);
        else if (ActiveDevice == DeviceKind.Speaker)
            SwitchTo(DeviceKind.Headset);
        else
            SwitchTo(DeviceKind.Headset);
    }

    private void ShowConfig()
    {
        var sinks = Pactl.GetSinks();
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
            Notify.SendWithAction($"{target} not configured", "Click to open settings", "Open Settings", ShowConfig);
            Console.WriteLine("Sink missing in config");
            return;
        }

        Console.WriteLine($"Switching to {sink} ({target})");

        if (Pactl.SetDefaultSink(sink))
            return;
            
        var sinkDesc = target switch
        {
            DeviceKind.Headset => _settings.HeadsetDescription,
            DeviceKind.Speaker => _settings.SpeakerDescription,
            _ => null
        };
            
        Notify.Send($"Device not found", $"Could not find {sinkDesc ?? sink}");
        Console.WriteLine($"Sink {sink} not found");
    }

    private void DetectCurrentDevice()
    {
        var defaultSink = Pactl.GetDefaultSink();
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

    private void UpdateIndicator()
    {
        var iconName = ActiveDevice switch
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
}
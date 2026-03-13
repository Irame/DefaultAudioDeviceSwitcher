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
        _settings.OnSave += SettingsSaved; 

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

    private void SettingsSaved()
    {
        if (ActiveDevice == null)
            DetectCurrentDevice();
        else if (!SwitchTo(ActiveDevice.Value))
            ActiveDevice = null;
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
        var cards = Pactl.GetCards();
        if (cards == null)
            return;

        var window = new ConfigWindow(_settings, cards);
        window.Show();
    }

    private void Exit()
    {
        _pulseWatcherCts.Cancel();
        Gtk.Application.Quit();
    }

    private bool SwitchTo(DeviceKind target)
    {
        var settings = target switch
        {
            DeviceKind.Headset => _settings.Headset,
            DeviceKind.Speaker => _settings.Speaker,
            _ => null
        };

        if (settings?.Card?.Name == null)
        {
            Notify.SendWithAction($"{target} not configured", "Click to open settings", "Open Settings", ShowConfig);
            Console.WriteLine("Sink missing in config");
            return false;
        }

        Console.WriteLine($"Switching to {settings.Card.Name} - {settings.Profile?.Name ?? "(default profile)"} ({target})");

        if (Pactl.SetDefaultCardProfile(settings.Card.Name, settings.Profile?.Name))
            return true;
            
        Notify.Send($"Device not found", $"Could not find {settings.Card.Description} - {settings.Profile?.Description ?? "(default profile)"}");
        Console.Error.WriteLine($"Sink {settings.Card.Name} - {settings.Profile?.Name ?? "(default profile)"} not found");
        return false;
    }

    private void DetectCurrentDevice()
    {
        var defInfo = Pactl.GetDefaultCardProfile();
        if (defInfo == null)
        {
            ActiveDevice = null;
            return;
        }

        if (defInfo.CardName == _settings.Headset.Card?.Name && (defInfo.ProfileName == _settings.Headset.Profile?.Name || _settings.Headset.Profile?.Name == null))
            ActiveDevice = DeviceKind.Headset;
        else if (defInfo.CardName == _settings.Speaker.Card?.Name && (defInfo.ProfileName == _settings.Speaker.Profile?.Name || _settings.Speaker.Profile?.Name == null))
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
namespace DefaultAudioDeviceSwitcher.Linux;

class ConfigWindow : Gtk.Window
{
    private readonly Settings _settings;
    private readonly List<CardInfo> _cards;
    private readonly Gtk.ComboBoxText _headsetCardCombo;
    private readonly Gtk.ComboBoxText _headsetProfileCombo;
    private readonly Gtk.ComboBoxText _speakerCardCombo;
    private readonly Gtk.ComboBoxText _speakerProfileCombo;

    public ConfigWindow(Settings settings, List<CardInfo> cards)
        : base(Gtk.WindowType.Toplevel)
    {
        _settings = settings;
        _cards = cards;

        Title = "Audio Device Configuration";
        DefaultWidth = 500;
        DefaultHeight = 200;
        WindowPosition = Gtk.WindowPosition.Center;
        DeleteEvent += (_, _) => Destroy();

        var vbox = new Gtk.Box(Gtk.Orientation.Vertical, 10)
            { MarginStart = 20, MarginEnd = 20, MarginTop = 20, MarginBottom = 20 };

        // Headset label and combo
        var headsetLabel = new Gtk.Label("Headset:") { Xalign = 0 };
        vbox.PackStart(headsetLabel, false, false, 0);

        var headsetComboContainer = new Gtk.Box(Gtk.Orientation.Horizontal, 10);

        _headsetCardCombo = new Gtk.ComboBoxText();
        UpdateComboItems(_headsetCardCombo, _cards, c => c.Name, c => c.ToString(), "(Not configured)");
        _headsetCardCombo.ActiveId = _settings.Headset.Card?.Name ?? "";
        _headsetCardCombo.Changed += CardChanged;
        headsetComboContainer.PackStart(_headsetCardCombo, true, true, 0);

        _headsetProfileCombo = new Gtk.ComboBoxText();
        UpdateProfiles(_headsetCardCombo, _headsetProfileCombo);
        _headsetProfileCombo.ActiveId = _settings.Headset.Profile?.Name ?? "";
        headsetComboContainer.PackStart(_headsetProfileCombo, true, true, 0);

        vbox.PackStart(headsetComboContainer, false, false, 0);

        // Speaker label and combo
        var speakerLabel = new Gtk.Label("Speaker:") { Xalign = 0 };
        vbox.PackStart(speakerLabel, false, false, 0);

        var speakerComboContainer = new Gtk.Box(Gtk.Orientation.Horizontal, 10);

        _speakerCardCombo = new Gtk.ComboBoxText();
        UpdateComboItems(_speakerCardCombo, _cards, c => c.Name, c => c.ToString(), "(Not configured)");
        _speakerCardCombo.ActiveId = _settings.Speaker.Card?.Name ?? "";
        _speakerCardCombo.Changed += CardChanged;
        speakerComboContainer.PackStart(_speakerCardCombo, true, true, 0);

        _speakerProfileCombo = new Gtk.ComboBoxText();
        UpdateProfiles(_speakerCardCombo, _speakerProfileCombo);
        _speakerProfileCombo.ActiveId = _settings.Speaker.Profile?.Name ?? "";
        speakerComboContainer.PackStart(_speakerProfileCombo, true, true, 0);

        vbox.PackStart(speakerComboContainer, false, false, 0);

        // Buttons
        var hBox = new Gtk.Box(Gtk.Orientation.Horizontal, 10) { MarginTop = 20 };
        var saveBtn = new Gtk.Button("Save");
        saveBtn.Clicked += OnSaveClicked;
        hBox.PackStart(saveBtn, true, true, 0);

        var cancelBtn = new Gtk.Button("Cancel");
        cancelBtn.Clicked += (_, _) => Destroy();
        hBox.PackStart(cancelBtn, true, true, 0);

        vbox.PackStart(hBox, false, false, 0);

        Add(vbox);
        ShowAll();
    }

    public override void Destroy()
    {
        _headsetCardCombo.Changed -= CardChanged;
        _speakerCardCombo.Changed -= CardChanged;

        base.Destroy();
    }

    private void CardChanged(object? sender, EventArgs args)
    {
        if (sender == _headsetCardCombo)
        {
            UpdateProfiles(_headsetCardCombo, _headsetProfileCombo);
            _headsetProfileCombo.ActiveId = _settings.Headset.Profile?.Name ?? "";
            _speakerProfileCombo.ActiveId ??= "";
        }
        else if (sender == _speakerCardCombo)
        {
            UpdateProfiles(_speakerCardCombo, _speakerProfileCombo);
            _speakerProfileCombo.ActiveId = _settings.Speaker.Profile?.Name ?? "";
            _speakerProfileCombo.ActiveId ??= "";
        }
    }

    public void UpdateProfiles(Gtk.ComboBoxText cardCombo, Gtk.ComboBoxText profileCombo)
    {
        profileCombo.RemoveAll();

        var card = _cards.FirstOrDefault(x => x.Name == cardCombo.ActiveId);
        if (card is null) return;

        UpdateComboItems(profileCombo, card.Profiles, p => p.Name, p => p.ToString(), "(Use default)");
    }

    public static void UpdateComboItems<T>(Gtk.ComboBoxText combo, List<T> items, Func<T, string> keySelector,
        Func<T, string> valueSelector, string? nullValue = null)
    {
        if (nullValue != null)
            combo.Append("", nullValue);

        foreach (var item in items)
            combo.Append(keySelector(item), valueSelector(item));
    }

    public static Gtk.ComboBoxText CreateCombo<T>(List<T> items, Func<T, string> keySelector,
        Func<T, string> valueSelector, string? selectedKey, string? nullValue)
    {
        var combo = new Gtk.ComboBoxText();

        if (nullValue != null)
            combo.Append(null, nullValue);

        foreach (var item in items)
            combo.Append(keySelector(item), valueSelector(item));

        combo.ActiveId = selectedKey;

        return combo;
    }

    private void OnSaveClicked(object? sender, EventArgs e)
    {
        _settings.Headset.Card = string.IsNullOrEmpty(_headsetCardCombo.ActiveId)
            ? null
            : new NameDescSetting { Name = _headsetCardCombo.ActiveId, Description = _headsetCardCombo.ActiveText };
        
        _settings.Headset.Profile = string.IsNullOrEmpty(_headsetProfileCombo.ActiveId)
            ? null
            : new NameDescSetting { Name = _headsetProfileCombo.ActiveId, Description = _headsetProfileCombo.ActiveText };

        _settings.Speaker.Card = string.IsNullOrEmpty(_speakerCardCombo.ActiveId)
            ? null
            : new NameDescSetting { Name = _speakerCardCombo.ActiveId, Description = _speakerCardCombo.ActiveText };
        
        _settings.Speaker.Profile = string.IsNullOrEmpty(_speakerProfileCombo.ActiveId)
            ? null
            : new NameDescSetting { Name = _speakerProfileCombo.ActiveId, Description = _speakerProfileCombo.ActiveText };

        _settings.Save();
        Destroy();
    }
}
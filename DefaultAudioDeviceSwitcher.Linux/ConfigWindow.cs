using Gtk;

namespace DefaultAudioDeviceSwitcher.Linux;

class ConfigWindow : Window
{
    private readonly Settings _settings;
    private readonly List<CardInfo> _cards;
    private readonly ComboBoxText _headsetCardCombo;
    private readonly ComboBoxText _headsetProfileCombo;
    private readonly ComboBoxText _speakerCardCombo;
    private readonly ComboBoxText _speakerProfileCombo;

    public ConfigWindow(Settings settings, List<CardInfo> cards)
        : base(WindowType.Toplevel)
    {
        _settings = settings;
        _cards = cards;

        SetStyle();

        Title = "Audio Device Configuration";
        DefaultWidth = 500;
        DefaultHeight = 200;
        WindowPosition = WindowPosition.Center;
        DeleteEvent += (_, _) => Destroy();

        var vbox = new Box(Orientation.Vertical, 10)
            { MarginStart = 20, MarginEnd = 20, MarginTop = 20, MarginBottom = 20 };

        // Headset label and combo
        (_headsetCardCombo, _headsetProfileCombo) = CreatDeviceSection(vbox, "Headset", _settings.Headset);

        // Speaker label and combo
        (_speakerCardCombo, _speakerProfileCombo) = CreatDeviceSection(vbox, "Speaker", _settings.Speaker);

        // Buttons
        CreateButtons(vbox);

        Add(vbox);
        ShowAll();
    }

    private (ComboBoxText CardCombo, ComboBoxText ProfileCombo) CreatDeviceSection(Box vbox, string title, SinkSettings settings)
    {
        var label = new Label($"{title}:") { Xalign = 0 };
        vbox.PackStart(label, false, false, 0);

        var comboContainer = new Box(Orientation.Horizontal, 10);

        var cardCombo = new ComboBoxText();
        UpdateComboItems(cardCombo, _cards, c => c.Name, c => c.ToString(), "(Not configured)");
        cardCombo.ActiveId = settings.Card?.Name ?? "";
        cardCombo.Changed += CardChanged;
        comboContainer.PackStart(cardCombo, true, true, 0);

        var profileCombo = new ComboBoxText();
        UpdateProfiles(profileCombo, cardCombo.ActiveId);
        profileCombo.ActiveId = settings.Profile?.Name ?? "";
        comboContainer.PackStart(profileCombo, true, true, 0);

        vbox.PackStart(comboContainer, false, false, 0);

        return (cardCombo, profileCombo);
    }

    private void CreateButtons(Box vbox)
    {
        var hBox = new Box(Orientation.Horizontal, 10) { MarginTop = 20 };
        var saveBtn = new Button("Save");
        saveBtn.Clicked += OnSaveClicked;
        hBox.PackStart(saveBtn, true, true, 0);

        var cancelBtn = new Button("Cancel");
        cancelBtn.Clicked += (_, _) => Destroy();
        hBox.PackStart(cancelBtn, true, true, 0);

        vbox.PackStart(hBox, false, false, 0);
    }

    public override void Destroy()
    {
        _headsetCardCombo.Changed -= CardChanged;
        _speakerCardCombo.Changed -= CardChanged;

        base.Destroy();
    }

    private void SetStyle()
    {
        StyleContext.AddClass("dads-config-window");
        
        var cssProvider = new CssProvider();
        cssProvider.LoadFromData(
            """
            .dads-config-window combobox button {
                min-height: 24px;
                padding-top: 3px;
                padding-bottom: 3px;
            }
            """);

        StyleContext.AddProviderForScreen(
            Gdk.Screen.Default,
            cssProvider,
            StyleProviderPriority.Application
        );
    }

    private void CardChanged(object? sender, EventArgs args)
    {
        if (sender == _headsetCardCombo)
        {
            UpdateProfiles(_headsetProfileCombo, _headsetCardCombo.ActiveId);
            _headsetProfileCombo.ActiveId = _settings.Headset.Profile?.Name ?? "";
            _speakerProfileCombo.ActiveId ??= "";
        }
        else if (sender == _speakerCardCombo)
        {
            UpdateProfiles(_speakerProfileCombo, _speakerCardCombo.ActiveId);
            _speakerProfileCombo.ActiveId = _settings.Speaker.Profile?.Name ?? "";
            _speakerProfileCombo.ActiveId ??= "";
        }
    }

    private void UpdateProfiles(ComboBoxText profileCombo, string? cardName)
    {
        profileCombo.RemoveAll();

        var card = _cards.FirstOrDefault(x => x.Name == cardName);
        if (card is null) return;

        UpdateComboItems(profileCombo, card.Profiles, p => p.Name, p => p.ToString(), "(Use default)");
    }

    private static void UpdateComboItems<T>(ComboBoxText combo, List<T> items, Func<T, string> keySelector,
        Func<T, string> valueSelector, string? nullValue = null)
    {
        if (nullValue != null)
            combo.Append("", nullValue);

        foreach (var item in items)
            combo.Append(keySelector(item), valueSelector(item));
    }

    public static ComboBoxText CreateCombo<T>(List<T> items, Func<T, string> keySelector,
        Func<T, string> valueSelector, string? selectedKey, string? nullValue)
    {
        var combo = new ComboBoxText();

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
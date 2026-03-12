namespace DefaultAudioDeviceSwitcher.Linux;

class ConfigWindow : Gtk.Window
{
    private readonly Settings _settings;
    private readonly List<SinkInfo> _sinks;
    private readonly Gtk.ComboBox _headsetCombo;
    private readonly Gtk.ComboBox _speakerCombo;

    public ConfigWindow(Settings settings, List<SinkInfo> sinks)
        : base(Gtk.WindowType.Toplevel)
    {
        _settings = settings;
        _sinks = sinks;

        Title = "Audio Device Configuration";
        DefaultWidth = 400;
        DefaultHeight = 200;
        WindowPosition = Gtk.WindowPosition.Center;
        DeleteEvent += (_, _) => Destroy();

        var vbox = new Gtk.Box(Gtk.Orientation.Vertical, 10) { MarginStart = 20, MarginEnd = 20, MarginTop = 20, MarginBottom = 20 };

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

    private void OnSaveClicked(object? sender, EventArgs e)
    {
        int headsetActive = _headsetCombo.Active;
        if (headsetActive > 0 && headsetActive <= _sinks.Count)
        {
            _settings.HeadsetName = _sinks[headsetActive - 1].Name;
            _settings.HeadsetDescription = _sinks[headsetActive - 1].Description;
        }
        else
        {
            _settings.HeadsetName = null;
            _settings.HeadsetDescription = null;
        }

        var speakerActive = _speakerCombo.Active;
        if (speakerActive > 0 && speakerActive <= _sinks.Count)
        {
            _settings.SpeakerName = _sinks[speakerActive - 1].Name;
            _settings.SpeakerDescription = _sinks[speakerActive - 1].Description;
        }
        else
        {
            _settings.SpeakerName = null;
            _settings.SpeakerDescription = null;
        }

        _settings.Save();
        Destroy();
    }
}
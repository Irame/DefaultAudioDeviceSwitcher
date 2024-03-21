using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DefaultAudioDeviceSwitcher
{
    public partial class ConfigForm : Form
    {
        private class DeviceListEntry
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public DeviceListEntry(string id, string name)
            {
                Id = id;
                Name = name;
            }
        }

        private Settings _settings;

        public ConfigForm(Settings settings)
        {
            _settings = settings;

            InitializeComponent();
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            SaveSettings();
            Close();
        }

        private void ConfigForm_Load(object sender, EventArgs e)
        {
            CalcPosition();
            FillCombos();
            LoadSettings();
        }

        private static Font RegularFont = new Font(Label.DefaultFont, FontStyle.Regular);
        private static Font BoldFont = new Font(Label.DefaultFont, FontStyle.Bold);

        public void HighlightDeviceKind(DeviceKind? deviceKind)
        {
            headsetLabel.Font = RegularFont;
            speakerLabel.Font = RegularFont;

            if (deviceKind == DeviceKind.Headset)
                headsetLabel.Font = BoldFont;
            else if (deviceKind == DeviceKind.Speaker)
                speakerLabel.Font = BoldFont;
        }

        private void CalcPosition()
        {
            var cursorPos = Cursor.Position;
            var screen = Screen.FromPoint(cursorPos);

            var rect = new Rectangle(cursorPos.X, cursorPos.Y, Width, Height);
            rect.Offset(-rect.Width / 2, -rect.Height / 2);

            var wa = screen.WorkingArea;

            if (rect.Left < wa.Left + 20)
                rect.Offset(wa.Left - rect.Left + 20, 0);

            if (rect.Top < wa.Top + 10)
                rect.Offset(0, wa.Top - rect.Top + 10);

            if (rect.Right > wa.Right - 20)
                rect.Offset(wa.Right - rect.Right - 20, 0);

            if (rect.Bottom > wa.Bottom - 10)
                rect.Offset(0, wa.Bottom - rect.Bottom - 10);

            Left = rect.Left;
            Top = rect.Top;
        }

        private void FillCombos()
        {
            var deviceList = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
                .Select(d => new DeviceListEntry(d.ID, d.FriendlyName))
                .ToList();

            headsetCombo.ValueMember = nameof(DeviceListEntry.Id);
            headsetCombo.DisplayMember = nameof(DeviceListEntry.Name);
            headsetCombo.DataSource = deviceList.ToList();

            speakerCombo.ValueMember = nameof(DeviceListEntry.Id);
            speakerCombo.DisplayMember = nameof(DeviceListEntry.Name);
            speakerCombo.DataSource = deviceList.ToList();
        }

        private void LoadSettings()
        {
            communicationCheck.Checked = _settings.ChangeCommunicationDevice;

            headsetCombo.SelectedValue = _settings.HeadsetId;
            speakerCombo.SelectedValue = _settings.SpeakerId;
        }

        private void SaveSettings()
        {
            _settings.HeadsetId = (string)headsetCombo.SelectedValue;
            _settings.SpeakerId = (string)speakerCombo.SelectedValue;
            _settings.ChangeCommunicationDevice = communicationCheck.Checked;

            _settings.Save();
        }
    }
}

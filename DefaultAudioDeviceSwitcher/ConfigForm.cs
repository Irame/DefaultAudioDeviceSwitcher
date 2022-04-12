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
            _settings.HeadsetId = (string)headsetCombo.SelectedValue;
            _settings.SpeakerId = (string)speakerCombo.SelectedValue;
            _settings.ChangeCommunicationDevice = communicationCheck.Checked;

            _settings.Save();
            Close();
        }

        private void ConfigForm_Load(object sender, EventArgs e)
        {
            communicationCheck.Checked = _settings.ChangeCommunicationDevice;

            var deviceList = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
                .Select(d => new DeviceListEntry(d.ID, d.FriendlyName))
                .ToList();

            headsetCombo.ValueMember = nameof(DeviceListEntry.Id);
            headsetCombo.DisplayMember = nameof(DeviceListEntry.Name);
            headsetCombo.DataSource = deviceList.ToList();

            speakerCombo.ValueMember = nameof(DeviceListEntry.Id);
            speakerCombo.DisplayMember = nameof(DeviceListEntry.Name);
            speakerCombo.DataSource = deviceList.ToList();

            headsetCombo.SelectedValue = _settings.HeadsetId;
            speakerCombo.SelectedValue = _settings.SpeakerId;
        }
    }
}

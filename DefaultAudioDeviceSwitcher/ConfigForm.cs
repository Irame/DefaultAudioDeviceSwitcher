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
            _settings.HeadsetName = headsetCombo.Text;
            _settings.SpeakerName = speakerCombo.Text;
            _settings.ChangeCommunicationDevice = communicationCheck.Checked;
            _settings.NirCmdPath = nirCmdEdit.Text;

            _settings.Save();
            Close();
        }

        private void ConfigForm_Load(object sender, EventArgs e)
        {
            headsetCombo.Text = _settings.HeadsetName;
            speakerCombo.Text = _settings.SpeakerName;
            communicationCheck.Checked = _settings.ChangeCommunicationDevice;
            nirCmdEdit.Text = _settings.NirCmdPath;

            var enumerator = new MMDeviceEnumerator();
            foreach (var endpoint in
                     enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                var name = endpoint.FriendlyName;
                var braceIndex = name.IndexOf(" (");

                name = name.Substring(0, braceIndex);

                headsetCombo.Items.Add(name);
                speakerCombo.Items.Add(name);
            }
        }

        private void browseNirCmdBtn_Click(object sender, EventArgs e)
        {
            var appDir = Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar;

            var initDir = Path.GetDirectoryName(nirCmdEdit.Text);

            if (string.IsNullOrEmpty(initDir))
                initDir = appDir;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "nircmd|nircmd*.exe";
            openFileDialog.InitialDirectory = initDir;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var fileName = openFileDialog.FileName;

                if (appDir != null && fileName.StartsWith(appDir))
                    fileName = fileName[appDir.Length..];

                nirCmdEdit.Text = fileName;
            }
        }
    }
}

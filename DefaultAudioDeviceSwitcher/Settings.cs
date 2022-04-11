using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DefaultAudioDeviceSwitcher
{
    public class Settings
    {
        private readonly string _filePath;

        public string HeadsetName { get; set; } = "";
        public string SpeakerName { get; set; } = "";
        public bool ChangeCommunicationDevice { get; set; } = false;
        public string NirCmdPath { get; set; } = "nircmd.exe";

        public Settings(string filePath)
        {
            _filePath = filePath;

            if (File.Exists(filePath))
            {
                Newtonsoft.Json.JsonConvert.PopulateObject(File.ReadAllText(_filePath), this);
            }
        }

        public void Save()
        {
            File.WriteAllText(_filePath, Newtonsoft.Json.JsonConvert.SerializeObject(this));
        }
    }
}

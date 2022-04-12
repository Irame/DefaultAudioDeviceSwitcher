using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DefaultAudioDeviceSwitcher
{
    public class Settings
    {
        private readonly string _filePath;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate, NullValueHandling = NullValueHandling.Ignore)]
        public string HeadsetId { get; set; } = "";

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate, NullValueHandling = NullValueHandling.Ignore)]
        public string SpeakerId { get; set; } = "";

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate, NullValueHandling = NullValueHandling.Ignore)]
        public bool ChangeCommunicationDevice { get; set; } = false;

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
            var dirName = Path.GetDirectoryName(_filePath);

            if (dirName != null)
                Directory.CreateDirectory(dirName);

            File.WriteAllText(_filePath, Newtonsoft.Json.JsonConvert.SerializeObject(this));
        }
    }
}

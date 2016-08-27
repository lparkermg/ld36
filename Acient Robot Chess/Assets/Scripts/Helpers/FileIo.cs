using UnityEngine;
using System.IO;
using System.Collections;
using Entities;

namespace Helpers.Filesystem
{
    public static class FileIo
    {
        private static string _settingsPath = Path.Combine(Application.persistentDataPath,"settings.json");

        public static Settings LoadSettingsOrDefault()
        {
            if (!File.Exists(_settingsPath))
            {
                //TODO: Create new settings object and save it with the defaults then pass it through.
                var settings = new Settings();
                SaveSettings(settings);
                return settings;
            }
            else
            {
                var settingsJson = File.ReadAllText(_settingsPath);
                var settings = JsonUtility.FromJson<Settings>(settingsJson);
                return settings;
            }
        }

        public static void SaveSettings(Settings settings)
        {
            var json = JsonUtility.ToJson(settings);
            File.WriteAllText(_settingsPath,json);
        }
    }
}

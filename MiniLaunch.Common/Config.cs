using System;
using System.IO;
using System.Text.Json.Serialization;

namespace MiniLaunch.Common
{
    public class Config
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public static readonly string DataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DDOMiniLaunch");

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public const string SettingsFileName = "config.json";

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public const string DatabaseFileName = "database.db";

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public static readonly string SettingsFilePath = Path.Combine(DataFolder, SettingsFileName);

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public static readonly string DatabaseFilePath = Path.Combine(DataFolder, DatabaseFileName);

        public string LastPlayedServer { get; set; } = null;

        public bool Use64Bit { get; set; } = Environment.Is64BitOperatingSystem;

        public string GameDirectory { get; set; } = "C:\\Program Files (x86)\\StandingStoneGames\\Dungeons & Dragons Online";

        public bool EnablePreview { get; set; } = false;

        public string PreviewGameDirectory { get; set; } = "C:\\Program Files (x86)\\StandingStoneGames\\Dungeons & Dragons Online (Preview)";
    }
}
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
        public const string DatabaseFileName = "database.dat";

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public static readonly string SettingsFilePath = Path.Combine(DataFolder, SettingsFileName);

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public static readonly string DatabaseFilePath = Path.Combine(DataFolder, DatabaseFileName);

        public Servers LastPlayedServer { get; set; } = Servers.Argonnessen;

        public string GameDirectory { get; set; } = "C:\\Program Files (x86)\\StandingStoneGames\\Dungeons & Dragons Online";

        public bool Use64Bit { get; set; } = Environment.Is64BitOperatingSystem;
    }
}
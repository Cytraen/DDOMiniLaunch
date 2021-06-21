using System;
using System.IO;
using System.Text.Json.Serialization;

namespace MiniLaunch.Common
{
    public class Config
    {
        #region Constants

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public const string LauncherExeName = "DNDLauncher.exe";

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public const string SettingsFileName = "config.json";

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public const string DatabaseFileName = "database.db";

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public static readonly string DataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DDOMiniLaunch");

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public static readonly string SettingsFilePath = Path.Combine(DataFolder, SettingsFileName);

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public static readonly string DatabaseFilePath = Path.Combine(DataFolder, DatabaseFileName);

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public const string DefaultGameDirectory = "C:\\Program Files (x86)\\StandingStoneGames\\Dungeons & Dragons Online";

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public const string PreviewDefaultGameDirectory = "C:\\Program Files (x86)\\StandingStoneGames\\Dungeons & Dragons Online (Preview)";

        #endregion Constants

        #region Settings

        public string LastPlayedServer { get; set; }

        public string GameDirectory { get; set; }

        public string PreviewGameDirectory { get; set; }

        public bool Use64Bit { get; set; }

        public bool EnablePreview { get; set; }

        public bool CheckUpdateAtStartup { get; set; } = true;

        #endregion Settings
    }
}
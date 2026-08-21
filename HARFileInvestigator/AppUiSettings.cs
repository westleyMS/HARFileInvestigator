using System.Text.Json;

namespace HARFileInvestigator
{
    internal sealed class AppUiSettings
    {
        public string LastOpenedFile { get; set; } = string.Empty;
        public string LastOpenedDirectory { get; set; } = string.Empty;
        public bool DarkTheme { get; set; }
        public string MethodFilter { get; set; } = "All";
        public string StatusGroupFilter { get; set; } = "All";
        public string StatusCodeFilter { get; set; } = string.Empty;
        public string HostFilter { get; set; } = string.Empty;
        public string SearchFilter { get; set; } = string.Empty;
        public List<string> QueryHistory { get; set; } = [];
        public List<string> FilterRules { get; set; } = [];
        public int ActiveFilterIndex { get; set; } = -1;
        public bool FilterEnabled { get; set; }
        public List<TagDefinition> TagDefinitions { get; set; } = [];
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int SplitterDistance { get; set; }
        public int DetailsSplitterDistance { get; set; }

        public static AppUiSettings Load(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return new AppUiSettings();
                }

                var json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<AppUiSettings>(json) ?? new AppUiSettings();
            }
            catch
            {
                return new AppUiSettings();
            }
        }

        public static void Save(string filePath, AppUiSettings settings)
        {
            try
            {
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(filePath, json);
            }
            catch
            {
                // Ignore persistence errors.
            }
        }
    }
}

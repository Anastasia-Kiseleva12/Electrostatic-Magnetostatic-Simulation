using System.IO;
using System.Text.Json;

namespace ElectroMagSimulator.IO
{
    public static class ProjectSerializer
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true,
            IncludeFields = true
        };

        public static void SaveToFile(ProjectData data, string path)
        {
            var json = JsonSerializer.Serialize(data, _options);
            File.WriteAllText(path, json);
        }

        public static ProjectData LoadFromFile(string path)
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<ProjectData>(json, _options)
                   ?? throw new IOException("Failed to deserialize project file.");
        }
    }
}

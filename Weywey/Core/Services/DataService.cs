using Newtonsoft.Json;
using System;
using System.IO;

namespace Weywey.Core.Services;

public static class DataService
{
    private static readonly string Path = "Data";

    public static void RunService()
    {
        if (!Directory.Exists(Path))
            Directory.CreateDirectory(Path);
    }

    public static void Save(string path, object data)
    {
        path = System.IO.Path.Combine(Path, path);
        using var writer = new StreamWriter(path);
        writer.Write(JsonConvert.SerializeObject(data, Formatting.Indented));
    }

    public static T Get<T>(string path)
    {
        path = System.IO.Path.Combine(Path, path);
        if (!File.Exists(path))
            File.Create(path).Close();

        using var reader = new StreamReader(path);
        return JsonConvert.DeserializeObject<T>(reader.ReadToEnd()) ?? Activator.CreateInstance<T>();
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Weywey.Core.Services
{
    public static class DataService
    {
        private static string _path { get; set; }

        public static void RunService()
        {
            _path = "Data";
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
        }

        public static void Save(string path, object data)
        {
            path = Path.Combine(_path, path);
            using var writer = new StreamWriter(path);
            writer.Write(JsonConvert.SerializeObject(data));
        }

        public static T Get<T>(string path)
        {
            path = Path.Combine(_path, path);
            if (!File.Exists(path))
                File.Create(path).Close();

            using var reader = new StreamReader(path);
            return JsonConvert.DeserializeObject<T>(reader.ReadToEnd()) ?? Activator.CreateInstance<T>();
        }
    }
}

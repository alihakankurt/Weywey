using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Weywey.Core.Services;

public static class ConfigurationService
{
    public static string Token { get; private set; }
    public static string Prefix { get; private set; }
    public static string Version { get; private set; }
    public static string WolframToken { get; private set; }

    public static void RunService()
    {
        try
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();
            Token = config["Token"];
            Prefix = config["Prefix"];
            Version = config["Version"];
            WolframToken = config["WolframToken"];
        }

        catch
        {
            Console.WriteLine("Configuration could not be loaded! Exiting...");
            Environment.Exit(0);
        }
    }
}

using System;
using Microsoft.Extensions.Configuration;

namespace LinuxCmd.Net.NetWork
{
    public class ConfigHander
    {
        public static IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("app.json")
            .Build();

        public static string GetString(string key)
        {
            return configuration[key];
        }

        public static int GetInt(string key)
        {
            return Convert.ToInt32(configuration[key]);
        }
    }
}
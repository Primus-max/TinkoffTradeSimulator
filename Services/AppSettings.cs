using Newtonsoft.Json;
using System;
using System.IO;
using TinkoffTradeSimulator.Models;

namespace TinkoffTradeSimulator.Services
{
    internal class AppSettings
    {
        private static readonly string ConfigFilePath = "config.json";

        public static AppConfig LoadConfig()
        {
            try
            {             
                string json = File.ReadAllText(ConfigFilePath);
                return JsonConvert.DeserializeObject<AppConfig>(json);
            }
            catch (Exception)
            {
                // Обработка ошибок
                return new AppConfig();
            }
        }

        public static void SaveConfig(AppConfig config)
        {
            try
            {
                string json = JsonConvert.SerializeObject(config);
                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception)
            {
                // Обработка ошибок
            }
        }
    }
}

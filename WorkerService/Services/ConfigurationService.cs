using System.IO;
using System.Text.Json;
using WorkerService.Models;

namespace WorkerService.Services
{
    public class ConfigurationService
    {
        public static ConfigurationModel GetConfiguration(string jsonPath)
        {
            string jsonText = File.ReadAllText(jsonPath);
            var config = JsonSerializer.Deserialize<ConfigurationModel>(jsonText);
            return config;
        }
    }
}
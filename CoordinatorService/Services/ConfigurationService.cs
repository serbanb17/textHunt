using System.IO;
using System.Text.Json;
using CoordinatorService.Models;

namespace CoordinatorService.Services
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
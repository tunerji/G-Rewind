using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace G_Rewind.classes
{
    internal class Config
    {
        public decimal SafeZOffset { get; set; }
        public decimal UserDefinedTopZ { get; set; } = 250.0m; // Use 'm' suffix for decimal
        public decimal UserDefinedBottomZ { get; set; } = 0.0m;
        public decimal MachineMaxZ { get; set; } = 200.0m;
        public decimal MachineMinZ { get; set; } = 0.0m;

        public List<string> MotionCommands { get; set; }
        public List<string> CoordinateCommands { get; set; }
        public List<string> FeedRateCommands { get; set; }

        private Config()
        {
            // Private constructor to prevent direct instantiation
        }

        public static Config LoadOrCreateConfig(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                Console.WriteLine("Config file not found. Creating a default config file.");
                var config = CreateDefaultConfig(configFilePath);
                return config;
            }

            try
            {
                Console.WriteLine("Loading JSON from file...");
                var json = File.ReadAllText(configFilePath);
                Console.WriteLine("JSON loaded successfully.");

                var settings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Error,
                    NullValueHandling = NullValueHandling.Include
                };

                Console.WriteLine("Attempting to deserialize JSON...");
                var config = JsonConvert.DeserializeObject<Config>(json, settings);
                Console.WriteLine("Deserialization succeeded.");
                return config;
            }
            catch (JsonSerializationException ex)
            {
                Console.WriteLine($"JSON deserialization error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                throw;
            }
        }

        private static Config CreateDefaultConfig(string configFilePath)
        {
            var defaultConfig = new Config
            {
                SafeZOffset = 10.0m,
                UserDefinedTopZ = 200.0m,
                UserDefinedBottomZ = 0.0m,
                MachineMaxZ = 200.0m,
                MachineMinZ = 0.0m,
                MotionCommands = new List<string> { "G0", "G1" },
                CoordinateCommands = new List<string> { "X", "Y", "Z" },
                FeedRateCommands = new List<string> { "F" }
            };

            var json = JsonConvert.SerializeObject(defaultConfig, Formatting.Indented);
            Directory.CreateDirectory(Path.GetDirectoryName(configFilePath));
            File.WriteAllText(configFilePath, json);

            return defaultConfig;
        }
    }
}

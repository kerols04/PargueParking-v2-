using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using PragueParkingV2.Core;

namespace PragueParkingV2.Data
{
    public static class FileManager
    {
        private static readonly string BaseDir = AppContext.BaseDirectory;

        private static readonly string dataFile = Path.Combine(BaseDir, "parkingdata.json");
        private static readonly string configFile = Path.Combine(BaseDir, "config.json");

        private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new VehicleConverter() }
        };

        public static void SaveData(List<ParkingSpot> parkingSpots)
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(parkingSpots, jsonOptions);
                File.WriteAllText(dataFile, jsonString);
            }
            catch { }
        }

        public static List<ParkingSpot> LoadData()
        {
            try
            {
                if (!File.Exists(dataFile))
                {
                    SaveData(new List<ParkingSpot>());
                    return new List<ParkingSpot>();
                }

                string jsonString = File.ReadAllText(dataFile);
                var data = JsonSerializer.Deserialize<List<ParkingSpot>>(jsonString, jsonOptions);
                return data ?? new List<ParkingSpot>();
            }
            catch
            {
                return new List<ParkingSpot>();
            }
        }

        public static void SaveConfig(Config config)
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configFile, jsonString);
            }
            catch { }
        }

        public static Config LoadConfig()
        {
            try
            {
                if (!File.Exists(configFile))
                {
                    var defaultConfig = new Config();
                    SaveConfig(defaultConfig);
                    return defaultConfig;
                }

                string jsonString = File.ReadAllText(configFile);
                var cfg = JsonSerializer.Deserialize<Config>(jsonString);
                return cfg ?? new Config();
            }
            catch
            {
                return new Config();
            }
        }
    }

    public class VehicleConverter : JsonConverter<Vehicle>
    {
        public override Vehicle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            string type = null;
            if (root.TryGetProperty("Type", out var typeProp) && typeProp.ValueKind == JsonValueKind.String)
                type = typeProp.GetString();

            int size = 0;
            if (root.TryGetProperty("Size", out var sizeProp) && sizeProp.ValueKind == JsonValueKind.Number)
                size = sizeProp.GetInt32();

            Vehicle vehicle = type?.ToLowerInvariant() switch
            {
                "car" => new Car(),
                "mc" => new MC(),
                _ => (size == 4 ? new Car() : new MC())
            };

            if (root.TryGetProperty("RegNo", out var regProp))
                vehicle.RegNo = regProp.GetString();

            if (root.TryGetProperty("CheckInTime", out var timeProp))
                vehicle.CheckInTime = timeProp.GetDateTime();

            if (root.TryGetProperty("Size", out var sProp) && sProp.ValueKind == JsonValueKind.Number)
                vehicle.Size = sProp.GetInt32();

            return vehicle;
        }

        public override void Write(Utf8JsonWriter writer, Vehicle value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString("Type", value.GetType().Name);
            writer.WriteString("RegNo", value.RegNo);
            writer.WriteString("CheckInTime", value.CheckInTime);
            writer.WriteNumber("Size", value.Size);

            writer.WriteEndObject();
        }
    }
}

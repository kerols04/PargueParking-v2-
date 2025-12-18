using PragueParkingV2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PragueParkingV2.Data
{
    public static class FileManager
    {
        private static readonly string dataFilePath = Path.Combine(AppContext.BaseDirectory, "parkingdata.json");
        private static readonly string configFilePath = Path.Combine(AppContext.BaseDirectory, "config.json");

        public static void SaveData(List<ParkingSpot> parkingSpots)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Converters = { new VehicleConverter() }
                };

                string json = JsonSerializer.Serialize(parkingSpots, options);
                File.WriteAllText(dataFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fel vid sparning av data: " + ex.Message);
            }
        }

        public static List<ParkingSpot> LoadData()
        {
            try
            {
                if (!File.Exists(dataFilePath))
                {
                    // Skapar filen första gången (tom lista)
                    SaveData(new List<ParkingSpot>());
                    return new List<ParkingSpot>();
                }

                var options = new JsonSerializerOptions
                {
                    Converters = { new VehicleConverter() }
                };

                string json = File.ReadAllText(dataFilePath);
                return JsonSerializer.Deserialize<List<ParkingSpot>>(json, options) ?? new List<ParkingSpot>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fel vid laddning av data: " + ex.Message);
                return new List<ParkingSpot>();
            }
        }

        public static void SaveConfig(Config config)
        {
            try
            {
                config.Normalize();

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(configFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fel vid sparning av config: " + ex.Message);
            }
        }

        public static Config LoadConfig()
        {
            try
            {
                if (!File.Exists(configFilePath))
                {
                    var defaultConfig = new Config();
                    defaultConfig.Normalize();
                    SaveConfig(defaultConfig);
                    return defaultConfig;
                }

                string json = File.ReadAllText(configFilePath);
                var cfg = JsonSerializer.Deserialize<Config>(json) ?? new Config();
                cfg.Normalize();
                return cfg;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fel vid laddning av config: " + ex.Message);
                return new Config();
            }
        }

        // Converter som sparar/laddar rätt subklass (Car/MC)
        private class VehicleConverter : JsonConverter<Vehicle>
        {
            public override Vehicle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                using var doc = JsonDocument.ParseValue(ref reader);
                var root = doc.RootElement;

                string type = root.GetProperty("Type").GetString() ?? "";
                string regNo = root.GetProperty("RegNo").GetString() ?? "";
                DateTime checkIn = root.GetProperty("CheckInTime").GetDateTime();

                Vehicle vehicle = type.ToUpperInvariant() switch
                {
                    "CAR" => new Car(),
                    "MC" => new MC(),
                    _ => throw new NotSupportedException("Okänd fordonstyp: " + type)
                };

                vehicle.RegNo = regNo;
                vehicle.CheckInTime = checkIn;

                // Storlek tas från config (säkrare än att lita på gamla sparade värden)
                vehicle.Size = GarageSettings.GetVehicleSize(vehicle.GetType().Name, vehicle.Size);

                return vehicle;
            }

            public override void Write(Utf8JsonWriter writer, Vehicle value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();

                // Type används för att kunna skapa rätt subklass vid Read()
                writer.WriteString("Type", value.GetType().Name);
                writer.WriteString("RegNo", value.RegNo);
                writer.WriteNumber("Size", value.Size);
                writer.WriteString("CheckInTime", value.CheckInTime);

                writer.WriteEndObject();
            }
        }
    }
}


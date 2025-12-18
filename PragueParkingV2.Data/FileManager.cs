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
        // Output-mappen där appen körs (bra på lärarens dator också)
        private static readonly string BaseDir = AppContext.BaseDirectory;

        // Filnamn hålls nära koden så det är lätt att hitta/ändra
        private static readonly string DataFilePath = Path.Combine(BaseDir, "parkingdata.json");
        private static readonly string ConfigFilePath = Path.Combine(BaseDir, "config.json");
        private static readonly string ErrorLogPath = Path.Combine(BaseDir, "pp_error.log");

        // JSON-inställningar + custom converter för Vehicle (arv Car/MC)
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new VehicleConverter() }
        };

        // Sparar hela listan av p-platser till JSON (kallas vid varje förändring)
        public static void SaveData(List<ParkingSpot> parkingSpots)
        {
            try
            {
                string json = JsonSerializer.Serialize(parkingSpots, JsonOptions);
                File.WriteAllText(DataFilePath, json);
            }
            catch (Exception ex)
            {
                LogError("SaveData", ex);
            }
        }

        // Läser p-platser från JSON. Om fil saknas skapas en tom datafil.
        public static List<ParkingSpot> LoadData()
        {
            try
            {
                if (!File.Exists(DataFilePath))
                {
                    // För G räcker tom startdata (för VG 2.1 kan du fylla på testdata här)
                    SaveData(new List<ParkingSpot>());
                    return new List<ParkingSpot>();
                }

                string json = File.ReadAllText(DataFilePath);
                var data = JsonSerializer.Deserialize<List<ParkingSpot>>(json, JsonOptions);

                return data ?? new List<ParkingSpot>();
            }
            catch (Exception ex)
            {
                LogError("LoadData", ex);
                return new List<ParkingSpot>();
            }
        }

        // Sparar konfiguration (antal platser, fordonstyper osv)
        public static void SaveConfig(Config config)
        {
            try
            {
                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                LogError("SaveConfig", ex);
            }
        }

        // Läser konfiguration. Om fil saknas skapas default config.
        public static Config LoadConfig()
        {
            try
            {
                if (!File.Exists(ConfigFilePath))
                {
                    var defaultConfig = new Config();
                    SaveConfig(defaultConfig);
                    return defaultConfig;
                }

                string json = File.ReadAllText(ConfigFilePath);
                var cfg = JsonSerializer.Deserialize<Config>(json);

                return cfg ?? new Config();
            }
            catch (Exception ex)
            {
                LogError("LoadConfig", ex);
                return new Config();
            }
        }

        // Enkel fellogg för att slippa “tyst fail” på lärarens dator
        private static void LogError(string where, Exception ex)
        {
            try
            {
                File.AppendAllText(
                    ErrorLogPath,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {where}: {ex}\n\n"
                );
            }
            catch
            {
                // Om logging också misslyckas finns inget mer att göra här.
            }
        }
    }

    // Converter som deserialiserar rätt subklass (Car/MC) när JSON läses in
    public class VehicleConverter : JsonConverter<Vehicle>
    {
        public override Vehicle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            string? type = null;
            if (root.TryGetProperty("Type", out var typeProp) && typeProp.ValueKind == JsonValueKind.String)
                type = typeProp.GetString();

            int size = 0;
            if (root.TryGetProperty("Size", out var sizeProp) && sizeProp.ValueKind == JsonValueKind.Number)
                size = sizeProp.GetInt32();

            // Välj subklass baserat på Type (eller fallback på Size)
            Vehicle vehicle = type?.ToLowerInvariant() switch
            {
                "car" => new Car(),
                "mc" => new MC(),
                _ => (size == 4 ? new Car() : new MC())
            };

            if (root.TryGetProperty("RegNo", out var regProp) && regProp.ValueKind == JsonValueKind.String)
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


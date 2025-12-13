using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using PragueParkingV2.Core;

namespace PragueParkingV2.Data
{
    // Den här statiska klassen sköter all läsning och skrivning till filer (JSON).
    public static class FileManager
    {
        // Filnamnet där parkeringsdata sparas
        private static string dataFile = "parkingdata.json";
        // Filnamnet för parkeringshuset grundinställningar
        private static string configFile = "config.json";

        // Sparar alla parkeringsplatser och fordon till parkingdata.json
        public static void SaveData(List<ParkingSpot> spots)
        {
            try
            {
                // Inställningar för att göra om listan till JSON-text
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true, // Gör filen lätt att läsa 
                    DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                    // Viktigt! Använder vår specialskrivare för Vehicle-klassen
                    Converters = { new VehicleConverter() }
                };

                // Gör om listan av platser till en JSON
                string json = JsonSerializer.Serialize(spots, options);

                // Skriver texten till filen
                File.WriteAllText(dataFile, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Kunde inte spara data: {ex.Message}");
            }
        }

        // Läser in alla sparade parkeringsplatser och fordon från parkingdata.json
        public static List<ParkingSpot> LoadData()
        {
            try
            {
                // Kollar om filen finns
                if (File.Exists(dataFile))
                {
                    // Läser in all text från filen
                    string json = File.ReadAllText(dataFile);

                    // Inställningar för att läsa in JSON-texten
                    var options = new JsonSerializerOptions
                    {
                        // Måste använda specialskrivaren för att få rätt fordonstyp (Bil/MC)
                        Converters = { new VehicleConverter() }
                    };

                    // Gör om JSON-texten till en lista av parkeringsplatser
                    List<ParkingSpot> spots = JsonSerializer.Deserialize<List<ParkingSpot>>(json, options);

                    return spots;
                }
                else
                {
                    Console.WriteLine($"Info: Datafil {dataFile} finns inte än. Skapar ny vid nästa sparning.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Kunde inte läsa in data: {ex.Message}");
            }

            // Om något gick fel eller filen inte hittades, returnera en tom lista
            return new List<ParkingSpot>();
        }

        // Sparar parkeringshusets inställningar till config.json
        public static void SaveConfig(Config config)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true // Gör filen lätt att läsa
                };

                string json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(configFile, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Kunde inte spara konfiguration: {ex.Message}");
            }
        }

        // Läser in parkeringshusets inställningar från config.json
        public static Config LoadConfig()
        {
            try
            {
                // Kollar om inställningsfilen finns
                if (File.Exists(configFile))
                {
                    string json = File.ReadAllText(configFile);
                    // Läser in JSON-texten direkt till Config-objektet
                    Config config = JsonSerializer.Deserialize<Config>(json);

                    return config;
                }
                else
                {
                    // Om filen saknas, skapar vi standardinställningar och sparar dem
                    Config newConfig = new Config();
                    SaveConfig(newConfig);
                    return newConfig;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Kunde inte läsa konfiguration: {ex.Message}");
                Console.WriteLine("Använder standardkonfiguration.");
                return new Config();
            }
        }
    }

    // Den här delen gör att programmet kan läsa in sparade fordon
    // trots att Vehicle är en abstrakt basklass (den MÅSTE ärvas).
    public class VehicleConverter : JsonConverter<Vehicle>
    {
        public override Vehicle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;
                // Kollar fordonets "Size" (Storlek) för att veta vilken typ det är
                if (root.TryGetProperty("Size", out JsonElement sizeElement))
                {
                    int size = sizeElement.GetInt32();

                    // Skapar rätt typ av klass (Bil eller MC) baserat på Storlek
                    Vehicle vehicle;
                    if (size == 4)
                    {
                        vehicle = new Car(); // Storlek 4 = Bil
                    }
                    else if (size == 1)
                    {
                        vehicle = new MC(); // Storlek 1 = Motorcykel
                    }
                    else
                    {
                        // Om storleken är okänd, antar vi att det är en Bil som standard
                        vehicle = new Car();
                    }
                    // Läser in de andra detaljerna (RegNr, CheckInTime) till det nya objektet
                    if (root.TryGetProperty("RegNo", out JsonElement regNoElement))
                    {
                        vehicle.RegNo = regNoElement.GetString();
                    }

                    if (root.TryGetProperty("CheckInTime", out JsonElement checkInElement))
                    {
                        vehicle.CheckInTime = checkInElement.GetDateTime();
                    }

                    return vehicle; // Returnerar den nya, korrekta Bil- eller MC-objektet
                }
            }
            return new Car(); // Fallback om något går fel
        }
        // Skriver ut fordonet till JSON-filen
        public override void Write(Utf8JsonWriter writer, Vehicle value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString("RegNo", value.RegNo);
            writer.WriteNumber("Size", value.Size);
            writer.WriteString("CheckInTime", value.CheckInTime);

            writer.WriteEndObject();
        }
    }
}
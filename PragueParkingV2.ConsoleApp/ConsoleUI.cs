using PragueParkingV2.Core;
using PragueParkingV2.Data;
using Spectre.Console;
using System;
using System.Linq;

namespace PragueParkingV2.ConsoleApp
{
    public class ConsoleUI
    {
        public void Run()
        {
            while (true)
            {
                Console.Clear();
                ShowParkingOverview();
                ShowQuickStats();

                int choice = ShowMainMenu();

                switch (choice)
                {
                    case 1:
                        ParkVehicle();
                        break;
                    case 2:
                        MoveVehicle();
                        break;
                    case 3:
                        RetrieveVehicle();
                        break;
                    case 4:
                        SearchVehicle();
                        break;
                    case 5:
                        ShowAllVehicles();
                        break;
                    case 6:
                        ShowAllSpots(); // ✅ nu implementerad
                        break;
                    case 7:
                        ReloadPrices();
                        break;
                    case 8:
                        ExitProgram();
                        return;
                }
            }
        }

        private int ShowMainMenu()
        {
            var menu = new SelectionPrompt<string>()
                .Title("[yellow]Välj ett alternativ:[/]")
                .AddChoices(
                    "1) Parkera fordon",
                    "2) Flytta fordon",
                    "3) Ta bort fordon (utcheckning)",
                    "4) Sök fordon",
                    "5) Visa alla fordon",
                    "6) Visa alla platser",
                    "7) Ladda om prislista",
                    "8) Avsluta"
                );

            string choice = AnsiConsole.Prompt(menu);
            return int.Parse(choice.Substring(0, 1));
        }

        private void ShowQuickStats()
        {
            var cfg = GarageSettings.Current;

            int total = PHus.GetAllSpots().Count;
            int occupied = PHus.GetAllSpots().Count(s => !s.IsEmpty());
            int vehicles = PHus.GetAllVehicles().Count;

            var panel = new Panel(
                $"[white]Platser:[/] {occupied}/{total}\n" +
                $"[white]Fordon:[/] {vehicles}\n" +
                $"[white]Standard platsstorlek:[/] {cfg.DefaultSpotSize}"
            )
            {
                Header = new PanelHeader("Snabbstatus"),
                Border = BoxBorder.Rounded
            };

            AnsiConsole.Write(panel);
        }

        private void ShowParkingOverview()
        {
            // Färgkodad karta som syns “på avstånd”
            var table = new Table().Border(TableBorder.Rounded);
            for (int c = 0; c < 10; c++)
                table.AddColumn(new TableColumn(""));

            var spots = PHus.GetAllSpots();
            for (int i = 0; i < spots.Count; i += 10)
            {
                var row = spots.Skip(i).Take(10).Select(FormatSpotCell).ToArray();
                table.AddRow(row);
            }

            AnsiConsole.Write(new Panel(table)
            {
                Header = new PanelHeader("Parkeringskarta (grön=tom, gul=delvis, röd=full)"),
                Border = BoxBorder.Double
            });
        }

        private string FormatSpotCell(ParkingSpot spot)
        {
            string label = spot.SpotNumber.ToString("00");

            string color =
                spot.IsEmpty() ? "green" :
                (spot.GetAvailableSpace() == 0 ? "red" : "yellow");

            // Bara nummer + färg = tydligt på håll
            return $"[{color}]{label}[/]";
        }

        private void ParkVehicle()
        {
            var cfg = GarageSettings.Current;

            string regNo = AnsiConsole.Ask<string>("Ange registreringsnummer:").Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(regNo))
            {
                Pause("Ogiltigt registreringsnummer.");
                return;
            }

            // Välj fordonstyp från config
            var choices = cfg.VehicleTypes.Select(v => $"{v.DisplayName} ({v.Key})").ToList();
            string picked = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Välj fordonstyp:")
                    .AddChoices(choices)
            );

            string key = cfg.VehicleTypes.First(v => $"{v.DisplayName} ({v.Key})" == picked).Key;

            Vehicle vehicle = key.Equals("Car", StringComparison.OrdinalIgnoreCase)
                ? new Car(regNo)
                : new MC(regNo);

            if (PHus.ParkVehicle(vehicle))
            {
                FileManager.SaveData(PHus.GetAllSpots().ToList());
                Pause($"✅ Fordonet {regNo} parkerades.");
            }
            else
            {
                Pause($"⛔ Ingen plats tillgänglig för {regNo}.");
            }
        }

        private void MoveVehicle()
        {
            string regNo = AnsiConsole.Ask<string>("Ange registreringsnummer att flytta:").Trim().ToUpperInvariant();
            int currentSpot = PHus.FindVehicle(regNo);

            if (currentSpot == -1)
            {
                Pause("⛔ Fordonet hittades inte.");
                return;
            }

            int targetSpot = AnsiConsole.Ask<int>($"Fordonet står på plats {currentSpot}. Ange ny plats (1-{PHus.GetAllSpots().Count}):");

            if (PHus.MoveVehicle(regNo, targetSpot))
            {
                FileManager.SaveData(PHus.GetAllSpots().ToList());
                Pause($"✅ Fordonet flyttades till plats {targetSpot}.");
            }
            else
            {
                Pause("⛔ Flytten gick inte (platsen är inte kompatibel eller full).");
            }
        }

        private void RetrieveVehicle()
        {
            string regNo = AnsiConsole.Ask<string>("Ange registreringsnummer att ta bort:").Trim().ToUpperInvariant();
            Vehicle? vehicle = PHus.GetVehicle(regNo);

            if (vehicle == null)
            {
                Pause("⛔ Fordonet hittades inte.");
                return;
            }

            decimal fee = vehicle.CalculateFee();

            if (PHus.RemoveVehicle(regNo))
            {
                FileManager.SaveData(PHus.GetAllSpots().ToList());

                var receipt = new Panel(
                    $"Fordon: {vehicle.GetVehicleTypeName()}\n" +
                    $"Regnr: {vehicle.RegNo}\n" +
                    $"Incheckning: {vehicle.CheckInTime}\n" +
                    $"Avgift: {fee} CZK"
                )
                {
                    Header = new PanelHeader("Kvitto"),
                    Border = BoxBorder.Rounded
                };

                AnsiConsole.Write(receipt);
                Pause();
            }
            else
            {
                Pause("⛔ Kunde inte ta bort fordonet.");
            }
        }

        private void SearchVehicle()
        {
            string regNo = AnsiConsole.Ask<string>("Ange registreringsnummer att söka:").Trim().ToUpperInvariant();
            int spot = PHus.FindVehicle(regNo);
            Vehicle? vehicle = PHus.GetVehicle(regNo);

            if (spot == -1 || vehicle == null)
            {
                Pause("⛔ Fordonet hittades inte.");
                return;
            }

            decimal fee = vehicle.CalculateFee();

            var panel = new Panel(
                $"Plats: {spot}\n" +
                $"Typ: {vehicle.GetVehicleTypeName()}\n" +
                $"Incheckning: {vehicle.CheckInTime}\n" +
                $"Avgift just nu: {fee} CZK"
            )
            {
                Header = new PanelHeader("Sökresultat"),
                Border = BoxBorder.Rounded
            };

            AnsiConsole.Write(panel);
            Pause();
        }

        private void ShowAllVehicles()
        {
            var vehicles = PHus.GetAllVehicles();

            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("Regnr");
            table.AddColumn("Typ");
            table.AddColumn("Incheckning");
            table.AddColumn("Avgift nu (CZK)");

            foreach (var v in vehicles)
            {
                table.AddRow(v.RegNo, v.GetVehicleTypeName(), v.CheckInTime.ToString(), v.CalculateFee().ToString());
            }

            AnsiConsole.Write(new Panel(table)
            {
                Header = new PanelHeader("Alla fordon"),
                Border = BoxBorder.Rounded
            });

            Pause();
        }

        private void ShowAllSpots()
        {
            var spots = PHus.GetAllSpots();

            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("Plats");
            table.AddColumn("Kapacitet");
            table.AddColumn("Status");
            table.AddColumn("Fordon (regnr)");

            foreach (var s in spots)
            {
                string regs = s.IsEmpty()
                    ? "-"
                    : string.Join(", ", s.ParkedVehicles.Select(v => v.RegNo));

                table.AddRow(
                    s.SpotNumber.ToString(),
                    s.Capacity.ToString(),
                    s.GetStatusDescription(),
                    regs
                );
            }

            AnsiConsole.Write(new Panel(table)
            {
                Header = new PanelHeader("Alla platser"),
                Border = BoxBorder.Rounded
            });

            Pause();
        }

        private void ReloadPrices()
        {
            PriceList.LoadPrices("PriceList.txt");

            var prices = PriceList.GetAllPrices();

            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("Fordonstyp");
            table.AddColumn("Pris (CZK/h)");

            foreach (var kvp in prices)
                table.AddRow(kvp.Key, kvp.Value.ToString());

            table.AddRow("FreeMinutes", PriceList.FreeMinutes.ToString());

            AnsiConsole.Write(new Panel(table)
            {
                Header = new PanelHeader("Prislista (omladdad)"),
                Border = BoxBorder.Rounded
            });

            Pause();
        }

        private void ExitProgram()
        {
            FileManager.SaveData(PHus.GetAllSpots().ToList());
            Pause("✅ Sparat. Hejdå!");
        }

        private void Pause(string? message = null)
        {
            if (!string.IsNullOrWhiteSpace(message))
                AnsiConsole.MarkupLine(message);

            AnsiConsole.MarkupLine("[grey]Tryck valfri tangent för att fortsätta...[/]");
            Console.ReadKey(true);
        }
    }
}


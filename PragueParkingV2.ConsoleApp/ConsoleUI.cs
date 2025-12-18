using System;
using System.Globalization;
using System.Linq;
using PragueParkingV2.Core;
using PragueParkingV2.Data;
using Spectre.Console;

namespace PragueParkingV2.ConsoleApp
{
    public static class ConsoleUI
    {
        public static void Run()
        {
            while (true)
            {
                AnsiConsole.Clear();
                ShowTitle();
                ShowParkingOverview();
                ShowQuickStats();

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[yellow]Välj ett menyval:[/]")
                        .AddChoices(
                            "1) Parkera fordon",
                            "2) Flytta fordon",
                            "3) Ta bort fordon (utcheckning)",
                            "4) Sök fordon",
                            "5) Visa alla fordon",
                            "6) Visa alla platser",
                            "7) Ladda om priser (PriceList.txt)",
                            "8) Avsluta"));

                switch (choice.Substring(0, 1))
                {
                    case "1": ParkVehicle(); break;
                    case "2": MoveVehicle(); break;
                    case "3": RetrieveVehicle(); break;
                    case "4": SearchVehicle(); break;
                    case "5": ShowAllVehicles(); break;
                    case "6": Pause("[grey]Tryck valfri tangent för att fortsätta...[/]"); break;
                    case "7": ReloadPrices(); break;
                    case "8": return;
                }
            }
        }

        private static void ShowTitle()
        {
            AnsiConsole.Write(new FigletText("Prague Parking").Centered());
            AnsiConsole.WriteLine();
        }

        private static void ShowQuickStats()
        {
            var cfg = GarageSettings.Current;

            var panel = new Panel(
                $"[green]Lediga:[/] {PHus.CountEmptySpots()}    " +
                $"[yellow]Delvis:[/] {PHus.CountPartialSpots()}    " +
                $"[red]Fulla:[/] {PHus.CountFullSpots()}\n" +
                $"[grey]Platser:[/] {PHus.GetAllSpots().Count}    " +
                $"[grey]Gratisminuter:[/] {PriceList.FreeMinutes}    " +
                $"[grey]Default SpotSize:[/] {cfg.DefaultSpotSize}"
            )
            { Header = new PanelHeader("Status") };

            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();
        }

        private static void ShowParkingOverview()
        {
            var spots = PHus.GetAllSpots().OrderBy(s => s.SpotNumber).ToList();

            int columns = 10;
            int rows = (int)Math.Ceiling(spots.Count / (double)columns);

            var table = new Table().Border(TableBorder.Rounded).Title("[bold]Parkeringskarta[/]");

            for (int c = 0; c < columns; c++)
                table.AddColumn(new TableColumn(""));

            for (int r = 0; r < rows; r++)
            {
                var rowCells = new string[columns];

                for (int c = 0; c < columns; c++)
                {
                    int spotIndex = r * columns + c;
                    if (spotIndex >= spots.Count)
                    {
                        rowCells[c] = "";
                        continue;
                    }

                    rowCells[c] = FormatSpotCell(spots[spotIndex]);
                }

                table.AddRow(rowCells);
            }

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }

        private static string FormatSpotCell(ParkingSpot spot)
        {
            string size = $"[grey]({spot.Capacity})[/]";
            string label = $"[bold]{spot.SpotNumber:00}[/] {size}";

            if (spot.ParkedVehicles.Count == 0)
                return $"{label} - [grey]tom[/]";

            string regs = string.Join("|", spot.ParkedVehicles.Select(v => v.RegNo));
            return $"{label} - [white]{regs}[/]";
        }

        private static void ParkVehicle()
        {
            AnsiConsole.Clear();
            ShowTitle();

            string typeChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Vilket fordon vill du parkera?[/]")
                    .AddChoices("Bil", "Motorcykel"));

            string regNo = AskRegNo();

            if (PHus.FindVehicle(regNo) != null)
            {
                AnsiConsole.MarkupLine($"[red]Regnr {regNo} finns redan parkerat.[/]");
                Pause();
                return;
            }

            Vehicle vehicle = typeChoice == "Bil" ? new Car(regNo) : new MC(regNo);

            bool parked = PHus.ParkVehicle(vehicle);

            if (parked)
            {
                int? spotNo = PHus.FindSpotNumber(regNo);
                FileManager.SaveData(PHus.GetAllSpots());
                AnsiConsole.MarkupLine($"[green]Klart![/] {vehicle.GetVehicleTypeName()} {regNo} parkerades på plats [bold]{spotNo}[/].");
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Parkeringen är full eller kan inte ta emot fler av den här typen på en plats.[/]");
            }

            Pause();
        }

        private static void RetrieveVehicle()
        {
            AnsiConsole.Clear();
            ShowTitle();

            string regNo = AskRegNo();

            var vehicle = PHus.RetrieveVehicle(regNo);
            if (vehicle == null)
            {
                AnsiConsole.MarkupLine($"[red]Hittade inget fordon med regnr {regNo}.[/]");
                Pause();
                return;
            }

            decimal fee = vehicle.CalculateFee();
            TimeSpan parkedTime = DateTime.Now - vehicle.CheckInTime;

            FileManager.SaveData(PHus.GetAllSpots());

            var panel = new Panel(
                $"[bold]Regnr:[/] {vehicle.RegNo}\n" +
                $"[bold]Typ:[/] {vehicle.GetVehicleTypeName()}\n" +
                $"[bold]Incheckning:[/] {vehicle.CheckInTime:yyyy-MM-dd HH:mm}\n" +
                $"[bold]Parkeringstid:[/] {FormatDuration(parkedTime)}\n" +
                $"[bold]Avgift:[/] {fee} kr")
            { Header = new PanelHeader("Kvitto") };

            AnsiConsole.Write(panel);
            Pause();
        }

        private static void MoveVehicle()
        {
            AnsiConsole.Clear();
            ShowTitle();

            string regNo = AskRegNo();

            if (PHus.FindVehicle(regNo) == null)
            {
                AnsiConsole.MarkupLine($"[red]Hittade inget fordon med regnr {regNo}.[/]");
                Pause();
                return;
            }

            int maxSpot = PHus.GetAllSpots().Count;

            string input = AnsiConsole.Ask<string>($"Ange [yellow]ny plats[/] (1–{maxSpot}), eller tryck Enter för [grey]auto[/]: ").Trim();

            bool moved;
            if (string.IsNullOrWhiteSpace(input))
            {
                moved = PHus.MoveVehicle(regNo);
            }
            else if (int.TryParse(input, out int target) && target >= 1 && target <= maxSpot)
            {
                moved = PHus.MoveVehicle(regNo, target);
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Ogiltig plats.[/]");
                Pause();
                return;
            }

            FileManager.SaveData(PHus.GetAllSpots());

            int? newSpot = PHus.FindSpotNumber(regNo);

            if (moved)
                AnsiConsole.MarkupLine($"[green]Flyttad![/] {regNo} står nu på plats [bold]{newSpot}[/].");
            else
                AnsiConsole.MarkupLine($"[yellow]Kunde inte flytta till önskad plats.[/] {regNo} står nu på plats [bold]{newSpot}[/].");

            Pause();
        }

        private static void SearchVehicle()
        {
            AnsiConsole.Clear();
            ShowTitle();

            string regNo = AskRegNo();

            var vehicle = PHus.FindVehicle(regNo);
            if (vehicle == null)
            {
                AnsiConsole.MarkupLine($"[red]Hittade inget fordon med regnr {regNo}.[/]");
                Pause();
                return;
            }

            int? spotNo = PHus.FindSpotNumber(regNo);
            TimeSpan parkedTime = DateTime.Now - vehicle.CheckInTime;

            var panel = new Panel(
                $"[bold]Regnr:[/] {vehicle.RegNo}\n" +
                $"[bold]Typ:[/] {vehicle.GetVehicleTypeName()}\n" +
                $"[bold]Plats:[/] {spotNo}\n" +
                $"[bold]Storlek:[/] {vehicle.Size}\n" +
                $"[bold]Incheckning:[/] {vehicle.CheckInTime:yyyy-MM-dd HH:mm}\n" +
                $"[bold]Parkeringstid:[/] {FormatDuration(parkedTime)}\n" +
                $"[bold]Avgift just nu:[/] {vehicle.CalculateFee()} kr")
            { Header = new PanelHeader("Info") };

            AnsiConsole.Write(panel);
            Pause();
        }

        private static void ShowAllVehicles()
        {
            AnsiConsole.Clear();
            ShowTitle();

            var table = new Table().Border(TableBorder.Rounded).Title("[bold]Alla fordon[/]");
            table.AddColumn("Plats");
            table.AddColumn("Typ");
            table.AddColumn("Regnr");
            table.AddColumn("Storlek");
            table.AddColumn("Incheckning");
            table.AddColumn("Tid");
            table.AddColumn("Avgift nu");

            var spots = PHus.GetAllSpots().OrderBy(s => s.SpotNumber);
            int count = 0;

            foreach (var spot in spots)
            {
                foreach (var v in spot.ParkedVehicles.OrderBy(x => x.RegNo))
                {
                    count++;
                    var duration = DateTime.Now - v.CheckInTime;

                    table.AddRow(
                        spot.SpotNumber.ToString(CultureInfo.InvariantCulture),
                        v.GetVehicleTypeName(),
                        v.RegNo,
                        v.Size.ToString(CultureInfo.InvariantCulture),
                        v.CheckInTime.ToString("yyyy-MM-dd HH:mm"),
                        FormatDuration(duration),
                        $"{v.CalculateFee()} kr"
                    );
                }
            }

            if (count == 0)
                AnsiConsole.MarkupLine("[grey]Inga fordon parkerade just nu.[/]");
            else
                AnsiConsole.Write(table);

            Pause();
        }

        private static void ReloadPrices()
        {
            AnsiConsole.Clear();
            ShowTitle();

            PriceList.LoadPrices();

            var table = new Table().Border(TableBorder.Rounded).Title("[bold]Prislista (PriceList.txt)[/]");
            table.AddColumn("Key");
            table.AddColumn("Pris (kr/h)");

            foreach (var kv in PriceList.GetAllPrices().OrderBy(k => k.Key))
                table.AddRow(kv.Key, kv.Value.ToString(CultureInfo.InvariantCulture));

            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"[grey]FreeMinutes:[/] {PriceList.FreeMinutes}");
            Pause();
        }

        private static string AskRegNo()
        {
            while (true)
            {
                string regNo = AnsiConsole.Ask<string>("Ange registreringsnummer (1–10 tecken): ").Trim().ToUpperInvariant();

                if (IsValidRegNo(regNo))
                    return regNo;

                AnsiConsole.MarkupLine("[red]Ogiltigt regnr.[/] (Inga mellanslag, max 10 tecken, ej # eller |)");
            }
        }

        private static bool IsValidRegNo(string regNo)
        {
            if (string.IsNullOrWhiteSpace(regNo)) return false;
            if (regNo.Length > 10) return false;
            if (regNo.Any(char.IsWhiteSpace)) return false;
            if (regNo.Contains('#') || regNo.Contains('|')) return false;
            return true;
        }

        private static string FormatDuration(TimeSpan ts)
            => $"{ts.Days}d {ts.Hours}h {ts.Minutes:00}m";

        private static void Pause(string message = "[grey]Tryck valfri tangent för att fortsätta...[/]")
        {
            AnsiConsole.MarkupLine(message);
            Console.ReadKey(true);
        }
    }
}


using PragueParkingV2.Core;
using PragueParkingV2.Data;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PragueParkingV2.ConsoleApp
{
    public class ConsoleUI
    {
        // HJÄLPMETODER FÖR UI
        // Rensar skärmen och skriver ut en snygg rubrik med ramar
        private static void ShowTitle(string text)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(
                new Panel($"[bold cyan]{text}[/]")
                    .BorderColor(Color.Cyan1)
                    .Padding(1, 0)
            );
            AnsiConsole.WriteLine();
        }
        // PARKERINGSÖVERSIKT
        // Visar en grafisk översikt över parkeringshuset
        private static void ShowParkingOverview()
        {
            var spots = PHus.GetAllSpots();
            var table = new Table();
            table.Border(TableBorder.Rounded);
            table.Title("[cyan]PARKERINGSKARTA[/]");
            table.AddColumn(new TableColumn("[yellow]Rad[/]").Centered());

            // Skapar 10 kolumner för rutorna 1-10
            for (int i = 1; i <= 10; i++)
            {
                table.AddColumn(new TableColumn($"[yellow]{i}[/]").Centered());
            }

            List<string> row = new List<string>();
            int count = 0;
            int rowNumber = 1;

            foreach (var spot in spots)
            {
                string display;

                // Väljer färg baserat på hur full platsen är
                if (spot.IsEmpty())
                {
                    display = $"[green]███[/]"; // Grön = helt tom
                }
                else if (spot.IsFull())
                {
                    display = $"[red]███[/]"; // Röd = helt full
                }
                else
                {
                    display = $"[yellow]▒▒▒[/]"; // Gul = halvfull (plats kvar)
                }

                // Visar platsnumret i mindre text under färgblocket
                display += $"\n[dim]{spot.SpotNumber}[/]";

                row.Add(display);
                count++;

                // När 10 platser är insamlade, lägg till som en ny rad i tabellen
                if (count == 10)
                {
                    // Lägg till radnummer (1, 2, 3...) först
                    row.Insert(0, $"[cyan]{rowNumber}[/]");
                    table.AddRow(row.ToArray());
                    row.Clear();
                    count = 0;
                    rowNumber++;
                }
            }

            // Lägger till sista ofullständiga raden om det behövs
            if (row.Count > 0)
            {
                while (row.Count < 10)
                {
                    row.Add("");
                }
                row.Insert(0, $"[cyan]{rowNumber}[/]");
                table.AddRow(row.ToArray());
            }

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();

            // Förklaring av färgkoderna
            var legend = new Panel(
                "[green]███ Ledig[/] | [yellow]▒▒▒ Halvfull[/] | [red]███ Full[/]"
            );
            legend.Header = new PanelHeader("[yellow]Förklaring[/]");
            legend.Border = BoxBorder.Rounded;
            AnsiConsole.Write(legend);

            // Visar statistik för antalet lediga/fulla platser
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[yellow]Tomma platser:[/] {PHus.CountEmptySpots()} st");
            AnsiConsole.MarkupLine($"[yellow]Halvfulla platser:[/] {PHus.CountPartialSpots()} st");
            AnsiConsole.MarkupLine($"[yellow]Fulla platser:[/] {PHus.CountFullSpots()} st");
        }
        // HUVUDMENY
        // Startar huvudloopen för applikationen
        public static void Run()
        {
            bool running = true;

            while (running)
            {
                ShowTitle("PRAGUE PARKING V2 - HUVUDMENY");

                // Visa en statusruta med aktuell beläggning
                var statusPanel = new Panel(
                    $"[yellow]Lediga enheter:[/] {PHus.GetAvailableSpace()}\n" +
                    $"[yellow]Tomma platser:[/] {PHus.CountEmptySpots()}\n" +
                    $"[yellow]Antal fordon:[/] {PHus.GetAllVehicles().Count}"
                );
                statusPanel.Header = new PanelHeader("[cyan]Status[/]");
                statusPanel.Border = BoxBorder.Rounded;
                AnsiConsole.Write(statusPanel);
                AnsiConsole.WriteLine();

                // Låter användaren välja i menyn
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[cyan]Vad vill du göra?[/]")
                        .PageSize(10)
                        .AddChoices(new[]
                        {
                            "1. Parkera fordon",
                            "2. Hämta fordon",
                            "3. Flytta fordon",
                            "4. Sök fordon",
                            "5. Visa alla fordon",
                            "6. Visa parkeringsöversikt",
                            "7. Visa prislista för parking",
                            "8. Avsluta"
                        }));

                // Plockar ut första siffran i valet ("1", "2", etc.)
                string selected = choice.Substring(0, 1);

                // Hanterar valet och kör rätt funktion
                switch (selected)
                {
                    case "1":
                        ParkVehicle();
                        break;
                    case "2":
                        RetrieveVehicle();
                        break;
                    case "3":
                        MoveVehicle();
                        break;
                    case "4":
                        SearchVehicle();
                        break;
                    case "5":
                        ShowAllVehicles();
                        break;
                    case "6":
                        ShowTitle("PARKERINGSÖVERSIKT");
                        ShowParkingOverview();
                        AnsiConsole.MarkupLine("\n[grey]Tryck på valfri tangent för att fortsätta...[/]");
                        Console.ReadKey(); // Pausar tills användaren trycker på tangent
                        break;
                    case "7":
                        // Kör funktionen för att visa prislistan
                        ReloadPrices();
                        break;
                    case "8":
                        // Avslutar loopen och programmet
                        running = false;
                        AnsiConsole.MarkupLine("\n[green]Tack och hej![/]");
                        break;
                }
            }
        }
        // PARKERA FORDON
        // Funktion för att checka in ett nytt fordon
        private static void ParkVehicle()
        {
            ShowTitle("PARKERA FORDON");

            // Låter användaren välja om det är Bil eller MC
            var typeChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Välj fordonstyp:")
                    .AddChoices(new[] { "Bil", "MC", "Avbryt" }));

            if (typeChoice == "Avbryt")
                return;

            // Frågar efter registreringsnumret
            string regNo = AnsiConsole.Ask<string>("Ange registreringsnummer:");

            // Kollar om fordonet redan finns i garaget
            var existing = PHus.FindVehicle(regNo);
            if (existing != null)
            {
                AnsiConsole.MarkupLine($"\n[red]Ett fordon med registreringsnummer {regNo.ToUpper()} är redan parkerat![/]");
                var spotNum = PHus.FindSpotNumber(regNo);
                AnsiConsole.MarkupLine($"[yellow]Det står på plats {spotNum}[/]");
                AnsiConsole.MarkupLine("\n[grey]Tryck på valfri tangent för att fortsätta...[/]");
                Console.ReadKey();
                return;
            }

            // Skapar rätt typ av fordon (Car eller MC)
            Vehicle vehicle;
            if (typeChoice == "Bil")
            {
                vehicle = new Car(regNo);
            }
            else
            {
                vehicle = new MC(regNo);
            }

            // Visar en liten animation (spinner) medan systemet letar
            AnsiConsole.Status()
                .Start("Letar efter ledig plats...", ctx =>
                {
                    // Litet delay för att spinnern ska synas
                    Thread.Sleep(500);
                });

            // Försöker parkera fordonet via PHus-klassen
            if (PHus.ParkVehicle(vehicle))
            {
                var spotNum = PHus.FindSpotNumber(regNo);

                // Grattis, parkering lyckades!
                AnsiConsole.MarkupLine($"\n[green]✓ Fordonet {regNo.ToUpper()} har parkerats![/]");
                AnsiConsole.MarkupLine($"[yellow]Plats:[/] {spotNum}");
                AnsiConsole.MarkupLine($"[yellow]Typ:[/] {vehicle.GetVehicleTypeName()}");
                AnsiConsole.MarkupLine($"[yellow]Incheckning:[/] {vehicle.CheckInTime:yyyy-MM-dd HH:mm:ss}");

                // Sparar datan direkt i bakgrunden
                FileManager.SaveData(PHus.GetAllSpots());
            }
            else
            {
                // Om ParkVehicle returnerar false
                AnsiConsole.MarkupLine("\n[red]✗ Det finns ingen ledig plats![/]");
            }

            AnsiConsole.MarkupLine("\n[grey]Tryck på valfri tangent för att fortsätta...[/]");
            Console.ReadKey();
        }
        // HÄMTA FORDON
        // Funktion för att checka ut ett fordon och visa kvitto
        private static void RetrieveVehicle()
        {
            ShowTitle("HÄMTA FORDON");

            string regNo = AnsiConsole.Ask<string>("Ange registreringsnummer:");

            // Försöker hämta ut fordonet från PHus
            Vehicle vehicle = PHus.RetrieveVehicle(regNo);

            if (vehicle != null)
            {
                // Räknar ut hur länge fordonet stått och vad det kostar
                TimeSpan duration = vehicle.GetParkingDuration();
                decimal fee = vehicle.CalculateFee();

                // Bygger upp kvittot i en snygg ram
                var receipt = new Panel(
                    $"[yellow]Registreringsnummer:[/] {vehicle.RegNo}\n" +
                    $"[yellow]Fordonstyp:[/] {vehicle.GetVehicleTypeName()}\n" +
                    $"[yellow]Incheckning:[/] {vehicle.CheckInTime:yyyy-MM-dd HH:mm:ss}\n" +
                    $"[yellow]Utcheckning:[/] {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                    $"[yellow]Parkerad tid:[/] {duration.Days}d {duration.Hours}h {duration.Minutes}m\n" +
                    $"[yellow]Timpris:[/] {vehicle.GetHourlyRate()} CZK/h\n" +
                    $"[cyan]══════════════════════════[/]\n" +
                    $"[green bold]TOTALKOSTNAD: {fee} CZK[/]"
                );
                receipt.Header = new PanelHeader("[green]✓ KVITTO[/]");
                receipt.Border = BoxBorder.Double;
                AnsiConsole.Write(receipt);

                // Visar ett extra meddelande om priset var 0
                if (fee == 0)
                {
                    AnsiConsole.MarkupLine("\n[green]★ Gratis parkering! ★[/]");
                }

                // Sparar datan direkt i bakgrunden
                FileManager.SaveData(PHus.GetAllSpots());
            }
            else
            {
                // Hittade inte fordonet
                AnsiConsole.MarkupLine($"\n[red]✗ Hittade inget fordon med registreringsnummer {regNo.ToUpper()}[/]");
            }

            AnsiConsole.MarkupLine("\n[grey]Tryck på valfri tangent för att fortsätta...[/]");
            Console.ReadKey();
        }

        // FLYTTA FORDON
        // Funktion för att flytta ett fordon till en ny plats (automatisk optimering)
        private static void MoveVehicle()
        {
            ShowTitle("FLYTTA FORDON");

            string regNo = AnsiConsole.Ask<string>("Ange registreringsnummer:");

            // Kollar var fordonet står just nu
            var oldSpot = PHus.FindSpotNumber(regNo);

            // Försöker flytta fordonet. (Flytt-funktionen i PHus kollar om det finns plats)
            if (PHus.MoveVehicle(regNo))
            {
                var newSpot = PHus.FindSpotNumber(regNo);

                // Flytt lyckades
                AnsiConsole.MarkupLine($"\n[green]✓ Fordonet {regNo.ToUpper()} har flyttats![/]");
                AnsiConsole.MarkupLine($"[yellow]Från plats:[/] {oldSpot}");
                AnsiConsole.MarkupLine($"[yellow]Till plats:[/] {newSpot}");

                // Sparar datan direkt i bakgrunden
                FileManager.SaveData(PHus.GetAllSpots());
            }
            else
            {
                // Hittade inte fordonet, eller kunde inte hitta ny plats
                AnsiConsole.MarkupLine($"\n[red]✗ Hittade inget fordon med registreringsnummer {regNo.ToUpper()}[/]");
            }

            AnsiConsole.MarkupLine("\n[grey]Tryck på valfri tangent för att fortsätta...[/]");
            Console.ReadKey();
        }
        // SÖK FORDON
        // Funktion för att hitta och visa information om ett parkerat fordon
        private static void SearchVehicle()
        {
            ShowTitle("SÖK FORDON");

            string regNo = AnsiConsole.Ask<string>("Ange registreringsnummer:");

            // Försöker hitta fordonet
            Vehicle vehicle = PHus.FindVehicle(regNo);

            if (vehicle != null)
            {
                // Beräknar position, tid och kostnad för visning
                var spotNum = PHus.FindSpotNumber(regNo);
                TimeSpan duration = vehicle.GetParkingDuration();
                decimal currentFee = vehicle.CalculateFee();

                // Visar all information i en ruta
                var info = new Panel(
                    $"[yellow]Registreringsnummer:[/] {vehicle.RegNo}\n" +
                    $"[yellow]Fordonstyp:[/] {vehicle.GetVehicleTypeName()}\

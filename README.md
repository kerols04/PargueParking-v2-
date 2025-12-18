# Prague Parking V2

Konsolapp (Spectre.Console) för att hantera parkering.

## Meny
1) Parkera fordon  
2) Flytta fordon  
3) Ta bort fordon (utcheckning)  
4) Sök fordon  
5) Visa alla fordon  
6) Visa alla platser  
7) Ladda om priser (PriceList.txt)  
8) Avsluta  

## Filer
- `config.json` – inställningar (antal platser, platsstorlekar, max per plats, fordonstyper)
- `PriceList.txt` – priser (kommentarer efter `#` stöds)
- `parkingdata.json` – skapas i output-mappen och uppdateras vid parkera/flytta/utcheckning

## Köra
Sätt `PragueParkingV2.ConsoleApp` som Startup Project och kör (F5).

## Tester
`PragueParkingV2.Tests` innehåller MSTest-tester.

Loggbok

Den här loggboken beskriver mitt arbete med att bygga parkeringsapplikationen Prague Parking V2. Jag har lagt mycket fokus på att lära mig C# klasser, hur man skapar olika filer och hur man utvecklar en användarvänlig applikation. 
.
11 November
Vad jag gjort: Först började jag med att läsa igenom beskrivning i inlämningsuppgiften för att enklare veta vad som förväntas från mig. Därefter planerade jag projektstrukturen med tre olika projekt (Core, Data och ConsoleApp).
Jag skapade dessa tre tomma projekt och lade till referenser mellan dem så att de kunde prata med varandra.

12 November
Vad jag gjort: Jag skapade grundläggande klasstrukturer.  Jag bestämde mig att använda arv att skapa en basklass som heter Vehicle (Fordon) och sedan bygga på den med subklasser CAR (Bil) och MC (Motorcykel). 
Jag skapade också klasser för själva parkeringen, ParkingSpot (alltså parkeringsplats) och PHus för hela parkering huset och implementerade dessa klasser.  Jag fyllde i de grundläggande egenskaperna i varje 
klass såsom registreringsnummer i Vehcle och storlek i Car & MC. Sedan fortsätt jag med projektet genom att börja med den grundläggande parkerings logiken.

Jag gjorde Vehicle klassen så att jag inte kan skapa ett fordon direkt från den, utan jag måste i stället alltid välja en mer specifik typ såsom Bil eller Motorcykel. 
Detta var särskilt viktigt eftersom jag inte ville ha ett ospecificerat fordon i parkeringshuset. 

13 November
Vad jag gjort: Jag arbetade med JSON filhantering. Detta var en stor utmaning eftersom jag behövde kunna spara alla fordon och parkeringsplatser i en fil och sedan läsa in dem exakt som de var.
Jag började implementera klassen FilManager och la till funktioner för att spara och läsa in data. Jag insåg snabbt att JSON hanteringen krävde en speciell lösning för att hantera arvet mellan Fordon och Bil/MC.

14 November
Vad jag gjort: Skapade Config.json för inställningar som antal platser och platsstorlek och skapade också PriceList.txt för priserna för parkering. Jag implementerade PriceList klassen för att kunna läsa in 
priserna från textfilen. FileManager klassen implementerades också med en anpassad logik för att se till att Bil och MC objekt läses in korrekt från JSON filen. 

15 November
Vad jag gjort: Implementerade’’Hämta Fordon’’ och ’’Parkera Fordon’’ funktionen. Såg till att fordon alltid checkas in med aktuell tid. Lade till tidsberäkning och prisberäkning i Vehicle klassen, vilket 
funkade oerhört bra eftersom logiken då gäller alla fordonstyper. Slutligen testade jag att hämta ett fordon för att se att kvittot med totala kostnad och parkerad tid visades korrekt. 



16 November
Vad jag gjort: Implementerade ’’Flytta Fordon’’ funktionen för att flytta fordon i parkeringshuset. Lade till översikt av alla parkerade fordon. Slutligen försökte implementera sökfunktion för att hitta fordon.
Stötte på problem på grund av att sökfunktionen inte hittade fordon ibland. Löste detta genom at använda ToUpper() funktionen, på så sätt spelade det ingen roll om användaren skrev in registreringsnumret med små 
eller stora bokstäver när de sökte.

17 November
Vad jag gjort: Jag började med att optimera MC-parkering, insåg snabbt att varje MC fick egen plats vilket slösade med utrymmet i P-huset. Började skapa en metod som först letar efter parkeringsplatser med en MC
för att para ihop två MC. Ifall den sedan inte lyckas hitta en ensam MC så letar det efter nästa tomma plats. På så sätt finns det fler parkeringsplatser för fordon i P-huset.

19 November
Vad jag gjort: Jag installerade och integrerade Spectre.Console. Arbetade på menyn och lade till färger och tabeller så att användargränssnittet ser fint ut. La mycket tid på detta för att förbättra gränssnittet 
och tittade på hel del YT videos samt andra i klassens gränssnitt. Fick inspiration från bland annat Susanne, Hannah och Kristebel som skapade jättefina användargränssnitt.

21 November
Vad jag gjort: Sista steget i projektet handlade om att städa bort all kod som var onödig eller gjorde koden svårare att läsa. Koden blev på så vis enklare att förstå och renare, precis som jag ursprungligen ville.
Tog bort mycket av mina kommentarer och döpte om variabler till tydligare namn. Jag stötte på några problem med Flytta Fordon funktionen. Buggen var att systemet kunde försöka flytta ett fordon till en plats som 
var för liten eller redan full. Lösningen blev att lägga till en extra kontroll, efter detta säkerställde jag att allt fungerar stabilt genom att köra alla funktioner.






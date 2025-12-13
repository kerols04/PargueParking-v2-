using Microsoft.VisualStudio.TestTools.UnitTesting;
using PragueParkingV2.Core;
using System;

namespace PragueParkingV2.Tests
{
    // Den här klassen innehåller alla tester för fordonen
    [TestClass]
    public class VehicleTests
    {
        // Körs innan varje test startar
        [TestInitialize]
        public void Setup()
        {
            // Laddar prislistan så att testerna vet vad priset är
            PriceList.LoadPrices();
        }

        // Testar att man inte kan skapa ett Vehicle-objekt direkt
        [TestMethod]
        public void Vehicle_CannotBeInstantiated_BecauseAbstract()
        {
            // Kollar om Vehicle-klassen faktiskt är abstract, alltså måste ärvas
            Type vehicleType = typeof(Vehicle);
            Assert.IsTrue(vehicleType.IsAbstract, "Vehicle-klassen ska vara abstract");
        }

        // Testar att avgiftsberäkningen fungerar korrekt
        [TestMethod]
        public void CalculateFee_OverTenMinutes_ChargesForFullHour()
        {
            // Gör klart för test
            var car = new Car("CAR123");
            // Ställer in tiden till 11 minuter sedan (ska kosta en hel timme)
            car.CheckInTime = DateTime.Now.AddMinutes(-11);

            // Kör funktionen
            decimal fee = car.CalculateFee();

            // Kollar resultatet
            // Priset ska vara 20 kr för 1 timme
            Assert.AreEqual(20, fee, "11 minuter ska räknas som 1 timme för en bil");
        }
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PragueParkingV2.Core;

namespace PragueParkingV2.Tests
{
    [TestClass]
    public class VehicleTests
    {
        [TestInitialize]
        public void Setup()
        {
            // Gör tester oberoende av externa filer
            GarageSettings.Apply(new Config());
            PriceList.LoadPrices("does_not_exist.txt"); // -> defaultpriser
        }

        [TestMethod]
        public void Car_Fee_For_2_Hours_Should_Be_40()
        {
            var car = new Car("ABC123");
            car.CheckInTime = System.DateTime.Now.AddHours(-2);

            Assert.AreEqual(40m, car.CalculateFee());
        }

        [TestMethod]
        public void MC_Fee_After_FreeMinutes_Should_Be_10()
        {
            var mc = new MC("MC1111");
            mc.CheckInTime = System.DateTime.Now.AddMinutes(-(PriceList.FreeMinutes + 1));

            Assert.AreEqual(10m, mc.CalculateFee());
        }
    }
}

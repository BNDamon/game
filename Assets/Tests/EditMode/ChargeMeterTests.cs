using BlockMerge.Core;
using NUnit.Framework;

namespace BlockMerge.Core.Tests
{
    public class ChargeMeterTests
    {
        [Test]
        public void Add_AccumulatesValue()
        {
            var meter = new ChargeMeter();
            meter.Add(30);
            meter.Add(20);

            Assert.AreEqual(50, meter.Value);
            Assert.IsFalse(meter.IsFull);
        }

        [Test]
        public void Add_ClampsAtMax()
        {
            var meter = new ChargeMeter();
            meter.Add(90);
            meter.Add(90);

            Assert.AreEqual(ChargeMeter.Max, meter.Value);
            Assert.IsTrue(meter.IsFull);
        }

        [Test]
        public void Reset_ZeroesValue()
        {
            var meter = new ChargeMeter();
            meter.Add(100);

            meter.Reset();

            Assert.AreEqual(0, meter.Value);
            Assert.IsFalse(meter.IsFull);
        }
    }
}

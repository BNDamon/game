using BlockMerge.Core;
using NUnit.Framework;

namespace BlockMerge.Core.Tests
{
    public class ComboTrackerTests
    {
        [Test]
        public void RegisterClear_FirstClear_StartsAtComboOne()
        {
            var combo = new ComboTracker();

            int count = combo.RegisterClear();

            Assert.AreEqual(1, count);
            Assert.AreEqual(1f, combo.CurrentMultiplier);
        }

        [Test]
        public void RegisterClear_WithinWindow_IncreasesComboAndMultiplier()
        {
            var combo = new ComboTracker();

            combo.RegisterClear();
            combo.Tick(1f);
            int second = combo.RegisterClear();
            combo.Tick(1f);
            int third = combo.RegisterClear();

            Assert.AreEqual(2, second);
            Assert.AreEqual(3, third);
            Assert.Greater(combo.CurrentMultiplier, 1f);
        }

        [Test]
        public void Tick_PastWindow_ResetsComboToZero()
        {
            var combo = new ComboTracker();
            combo.RegisterClear();

            combo.Tick(ComboTracker.ComboWindowSeconds + 0.1f);

            Assert.AreEqual(0, combo.ComboCount);
        }

        [Test]
        public void RegisterClear_AfterWindowExpired_RestartsAtComboOne()
        {
            var combo = new ComboTracker();
            combo.RegisterClear();
            combo.Tick(ComboTracker.ComboWindowSeconds + 0.1f);

            int count = combo.RegisterClear();

            Assert.AreEqual(1, count);
        }

        [Test]
        public void CurrentMultiplier_NeverExceedsCap()
        {
            var combo = new ComboTracker();
            for (int i = 0; i < 50; i++)
            {
                combo.RegisterClear();
                combo.Tick(0.1f);
            }

            Assert.LessOrEqual(combo.CurrentMultiplier, 3f);
        }

        [Test]
        public void Reset_ClearsComboAndMultiplier()
        {
            var combo = new ComboTracker();
            combo.RegisterClear();
            combo.Tick(1f);
            combo.RegisterClear();

            combo.Reset();

            Assert.AreEqual(0, combo.ComboCount);
            Assert.AreEqual(1f, combo.CurrentMultiplier);
        }
    }
}

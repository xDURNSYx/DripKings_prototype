#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEngine;
using DripKings.MVP;

namespace DripKings.Tests
{
    public sealed class CheckpointRaceTests
    {
        private GameObject root;
        private CheckpointRace race;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("Race test");
            race = root.AddComponent<CheckpointRace>();
            race.ResetRace();
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(root);

        [Test]
        public void CheckpointsMustBeVisitedInOrder()
        {
            race.ReachCheckpoint(2);
            Assert.AreEqual(0, race.NextCheckpoint);
            race.ReachCheckpoint(0);
            Assert.AreEqual(1, race.NextCheckpoint);
            race.ReachCheckpoint(3);
            Assert.AreEqual(1, race.NextCheckpoint);
        }

        [Test]
        public void FourGatesFinishOneLapOnlyOnce()
        {
            for (int i = 0; i < 4; i++) race.ReachCheckpoint(i);
            Assert.IsTrue(race.Finished);
            Assert.AreEqual(1, race.CompletedLaps);
            for (int i = 0; i < 4; i++) race.ReachCheckpoint(i);
            Assert.AreEqual(1, race.CompletedLaps);
        }

        [Test]
        public void ResetClearsProgressAndFinish()
        {
            for (int i = 0; i < 4; i++) race.ReachCheckpoint(i);
            race.ResetRace();
            Assert.IsFalse(race.Finished);
            Assert.AreEqual(0, race.NextCheckpoint);
            Assert.AreEqual(0, race.CompletedLaps);
        }
    }
}
#endif

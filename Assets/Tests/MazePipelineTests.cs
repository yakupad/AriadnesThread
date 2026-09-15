using AriadnesThread.Core.Generation;
using NUnit.Framework;

namespace AriadnesThread.Core.Tests
{
    public class MazePipelineTests
    {
        // The core promise of the whole procedural pipeline: every generated level must be
        // solvable, across a wide range of seeds and with every optional system turned on.
        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void Generate_AcrossManySeeds_AlwaysProducesASolvableLevel(bool includeGuard, bool includeKeyLock)
        {
            var parameters = new LevelParameters
            {
                Width = 12,
                Height = 12,
                IncludeGuard = includeGuard,
                IncludeKeyLock = includeKeyLock,
            };

            for (int seed = 0; seed < 50; seed++)
            {
                var level = MazePipeline.Generate(seed, parameters);

                Assert.That(
                    SolvabilityValidator.IsSolvable(level.Grid, level.Start, level.Exit, level.KeyLock),
                    Is.True,
                    $"Level generated from seed {seed} (guard={includeGuard}, keyLock={includeKeyLock}) is not solvable.");
            }
        }

        [Test]
        public void Generate_WithGuard_ProducesANonTrivialPatrolLoop()
        {
            var parameters = new LevelParameters { Width = 14, Height = 14, IncludeGuard = true, MinPatrolLoopLength = 6 };
            var level = MazePipeline.Generate(seed: 100, parameters);

            Assert.That(level.Patrol, Is.Not.Null);
            Assert.That(level.Patrol.Cells.Count, Is.GreaterThanOrEqualTo(parameters.MinPatrolLoopLength));
            Assert.That(level.GuardBuffer.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Generate_WithKeyLock_AttachmentComesBeforeDoorOnMainPath()
        {
            var parameters = new LevelParameters { Width = 14, Height = 14, IncludeGuard = false, IncludeKeyLock = true, MinKeyLockDistance = 4 };
            var level = MazePipeline.Generate(seed: 200, parameters);

            Assert.That(level.KeyLock, Is.Not.Null);

            int doorIndex = -1;
            for (int i = 0; i < level.Analysis.MainPath.Count; i++)
                if (level.Analysis.MainPath[i].Equals(level.KeyLock.LockedDoorFrom))
                    doorIndex = i;

            Assert.That(doorIndex, Is.GreaterThanOrEqualTo(0), "Locked door should sit on the main path.");
        }
    }
}

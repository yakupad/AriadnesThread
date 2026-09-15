using System;
using AriadnesThread.Core.Audio;
using NUnit.Framework;

namespace AriadnesThread.Core.Tests
{
    public class ToneGeneratorTests
    {
        [Test]
        public void Generate_ProducesExpectedSampleCount()
        {
            var samples = ToneGenerator.Generate(440f, 0.1f);
            Assert.That(samples.Length, Is.EqualTo((int)(ToneGenerator.SampleRate * 0.1f)));
        }

        [Test]
        public void Generate_StartsAndEndsAtSilence_ForClickFreePlayback()
        {
            var samples = ToneGenerator.Generate(440f, 0.2f, attackSeconds: 0.01f, decaySeconds: 0.01f);
            Assert.That(samples[0], Is.EqualTo(0f));
            Assert.That(samples[samples.Length - 1], Is.EqualTo(0f));
        }

        [Test]
        public void Generate_NeverExceedsRequestedAmplitude()
        {
            var samples = ToneGenerator.Generate(880f, 0.3f, amplitude: 0.4f);
            foreach (var s in samples)
                Assert.That(Math.Abs(s), Is.LessThanOrEqualTo(0.4f + 0.0001f));
        }

        [Test]
        public void GenerateSweep_ProducesExpectedSampleCount()
        {
            var samples = ToneGenerator.GenerateSweep(200f, 800f, 0.15f);
            Assert.That(samples.Length, Is.EqualTo((int)(ToneGenerator.SampleRate * 0.15f)));
        }

        [Test]
        public void GenerateSweep_StartsAndEndsAtSilence()
        {
            var samples = ToneGenerator.GenerateSweep(200f, 800f, 0.15f, attackSeconds: 0.01f, decaySeconds: 0.01f);
            Assert.That(samples[0], Is.EqualTo(0f));
            Assert.That(samples[samples.Length - 1], Is.EqualTo(0f));
        }

        [Test]
        public void DifferentShapes_ProduceDifferentWaveforms()
        {
            var sine = ToneGenerator.Generate(440f, 0.05f, WaveShape.Sine, attackSeconds: 0, decaySeconds: 0);
            var square = ToneGenerator.Generate(440f, 0.05f, WaveShape.Square, attackSeconds: 0, decaySeconds: 0);
            Assert.That(sine, Is.Not.EqualTo(square));
        }

        [Test]
        public void ZeroDuration_ProducesEmptyBuffer_NotAnException()
        {
            var samples = ToneGenerator.Generate(440f, 0f);
            Assert.That(samples.Length, Is.EqualTo(0));
        }
    }
}

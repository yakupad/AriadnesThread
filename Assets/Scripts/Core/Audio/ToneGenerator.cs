using System;

namespace AriadnesThread.Core.Audio
{
    /// <summary>
    /// Synthesizes short sample buffers at runtime — the prototype has no audio assets, so
    /// every SFX cue is generated math rather than authored. Engine-agnostic on purpose: the
    /// Unity side just wraps the returned float[] in an AudioClip.
    /// </summary>
    public static class ToneGenerator
    {
        public const int SampleRate = 44100;

        public static float[] Generate(float frequencyHz, float durationSeconds, WaveShape shape = WaveShape.Sine,
            float amplitude = 0.5f, float attackSeconds = 0.005f, float decaySeconds = 0.05f)
        {
            int sampleCount = SecondsToSamples(durationSeconds);
            var samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                double phase = (double)frequencyHz * i / SampleRate;
                samples[i] = Waveform(shape, phase) * amplitude;
            }

            ApplyEnvelope(samples, SecondsToSamples(attackSeconds), SecondsToSamples(decaySeconds));
            return samples;
        }

        /// <summary>A linear frequency sweep — for whoosh/toggle cues.</summary>
        public static float[] GenerateSweep(float startFrequencyHz, float endFrequencyHz, float durationSeconds,
            WaveShape shape = WaveShape.Sine, float amplitude = 0.5f, float attackSeconds = 0.005f, float decaySeconds = 0.02f)
        {
            int sampleCount = SecondsToSamples(durationSeconds);
            var samples = new float[sampleCount];
            double phase = 0;

            for (int i = 0; i < sampleCount; i++)
            {
                double tNorm = sampleCount <= 1 ? 0 : (double)i / (sampleCount - 1);
                double freq = startFrequencyHz + (endFrequencyHz - startFrequencyHz) * tNorm;
                phase += freq / SampleRate;
                samples[i] = Waveform(shape, phase) * amplitude;
            }

            ApplyEnvelope(samples, SecondsToSamples(attackSeconds), SecondsToSamples(decaySeconds));
            return samples;
        }

        private static float Waveform(WaveShape shape, double phase)
        {
            double cycles = phase - Math.Floor(phase); // wrap into [0, 1) — keeps large sweeps numerically stable
            double angle = cycles * 2 * Math.PI;

            return shape switch
            {
                WaveShape.Sine => (float)Math.Sin(angle),
                WaveShape.Square => Math.Sin(angle) >= 0 ? 1f : -1f,
                WaveShape.Triangle => (float)(2.0 / Math.PI * Math.Asin(Math.Sin(angle))),
                _ => 0f
            };
        }

        private static void ApplyEnvelope(float[] samples, int attackSamples, int decaySamples)
        {
            int n = samples.Length;
            attackSamples = Math.Min(attackSamples, n);
            decaySamples = Math.Min(decaySamples, n);

            for (int i = 0; i < attackSamples; i++)
                samples[i] *= (float)i / attackSamples;

            for (int i = 0; i < decaySamples; i++)
                samples[n - 1 - i] *= (float)i / decaySamples;
        }

        private static int SecondsToSamples(float seconds) => Math.Max(0, (int)(SampleRate * seconds));
    }
}

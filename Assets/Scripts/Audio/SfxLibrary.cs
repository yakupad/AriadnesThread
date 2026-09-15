using System;
using AriadnesThread.Core.Audio;
using UnityEngine;

namespace AriadnesThread.Audio
{
    /// <summary>Builds and caches short synthesized AudioClips — the prototype has no audio assets.</summary>
    public static class SfxLibrary
    {
        private static AudioClip _step, _markerPlace, _markerRetrieve, _echoPing, _torchOn, _torchOff, _guardSpotted, _caught, _win, _drone;

        public static AudioClip Step => _step ??=
            Build(ToneGenerator.Generate(220f, 0.04f, WaveShape.Sine, 0.15f, 0.002f, 0.03f));

        public static AudioClip MarkerPlace => _markerPlace ??=
            Build(ToneGenerator.Generate(660f, 0.06f, WaveShape.Triangle, 0.3f, 0.002f, 0.05f));

        public static AudioClip MarkerRetrieve => _markerRetrieve ??=
            Build(ToneGenerator.GenerateSweep(500f, 800f, 0.08f, WaveShape.Triangle, 0.3f));

        public static AudioClip EchoPing => _echoPing ??=
            Build(ToneGenerator.GenerateSweep(1200f, 400f, 0.25f, WaveShape.Sine, 0.35f, 0.005f, 0.2f));

        public static AudioClip TorchOn => _torchOn ??=
            Build(ToneGenerator.GenerateSweep(300f, 700f, 0.12f, WaveShape.Sine, 0.3f));

        public static AudioClip TorchOff => _torchOff ??=
            Build(ToneGenerator.GenerateSweep(700f, 250f, 0.12f, WaveShape.Sine, 0.3f));

        public static AudioClip GuardSpotted => _guardSpotted ??=
            Build(ToneGenerator.GenerateSweep(200f, 900f, 0.3f, WaveShape.Square, 0.25f, 0.005f, 0.15f));

        // Descending, minor-ish triad — a cheap but deliberate "bad news" stinger.
        public static AudioClip Caught => _caught ??= Build(Concat(
            ToneGenerator.Generate(392.00f, 0.15f, WaveShape.Square, 0.3f, 0.005f, 0.05f),  // G4
            ToneGenerator.Generate(311.13f, 0.15f, WaveShape.Square, 0.3f, 0.005f, 0.05f),  // Eb4
            ToneGenerator.Generate(220.00f, 0.35f, WaveShape.Square, 0.35f, 0.005f, 0.25f)  // A3
        ));

        // Ascending major triad — a cheap but deliberate "good news" chime.
        public static AudioClip Win => _win ??= Build(Concat(
            ToneGenerator.Generate(523.25f, 0.12f, WaveShape.Sine, 0.3f, 0.005f, 0.05f), // C5
            ToneGenerator.Generate(659.25f, 0.12f, WaveShape.Sine, 0.3f, 0.005f, 0.05f), // E5
            ToneGenerator.Generate(783.99f, 0.25f, WaveShape.Sine, 0.35f, 0.005f, 0.2f)  // G5
        ));

        /// <summary>A low hum, 80 cycles in exactly 1 second so the loop point (set via
        /// AudioSource.loop, not here) has no click.</summary>
        public static AudioClip Drone => _drone ??=
            Build(ToneGenerator.Generate(80f, 1f, WaveShape.Sine, 0.5f, 0f, 0f));

        private static AudioClip Build(float[] samples)
        {
            var clip = AudioClip.Create("SFX", Math.Max(1, samples.Length), 1, ToneGenerator.SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static float[] Concat(params float[][] parts)
        {
            int total = 0;
            foreach (var p in parts) total += p.Length;

            var result = new float[total];
            int offset = 0;
            foreach (var p in parts)
            {
                Array.Copy(p, 0, result, offset, p.Length);
                offset += p.Length;
            }
            return result;
        }
    }
}

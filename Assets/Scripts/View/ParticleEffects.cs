using UnityEngine;

namespace AriadnesThread.View
{
    /// <summary>
    /// Small ParticleSystem-based effects. Deliberately left on Unity's default particle
    /// material rather than hand-configuring a shader's blend state — that's exactly what
    /// went wrong with the guard vision cone's transparency attempt; the built-in default
    /// is already set up correctly.
    /// </summary>
    public static class ParticleEffects
    {
        /// <summary>A short one-shot burst that self-destroys once its particles are gone.</summary>
        public static void SpawnBurst(Vector3 position, Color color, int count = 20, float speed = 3f, float lifetime = 0.5f, float size = 0.15f)
        {
            var go = new GameObject("Burst");
            go.transform.position = position;
            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.loop = false;
            main.duration = lifetime;
            main.startLifetime = lifetime;
            main.startSpeed = speed;
            main.startSize = size;
            main.startColor = color;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;

            SetFadeOutGradient(ps, color);

            Object.Destroy(go, lifetime + 0.5f);
        }

        /// <summary>A continuous flame-like emitter — caller starts/stops it via Play()/Stop().</summary>
        public static ParticleSystem CreateContinuousFlame(Transform parent)
        {
            var go = new GameObject("Flame");
            go.transform.SetParent(parent);
            go.transform.localPosition = Vector3.zero;
            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.loop = true;
            main.startLifetime = 0.5f;
            main.startSpeed = 0.8f;
            main.startSize = 0.12f;
            main.gravityModifier = -0.15f; // drifts upward like a flame

            var emission = ps.emission;
            emission.rateOverTime = 25f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 8f;
            shape.radius = 0.05f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(new Color(1f, 0.8f, 0.3f), 0f), new GradientColorKey(new Color(0.6f, 0.15f, 0.05f), 1f) },
                new[] { new GradientAlphaKey(0.85f, 0f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = gradient;

            ps.Stop();
            return ps;
        }

        private static void SetFadeOutGradient(ParticleSystem ps, Color color)
        {
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = gradient;
        }
    }
}

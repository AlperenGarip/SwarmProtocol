using System.Collections.Generic;
using UnityEngine;

namespace SwarmProtocol.Vfx
{
    /// <summary>
    /// Tiny pooled VFX service. Built-in particle bursts (no prefabs needed) — currently
    /// just enemy death bursts. Add more burst presets here as the game grows.
    /// Singleton, lives on the Bootstrap GameObject.
    /// </summary>
    public class VfxService : MonoBehaviour
    {
        public static VfxService Instance { get; private set; }

        private const int PoolSize = 16;
        private readonly Queue<ParticleSystem> _pool = new();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            for (int i = 0; i < PoolSize; i++)
                _pool.Enqueue(BuildBurst());
        }

        private ParticleSystem BuildBurst()
        {
            var go = new GameObject("DeathBurst");
            go.transform.SetParent(transform, false);
            go.SetActive(false);

            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration            = 0.6f;
            main.loop                = false;
            main.startLifetime       = 0.5f;
            main.startSpeed          = 5f;
            main.startSize           = 0.35f;
            main.startColor          = Color.white;
            main.gravityModifier     = 0.6f;
            main.maxParticles        = 60;
            main.stopAction          = ParticleSystemStopAction.Disable;
            main.simulationSpace     = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 30) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius    = 0.3f;

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            var sizeCurve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(1f, 0f));
            size.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            var col = ps.colorOverLifetime;
            col.enabled = true;
            col.color = new ParticleSystem.MinMaxGradient(BuildFadeGradient(Color.white));

            // Renderer config — uses Unity's built-in default particle material
            var rend = ps.GetComponent<ParticleSystemRenderer>();
            rend.renderMode = ParticleSystemRenderMode.Billboard;
            rend.material   = new Material(Shader.Find("Particles/Standard Unlit"));

            return ps;
        }

        private static Gradient BuildFadeGradient(Color c)
        {
            var g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(c, 0f), new GradientColorKey(c, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            return g;
        }

        /// <summary>Plays a one-shot death burst at a world position, tinted by color.</summary>
        public void SpawnDeathBurst(Vector3 position, Color color)
        {
            if (_pool.Count == 0) return; // pool exhausted — drop the request rather than allocate

            var ps = _pool.Dequeue();
            ps.gameObject.SetActive(true);
            ps.transform.position = position;

            var main = ps.main;
            main.startColor = color;

            var col = ps.colorOverLifetime;
            col.color = new ParticleSystem.MinMaxGradient(BuildFadeGradient(color));

            ps.Clear();
            ps.Play();

            StartCoroutine(ReturnToPoolAfter(ps, main.duration + main.startLifetime.constant));
        }

        private System.Collections.IEnumerator ReturnToPoolAfter(ParticleSystem ps, float delay)
        {
            yield return new WaitForSeconds(delay);
            ps.gameObject.SetActive(false);
            _pool.Enqueue(ps);
        }
    }
}

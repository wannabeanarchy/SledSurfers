using UnityEngine;

namespace SledSurfers.Gameplay.Effects
{
    public static class ParticleEffectSpawner
    {
        public static void Play(GameObject prefab, Vector3 position)
        {
            if (prefab == null)
            {
                return;
            }

            var effect = Object.Instantiate(prefab, position, Quaternion.identity);
            var systems = effect.GetComponentsInChildren<ParticleSystem>(true);
            var lifetime = 0f;

            foreach (var system in systems)
            {
                var main = system.main;
                main.loop = false;
                lifetime = Mathf.Max(
                    lifetime,
                    main.duration + main.startDelay.constantMax + main.startLifetime.constantMax);
                system.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            foreach (var system in systems)
            {
                system.Play(false);
            }

            Object.Destroy(effect, Mathf.Max(lifetime, 0.1f));
        }
    }
}

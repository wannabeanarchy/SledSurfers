using System.Collections;
using SledSurfers.Gameplay.Collectibles;
using UnityEngine;

namespace SledSurfers.Presentation
{
    public sealed class CoinRewardBurst : MonoBehaviour
    {
        [SerializeField] private CoinPickup _coinPrefab;
        [SerializeField, Range(1, 80)] private int _coinCount = 40;
        [SerializeField, Range(.1f, 1f)] private float _coinScaleMultiplier = .5f;
        [SerializeField, Min(0)] private float _spawnRadius = .12f;
        [SerializeField, Min(0)] private float _minimumImpulse = 2.5f;
        [SerializeField, Min(0)] private float _maximumImpulse = 4f;
        [SerializeField, Min(.01f)] private float _colliderSize = .24f;
        [SerializeField, Min(0)] private float _settleDelay = .75f;
        [SerializeField, Min(0)] private float _surfaceClearance = .12f;
        [SerializeField, Min(.1f)] private float _lifetime = 4f;

        public void Play(Vector3 position)
        {
            if (_coinPrefab == null)
            {
                Debug.LogError("Reward coin prefab is not assigned.", this);
                return;
            }

            var coinPrefab = _coinPrefab.gameObject;
            if (!coinPrefab.TryGetComponent<Rigidbody>(out _))
            {
                Debug.LogError("Reward coin prefab needs a Rigidbody.", this);
                return;
            }

            var minimumImpulse = Mathf.Min(_minimumImpulse, _maximumImpulse);
            var maximumImpulse = Mathf.Max(_minimumImpulse, _maximumImpulse);
            for (var i = 0; i < _coinCount; i++)
            {
                var spawnPosition = position + Random.insideUnitSphere * _spawnRadius;
                var coin = Instantiate(coinPrefab, spawnPosition, Random.rotationUniform);
                coin.transform.localScale = coinPrefab.transform.localScale * _coinScaleMultiplier;
                var pickup = coin.GetComponent<CoinPickup>();
                if (pickup != null)
                {
                    pickup.enabled = false;
                }

                foreach (var collider in coin.GetComponentsInChildren<Collider>(true))
                {
                    collider.enabled = true;
                    collider.isTrigger = false;
                    if (collider is BoxCollider boxCollider)
                    {
                        boxCollider.size = Vector3.one * _colliderSize;
                    }
                }

                var body = coin.GetComponent<Rigidbody>();
                body.isKinematic = false;
                body.useGravity = true;
                body.constraints = RigidbodyConstraints.None;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                body.interpolation = RigidbodyInterpolation.Interpolate;
                body.velocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;

                var direction = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(.8f, 1.5f),
                    Random.Range(-1f, 1f)).normalized;
                body.AddForce(direction * Random.Range(minimumImpulse, maximumImpulse), ForceMode.Impulse);
                body.AddTorque(Random.insideUnitSphere * Random.Range(1f, 3f), ForceMode.Impulse);
                StartCoroutine(SettleCoin(coin, body));
                Destroy(coin, Mathf.Max(.1f, _lifetime));
            }
        }

        private IEnumerator SettleCoin(GameObject coin, Rigidbody body)
        {
            yield return new WaitForSecondsRealtime(_settleDelay);
            if (coin == null || body == null)
            {
                yield break;
            }

            var rayOrigin = coin.transform.position + Vector3.up * 6f;
            var hits = Physics.RaycastAll(rayOrigin, Vector3.down, 20f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            var hasGround = false;
            var groundHit = default(RaycastHit);
            for (var i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                if (hit.normal.y < .25f || hit.collider.attachedRigidbody != null || hit.collider.transform.IsChildOf(coin.transform))
                {
                    continue;
                }

                if (!hasGround || hit.distance < groundHit.distance)
                {
                    groundHit = hit;
                    hasGround = true;
                }
            }

            if (!hasGround)
            {
                yield break;
            }

            var supportDistance = GetSupportDistance(coin.transform, groundHit.normal);
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.isKinematic = true;
            body.position = groundHit.point + groundHit.normal * (supportDistance + _surfaceClearance);
            foreach (var collider in coin.GetComponentsInChildren<Collider>(true))
            {
                collider.enabled = false;
            }
        }

        private float GetSupportDistance(Transform coinTransform, Vector3 surfaceNormal)
        {
            var halfSize = Vector3.one * (_colliderSize * .5f);
            halfSize = Vector3.Scale(halfSize, coinTransform.lossyScale);
            return Mathf.Abs(Vector3.Dot(coinTransform.right, surfaceNormal)) * halfSize.x
                + Mathf.Abs(Vector3.Dot(coinTransform.up, surfaceNormal)) * halfSize.y
                + Mathf.Abs(Vector3.Dot(coinTransform.forward, surfaceNormal)) * halfSize.z;
        }
    }
}

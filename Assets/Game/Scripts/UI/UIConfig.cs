using System;
using System.Collections.Generic;
using UnityEngine;

namespace SledSurfers.UI
{
    [CreateAssetMenu(menuName = "Sled Surfers/UI Catalog")]
    public sealed class UIConfig : ScriptableObject
    {
        [SerializeField] private UIView[] _prefabs = Array.Empty<UIView>();

        public Dictionary<Type, UIView> CreateCatalog()
        {
            var catalog = new Dictionary<Type, UIView>();
            foreach (var prefab in _prefabs)
            {
                if (prefab == null)
                {
                    throw new InvalidOperationException("UI catalog contains a missing prefab.");
                }
                if (catalog.ContainsKey(prefab.GetType()))
                {
                    throw new InvalidOperationException($"Duplicate UI prefab: {prefab.GetType().Name}.");
                }
                catalog.Add(prefab.GetType(), prefab);
            }
            return catalog;
        }
    }
}

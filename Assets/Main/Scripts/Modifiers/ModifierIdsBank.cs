using System;
using System.Collections.Generic;
using UnityEngine;

namespace Main.Scripts.Modifiers
{
    public class ModifierIdsBank : MonoBehaviour
    {
        public const int MODIFIERS_COUNT = 32;

        [SerializeField]
        private List<ModifierId> modifierIds = new();

        private Dictionary<int, ModifierId> modifiersMap = default!;
        private Dictionary<string, int> modifierTokens = default!;

        private void Awake()
        {
            modifiersMap = new Dictionary<int, ModifierId>(modifierIds.Count);
            modifierTokens = new Dictionary<string, int>(modifierIds.Count);

            for (var i = 0; i < modifierIds.Count; i++)
            {
                var modifierId = modifierIds[i];
                if (modifierTokens.ContainsKey(modifierId.name))
                {
                    throw new ArgumentException(
                        $"ModifierId {modifierId.name} is already exist");
                }

                modifiersMap.Add(i, modifierId);
                modifierTokens.Add(modifierId.name, i);
            }
        }

        private void OnValidate()
        {
            var ids = new HashSet<string>();
            modifierIds.Clear();

            var modifierIdsObjects = Resources.LoadAll("Scriptable/ModifierIds", typeof(ModifierId));
            foreach (var modifierIdObject in modifierIdsObjects)
            {
                if (modifierIdObject is ModifierId modifierId)
                {
                    if (ids.Contains(modifierId.name))
                    {
                        throw new ArgumentException(
                            $"{modifierId.name}: ModifierId {modifierId.name} is already exist");
                    }

                    modifierIds.Add(modifierId);
                    ids.Add(modifierId.name);
                }
            }

            if (modifierIds.Count > MODIFIERS_COUNT)
            {
                throw new ArgumentException(
                    $"ModifierIds count is larger than MODIFIERS_COUNT value");
            }
        }

        public IEnumerable<ModifierId> GetModifierIds()
        {
            return modifierIds;
        }

        public ModifierId GetModifierId(int id)
        {
            if (!modifiersMap.ContainsKey(id))
            {
                throw new ArgumentException($"ModifierId {id} is not registered in ModifierIdsBank");
            }

            return modifiersMap[id];
        }

        public int GetModifierIdToken(ModifierId modifierId)
        {
            if (!modifierTokens.ContainsKey(modifierId.name))
            {
                throw new ArgumentException(
                    $"ModifierId {modifierId.name} is not registered in ModifierIdsBank. Check ModifierId file path.");
            }

            return modifierTokens[modifierId.name];
        }

        public int GetModifierIdToken(string modifierId)
        {
            if (!modifierTokens.ContainsKey(modifierId))
            {
                throw new ArgumentException(
                    $"ModifierId {modifierId} is not registered in ModifierIdsBank. Check ModifierId file path.");
            }

            return modifierTokens[modifierId];
        }
    }
}
using System;
using System.Linq;
using Fusion;
using Main.Scripts.Core.Resources;
using Main.Scripts.Modifiers;
using Newtonsoft.Json.Linq;

namespace Main.Scripts.Player.Data
{
    public struct ModifiersData : INetworkStruct
    {
        [Networked, Capacity(ModifierIdsBank.MODIFIERS_COUNT)]
        public NetworkArray<ushort> ModifiersLevel => default;

        public static ModifiersData GetDefault()
        {
            var modifiersData = new ModifiersData();
            for (var i = 0; i < modifiersData.ModifiersLevel.Count(); i++)
            {
                modifiersData.ModifiersLevel.Set(i, 0);
            }

            return modifiersData;
        }

        public static ModifiersData ParseJSON(GlobalResources resources, JArray jArray)
        {
            var bank = resources.ModifierIdsBank;
            var modifiersData = GetDefault();
            foreach (var modifierJ in jArray)
            {
                var modifierId = modifierJ.Value<string>(KEY_ID);
                var modifierValue = modifierJ.Value<ushort>(KEY_VALUE);
                var token = bank.GetModifierIdToken(modifierId);
                modifiersData.ModifiersLevel.Set(token, modifierValue);
            }

            return modifiersData;
        }

        public JArray toJSONArray(GlobalResources resources)
        {
            var bank = resources.ModifierIdsBank;
            var jArray = new JArray();

            var modifiersCount = bank.GetModifierIds().Count();
            for (var token = 0; token < modifiersCount; token++)
            {
                var modifierJ = new JObject();
                modifierJ.Add(KEY_ID, bank.GetModifierId(token).Id);
                modifierJ.Add(KEY_VALUE, ModifiersLevel[token]);
                jArray.Add(modifierJ);
            }

            return jArray;
        }

        private static string KEY_ID = "id";
        private static string KEY_VALUE = "value";
    }
}
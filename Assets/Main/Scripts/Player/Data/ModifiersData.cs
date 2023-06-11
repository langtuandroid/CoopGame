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
        public NetworkArray<NetworkBool> Values => default;

        public static ModifiersData GetDefault()
        {
            var modifiersData = new ModifiersData();
            for (var i = 0; i < modifiersData.Values.Count(); i++)
            {
                modifiersData.Values.Set(i, false);
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
                var modifierValue = modifierJ.Value<bool>(KEY_VALUE);
                var token = bank.GetModifierIdToken(modifierId);
                modifiersData.Values.Set(token, modifierValue);
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
                modifierJ.Add(KEY_ID, bank.GetModifierId(token).name);
                modifierJ.Add(KEY_VALUE, (bool) Values[token]);
                jArray.Add(modifierJ);
            }

            return jArray;
        }

        private static string KEY_ID = "id";
        private static string KEY_VALUE = "value";
    }
}
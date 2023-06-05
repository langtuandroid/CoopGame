using Fusion;
using Main.Scripts.Core.Resources;
using Main.Scripts.Customization.Banks;
using Main.Scripts.Customization.Configs;
using Newtonsoft.Json.Linq;

namespace Main.Scripts.Player.Data
{
    public struct CustomizationData : INetworkStruct
    {
        public int headId;
        public int bodyId;
        public int handsId;
        public int legsId;
        public int footsId;
        public int fullSetId;

        public static CustomizationData GetDefault()
        {
            var data = new CustomizationData();
            data.fullSetId = -1;

            return data;
        }

        public static CustomizationData parseJSON(GlobalResources resources, JObject jObject)
        {
            var bank = resources.CustomizationConfigsBank;
            var customizationData = new CustomizationData();
            customizationData.headId = ParseItem(bank.HeadConfigs, jObject, KEY_HEAD_ID, 0);
            customizationData.bodyId = ParseItem(bank.BodyConfigs, jObject, KEY_BODY_ID, 0);
            customizationData.handsId = ParseItem(bank.HandsConfigs, jObject, KEY_HANDS_ID, 0);
            customizationData.legsId = ParseItem(bank.LegsConfigs, jObject, KEY_LEGS_ID, 0);
            customizationData.footsId = ParseItem(bank.FootsConfigs, jObject, KEY_FOOTS_ID, 0);
            customizationData.fullSetId = ParseItem(bank.FullSetConfigs, jObject, KEY_FULL_SET_ID, -1);

            return customizationData;
        }

        private static int ParseItem<T>(
            CustomizationConfigsBankBase<T> bank,
            JObject jObject,
            string key,
            int defaultValue
        ) where T : CustomizationItemConfigBase
        {
            var itemId = jObject.Value<string>(key);
            if (itemId != null)
            {
                return bank.GetCustomizationConfigId(itemId);
            }

            return defaultValue;
        }

        public JObject toJSON(GlobalResources resources)
        {
            var bank = resources.CustomizationConfigsBank;
            var jObject = new JObject();
            jObject.Add(KEY_HEAD_ID, bank.HeadConfigs.GetCustomizationConfig(headId).NameId);
            jObject.Add(KEY_BODY_ID, bank.BodyConfigs.GetCustomizationConfig(bodyId).NameId);
            jObject.Add(KEY_HANDS_ID, bank.HandsConfigs.GetCustomizationConfig(handsId).NameId);
            jObject.Add(KEY_LEGS_ID, bank.LegsConfigs.GetCustomizationConfig(legsId).NameId);
            jObject.Add(KEY_FOOTS_ID, bank.FootsConfigs.GetCustomizationConfig(footsId).NameId);

            if (fullSetId >= 0)
            {
                jObject.Add(KEY_FULL_SET_ID, bank.FullSetConfigs.GetCustomizationConfig(fullSetId).NameId);
            }

            return jObject;
        }

        private static string KEY_HEAD_ID = "head_id";
        private static string KEY_BODY_ID = "body_id";
        private static string KEY_HANDS_ID = "hands_id";
        private static string KEY_LEGS_ID = "legs_id";
        private static string KEY_FOOTS_ID = "foots_id";
        private static string KEY_FULL_SET_ID = "full_set_id";
    }
}
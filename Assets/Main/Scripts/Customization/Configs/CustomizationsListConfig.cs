using System.Collections.Generic;
using UnityEngine;

namespace Main.Scripts.Customization.Configs
{
    [CreateAssetMenu(fileName = "CustomizationsListConfig", menuName = "Customization/CustomizationsList")]
    public class CustomizationsListConfig : ScriptableObject
    {
        [SerializeField]
        private List<CustomizationBodyItemConfig> bodyItemConfigs = new();
        [SerializeField]
        private List<CustomizationFootsItemConfig> footsItemConfigs = new();
        [SerializeField]
        private List<CustomizationHandsItemConfig> handsItemConfigs = new();
        [SerializeField]
        private List<CustomizationLegsItemConfig> legsItemConfigs = new();
        [SerializeField]
        private List<CustomizationHeadItemConfig> headItemConfigs = new();
        [SerializeField]
        private List<CustomizationFullSetItemConfig> fullSetItemConfigs = new();
        
        public List<CustomizationBodyItemConfig> BodyItemConfigs => bodyItemConfigs;
        public List<CustomizationFootsItemConfig> FootsItemConfigs => footsItemConfigs;
        public List<CustomizationHandsItemConfig> HandsItemConfigs => handsItemConfigs;
        public List<CustomizationLegsItemConfig> LegsItemConfigs => legsItemConfigs;
        public List<CustomizationHeadItemConfig> HeadItemConfigs => headItemConfigs;
        public List<CustomizationFullSetItemConfig> FullSetItemConfigs => fullSetItemConfigs;
    }
}
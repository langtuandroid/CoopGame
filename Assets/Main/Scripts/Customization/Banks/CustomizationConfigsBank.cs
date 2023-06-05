using UnityEngine;

namespace Main.Scripts.Customization.Banks
{
    [RequireComponent(typeof(CustomizationHeadConfigsBank))]
    [RequireComponent(typeof(CustomizationBodyConfigsBank))]
    [RequireComponent(typeof(CustomizationHandsConfigsBank))]
    [RequireComponent(typeof(CustomizationLegsConfigsBank))]
    [RequireComponent(typeof(CustomizationFootsConfigsBank))]
    [RequireComponent(typeof(CustomizationFullSetConfigsBank))]
    public class CustomizationConfigsBank : MonoBehaviour
    {
        public CustomizationHeadConfigsBank HeadConfigs { get; private set; } = default!;
        public CustomizationBodyConfigsBank BodyConfigs { get; private set; } = default!;
        public CustomizationHandsConfigsBank HandsConfigs { get; private set; } = default!;
        public CustomizationLegsConfigsBank LegsConfigs { get; private set; } = default!;
        public CustomizationFootsConfigsBank FootsConfigs { get; private set; } = default!;
        public CustomizationFullSetConfigsBank FullSetConfigs { get; private set; } = default!;

        private void Awake()
        {
            HeadConfigs = GetComponent<CustomizationHeadConfigsBank>();
            BodyConfigs = GetComponent<CustomizationBodyConfigsBank>();
            HandsConfigs = GetComponent<CustomizationHandsConfigsBank>();
            LegsConfigs = GetComponent<CustomizationLegsConfigsBank>();
            FootsConfigs = GetComponent<CustomizationFootsConfigsBank>();
            FullSetConfigs = GetComponent<CustomizationFullSetConfigsBank>();
        }
    }
}
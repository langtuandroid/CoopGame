using System;
using Cysharp.Threading.Tasks;
using Main.Scripts.Customization.Configs;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
        [SerializeField]
        private AssetReferenceT<CustomizationsListConfig> customizationsListConfigReference = null!;
        public CustomizationHeadConfigsBank HeadConfigs { get; private set; } = default!;
        public CustomizationBodyConfigsBank BodyConfigs { get; private set; } = default!;
        public CustomizationHandsConfigsBank HandsConfigs { get; private set; } = default!;
        public CustomizationLegsConfigsBank LegsConfigs { get; private set; } = default!;
        public CustomizationFootsConfigsBank FootsConfigs { get; private set; } = default!;
        public CustomizationFullSetConfigsBank FullSetConfigs { get; private set; } = default!;

        public async UniTask Init()
        {
            HeadConfigs = GetComponent<CustomizationHeadConfigsBank>();
            BodyConfigs = GetComponent<CustomizationBodyConfigsBank>();
            HandsConfigs = GetComponent<CustomizationHandsConfigsBank>();
            LegsConfigs = GetComponent<CustomizationLegsConfigsBank>();
            FootsConfigs = GetComponent<CustomizationFootsConfigsBank>();
            FullSetConfigs = GetComponent<CustomizationFullSetConfigsBank>();

            var configs = await Addressables
                .LoadAssetAsync<CustomizationsListConfig>(customizationsListConfigReference)
                .WithCancellation(this.GetCancellationTokenOnDestroy());
            if (configs == null)
            {
                throw new Exception("CustomizationsListConfigReference is not exist");
            }

            HeadConfigs.Init(configs.HeadItemConfigs);
            BodyConfigs.Init(configs.BodyItemConfigs);
            HandsConfigs.Init(configs.HandsItemConfigs);
            LegsConfigs.Init(configs.LegsItemConfigs);
            FootsConfigs.Init(configs.FootsItemConfigs);
            FullSetConfigs.Init(configs.FullSetItemConfigs);
        }
    }
}
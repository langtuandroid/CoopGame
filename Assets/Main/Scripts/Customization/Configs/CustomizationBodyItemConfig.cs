using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Main.Scripts.Customization.Configs
{
    [CreateAssetMenu(fileName = "CustomizationBodyItem", menuName = "Customization/Body")]
    public class CustomizationBodyItemConfig : CustomizationItemConfigBase
    {
        [SerializeField]
        private AssetReferenceT<Mesh> spine = default!;
        [SerializeField]
        private AssetReferenceT<Mesh> chest = default!;
        [SerializeField]
        private AssetReferenceT<Mesh> armLeftLower = default!;
        [SerializeField]
        private AssetReferenceT<Mesh> armLeftUpper = default!;
        [SerializeField]
        private AssetReferenceT<Mesh> armRightLower = default!;
        [SerializeField]
        private AssetReferenceT<Mesh> armRightUpper = default!;

        public AssetReferenceT<Mesh> Spine => spine;
        public AssetReferenceT<Mesh> Chest => chest;
        public AssetReferenceT<Mesh> ArmLeftLower => armLeftLower;
        public AssetReferenceT<Mesh> ArmLeftUpper => armLeftUpper;
        public AssetReferenceT<Mesh> ArmRightLower => armRightLower;
        public AssetReferenceT<Mesh> ArmRightUpper => armRightUpper;
    }
}
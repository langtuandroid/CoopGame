using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Main.Scripts.Customization.Configs
{
    [CreateAssetMenu(fileName = "CustomizationLegsItem", menuName = "Customization/Legs")]
    public class CustomizationLegsItemConfig : CustomizationItemConfigBase
    {
        [SerializeField]
        private AssetReferenceT<Mesh> hips = default!;
        [SerializeField]
        private AssetReferenceT<Mesh> legLeftLower = default!;
        [SerializeField]
        private AssetReferenceT<Mesh> legLeftUpper = default!;
        [SerializeField]
        private AssetReferenceT<Mesh> legRightLower = default!;
        [SerializeField]
        private AssetReferenceT<Mesh> legRightUpper = default!;

        public AssetReferenceT<Mesh> Hips => hips;
        public AssetReferenceT<Mesh> LegLeftLower => legLeftLower;
        public AssetReferenceT<Mesh> LegLeftUpper => legLeftUpper;
        public AssetReferenceT<Mesh> LegRightLower => legRightLower;
        public AssetReferenceT<Mesh> LegRightUpper => legRightUpper;
    }
}
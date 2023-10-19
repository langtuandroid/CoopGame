using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Main.Scripts.Customization.Configs
{
    [CreateAssetMenu(fileName = "CustomizationHandsItem", menuName = "Customization/Hands")]
    public class CustomizationHandsItemConfig : CustomizationItemConfigBase
    {
        [SerializeField]
        private AssetReferenceT<Mesh> handLeft = default!;
        [SerializeField]
        private AssetReferenceT<Mesh> handRight = default!;

        public AssetReferenceT<Mesh> HandLeft => handLeft;
        public AssetReferenceT<Mesh> HandRight => handRight;
    }
}
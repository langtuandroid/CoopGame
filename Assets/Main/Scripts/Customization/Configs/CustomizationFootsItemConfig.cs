using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Main.Scripts.Customization.Configs
{
    [CreateAssetMenu(fileName = "CustomizationFootsItem", menuName = "Customization/Foots")]
    public class CustomizationFootsItemConfig : CustomizationItemConfigBase
    {
        [SerializeField]
        private AssetReferenceT<Mesh> footLeft = default!;
        [SerializeField]
        private AssetReferenceT<Mesh> footRight = default!;

        public AssetReferenceT<Mesh> FootLeft => footLeft;
        public AssetReferenceT<Mesh> FootRight => footRight;
    }
}
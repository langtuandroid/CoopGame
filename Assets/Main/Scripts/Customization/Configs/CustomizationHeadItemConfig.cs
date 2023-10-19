using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Main.Scripts.Customization.Configs
{
    [CreateAssetMenu(fileName = "CustomizationHeadItem", menuName = "Customization/Head")]
    public class CustomizationHeadItemConfig : CustomizationItemConfigBase
    {
        [SerializeField]
        private AssetReferenceT<Mesh> head = default!;

        public AssetReferenceT<Mesh> Head => head;
    }
}
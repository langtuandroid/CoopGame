using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Main.Scripts.Customization.Configs
{
    [CreateAssetMenu(fileName = "CustomizationFullSetItem", menuName = "Customization/FullSet")]
    public class CustomizationFullSetItemConfig : CustomizationItemConfigBase
    {
        [SerializeField]
        private AssetReferenceT<Mesh> head = default!;

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

        [SerializeField]
        private AssetReferenceT<Mesh> handLeft = default!;
        [SerializeField]
        private AssetReferenceT<Mesh> handRight = default!;

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

        [SerializeField]
        private AssetReferenceT<Mesh> footLeft = default!;
        [SerializeField]
        private AssetReferenceT<Mesh> footRight = default!;

        public AssetReferenceT<Mesh> Head => head;

        public AssetReferenceT<Mesh> Spine => spine;
        public AssetReferenceT<Mesh> Chest => chest;
        public AssetReferenceT<Mesh> ArmLeftLower => armLeftLower;
        public AssetReferenceT<Mesh> ArmLeftUpper => armLeftUpper;
        public AssetReferenceT<Mesh> ArmRightLower => armRightLower;
        public AssetReferenceT<Mesh> ArmRightUpper => armRightUpper;

        public AssetReferenceT<Mesh> HandLeft => handLeft;
        public AssetReferenceT<Mesh> HandRight => handRight;

        public AssetReferenceT<Mesh> Hips => hips;
        public AssetReferenceT<Mesh> LegLeftLower => legLeftLower;
        public AssetReferenceT<Mesh> LegLeftUpper => legLeftUpper;
        public AssetReferenceT<Mesh> LegRightLower => legRightLower;
        public AssetReferenceT<Mesh> LegRightUpper => legRightUpper;

        public AssetReferenceT<Mesh> FootLeft => footLeft;
        public AssetReferenceT<Mesh> FootRight => footRight;
    }
}
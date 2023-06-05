using UnityEngine;

namespace Main.Scripts.Customization.Configs
{
    [CreateAssetMenu(fileName = "CustomizationFullSetItem", menuName = "Customization/FullSet")]
    public class CustomizationFullSetItemConfig : CustomizationItemConfigBase
    {
        [SerializeField]
        private Mesh head = default!;

        [SerializeField]
        private Mesh spine = default!;
        [SerializeField]
        private Mesh chest = default!;
        [SerializeField]
        private Mesh armLeftLower = default!;
        [SerializeField]
        private Mesh armLeftUpper = default!;
        [SerializeField]
        private Mesh armRightLower = default!;
        [SerializeField]
        private Mesh armRightUpper = default!;

        [SerializeField]
        private Mesh handLeft = default!;
        [SerializeField]
        private Mesh handRight = default!;

        [SerializeField]
        private Mesh hips = default!;
        [SerializeField]
        private Mesh legLeftLower = default!;
        [SerializeField]
        private Mesh legLeftUpper = default!;
        [SerializeField]
        private Mesh legRightLower = default!;
        [SerializeField]
        private Mesh legRightUpper = default!;

        [SerializeField]
        private Mesh footLeft = default!;
        [SerializeField]
        private Mesh footRight = default!;

        public Mesh Head => head;

        public Mesh Spine => spine;
        public Mesh Chest => chest;
        public Mesh ArmLeftLower => armLeftLower;
        public Mesh ArmLeftUpper => armLeftUpper;
        public Mesh ArmRightLower => armRightLower;
        public Mesh ArmRightUpper => armRightUpper;

        public Mesh HandLeft => handLeft;
        public Mesh HandRight => handRight;

        public Mesh Hips => hips;
        public Mesh LegLeftLower => legLeftLower;
        public Mesh LegLeftUpper => legLeftUpper;
        public Mesh LegRightLower => legRightLower;
        public Mesh LegRightUpper => legRightUpper;

        public Mesh FootLeft => footLeft;
        public Mesh FootRight => footRight;
    }
}
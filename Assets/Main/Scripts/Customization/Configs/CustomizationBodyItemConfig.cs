using UnityEngine;

namespace Main.Scripts.Customization.Configs
{
    [CreateAssetMenu(fileName = "CustomizationBodyItem", menuName = "Customization/Body")]
    public class CustomizationBodyItemConfig : CustomizationItemConfigBase
    {
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

        public Mesh Spine => spine;
        public Mesh Chest => chest;
        public Mesh ArmLeftLower => armLeftLower;
        public Mesh ArmLeftUpper => armLeftUpper;
        public Mesh ArmRightLower => armRightLower;
        public Mesh ArmRightUpper => armRightUpper;
    }
}
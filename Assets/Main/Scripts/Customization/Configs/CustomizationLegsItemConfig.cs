using UnityEngine;

namespace Main.Scripts.Customization.Configs
{
    [CreateAssetMenu(fileName = "CustomizationLegsItem", menuName = "Customization/Legs")]
    public class CustomizationLegsItemConfig : CustomizationItemConfigBase
    {
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

        public Mesh Hips => hips;
        public Mesh LegLeftLower => legLeftLower;
        public Mesh LegLeftUpper => legLeftUpper;
        public Mesh LegRightLower => legRightLower;
        public Mesh LegRightUpper => legRightUpper;
    }
}
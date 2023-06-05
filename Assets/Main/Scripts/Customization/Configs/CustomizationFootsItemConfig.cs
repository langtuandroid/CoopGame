using UnityEngine;

namespace Main.Scripts.Customization.Configs
{
    [CreateAssetMenu(fileName = "CustomizationFootsItem", menuName = "Customization/Foots")]
    public class CustomizationFootsItemConfig : CustomizationItemConfigBase
    {
        [SerializeField]
        private Mesh footLeft = default!;
        [SerializeField]
        private Mesh footRight = default!;

        public Mesh FootLeft => footLeft;
        public Mesh FootRight => footRight;
    }
}
using UnityEngine;

namespace Main.Scripts.Customization.Configs
{
    [CreateAssetMenu(fileName = "CustomizationHandsItem", menuName = "Customization/Hands")]
    public class CustomizationHandsItemConfig : CustomizationItemConfigBase
    {
        [SerializeField]
        private Mesh handLeft = default!;
        [SerializeField]
        private Mesh handRight = default!;

        public Mesh HandLeft => handLeft;
        public Mesh HandRight => handRight;
    }
}
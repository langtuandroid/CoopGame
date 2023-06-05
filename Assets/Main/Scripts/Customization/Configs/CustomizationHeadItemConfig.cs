using UnityEngine;

namespace Main.Scripts.Customization.Configs
{
    [CreateAssetMenu(fileName = "CustomizationHeadItem", menuName = "Customization/Head")]
    public class CustomizationHeadItemConfig : CustomizationItemConfigBase
    {
        [SerializeField]
        private Mesh head = default!;

        public Mesh Head => head;
    }
}
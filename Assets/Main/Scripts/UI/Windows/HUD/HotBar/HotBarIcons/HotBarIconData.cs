using UnityEngine;

namespace Main.Scripts.UI.Windows.HUD.HotBar.HotBarIcons
{
    [CreateAssetMenu(fileName = "HotBarIcon", menuName = "Scriptable/HotBarIcon")]
    public class HotBarIconData : ScriptableObject
    {
        [SerializeField]
        private Sprite? image;

        public Sprite? Image => image;
    }
}
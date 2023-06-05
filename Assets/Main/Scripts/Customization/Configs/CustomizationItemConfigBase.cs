using UnityEngine;

namespace Main.Scripts.Customization.Configs
{
    public abstract class CustomizationItemConfigBase : ScriptableObject
    {
        [SerializeField]
        private string nameId = default!;

        public string NameId => nameId;
    }
}
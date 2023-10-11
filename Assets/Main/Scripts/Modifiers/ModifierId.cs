using UnityEngine;

namespace Main.Scripts.Modifiers
{
    [CreateAssetMenu(fileName = "ModifierId", menuName = "Modifiers/Id")]
    public class ModifierId : ModifierBase
    {
        [SerializeField]
        [Min(1)]
        private int upgradeLevels;

        public ushort UpgradeLevels => (ushort) upgradeLevels;
        public string Id { get; private set; } = "";

        public void OnEnable()
        {
            Id = name;
        }
    }
}
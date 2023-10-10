using UnityEngine;

namespace Main.Scripts.Modifiers
{
    [CreateAssetMenu(fileName = "ModifierId", menuName = "Modifiers/Id")]
    public class ModifierId : ScriptableObject
    {
        [SerializeField]
        [Min(1)]
        private int levelsCount;
        [SerializeField]
        [Min(1)]
        private int chargeLevel;

        public ushort LevelsCount => (ushort) levelsCount;
        public int ChargeLevel => chargeLevel;
        public string Id { get; private set; } = "";

        public void OnEnable()
        {
            Id = name;
        }
    }
}
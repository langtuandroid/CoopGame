using UnityEngine;

namespace Main.Scripts.Modifiers
{
    [CreateAssetMenu(fileName = "ModifierId", menuName = "Modifiers/Id")]
    public class ModifierId : ScriptableObject
    {
        [SerializeField]
        [Min(1)]
        private int levelsCount;
        
        public ushort LevelsCount => (ushort) levelsCount;
        public string Id { get; private set; } = "";

        public void OnEnable()
        {
            Id = name;
        }
    }
}
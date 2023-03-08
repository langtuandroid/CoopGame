using UnityEngine;

namespace Main.Scripts.Skills
{
    [CreateAssetMenu(fileName = "Data", menuName = "Scriptable/SkillInfo")]
    public class SkillInfo: ScriptableObject
    {
        [SerializeField]
        private SkillType type;
        [SerializeField]
        private string title = default!;
        [SerializeField]
        private string description = default!;
        [SerializeField]
        private int maxLevel;
        
        public SkillType Type => type;

        public string Title => title;

        public string Description => description;

        public int MaxLevel => maxLevel;
    }
}
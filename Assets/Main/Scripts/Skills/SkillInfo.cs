using Main.Scripts.Modifiers;
using UnityEngine;

namespace Main.Scripts.Skills
{
    [CreateAssetMenu(fileName = "Data", menuName = "Scriptable/SkillInfo")]
    public class SkillInfo: ScriptableObject
    {
        [SerializeField]
        private ModifierId modifierId = null!;
        [SerializeField]
        private string title = null!;
        [SerializeField]
        [Multiline]
        private string description = null!;

        public ModifierId ModifierId => modifierId;
        public string Title => title;
        public string Description => description;
    }
}
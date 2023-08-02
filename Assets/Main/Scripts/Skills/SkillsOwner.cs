using Main.Scripts.Skills.ActiveSkills;

namespace Main.Scripts.Skills
{
    public interface SkillsOwner
    {
        public int GetActiveSkillCooldownLeftTicks(ActiveSkillType skillType);
        public void AddSkillListener(SkillsOwner.Listener listener);
        public void RemoveSkillListener(SkillsOwner.Listener listener);

        public interface Listener
        {
            void OnActiveSkillCooldownChanged(ActiveSkillType skillType, int cooldownLeftTicks);
        }
    }
}
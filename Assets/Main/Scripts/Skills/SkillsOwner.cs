using Main.Scripts.Skills.ActiveSkills;

namespace Main.Scripts.Skills
{
    public interface SkillsOwner
    {
        public int GetActiveSkillCooldownLeftTicks(ActiveSkillType skillType);
        public bool CanActivateSkill(ActiveSkillType skillType);
        public void AddSkillListener(Listener listener);
        public void RemoveSkillListener(Listener listener);

        public interface Listener
        {
            void OnActiveSkillCooldownChanged(ActiveSkillType skillType, int cooldownLeftTicks);
            void OnPowerChargeProgressChanged(bool isCharging, int powerChargeLevel, int powerChargeProgress);
        }
    }
}
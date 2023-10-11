using System;
using Main.Scripts.Skills;
using Main.Scripts.Skills.ActiveSkills;
using Main.Scripts.Skills.Charge;
using Main.Scripts.UI.Windows.HUD.HotBar;
using Main.Scripts.UI.Windows.HUD.HotBar.HotBarIcons;

namespace Main.Scripts.UI.Windows.HUD
{
    public class HUDPresenterImpl : HUDContract.HotBarPresenter, SkillsOwner.Listener, SkillChargeManager.Listener
    {
        private HUDContract.HotBarView view;
        private HotBarIconDataHolder dataHolder;
        private SkillsOwner skillsOwner;
        private SkillChargeManager skillChargeManager;
        private int tickRate;

        public HUDPresenterImpl(
            HUDContract.HotBarView view,
            HotBarIconDataHolder dataHolder,
            SkillsOwner skillsOwner,
            SkillChargeManager skillChargeManager,
            int tickRate
        )
        {
            this.view = view;
            this.dataHolder = dataHolder;
            this.skillsOwner = skillsOwner;
            this.skillChargeManager = skillChargeManager;
            this.tickRate = tickRate;
        }

        public void OnOpen()
        {
            var hotBarData = new HotBarData
            {
                PrimaryIconData = new HotBarItemData(dataHolder.GetIconData(ActiveSkillType.PRIMARY),
                    TicksToCeilSec(skillsOwner.GetActiveSkillCooldownLeftTicks(ActiveSkillType.PRIMARY))),
                DashIconData = new HotBarItemData(dataHolder.GetIconData(ActiveSkillType.DASH),
                    TicksToCeilSec(skillsOwner.GetActiveSkillCooldownLeftTicks(ActiveSkillType.DASH))),
                FirstSkillIconData = new HotBarItemData(dataHolder.GetIconData(ActiveSkillType.FIRST_SKILL),
                    TicksToCeilSec(skillsOwner.GetActiveSkillCooldownLeftTicks(ActiveSkillType.FIRST_SKILL))),
                SecondSkillIconData = new HotBarItemData(dataHolder.GetIconData(ActiveSkillType.SECOND_SKILL),
                    TicksToCeilSec(skillsOwner.GetActiveSkillCooldownLeftTicks(ActiveSkillType.SECOND_SKILL))),
                ThirdSkillIconData = new HotBarItemData(dataHolder.GetIconData(ActiveSkillType.THIRD_SKILL),
                    TicksToCeilSec(skillsOwner.GetActiveSkillCooldownLeftTicks(ActiveSkillType.THIRD_SKILL))),
            };
            view.Bind(ref hotBarData);
            OnChargeInfoChanged();
            OnPowerChargeProgressChanged(false, 0, 0);
            skillsOwner.AddSkillListener(this);
            skillChargeManager.AddListener(this);
            view.SetVisibility(true);
        }

        public void OnClose()
        {
            view.SetVisibility(false);
            skillsOwner.RemoveSkillListener(this);
            skillChargeManager.RemoveListener(this);
        }

        public void OnActiveSkillCooldownChanged(ActiveSkillType skillType, int cooldownLeftTicks)
        {
            var cooldownLeftSec = TicksToCeilSec(cooldownLeftTicks);

            view.UpdateSkillCooldown(skillType, cooldownLeftSec);
        }

        private int TicksToCeilSec(int ticks)
        {
            return (int)Math.Ceiling((float)ticks / tickRate);
        }

        public void OnChargeInfoChanged()
        {
            var chargeLevel = skillChargeManager.ChargeLevel;
            var progressPercent = skillChargeManager.ChargeProgress;
            var charProgressTarget = skillChargeManager.GetProgressForNextLevel();
            var isMaxLevel = skillChargeManager.IsMaxChargeLevel;
            view.OnChargeInfoChanged(chargeLevel, progressPercent, charProgressTarget, isMaxLevel);
        }

        public void OnPowerChargeProgressChanged(bool isCharging, int powerChargeLevel, int powerChargeProgress)
        {
            view.OnPowerChargeInfoChanged(isCharging, powerChargeLevel + 1, powerChargeProgress);
        }
    }
}
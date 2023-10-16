using System.Collections.Generic;
using Fusion;
using Main.Scripts.Modifiers;
using Main.Scripts.Player.Data;
using Main.Scripts.Skills;

namespace Main.Scripts.UI.Windows.SkillTree
{
    public class SkillTreePresenterImpl : SkillTreeContract.SkillTreePresenter
    {
        private SkillTreeContract.SkillTreeView view;
        private PlayerDataManager playerDataManager;
        private SkillInfoBank skillInfoBank;
        private ModifierIdsBank modifierIdsBank;

        private List<SkillInfoData> skillInfoDataList = new();

        public SkillTreePresenterImpl(
            PlayerDataManager playerDataManager,
            SkillInfoBank skillInfoBank,
            ModifierIdsBank modifierIdsBank,
            SkillTreeContract.SkillTreeView view
        )
        {
            this.view = view;
            this.playerDataManager = playerDataManager;
            this.skillInfoBank = skillInfoBank;
            this.modifierIdsBank = modifierIdsBank;
        }

        private void OnHeroDataChanged(PlayerRef playerRef)
        {
            if (playerRef != playerDataManager.Runner.LocalPlayer) return;

            Rebind();
        }

        public void Show()
        {
            playerDataManager.OnHeroDataChangedEvent.AddListener(OnHeroDataChanged);
            Rebind();
            view.SetVisibility(true);
        }

        private void Rebind()
        {
            var heroData = playerDataManager.GetLocalHeroData();
            skillInfoDataList.Clear();
            foreach (var skillInfo in skillInfoBank.GetSkillInfos())
            {
                var skillInfoData = new SkillInfoData
                {
                    SkillInfo = skillInfo,
                    CurrentLevel =
                        heroData.Modifiers.ModifiersLevel[modifierIdsBank.GetModifierIdToken(skillInfo.ModifierId)]
                };
                skillInfoDataList.Add(skillInfoData);
            }

            view.Bind(new PlayerInfoData
            {
                Level = heroData.Level,
                Experience = heroData.Experience,
                AvailableSkillPoints = heroData.GetAvailableSkillPoints(),
                MaxSkillPoints = heroData.MaxSkillPoints,
                SkillInfoDataList = skillInfoDataList
            });
        }

        public void Hide()
        {
            playerDataManager.OnHeroDataChangedEvent.RemoveListener(OnHeroDataChanged);
            view.SetVisibility(false);
        }

        public void ResetSkillPoints()
        {
            playerDataManager.ResetAllModifiersLevel();
        }

        public void OnIncreaseSkillLevelClicked(SkillInfoData skillInfoData)
        {
            var modifierId = skillInfoData.SkillInfo.ModifierId;
            var modifierToken = modifierIdsBank.GetModifierIdToken(modifierId);
            if (skillInfoData.CurrentLevel < modifierId.UpgradeLevels)
            {
                playerDataManager.SetModifierLevel(modifierToken, (ushort)(skillInfoData.CurrentLevel + 1));
            }
        }

        public void OnDecreaseSkillLevelClicked(SkillInfoData skillInfoData)
        {
            var modifierId = skillInfoData.SkillInfo.ModifierId;
            var modifierToken = modifierIdsBank.GetModifierIdToken(modifierId);
            if (skillInfoData.CurrentLevel > 0)
            {
                playerDataManager.SetModifierLevel(modifierToken, (ushort)(skillInfoData.CurrentLevel - 1));
            }
        }
    }
}
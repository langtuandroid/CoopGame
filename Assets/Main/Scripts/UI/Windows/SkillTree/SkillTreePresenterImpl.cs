using System.Collections.Generic;
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

        private void OnPlayerDataChanged(UserId userId, PlayerData playerData, PlayerData oldPlayerData)
        {
            if (!userId.Equals(playerDataManager.LocalUserId)) return;

            Rebind();
        }

        public void Show()
        {
            playerDataManager.OnPlayerDataChangedEvent.AddListener(OnPlayerDataChanged);
            Rebind();
            view.SetVisibility(true);
        }

        private void Rebind()
        {
            var playerData = playerDataManager.LocalPlayerData;
            skillInfoDataList.Clear();
            foreach (var skillInfo in skillInfoBank.GetSkillInfos())
            {
                var skillInfoData = new SkillInfoData
                {
                    SkillInfo = skillInfo,
                    CurrentLevel =
                        playerData.Modifiers.ModifiersLevel[modifierIdsBank.GetModifierIdToken(skillInfo.ModifierId)]
                };
                skillInfoDataList.Add(skillInfoData);
            }

            view.Bind(new PlayerInfoData
            {
                Level = playerData.Level,
                Experience = playerData.Experience,
                AvailableSkillPoints = playerData.GetAvailableSkillPoints(),
                MaxSkillPoints = playerData.MaxSkillPoints,
                SkillInfoDataList = skillInfoDataList
            });
        }

        public void Hide()
        {
            playerDataManager.OnPlayerDataChangedEvent.RemoveListener(OnPlayerDataChanged);
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
            if (skillInfoData.CurrentLevel < modifierId.LevelsCount)
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
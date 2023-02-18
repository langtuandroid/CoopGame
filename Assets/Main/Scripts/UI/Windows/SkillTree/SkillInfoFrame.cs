using Main.Scripts.Skills;
using UnityEngine.UIElements;

namespace Main.Scripts.UI.Windows.SkillTree
{
    public class SkillInfoFrame
    {
        private Label titleLabel;
        private Label descriptionLabel;
        private Label skillLevelLabel;
        private Button increaseLevelButton;
        private Button decreaseLevelButton;

        private SkillType type;
        private InteractionCallback callback;

        private SkillInfoFrame() { }

        public static SkillInfoFrame from(VisualElement view)
        {
            var element = new SkillInfoFrame
            {
                titleLabel = view.Q<Label>("SkillTitle"),
                descriptionLabel = view.Q<Label>("SkillDescription"),
                skillLevelLabel = view.Q<Label>("SkillLevelText"),
                increaseLevelButton = view.Q<Button>("SkillIncreasePoint"),
                decreaseLevelButton = view.Q<Button>("SkillDecreasePoint"),
            };
            element.increaseLevelButton.clicked += element.OnIncreaseClicked;
            element.decreaseLevelButton.clicked += element.OnDecreaseClicked;
            return element;
        }

        public void Bind(SkillInfo skillInfo, int currentLevel)
        {
            type = skillInfo.Type;
            titleLabel.text = skillInfo.Title;
            descriptionLabel.text = skillInfo.Description;
            skillLevelLabel.text = $"{currentLevel}/{skillInfo.MaxLevel}";
        }

        public void SetInteractionCallback(InteractionCallback callback)
        {
            this.callback = callback;
        }

        private void OnIncreaseClicked()
        {
            callback?.OnIncreaseClicked(type);
        }

        private void OnDecreaseClicked()
        {
            callback?.OnDecreaseClicked(type);
        }

        public interface InteractionCallback
        {
            void OnIncreaseClicked(SkillType skillType);
            void OnDecreaseClicked(SkillType skillType);
        }
    }
}
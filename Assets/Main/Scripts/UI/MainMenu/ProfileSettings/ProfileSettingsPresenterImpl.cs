using Main.Scripts.Utils.Save;

namespace Main.Scripts.UI.MainMenu.ProfileSettings
{
    public class ProfileSettingsPresenterImpl : ProfileSettingsContract.ProfileSettingsPresenter
    {
        private ProfileSettingsContract.ProfileSettingsView view;

        public ProfileSettingsPresenterImpl(
            ProfileSettingsContract.ProfileSettingsView view
        )
        {
            this.view = view;
        }

        public void Show()
        {
            UpdateData();
            view.SetVisibility(true);
        }

        public void Hide()
        {
            view.CloseWindow();
        }

        public void OnDeleteClicked(string saveName)
        {
            SaveLoadUtils.DeleteSave(saveName);
            UpdateData();
        }

        public void OnDeleteAllClicked()
        {
            SaveLoadUtils.DeleteAllSaves();
            UpdateData();
        }

        public void OnBackClicked()
        {
            Hide();
        }

        private void UpdateData()
        {
            view.Bind(SaveLoadUtils.GetSavesList());
        }
    }
}
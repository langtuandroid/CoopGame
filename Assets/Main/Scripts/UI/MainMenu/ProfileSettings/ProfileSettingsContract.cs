using System.Collections.Generic;
using Main.Scripts.Core.Mvp;

namespace Main.Scripts.UI.MainMenu.ProfileSettings
{
    public interface ProfileSettingsContract : MvpContract
    {
        public interface ProfileSettingsPresenter : Presenter
        {
            void Show();
            void Hide();
            void OnDeleteClicked(string userId);
            void OnDeleteAllClicked();
            void OnBackClicked();
        }

        public interface ProfileSettingsView : View<ProfileSettingsPresenter>
        {
            void Bind(List<string> userId);
            void SetVisibility(bool isVisible);
            void CloseWindow();
        }
    }
}
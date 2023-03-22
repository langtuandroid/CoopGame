using System;
using System.Collections.Generic;
using Main.Scripts.Core.Mvp;
using UnityEngine.UIElements;

namespace Main.Scripts.UI.MainMenu.ProfileSettings
{
    public class ProfileSettingsWindow : MvpMonoBehavior<ProfileSettingsContract.ProfileSettingsPresenter>,
        ProfileSettingsContract.ProfileSettingsView
    {
        protected override ProfileSettingsContract.ProfileSettingsPresenter? presenter { get; set; }

        private UIDocument doc = default!;
        private DropdownField playerNameDropdown = default!;
        private Button deleteProfileButton = default!;
        private Button deleteAllProfilesButton = default!;
        private Button backButton = default!;

        public Action? OnClosedEvent;

        private void Awake()
        {
            presenter = new ProfileSettingsPresenterImpl(view: this);
            doc = GetComponent<UIDocument>();
            var root = doc.rootVisualElement;
            playerNameDropdown = root.Q<DropdownField>("PlayerNameDropdown");
            deleteProfileButton = root.Q<Button>("DeleteProfileButton");
            deleteAllProfilesButton = root.Q<Button>("DeleteAllProfilesButton");
            backButton = root.Q<Button>("ProfileSettingsBackButton");

            backButton.clicked += OnBackClicked;
            deleteProfileButton.clicked += OnDelete;
            deleteAllProfilesButton.clicked += OnDeleteAll;

            SetVisibility(false);
        }

        public void Show()
        {
            presenter?.Show();
        }

        public void Hide()
        {
            presenter?.Hide();
        }

        public void Bind(List<string> list)
        {
            playerNameDropdown.choices = list;
            playerNameDropdown.value = list.Count > 0 ? list[0] : default;
        }

        private void OnDelete()
        {
            presenter?.OnDeleteClicked(playerNameDropdown.value);
        }

        private void OnDeleteAll()
        {
            presenter?.OnDeleteAllClicked();
        }

        private void OnBackClicked()
        {
            presenter?.OnBackClicked();
        }

        public void SetVisibility(bool isVisible)
        {
            doc.rootVisualElement.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void CloseWindow()
        {
            SetVisibility(false);
            OnClosedEvent?.Invoke();
        }
    }
}
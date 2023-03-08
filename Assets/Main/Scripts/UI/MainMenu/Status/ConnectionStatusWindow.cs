using System;
using Main.Scripts.Connection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Main.Scripts.UI.MainMenu.Status
{
    [RequireComponent(typeof(UIDocument))]
    public class ConnectionStatusWindow : MonoBehaviour
    {
        private UIDocument doc = default!;
        private Label connectionStatusLabel = default!;
        private Button returnButton = default!;

        public Action? OnReturnButtonClicked;

        private void Awake()
        {
            doc = GetComponent<UIDocument>();
            var root = doc.rootVisualElement;
            connectionStatusLabel = root.Q<Label>("ConnectionStatusLabel");
            returnButton = root.Q<Button>("ReturnButton");
            returnButton.clicked += () => { OnReturnButtonClicked?.Invoke(); };
        }

        public void SetVisibility(bool isVisible)
        {
            doc.rootVisualElement.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void SetStatus(ConnectionStatus status)
        {
            var statusText = status switch
            {
                ConnectionStatus.Disconnected => "Disconnected",
                ConnectionStatus.Connecting => "Connecting...",
                ConnectionStatus.Failed => "Connection failed",
                ConnectionStatus.Connected => "",
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };

            connectionStatusLabel.text = statusText;

            bool isButtonVisible;
            switch (status)
            {
                case ConnectionStatus.Disconnected:
                case ConnectionStatus.Failed:
                    isButtonVisible = true;
                    break;
                case ConnectionStatus.Connecting:
                case ConnectionStatus.Connected:
                    isButtonVisible = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }

            returnButton.style.display = isButtonVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
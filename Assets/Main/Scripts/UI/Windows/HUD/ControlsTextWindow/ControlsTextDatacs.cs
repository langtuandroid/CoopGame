using System;

namespace Main.Scripts.UI.Windows.HUD.ControlsTextWindow
{
    [Serializable]
    public struct ControlsTextData
    {
        public string ControlKeyName;
        public string ControlKeyDescription;

        public ControlsTextData(string name, string description)
        {
            ControlKeyName = name;
            ControlKeyDescription = description;
        }
    }
}
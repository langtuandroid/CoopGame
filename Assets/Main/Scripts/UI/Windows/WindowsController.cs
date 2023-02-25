using System;
using System.Linq;
using Fusion;
using Main.Scripts.Utils;

namespace Main.Scripts.UI.Windows
{
    public class WindowsController : NetworkBehaviour
    {
        private WindowsHolder windowsHolder = default!;

        [Networked(OnChanged = nameof(OnCurrentWindowChanged))]
        public WindowType CurrentWindow { get; private set; }

        public override void Spawned()
        {
            windowsHolder = FindObjectOfType<WindowsHolder>().ThrowWhenNull();
            CurrentWindow = WindowType.NONE;
        }

        private static void OnCurrentWindowChanged(Changed<WindowsController> changed)
        {
            if (changed.Behaviour)
            {
                changed.Behaviour.OnCurrentWindowChanged();
            }
        }

        public void SetCurrentWindowType(WindowType windowType)
        {
            CurrentWindow = windowType;
        }

        private void OnCurrentWindowChanged()
        {
            if (!Object.HasInputAuthority)
            {
                return;
            }

            foreach (var windowType in Enum.GetValues(typeof(WindowType)).Cast<WindowType>())
            {
                windowsHolder.GetWindow(windowType)?.Hide();
            }

            if (CurrentWindow != WindowType.NONE)
            {
                windowsHolder.GetWindow(CurrentWindow)?.Show();
            }
        }
    }
}
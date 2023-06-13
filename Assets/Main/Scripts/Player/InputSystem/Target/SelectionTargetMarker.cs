using System;
using UnityEngine;

namespace Main.Scripts.Player.InputSystem.Target
{
    public class SelectionTargetMarker : MonoBehaviour
    {
        [SerializeField]
        private MeshRenderer marker = default!;

        private void Awake()
        {
            SetTargetFocusState(TargetFocusState.NONE);
        }

        public void SetTargetFocusState(TargetFocusState targetFocusState)
        {
            switch (targetFocusState)
            {
                case TargetFocusState.NONE:
                    marker.enabled = false;
                    break;
                case TargetFocusState.FOCUSED:
                    marker.enabled = true;
                    break;
                case TargetFocusState.SELECTED:
                    marker.enabled = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(targetFocusState), targetFocusState, null);
            }
        }
    }
}
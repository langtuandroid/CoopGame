using UnityEngine;

namespace Main.Scripts.Utils
{
    public static class CursorUtils
    {
        public static void getCursorWorldPosition(Camera camera, Plane plane, out Vector3 cursorPosition)
        {
            var ray = camera.ScreenPointToRay(Input.mousePosition);
            cursorPosition = plane.Raycast(ray, out var cursorDistance) ? ray.GetPoint(cursorDistance) : Vector3.zero;
        }
    }
}
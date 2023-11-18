using System;
using UnityEngine;

namespace Main.Scripts.Utils
{
public static class CursorUtils
{
    public static void GetCursorWorldPosition(Camera camera, Plane plane, out Vector3 cursorPosition)
    {
        var ray = camera.ScreenPointToRay(Input.mousePosition);
        cursorPosition = plane.Raycast(ray, out var cursorDistance) ? ray.GetPoint(cursorDistance) : Vector3.zero;
    }

    public static Vector3 GetCursorOffsetNormalized()
    {
        var mousePosition = Input.mousePosition;
        var minScreenSize = Math.Min(Screen.width, Screen.height);
        return new Vector3(
            (mousePosition.x - Screen.width / 2f) / minScreenSize * 2,
            0,
            (mousePosition.y - Screen.height / 2f) / minScreenSize * 2
        );
    }
}
}
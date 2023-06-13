using UnityEngine;

namespace Main.Scripts.Player.InputSystem
{
    public static class MousePositionHelper
    {
        public static Vector3 GetMapPoint(LayerMask mouseRayMask)
        {
            Vector3 mousePos = Input.mousePosition;

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            
            Vector3 mouseCollisionPoint = Vector3.zero;
            // Raycast towards the mouse collider box in the world
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, mouseRayMask))
            {
                if (hit.collider != null)
                {
                    mouseCollisionPoint = hit.point;
                }
            }

            return mouseCollisionPoint;
        }
    }
}
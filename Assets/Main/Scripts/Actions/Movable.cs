using UnityEngine;

namespace Main.Scripts.Actions
{
    public interface Movable
    {
        Vector3 GetMovingDirection();
        void Move(Vector3 direction);
    }
}
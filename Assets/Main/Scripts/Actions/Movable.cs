using UnityEngine;

namespace Main.Scripts.Actions
{
    public interface Movable
    {
        Vector3 GetMovingDirection();
        void Move(ref Vector3 direction);
    }
}
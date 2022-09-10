using UnityEngine;

namespace Main.Scripts.Actions
{
    public interface ObjectWithGettingKnockBack
    {
        void ApplyKnockBack(Vector3 direction);
    }
}
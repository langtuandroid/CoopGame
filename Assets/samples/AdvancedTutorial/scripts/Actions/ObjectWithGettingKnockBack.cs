using UnityEngine;

namespace Bolt.samples.AdvancedTutorial.scripts.Actions
{
    public interface ObjectWithGettingKnockBack
    {
        void ApplyKnockBack(Vector3 direction, float force);
    }
}
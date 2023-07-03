using UnityEngine;

namespace Main.Scripts.Actions
{
    public interface Dashable
    {
        void Dash(ref Vector3 direction, float speed, float durationSec);
    }
}
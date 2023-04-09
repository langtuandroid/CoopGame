using UnityEngine;

namespace Main.Scripts.Actions
{
    public interface Dashable
    {
        void Dash(Vector3 direction, float speed, float durationSec);
    }
}
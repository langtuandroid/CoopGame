using Main.Scripts.LevelGeneration.Data.Colliders;
using UnityEngine;

namespace Main.Scripts.LevelGeneration.Data
{
public class PlaceTargetData
{
    public Vector3 Position { get; }
    public ColliderInfo ColliderInfo { get; }

    public PlaceTargetData(
        Vector3 position,
        ColliderInfo colliderInfo
    )
    {
        Position = position;
        ColliderInfo = colliderInfo;
    }
}
}
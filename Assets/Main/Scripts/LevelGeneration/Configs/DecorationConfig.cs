using Main.Scripts.LevelGeneration.Data.Colliders;
using UnityEngine;

namespace Main.Scripts.LevelGeneration.Configs
{
[CreateAssetMenu(fileName = "DecorationConfig", menuName = "Level/Decorations/DecorationConfig")]
public class DecorationConfig : ScriptableObject
{
    [SerializeField]
    private GameObject decorationPrefab = null!;
    [SerializeField]
    private ColliderInfo colliderInfo;

    public GameObject DecorationPrefab => decorationPrefab;
    public ColliderInfo ColliderInfo => colliderInfo;
}
}
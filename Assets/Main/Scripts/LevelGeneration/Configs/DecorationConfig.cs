using UnityEngine;

namespace Main.Scripts.LevelGeneration.Configs
{
[CreateAssetMenu(fileName = "DecorationConfig", menuName = "Level/Decorations/DecorationConfig")]
public class DecorationConfig : ScriptableObject
{
    [SerializeField]
    private GameObject decorationPrefab = null!;
    [SerializeField]
    private Vector2 bounds;

    public GameObject DecorationPrefab => decorationPrefab;
    public Vector2 Bounds => bounds;
}
}
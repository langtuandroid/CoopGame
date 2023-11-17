using System.Collections.Generic;
using UnityEngine;

namespace Main.Scripts.LevelGeneration.Configs
{
[CreateAssetMenu(fileName = "DecorationsPack", menuName = "Level/Decorations/DecorationsPack")]
public class DecorationsPack : ScriptableObject
{
    [SerializeField]
    private List<DecorationConfig> bigDecorations = null!;
    [SerializeField]
    private List<DecorationConfig> mediumDecorations = null!;
    [SerializeField]
    private List<DecorationConfig> smallDecorations = null!;

    public List<DecorationConfig> BigDecorations => bigDecorations;
    public List<DecorationConfig> MediumDecorations => mediumDecorations;
    public List<DecorationConfig> SmallDecorations => smallDecorations;
}
}
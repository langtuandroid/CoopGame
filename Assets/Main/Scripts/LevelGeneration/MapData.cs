using System.Collections.Generic;
using Main.Scripts.LevelGeneration.Places;

namespace Main.Scripts.LevelGeneration
{
public struct MapData
{
    public List<Place> Places;
    public List<KeyValuePair<int, int>> Roads;
}
}
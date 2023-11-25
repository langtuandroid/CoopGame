using System.Collections.Generic;
using Main.Scripts.LevelGeneration.Places;

namespace Main.Scripts.LevelGeneration
{
public class MapGraph
{
    public List<Place> Places { get; }
    public List<RoadData> Roads { get; }

    public MapGraph(
        List<Place> places,
        List<RoadData> roads
    )
    {
        Places = places;
        Roads = roads;
    }
}
}
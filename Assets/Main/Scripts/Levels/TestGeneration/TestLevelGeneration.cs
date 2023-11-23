using Main.Scripts.LevelGeneration.Configs;
using Main.Scripts.Levels.Map;
using UnityEngine;

namespace Main.Scripts.Levels.TestGeneration
{
public class TestLevelGeneration : MonoBehaviour
{
    [SerializeField]
    private LevelGenerationConfig levelGenerationConfig = null!;
    [SerializeField]
    private LevelStyleConfig levelStyleConfig = null!;
    [SerializeField]
    private AstarPath pathfinder = default!;
    [SerializeField]
    private float visibilityBoundsSize = 25;

    private LevelMapController levelMapController = null!;
    private Vector2 visibilityPosition;

    private void Awake()
    {
        levelMapController = new LevelMapController(levelGenerationConfig, levelStyleConfig, pathfinder);

        var seed = (int)(Random.value * int.MaxValue);
        Debug.Log($"Seed: {seed}");
        Generate(seed);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var seed = (int)(Random.value * int.MaxValue);
            Debug.Log($"Seed: {seed}");
            Generate(seed);
        }

        var speed = 10f;
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            visibilityPosition += Vector2.left * speed;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            visibilityPosition += Vector2.right * speed;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            visibilityPosition += Vector2.up * speed;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            visibilityPosition += Vector2.down * speed;
        }


        var halfBoundsSize = visibilityBoundsSize / 2;

        levelMapController.UpdateChunksVisibilityBounds(
            visibilityPosition.x - halfBoundsSize,
            visibilityPosition.x + halfBoundsSize,
            visibilityPosition.y - halfBoundsSize,
            visibilityPosition.y + halfBoundsSize
        );
    }

    private void Generate(int seed)
    {
        levelMapController.GenerateMap(seed);
    }
}
}
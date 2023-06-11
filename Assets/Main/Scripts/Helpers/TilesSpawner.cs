using UnityEngine;

namespace Main.Scripts.Helpers
{
    public class TilesSpawner : MonoBehaviour
    {
        [SerializeField]
        private Object grassPrefab = default!;

        [SerializeField]
        private int tileSize = 10;

        [SerializeField]
        private int widthCount = 1;

        [SerializeField]
        private int heightCount = 1;

        [SerializeField]
        private int leftDownX;
        [SerializeField]
        private int leftDownZ;

        private void Awake()
        {
            for (var i = 0; i < widthCount; i++)
            {
                for (var j = 0; j < heightCount; j++)
                {
                    Instantiate(
                        grassPrefab,
                        new Vector3(leftDownX + (i + 0.5f) * tileSize, 0f, leftDownZ + (j + 0.5f) * tileSize),
                        Quaternion.identity
                    );
                }
            }
        }
    }
}
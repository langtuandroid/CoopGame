using System.Collections.Generic;
using UnityEngine;

namespace Main.Scripts.Helpers
{
    public class TilesSpawner : MonoBehaviour
    {
        [SerializeField]
        private List<Object> decorationPrefabs = new();
        [SerializeField]
        private float decorationsDensity = 0.2f;
        
        [SerializeField]
        private Object? grassPrefab;

        [SerializeField]
        private int tileSize = 10;

        [SerializeField]
        private int rowsCount = 1;

        [SerializeField]
        private int columnsCount = 1;

        [SerializeField]
        private int leftDownX;
        [SerializeField]
        private int leftDownZ;

        private void Awake()
        {
            var width = tileSize * rowsCount;
            var height = tileSize * columnsCount;
            var decorationsCount = width * height * decorationsDensity;
            if (decorationPrefabs.Count > 0)
            {
                for (var i = 0; i < decorationsCount; i++)
                {
                    Instantiate(
                        original: decorationPrefabs[Random.Range(0, decorationPrefabs.Count)],
                        position: new Vector3(leftDownX + Random.Range(0f, width), 0f, leftDownZ + Random.Range(0f, height)),
                        rotation: Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
                    );
                }
            }

            if (grassPrefab != null)
            {
                for (var i = 0; i < rowsCount; i++)
                {
                    for (var j = 0; j < columnsCount; j++)
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
}
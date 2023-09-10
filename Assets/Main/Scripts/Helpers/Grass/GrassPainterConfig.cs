using UnityEngine;

namespace Main.Scripts.Helpers.Grass
{
    [CreateAssetMenu(fileName = "GrassPainterConfig", menuName = "Grass/GrassPainterConfig")]
    public class GrassPainterConfig : ScriptableObject
    {
        [SerializeField]
        private float force;

        public float Force => force;
    }
}
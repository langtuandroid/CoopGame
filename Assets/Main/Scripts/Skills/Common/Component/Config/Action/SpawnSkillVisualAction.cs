using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Action
{
    [CreateAssetMenu(fileName = "SpawnVisualAction", menuName = "Skill/Action/SpawnVisual")]
    public class SpawnSkillVisualAction : SpawnSkillActionBase
    {
        [SerializeField]
        private GameObject prefabToSpawn = default!;
        [SerializeField]
        private bool waitDespawnByAction;
        [SerializeField]
        private float moveSpeed;

        public GameObject PrefabToSpawn => prefabToSpawn;
        public bool WaitDespawnByAction => waitDespawnByAction;
        public float MoveSpeed => moveSpeed;
    }
}
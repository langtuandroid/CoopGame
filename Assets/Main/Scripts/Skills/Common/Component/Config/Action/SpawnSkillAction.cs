using Fusion;
using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Action
{
    [CreateAssetMenu(fileName = "SpawnAction", menuName = "Skill/Action/Spawn")]
    public class SpawnSkillAction : SkillActionBase
    {
        [SerializeField]
        private NetworkObject prefabToSpawn = default!;
        [SerializeField]
        private SkillPointType spawnPointType;
        [SerializeField]
        private SkillDirectionType spawnDirectionType;

        public NetworkObject PrefabToSpawn => prefabToSpawn;
        public SkillPointType SpawnPointType => spawnPointType;
        public SkillDirectionType SpawnDirectionType => spawnDirectionType;
    }
}
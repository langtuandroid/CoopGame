using Fusion;
using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Action
{
    [CreateAssetMenu(fileName = "SpawnPrefabAction", menuName = "Skill/Action/SpawnPrefab")]
    public class SpawnPrefabSkillAction : SpawnSkillActionBase
    {
        [SerializeField]
        private NetworkObject prefabToSpawn = default!;

        public NetworkObject PrefabToSpawn => prefabToSpawn;
    }
}
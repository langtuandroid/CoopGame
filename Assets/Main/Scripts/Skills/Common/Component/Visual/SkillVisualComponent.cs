using Main.Scripts.Skills.Common.Component.Config.Action;
using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Visual
{
    public class SkillVisualComponent : MonoBehaviour
    {
        private SpawnSkillVisualAction skillVisualConfig = default!;

        public void Init(SpawnSkillVisualAction skillVisualConfig)
        {
            this.skillVisualConfig = skillVisualConfig;
        }

        private void Update()
        {
            transform.position += skillVisualConfig.MoveSpeed * Time.deltaTime * transform.forward;
        }
    }
}
using System;
using Fusion;
using Main.Scripts.Room;
using Main.Scripts.Utils;
using UnityEngine;

namespace Main.Scripts.Skills
{
    public class SkillInfoHolder : MonoBehaviour
    {
        [SerializeField]
        private SkillInfo healthBoostInfo = default!;
        [SerializeField]
        private SkillInfo damageBoostInfo = default!;
        [SerializeField]
        private SkillInfo speedBoostInfo = default!;

        private ConnectionManager connectionManager = default!;

        private void Awake()
        {
            DontDestroyOnLoad(this);

            connectionManager = FindObjectOfType<ConnectionManager>().ThrowWhenNull();
            connectionManager.OnPlayerDisconnectEvent.AddListener(OnPlayerDisconnect); //todo subscribe on disconnect
        }

        public SkillInfo GetSkillInfo(SkillType skillType)
        {
            return skillType switch
            {
                SkillType.HEALTH_BOOST_PASSIVE => healthBoostInfo,
                SkillType.DAMAGE_BOOST_PASSIVE => damageBoostInfo,
                SkillType.SPEED_BOOST_PASSIVE => speedBoostInfo,
                _ => throw new ArgumentOutOfRangeException(nameof(skillType), skillType, "Skill is not allowed")
            };
        }

        private void OnPlayerDisconnect(NetworkRunner runner, PlayerRef playerRef)
        {
            if (runner.LocalPlayer == playerRef)
            {
                connectionManager.OnPlayerDisconnectEvent.RemoveListener(OnPlayerDisconnect);
                Destroy(this);
            }
        }
    }
}
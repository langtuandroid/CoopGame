using Fusion;
using Main.Scripts.Player;
using UnityEngine;

namespace Main.Scripts.Weapon
{
    public class SkillManager : NetworkBehaviour
    {
        public enum WeaponType
        {
            PRIMARY
        }

        [SerializeField]
        private SkillBase primarySkillBase;

        [SerializeField]
        private PlayerController _player;

        /// <summary>
        /// Fire the current weapon. This is called from the Input Auth Client and on the Server in
        /// response to player input. Input Auth Client spawns a dummy shot that gets replaced by the networked shot
        /// whenever it arrives
        /// </summary>
        public void FireWeapon(WeaponType weaponType)
        {
            if (!IsWeaponFireAllowed(weaponType))
                return;

            var weapon = primarySkillBase;
            weapon.Activate(Object.InputAuthority);
        }

        private bool IsWeaponFireAllowed(WeaponType weaponType)
        {
            if (!_player.isActivated)
                return false;

            return true;
        }
    }
}
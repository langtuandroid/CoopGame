using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Main.Scripts.Weapon
{
    public abstract class SkillBase : NetworkBehaviour
    {
        public abstract void Activate(PlayerRef owner);
    }
}
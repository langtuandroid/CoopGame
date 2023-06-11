using System;
using System.Collections.Generic;
using Main.Scripts.Modifiers;

namespace Main.Scripts.Skills.Common.Component.Config
{
    [Serializable]
    public struct ModifiableList<T>
    {
        public ModifierId ModifierId;
        public List<T> ItemsToApply;
    }
}
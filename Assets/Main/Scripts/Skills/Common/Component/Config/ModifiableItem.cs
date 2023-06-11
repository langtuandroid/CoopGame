using System;
using Main.Scripts.Modifiers;

namespace Main.Scripts.Skills.Common.Component.Config
{
    [Serializable]
    public struct ModifiableItem<T>
    {
        public ModifierId ModifierId;
        public T ItemToApply;
    }
}
using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Main.Scripts.Skills.Common.Component.Config
{
    [Serializable]
    public struct SerializableList<T>
    {
        public List<T> Value;
    }
}
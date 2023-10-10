using System;
using System.Collections.Generic;

namespace Main.Scripts.Utils
{
    [Serializable]
    public struct SerializableList<T>
    {
        public List<T> Value;
    }
}
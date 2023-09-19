using UnityEngine;

namespace Main.Scripts.Modifiers
{
    [CreateAssetMenu(fileName = "ModifierId", menuName = "Modifiers/Id")]
    public class ModifierId : ScriptableObject
    {
        private string id = "";
        public string Id => id;

        public void OnEnable()
        {
            id = name;
        }
    }
}
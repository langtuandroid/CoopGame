using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Main.Scripts.Gui
{
    public class HealthBar : MonoBehaviour
    {
        [FormerlySerializedAs("slider")] [SerializeField]
        private Slider healthSlider;
        [FormerlySerializedAs("text")] [SerializeField]
        private Text healthText;

        public void SetMaxHealth(int health)
        {
            healthSlider.maxValue = health;
            healthSlider.value = health;
            healthText.text = healthSlider.value + " / " + healthSlider.maxValue;
        }
    
        public void SetHealth(int health)
        {
            healthSlider.value = health;
            healthText.text = healthSlider.value + " / " + healthSlider.maxValue;
        }

        private void Update()
        {
            transform.LookAt(transform.position + Camera.main.transform.forward);
        }
    }
}
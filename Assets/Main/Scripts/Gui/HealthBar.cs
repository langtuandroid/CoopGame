using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Main.Scripts.Gui
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField]
        private Slider healthSlider = default!;
        [SerializeField]
        private Text healthText= default!;

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

        private void LateUpdate()
        {
            transform.LookAt(transform.position + Camera.main.transform.forward);
        }
    }
}
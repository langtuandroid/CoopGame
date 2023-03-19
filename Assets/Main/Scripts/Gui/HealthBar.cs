using UnityEngine;
using UnityEngine.UI;

namespace Main.Scripts.Gui
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField]
        private Slider healthSlider = default!;
        [SerializeField]
        private Text healthText= default!;

        public void SetMaxHealth(uint health)
        {
            healthSlider.maxValue = health;
            healthSlider.value = health;
            healthText.text = healthSlider.value + " / " + healthSlider.maxValue;
        }
    
        public void SetHealth(uint health)
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
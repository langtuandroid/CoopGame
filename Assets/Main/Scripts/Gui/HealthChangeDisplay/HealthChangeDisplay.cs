using System;
using UnityEngine;
using UnityEngine.UI;

namespace Main.Scripts.Gui.HealthChangeDisplay
{
    public class HealthChangeDisplay : MonoBehaviour
    {
        [SerializeField]
        private Text healText = default!;
        [SerializeField]
        private Text damageText = default!;

        private float finishTime;
        private float textSpeed;

        public Action<HealthChangeDisplay>? OnDisplayDisableAction;

        private void Update()
        {
            if (finishTime < Time.time)
            {
                OnDisplayDisableAction?.Invoke(this);
                gameObject.SetActive(false);
            }

            transform.position = new Vector3(transform.position.x, transform.position.y + textSpeed * Time.deltaTime,
                transform.position.z);
        }

        private void LateUpdate()
        {
            transform.LookAt(transform.position + Camera.main!.transform.forward);
        }

        public void SetDamage(float damage)
        {
            damageText.text = damage == 0f ? "" : damage.ToString();
        }

        public void SetHeal(float heal)
        {
            healText.text = heal == 0f ? "" : heal.ToString();
        }

        public void SetTextSpeed(float speed)
        {
            textSpeed = speed;
        }

        public void SetTimer(float timer)
        {
            finishTime = Time.time + timer;
        }

        public void SetActive()
        {
            gameObject.SetActive(true);
            transform.position = transform.parent.position;
        }

        private void OnDisable()
        {
            OnDisplayDisableAction = null;
        }
    }
}
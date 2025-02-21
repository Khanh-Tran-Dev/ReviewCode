using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SurZom.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameObject damagePopupPrefab;
        [SerializeField] private GameObject healPopupPrefab;
        [SerializeField] private Slider healthBar;
        private HealthSystem healthSystem;

        private void Start()
        {
            healthSystem = GetComponent<HealthSystem>();
            healthSystem.OnHealthChanged += UpdateHealthBar;
        }

        private void UpdateHealthBar(int currentHealth)
        {
            healthBar.value = (float)currentHealth / healthSystem.GetCurrentHealth();
        }

        public void ShowDamagePopup(Vector3 position, int damage)
        {
            GameObject popup = Instantiate(damagePopupPrefab, position, Quaternion.identity);
            popup.GetComponentInChildren<TextMeshProUGUI>().text = "-" + damage.ToString();
        }

        public void ShowHealPopup(Vector3 position, int healAmount)
        {
            GameObject popup = Instantiate(healPopupPrefab, position, Quaternion.identity);
            popup.GetComponentInChildren<TextMeshProUGUI>().text = "+" + healAmount.ToString();
        }
    }
}


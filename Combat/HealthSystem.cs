using UnityEngine;
using System;

namespace SurZom.Combat
{
    public class HealthSystem : MonoBehaviour
    {
        public event Action<int> OnHealthChanged;
        public event Action OnDeath;
        private CharacterStat stats;
        private int currentHealth;

        private void Start()
        {
            stats = GetComponent<CharacterStat>();
            currentHealth = stats.MaxHealth;
        }

        public void TakeDamage(int damage, bool isMagic = false)
        {
            int finalDamage = isMagic
                ? Mathf.CeilToInt(damage * 40f / (40f + stats.MagicResist))
                : Mathf.CeilToInt(damage * 40f / (40f + stats.DefensePower));

            currentHealth = Mathf.Max(currentHealth - finalDamage, 0);
            OnHealthChanged?.Invoke(currentHealth);

            if (currentHealth == 0)
            {
                if (stats.CanRevive)
                {
                    Revive();
                }
                else
                {
                    OnDeath?.Invoke();
                }
            }
        }

        public void Heal(int amount)
        {
            currentHealth = Mathf.Min(currentHealth + amount, stats.MaxHealth);
            OnHealthChanged?.Invoke(currentHealth);
        }

        private void Revive()
        {
            stats.SetRevive(false);
            Heal(stats.MaxHealth * 7 / 10);
        }

        public int GetCurrentHealth() => currentHealth;
    }
}


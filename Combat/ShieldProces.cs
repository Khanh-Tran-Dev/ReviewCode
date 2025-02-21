using System.Collections.Generic;
using UnityEngine;
using SurZom.Core;

namespace SurZom.Combat
{
    public class ShieldProces : MonoBehaviour
    {
        [SerializeField] private List<Shield> shieldList = new();
        [SerializeField] private float totalShield;
        [SerializeField] private GameObject shieldPrefab;
        [SerializeField] private GameObject shield;

        private HealthBar healthBar;
        private Vector3 shieldDisplayScale;
        private Vector3 shieldDisplayPosition;
        private float spriteWidth;

        private void Awake()
        {
            shield = Instantiate(shieldPrefab, transform.position + new Vector3(0, 3, -2f), Quaternion.identity, transform);
            GameLevel.EndBattle += DestroyShield;
            var stat = GetComponent<Stat>();
            stat.Created += InitializeHealthBar;
            stat.SetShieldProcess(this);
        }

        private void FixedUpdate()
        {
            if (healthBar == null) return;
            UpdateShieldStatus();
            ShieldBarProcess();
        }

        private void UpdateShieldStatus()
        {
            totalShield = 0;
            shieldList.RemoveAll(shield => shield.timeRemain <= 0);

            foreach (var shield in shieldList)
            {
                shield.UpdateShield();
                totalShield += shield.shieldAmount;
            }

            shield.SetActive(totalShield > 0);
            if (totalShield > 0)
            {
                shield.transform.position = transform.position + new Vector3(0, 3, -2f);
            }
        }

        private void ShieldBarProcess()
        {
            float currentHealth = GetComponent<Stat>().GetCurrentHealth();
            float maxHealth = GetComponent<Stat>().GetMaxHealth();
            float totalLife = currentHealth + totalShield;
            var shieldSlider = healthBar.shieldImageSlider;
            var shieldRect = shieldSlider.GetComponent<RectTransform>();

            shieldSlider.gameObject.SetActive(totalShield > 0);

            if (totalShield <= 0) return;

            float shieldFraction = totalShield / Mathf.Max(totalLife, maxHealth);
            shieldRect.localScale = new Vector2(shieldFraction, shieldDisplayScale.y);

            float fullLength = healthBar.rear.localPosition.x - healthBar.front.localPosition.x;
            if (currentHealth == maxHealth)
            {
                shieldSlider.transform.localPosition = new Vector3(
                    healthBar.rear.localPosition.x - (fullLength * shieldFraction),
                    shieldDisplayPosition.y, shieldDisplayPosition.z);
            }
            else
            {
                float handlePosX = healthBar.handle.localPosition.x;
                float positionX = (totalLife > maxHealth) ?
                    healthBar.rear.localPosition.x - (fullLength * shieldFraction) / 2 :
                    handlePosX + (fullLength * shieldFraction) / 2;

                shieldSlider.transform.localPosition = new Vector3(positionX, shieldDisplayPosition.y, shieldDisplayPosition.z);
            }
        }

        public int ShieldDamage(int damage)
        {
            if (totalShield == 0) return damage;

            foreach (var shield in new List<Shield>(shieldList))
            {
                float remainingDamage = shield.TakeDamage(damage);
                if (shield.shieldAmount <= 0)
                {
                    shieldList.Remove(shield);
                }
                if (remainingDamage <= 0) return 0;
                damage = Mathf.CeilToInt(remainingDamage);
            }
            return damage;
        }

        private void InitializeHealthBar()
        {
            healthBar = GetComponent<Stat>().healthBar.GetComponent<HealthBar>();
            shieldDisplayScale = healthBar.shieldImageSlider.GetComponent<RectTransform>().localScale;
            shieldDisplayPosition = healthBar.shieldImageSlider.transform.localPosition;
            spriteWidth = Mathf.Abs(healthBar.rear.localPosition.x - healthBar.front.localPosition.x) / 2;
        }

        private void DestroyShield()
        {
            shieldList.Clear();
            if (shield != null) shield.SetActive(false);
        }
    }
}


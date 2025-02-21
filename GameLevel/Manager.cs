using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurZom.Controller;
using TMPro;

namespace SurZom.Core
{
    public class Manager : MonoBehaviour
    {
        [SerializeField] GameObject battleButton;
        [SerializeField] TextMeshProUGUI goldPlace;
        [SerializeField] TextMeshProUGUI diamondPlace;

        private int gold = 10000;
        private int diamond = 0;

        private void Start()
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            goldPlace.text = gold.ToString();
            diamondPlace.text = diamond.ToString();
        }

        public void AdjustGold(int amount)
        {
            gold += amount;
            UpdateUI();
        }

        public void ReadyToStart()
        {
            battleButton.SetActive(true);
        }

        public void StartBattle()
        {
            battleButton.SetActive(false);
            FindObjectOfType<GameLevel>().NewLevel();
        }

        public int GetGold()
        {
            return gold;
        }

        public void AdjustDiamond(int amount)
        {
            diamond += amount;
            UpdateUI();
        }

        public int GetDiamond()
        {
            return diamond;
        }
    }
}



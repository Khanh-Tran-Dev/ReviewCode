using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurZom.Combat;

namespace SurZom.Core
{
    public class GameLevelManager : MonoBehaviour
    {
        [SerializeField] private int level = 0;
        [SerializeField] private List<GameObject> enemyPrefabs;
        [SerializeField] private List<GameObject> bossPrefabs;
        [SerializeField] private List<Transform> spawnPoints;
        [SerializeField] private Stat player;

        private Dictionary<int, GameObject> enemyDictionary = new Dictionary<int, GameObject>();
        private List<GameObject> currentEnemies = new List<GameObject>();
        private List<GameObject> currentBosses = new List<GameObject>();
        private List<GameObject> enemyPool = new List<GameObject>();

        public delegate void BattleEvent();
        public static event BattleEvent OnBattleStart;
        public static event BattleEvent OnBattleEnd;

        private bool isBattleActive = true;

        private void Start()
        {
            InitializeEnemyDictionary();
            StartNewLevel();
        }

        private void Update()
        {
            if (currentEnemies.Count == 0 && currentBosses.Count == 0 && isBattleActive)
            {
                EndBattle();
            }
        }

        private void InitializeEnemyDictionary()
        {
            for (int i = 0; i < enemyPrefabs.Count; i++)
            {
                enemyDictionary[i + 1] = enemyPrefabs[i];
            }
        }

        private void StartNewLevel()
        {
            OnBattleStart?.Invoke();
            isBattleActive = true;
            level++;

            var enemyComposition = GetEnemyCompositionForLevel(level);
            GenerateEnemyPool(enemyComposition);
            SpawnEnemies();
        }

        private (int x, int y, int z) GetEnemyCompositionForLevel(int level)
        {
            if (level <= 1) return (1, 0, 0);
            if (level <= 3) return (1, 2, 0);
            if (level <= 6) return (1, 2, 3);
            if (level <= 11) return (2, 3, 4);
            if (level <= 16) return (3, 4, 5);
            if (level <= 21) return (4, 5, 6);
            if (level <= 26) return (5, 6, 7);
            if (level <= 31) return (6, 7, 8);
            if (level <= 36) return (7, 8, 9);
            return (8, 9, 10);
        }

        private void GenerateEnemyPool((int x, int y, int z) composition)
        {
            enemyPool.Clear();
            int totalValue = level * 5;
            int valueX = totalValue / composition.x;
            int valueY = composition.y == 0 ? 0 : Random.Range(1, 7);
            int remaining = totalValue - (valueX * composition.x + valueY * composition.y);
            int valueZ = (composition.z == 0 || remaining <= 0) ? 0 : remaining / composition.z;

            AddEnemiesToPool(composition.x, valueX);
            AddEnemiesToPool(composition.y, valueY);
            AddEnemiesToPool(composition.z, valueZ);
        }

        private void AddEnemiesToPool(int enemyType, int count)
        {
            if (enemyType == 0 || !enemyDictionary.ContainsKey(enemyType)) return;
            for (int i = 0; i < count; i++)
            {
                enemyPool.Add(enemyDictionary[enemyType]);
            }
        }

        private void SpawnEnemies()
        {
            foreach (var enemy in enemyPool)
            {
                int spawnIndex = Random.Range(0, spawnPoints.Count);
                var spawnedEnemy = Instantiate(enemy, spawnPoints[spawnIndex].position, Quaternion.identity);
                currentEnemies.Add(spawnedEnemy);
            }

            if (level % 10 == 0)
            {
                SpawnBoss();
            }

            GetComponent<WarnProcess>().ProcessWarning();
        }

        private void SpawnBoss()
        {
            int spawnIndex = Random.Range(0, spawnPoints.Count);
            int bossIndex = Random.Range(0, bossPrefabs.Count);
            var boss = Instantiate(bossPrefabs[bossIndex], spawnPoints[spawnIndex].position, Quaternion.identity);
            Stat bossStat = boss.GetComponent<Stat>();
            ScaleBossStats(bossStat);
            currentBosses.Add(boss);
        }

        private void ScaleBossStats(Stat bossStat)
        {
            bossStat.SetMaxHealth(bossStat.GetMaxHealth() * level);
            bossStat.SetAttackPower(bossStat.GetAttackPower() * level);
            bossStat.SetMagicPower(bossStat.GetMagicPower() * level);
            bossStat.SetDefensePower(bossStat.GetDefensePower() * level / 2);
            bossStat.SetMagicResist(bossStat.GetMagicResist() * level / 2);
        }

        private void EndBattle()
        {
            OnBattleEnd?.Invoke();
            isBattleActive = false;
            if (level > 0)
            {
                player.LevelUp();
            }
        }

        public void RemoveEnemy(GameObject enemy)
        {
            currentEnemies.Remove(enemy);
        }

        public void RemoveBoss(GameObject boss)
        {
            currentBosses.Remove(boss);
        }
    }
}


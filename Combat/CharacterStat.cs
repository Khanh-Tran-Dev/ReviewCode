using UnityEngine;

namespace SurZom.Combat
{
    public class CharacterStat : MonoBehaviour
    {
        public int Level { get; private set; }
        public int MaxHealth { get; private set; }
        public int Speed { get; private set; }
        public int AttackPower { get; private set; }
        public int MagicPower { get; private set; }
        public int DefensePower { get; private set; }
        public int MagicResist { get; private set; }
        public int AttackRange { get; private set; }
        public float AttackSpeed { get; private set; }
        public int Gold { get; private set; }
        public bool CanRevive { get; private set; }
        public int Slow { get; private set; }
        public bool CcResist { get; private set; }

        public void Initialize(int level, int maxHealth, int speed, int attackPower, int magicPower,
            int defensePower, int magicResist, int attackRange, float attackSpeed, int gold)
        {
            Level = level;
            MaxHealth = maxHealth;
            Speed = speed;
            AttackPower = attackPower;
            MagicPower = magicPower;
            DefensePower = defensePower;
            MagicResist = magicResist;
            AttackRange = attackRange;
            AttackSpeed = attackSpeed;
            Gold = gold;
            CanRevive = false;
            Slow = 0;
        }

        public void IncreaseStat(int hp, int atk, int def, int magicRes)
        {
            MaxHealth += hp;
            AttackPower += atk;
            DefensePower += def;
            MagicResist += magicRes;
        }

        public void SetRevive(bool status) => CanRevive = status;
        public void SetSlow(int value) => Slow = value;
    }
}


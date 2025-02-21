using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurZom.Controller;
using SurZom.Core;

namespace SurZom.Combat
{
    public class Buildings : MonoBehaviour
    {
        public static Buildings Instance { get; private set; }

        [SerializeField] public GameObject icon;
        [SerializeField] public string buildName;
        [SerializeField] private float reloadTime;
        [SerializeField] public Transform barrel;
        [SerializeField] public Transform rotater;
        [SerializeField] public GameObject fireEffect;
        [SerializeField] public GameObject levelUpEffect;
        [SerializeField] public AnimationClip attackClip;
        public int level = 1;
        [SerializeField] private int atkLevelUp;
        [SerializeField] private int apLevelUp;
        [SerializeField] private float atkSpdLevelUp;
        public Transform target;
        public bool skill = false;

        private float distance = Mathf.Infinity;
        private Animator ani;
        private Stat stat;
        private AttackProcess attackProcess;
        private Vision vision;
        private ButtonController controller;
        private PlayerController player;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            player = FindObjectOfType<PlayerController>();
            controller = FindObjectOfType<ButtonController>();
            ani = GetComponent<Animator>();
            attackProcess = GetComponent<AttackProcess>();
            stat = GetComponent<Stat>();
            vision = GetComponentInChildren<Vision>();
        }

        private void Update()
        {
            if (skill) return;

            if (reloadTime > 0) reloadTime -= Time.deltaTime;

            OnTargetEnemy();

            if (target == null) return;
            RotaterProcess();
        }

        private void FixedUpdate()
        {
            if (skill || target == null || reloadTime > 0) return;

            if (!DistanceCalculate()) return;

            AttackAnimation();
        }

        private void RotaterProcess()
        {
            if (rotater == null) return;

            Vector3 direction = target.position - rotater.position;
            direction.y = 0;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rotater.rotation = Quaternion.RotateTowards(rotater.rotation, targetRotation, 200 * Time.deltaTime);
        }

        private void AttackAnimation()
        {
            ani.SetTrigger("attack");
            reloadTime = stat.GetAtkSpeed();

            if (attackClip == null) return;

            if (stat.GetAtkSpeed() <= attackClip.length)
            {
                ani.speed = attackClip.length / stat.GetAtkSpeed();
            }
        }

        public void LevelUp()
        {
            level += 1;
            stat.SetGold(75);
            stat.SetAttackPower(atkLevelUp);
            stat.SetMagicPower(apLevelUp);
            stat.SetAtkSpeed(atkSpdLevelUp);

            if (levelUpEffect != null)
            {
                var x = Instantiate(levelUpEffect, transform.position, Quaternion.identity);
                Destroy(x, 1.5f);
            }
        }

        private void OnTargetEnemy()
        {
            if (vision.targets.Count == 0)
            {
                target = null;
                return;
            }

            foreach (Transform enemy in vision.targets)
            {
                if (enemy.GetComponent<Stat>().GetCurrentHealth() <= 0)
                {
                    vision.RemoveTarget(enemy);
                    target = null;
                    return;
                }

                float currentDistance = Vector3.Distance(transform.position, enemy.position);

                if (target == null || currentDistance < distance)
                {
                    distance = currentDistance;
                    target = enemy;
                }
            }
        }

        public void Fire()
        {
            if (fireEffect != null) Instantiate(fireEffect, barrel.position, Quaternion.identity);
            if (target == null) return;

            attackProcess.LauchProjectTitle(barrel, target);
        }

        public void ExitAttack()
        {
            ani.SetTrigger("reload");
        }

        private bool DistanceCalculate()
        {
            return Vector3.Distance(transform.position, target.position) <= stat.GetAttackRange();
        }

        public void Buff(int atk, float atkspd, int atkrg)
        {
            stat.SetAttackPower(atk);
            stat.SetAtkSpeed(atkspd);
            stat.SetAttackRange(atkrg);
        }

        public Buildings GetInfo()
        {
            return this;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.transform != player.transform) return;
            controller.OnBuildRange(this.gameObject);
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.transform != player.transform) return;
            controller.OutBuildRange();
        }
    }
}


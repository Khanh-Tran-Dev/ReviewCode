using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurZom.Combat;
using SurZom.Core;
using UnityEngine.AI;

namespace SurZom.Controller
{
    public class WarriorControllers : MonoBehaviour
    {
        public static WarriorControllers Instance { get; private set; }

        [SerializeField] private Transform weapon;
        [SerializeField] public AnimationClip attackClip;

        private float realSpeed;
        private Transform target;
        private Vision vision;
        private Animator animator;
        private AttackProcess attackProcess;
        private Stat stat;
        private NavMeshAgent nav;
        private TrainingBuilding building;
        private bool exitAttack = true;
        private bool attack;
        private bool stunned = false;
        private float timeBetweenEachAtk = 0;
        private float distance = Mathf.Infinity;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            nav = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            stat = GetComponent<Stat>();
            attackProcess = GetComponent<AttackProcess>();
        }

        private void Start()
        {
            vision = GetComponentInChildren<Vision>();
            attackProcess.SetInfo("Enemy", true);
            nav.speed = stat.GetSpeed();
            stat.StunnedEvent += Stunned;
            stat.ReleaseControlEvent += ReleaseControl;
        }

        private void Update()
        {
            if (!FindObjectOfType<GameLevel>().battle || stat.GetIsDead())
            {
                stat.OffHealthBar();
                Death();
                return;
            }

            if (timeBetweenEachAtk > 0) timeBetweenEachAtk -= Time.deltaTime;
            NearestEnemy();
            if (stunned) return;

            Action();
            SetAnimationMove();
        }

        private void FixedUpdate() => CheckSpeed();

        private void Action()
        {
            if (target == null) return;

            transform.rotation = GetQuaternion();
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            if (stat.GetAttackRange() >= distanceToTarget + 1)
            {
                AttackEnemy();
            }
            else if (stat.GetAttackRange() < distanceToTarget && exitAttack && !attack)
            {
                MoveToEnemy();
            }
        }

        private void AttackEnemy()
        {
            attack = true;
            nav.speed = 0;
            nav.velocity = Vector3.zero;
            nav.SetDestination(transform.position);

            if (timeBetweenEachAtk > 0) return;
            SetAnimationAttack();
        }

        private void MoveToEnemy()
        {
            OnNavMeshUpdate();
            nav.SetDestination(target.position);
            nav.speed = AdjustSpeedBasedOnAngle();
        }

        private float AdjustSpeedBasedOnAngle()
        {
            float y = transform.rotation.eulerAngles.y;
            y = y > 180f ? y - 360f : y;

            if (Mathf.Abs(y) <= 20f || Mathf.Abs(y) >= 160f) return stat.GetSpeed() * 1.15f;
            if (Mathf.Abs(y) <= 30f || Mathf.Abs(y) >= 150f) return stat.GetSpeed() * 1.1f;
            if (Mathf.Abs(y) <= 55f || Mathf.Abs(y) >= 125f) return stat.GetSpeed() * 1.05f;
            return stat.GetSpeed();
        }

        private void CheckSpeed()
        {
            nav.speed = vision.targets.Count == 0 ? 0 : realSpeed;
        }

        private Quaternion GetQuaternion()
        {
            if (target == null) return Quaternion.identity;
            Vector3 direction = target.position - transform.position;
            return Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), 500 * Time.deltaTime);
        }

        public void Fire()
        {
            if (target == null || !attack) return;
            attackProcess.LauchProjectTitle(weapon, target);
        }

        public void ExitAttack()
        {
            attack = false;
            exitAttack = true;
            animator.SetTrigger("exitAttack");
        }

        private void SetAnimationMove() => animator.SetFloat("speed", realSpeed);

        private void SetAnimationAttack()
        {
            animator.SetTrigger("attack");
            timeBetweenEachAtk = stat.GetAtkSpeed();
            if (attackClip == null) return;
            if (stat.GetAtkSpeed() <= attackClip.length)
            {
                animator.speed = attackClip.length / stat.GetAtkSpeed();
            }
        }

        public void SetBuilding(TrainingBuilding building) => this.building = building;

        private void NearestEnemy()
        {
            if (vision.targets.Count == 0)
            {
                target = null;
                return;
            }

            target = null;
            distance = Mathf.Infinity;

            foreach (Transform enemy in vision.targets)
            {
                if (enemy.GetComponent<Stat>().GetCurrentHealth() <= 0)
                {
                    vision.RemoveTarget(enemy);
                    ExitAttack();
                    continue;
                }

                float newDistance = Vector3.Distance(transform.position, enemy.position);
                if (newDistance < distance)
                {
                    distance = newDistance;
                    target = enemy;
                }
            }
        }

        public void Stunned()
        {
            stunned = true;
            nav.speed = 0;
        }

        public void ReleaseControl() => stunned = false;


        private void Death()
        {
            animator.SetTrigger("isDead");
            nav.speed = 0;
            Destroy(gameObject, 3f);
        }
    }
}


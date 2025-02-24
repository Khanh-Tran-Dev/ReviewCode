using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurZom.Combat;
using UnityEngine.AI;
using SurZom.Core;

namespace SurZom.Controller
{
    public class EnemyControllers : MonoBehaviour
    {
        [SerializeField] public Transform target;
        [SerializeField] private GameObject targetCircle;
        [SerializeField] private float realSpeed;
        [SerializeField] private Transform weapon;
        [SerializeField] public AnimationClip attackClip;

        public delegate void FireAction();
        public event FireAction Action;

        private Animator animator;
        private Vision vision;
        private PlayerController player;
        private AttackProcess attackProcess;
        private Stat stat;
        private NavMeshAgent nav;

        private float timeBetweenEachAtk = 0;
        private bool exitAttack = true;
        private bool stunned = false;
        public bool skillReady = false;
        public bool lockRotate = false;

        private void Awake()
        {
            vision = GetComponentInChildren<Vision>();
            player = FindObjectOfType<PlayerController>();
            nav = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            stat = GetComponent<Stat>();
            attackProcess = GetComponent<AttackProcess>();
            target = player.transform;
        }

        private void Start()
        {
            attackProcess.SetInfo("Player", true);
            nav.speed = stat.GetSpeed();
            stat.StunnedEvent += Stunned;
            stat.ReleaseControlEvent += ReleaseControl;
        }

        private void FixedUpdate()
        {
            if (stat.GetIsDead())
            {
                Death();
                return;
            }

            if (timeBetweenEachAtk > 0) timeBetweenEachAtk -= Time.deltaTime;
            if (stunned) return;

            CheckIfThisIsTarget();
            CheckSpeed();
            ProcessAI();
            SetAnimationMove();
        }

        private void ProcessAI()
        {
            if (skillReady)
            {
                EngageTarget();
                return;
            }

            NearestTarget();
            ActionProcess();
        }

        private void EngageTarget()
        {
            target = player.transform;
            transform.rotation = GetQuaternion(target.position);
            nav.speed = 0;
        }

        private void ActionProcess()
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            if (stat.GetAttackRange() >= distanceToTarget)
            {
                AttackTarget();
            }
            else if (exitAttack)
            {
                ChaseTarget(distanceToTarget);
            }
        }

        private void AttackTarget()
        {
            transform.rotation = GetQuaternion(target.position);
            nav.velocity = Vector3.zero;
            nav.speed = 0;
            nav.SetDestination(transform.position);
            if (timeBetweenEachAtk > 0) return;
            SetAnimationAttack();
        }

        private void ChaseTarget(float distance)
        {
            nav.SetDestination(target.position);
            AdjustSpeedBasedOnAngle();
            nav.SetDestination(target.position);
        }

        private void AdjustSpeedBasedOnAngle()
        {
            float y = transform.rotation.eulerAngles.y;
            y = (y > 180f) ? y - 360f : y;
            float speedMultiplier = GetSpeedMultiplier(y);
            nav.speed = stat.GetSpeed() * speedMultiplier;
        }

        private float GetSpeedMultiplier(float y)
        {
            if (Mathf.Abs(y) <= 20f || Mathf.Abs(y) >= 160f) return 1.15f;
            if (Mathf.Abs(y) <= 30f) return 1.1f;
            if (Mathf.Abs(y) <= 55f) return 1.05f;
            return 1f;
        }

        private void CheckIfThisIsTarget()
        {
            targetCircle.SetActive(player.GetEnemy() == this);
        }

        private void CheckSpeed()
        {
            realSpeed = nav.speed;
        }

        private Quaternion GetQuaternion(Vector3 lookAt)
        {
            if (target == null) target = player.transform;
            if (lockRotate) return transform.rotation;

            Vector3 direction = lookAt - transform.position;
            return direction == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(direction);
        }

        public void Fire()
        {
            Action?.Invoke();
            attackProcess.LauchProjectTitle(weapon, target);
        }

        public void ExitAttack()
        {
            exitAttack = true;
        }

        private void SetAnimationMove()
        {
            animator.SetFloat("speed", realSpeed);
        }

        private void SetAnimationAttack()
        {
            exitAttack = false;
            animator.SetTrigger("attack");
            timeBetweenEachAtk = stat.GetAtkSpeed();
            if (attackClip == null) return;
            animator.speed = Mathf.Max(1, attackClip.length / stat.GetAtkSpeed());
        }

        private void NearestTarget()
        {
            if (vision.targets.Count == 0 || !stat.GetBeingAttack() || Vector3.Distance(transform.position, player.transform.position) < stat.GetAttackRange() + 10)
            {
                target = player.transform;
                return;
            }

            Transform nearestTarget = GetNearestTarget();
            target = nearestTarget ?? player.transform;
        }

        private Transform GetNearestTarget()
        {
            float nearestDistanceSqr = Mathf.Infinity;
            Transform nearestTarget = null;
            foreach (var enemy in vision.targets)
            {
                if (enemy == null || enemy.GetComponent<Stat>().GetCurrentHealth() <= 0)
                {
                    vision.RemoveTarget(enemy);
                    ExitAttack();
                    continue;
                }
                float distanceSqr = (enemy.position - transform.position).sqrMagnitude;
                if (distanceSqr < nearestDistanceSqr)
                {
                    nearestDistanceSqr = distanceSqr;
                    nearestTarget = enemy;
                }
            }
            return nearestTarget;
        }

        public void Stunned()
        {
            stunned = true;
            nav.speed = 0;
        }

        public void ReleaseControl()
        {
            stunned = false;
        }

        private void Death()
        {
            FindObjectOfType<GameLevel>().RemoveEnemy(gameObject);
            targetCircle.SetActive(false);
            nav.speed = 0;
            animator.SetTrigger("isDead");
            gameObject.tag = "Untagged";
            this.enabled = false;
            Destroy(gameObject, 3f);
        }
    }
}


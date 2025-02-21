using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurZom.Combat;
using UnityEngine.AI;

namespace SurZom.Controller
{
    public class PlayerControllers : MonoBehaviour
    {
        public static PlayerControllers Instance { get; private set; }

        [Header("Components")]
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Animator animator;
        [SerializeField] private Transform weapon;
        [SerializeField] private NavMeshAgent agent;

        [Header("Joysticks")]
        [SerializeField] private FixedJoystick moveJoystick;
        [SerializeField] private FixedJoystick attackJoystick;

        [Header("Combat Settings")]
        [SerializeField] private AnimationClip attackClip;
        [SerializeField] private float timeBetweenAttack = 0;
        [SerializeField] private bool reloadAttack = false;
        [SerializeField] private float reloadHit = 0;
        [SerializeField] private bool reloadingHit = false;

        [Header("Movement Settings")]
        [SerializeField] private float realSpeed;
        [SerializeField] private float damping = 0.1f;

        private Transform target;
        private Transform nearestEnemy;
        private float distance = Mathf.Infinity;
        private bool attack;
        private bool move = false;
        private bool skill = false;
        private bool stunned = false;
        private AttackProcess attackProcess;
        private Stat stat;
        private Vision vision;
        private RuntimeAnimatorController defaultAnimatorController;

        public delegate void AttackAction();
        public static event AttackAction StartAttack;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            defaultAnimatorController = animator.runtimeAnimatorController;
            vision = GetComponentInChildren<Vision>();
            attackProcess = GetComponent<AttackProcess>();
            attackProcess.SetInfo("Enemy", true);
            stat = GetComponent<Stat>();
            stat.StunnedEvent += Stunned;
            stat.ReleaseControlEvent += ReleaseControl;
        }

        private void FixedUpdate()
        {
            ReloadTimeBetweenAttack();
            NearestEnemy();
            ReloadHitProcess();
            if (skill || stunned) return;

            if (ChainOfAttackAction()) return;
            SetAnimationMove();
            MoveAction();
        }

        #region Movement
        private void MoveAction()
        {
            if (moveJoystick.onPointDown && moveJoystick.onDarg)
            {
                if (!move)
                {
                    move = true;
                    ExitAttack();
                }
                if (attack) return;

                Vector3 targetDirection = new Vector3(moveJoystick.Horizontal, 0, moveJoystick.Vertical).normalized;
                Vector3 targetPosition = transform.position + targetDirection * stat.GetSpeed() * damping;
                agent.speed = stat.GetSpeed();
                agent.acceleration = 100;
                agent.angularSpeed = 720;
                agent.destination = targetPosition;
                realSpeed = agent.velocity.magnitude;
                ExitAttack();
            }
            else
            {
                StopMovement();
            }
        }

        private void StopMovement()
        {
            if (agent.enabled)
            {
                agent.velocity = Vector3.zero;
                agent.speed = 0;
                agent.ResetPath();
            }
            realSpeed = Mathf.Lerp(realSpeed, 0, Time.deltaTime * 10);
            animator.SetFloat("speed", 0);
            move = false;
        }
        #endregion

        #region Combat
        private bool ChainOfAttackAction()
        {
            if (!attack || reloadAttack || reloadingHit || target == null) return false;
            Vector3 direction = target.position - transform.position;

            if (stat.GetAttackRange() >= Vector3.Distance(transform.position, target.position))
            {
                StopMovement();
                transform.rotation = Quaternion.LookRotation(direction);
                SetAnimationAttack();
                return true;
            }
            else
            {
                MoveTowardsTarget(direction);
                return true;
            }
        }

        private void MoveTowardsTarget(Vector3 direction)
        {
            transform.rotation = Quaternion.LookRotation(direction);
            transform.Translate(Vector3.forward * stat.GetSpeed() * Time.deltaTime);
            realSpeed = stat.GetSpeed();
        }

        public void AttackProcess()
        {
            attack = true;
            FindTarget();
        }

        public void Fire()
        {
            if (!attack || reloadingHit || target == null) return;
            reloadAttack = true;
            timeBetweenAttack = stat.GetAtkSpeed();
            attackProcess.LauchProjectTitle(weapon, target);
            StartAttack?.Invoke();
        }
        public void ExitAttack()
        {
            animator.SetTrigger("exitAttack");
            if (move) attack = false;
            else FindTarget();
        }
        private void ReloadTimeBetweenAttack()
        {
            if (timeBetweenAttack > 0) timeBetweenAttack -= Time.deltaTime;
            else
            {
                timeBetweenAttack = 0;
                reloadAttack = false;
            }
        }
        private void ReloadHitProcess()
        {
            if (reloadHit > 0)
            {
                reloadHit -= Time.deltaTime;
                animator.speed = 1;
            }
            if (reloadHit <= 0 && reloadingHit)
            {
                reloadHit = 0;
                reloadingHit = false;
                StartAttack?.Invoke();
            }
        }
        #endregion

        #region Utility
        private void FindTarget()
        {
            target = nearestEnemy;
        }

        private void NearestEnemy()
        {
            float distanceCalculate = Mathf.Infinity;
            if (vision.targets.Count <= 0)
            {
                ResetTarget();
                return;
            }

            foreach (var enemy in vision.targets)
            {
                if (enemy.GetComponent<Stat>().GetCurrentHealth() <= 0)
                {
                    vision.RemoveTarget(enemy);
                    continue;
                }
                float currentDistance = Vector3.Distance(transform.position, enemy.position);
                if (currentDistance < distanceCalculate)
                {
                    nearestEnemy = enemy;
                    target = attack ? nearestEnemy : null;
                    distanceCalculate = currentDistance;
                }
            }
            distance = distanceCalculate;
        }

        private void ResetTarget()
        {
            attack = false;
            nearestEnemy = null;
            target = null;
            distance = Mathf.Infinity;
        }
        #endregion

        #region Animation
        private void SetAnimationMove()
        {
            animator.speed = 1;
            animator.SetFloat("speed", realSpeed);
        }

        private void SetAnimationAttack()
        {
            if (target == null || target.GetComponent<Stat>().GetCurrentHealth() <= 0 || !attack)
            {
                animator.SetTrigger("exitAttack");
                return;
            }
            animator.SetTrigger("attack");
        }
        #endregion

        #region Action
        public void Stunned()
        {
            stunned = true;
        }
        public void ReleaseControl()
        {
            stunned = false;
        }
        #endregion
    }
}


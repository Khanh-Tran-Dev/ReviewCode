using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using SurZom.Combat;
using SurZom.Core;

namespace SurZom.Controller
{
    public class BossController : MonoBehaviour
    {
        public static BossController Instance { get; private set; }

        [Header("Boss Settings")]
        [SerializeField] private float standingTime = 5f;
        [SerializeField] private bool isMoving = false;
        [SerializeField] private bool onSkill = false;

        [Header("Skill Cooldowns & Stacks")]
        [SerializeField] private float skill1CD;
        [SerializeField] private float skill2CD;
        [SerializeField] private float skill3CD;
        [SerializeField] private int skill1Stack = 0;
        [SerializeField] private int skill2Stack = 0;

        [Header("Skill & Creep Prefabs")]
        [SerializeField] private GameObject skillPrefab;
        [SerializeField] private GameObject skill1Prefab;
        [SerializeField] private GameObject skill2Prefab;
        [SerializeField] private GameObject skill3Prefab;
        [SerializeField] private GameObject creepPrefab;

        private Transform player;
        private NavMeshAgent nav;
        private Stat stat;
        private Animator animator;
        private bool skillTurn = false;

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

            player = FindObjectOfType<PlayerController>().transform;
            nav = GetComponent<NavMeshAgent>();
            stat = GetComponent<Stat>();
            animator = GetComponent<Animator>();
        }

        private void Start()
        {
            StartMoving();
        }

        private void FixedUpdate()
        {
            if (stat.GetIsDead())
            {
                Die();
                return;
            }

            if (isMoving && Vector3.Distance(nav.destination, transform.position) <= 0.5f)
            {
                StopMoving();
            }
            else if (isMoving)
            {
                UpdateNavMesh();
                return;
            }

            if (!onSkill)
            {
                standingTime -= Time.deltaTime;
                transform.rotation = GetLookRotation(player.position);
            }

            if (standingTime <= 0 && !isMoving)
            {
                StartMoving();
            }
            else if (skillTurn)
            {
                ExecuteSkillSequence();
            }
        }

        private Quaternion GetLookRotation(Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - transform.position;
            if (direction == Vector3.zero) return Quaternion.identity;
            return Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), 20 * Time.deltaTime);
        }

        private void StartMoving()
        {
            isMoving = true;
            Vector3 destination = new Vector3(
                Random.Range(player.position.x - 10, player.position.x + 10),
                0,
                Random.Range(player.position.z - 10, player.position.z + 10)
            );
            nav.speed = stat.GetSpeed();
            nav.SetDestination(destination);
            animator.SetFloat("speed", nav.speed);
        }

        private void StopMoving()
        {
            skillTurn = true;
            isMoving = false;
            nav.SetDestination(transform.position);
            animator.SetFloat("speed", 0);
            standingTime = 5f;
        }

        private void ExecuteSkillSequence()
        {
            skillTurn = false;
            onSkill = true;

            if (skill2Stack == 2 && standingTime >= 1.5f && standingTime <= 3.5f)
            {
                skill2Stack = 0;
                animator.SetTrigger("skill3");
                Instantiate(skill3Prefab, transform.position, Quaternion.identity);
            }
            else if (skill1Stack == 1 && standingTime >= 1.5f && standingTime <= 3.5f)
            {
                skill1Stack = 0;
                skill2Stack += 1;
                animator.SetTrigger("skill2");
                Instantiate(skill2Prefab, transform.position, Quaternion.identity);
            }
            else if (skill1Stack == 0 && standingTime >= 1.5f && standingTime <= 3.5f)
            {
                skill1Stack += 1;
                float distance = Vector3.Distance(transform.position, player.position);
                if (distance <= 15)
                {
                    animator.SetTrigger("skill1");
                    Instantiate(skill1Prefab, transform.position, Quaternion.identity);
                }
                else
                {
                    animator.SetTrigger("skill");
                    Instantiate(skillPrefab, player.position, Quaternion.identity);
                }
            }
        }

        public void EndSkill()
        {
            onSkill = false;
            animator.SetTrigger("endSkill");
        }

        private void UpdateNavMesh()
        {
            if (nav.path != null && nav.path.corners.Length > 1)
            {
                transform.rotation = GetLookRotation(nav.path.corners[1]);
            }
        }

        private void Die()
        {
            FindObjectOfType<GameLevel>().RemoveBoss(gameObject);
            nav.speed = 0;
            animator.SetTrigger("isDead");
            this.enabled = false;
            Destroy(gameObject, 3f);
        }
    }
}


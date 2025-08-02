using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentMoveScript : MonoBehaviour
{
    [Header("Navigation")]
    private Transform[] PatrolPoints;
    [SerializeField] private NavMeshAgent NavMeshAgent;
    [SerializeField] private float StoppingDistance = 2f;

    [Header("Detection Settings")]
    [SerializeField] private float DetectionRadius = 10f;
    [SerializeField] private float DetectionAngle = 60f;
    [SerializeField] private LayerMask DetectionLayer;
    [SerializeField] private float MemoryDuration = 5f;
    [SerializeField] private float InvestigateThreshold = 1.5f;
    [SerializeField] private float RotationSpeed = 10f;

    [SerializeField] private AIBulletManager BulletManager;

    [Header("Footstep Settings")]
    [SerializeField] private AudioClip[] FootstepAudioClips;
    [SerializeField] private float FootstepAudioVolume = 1f;

    private int currentPatrolIndex = 0;
    private Transform Player;
    private float memoryTimer = 0f;
    private bool hasSeenPlayer = false;
    private Vector3 lastKnownPlayerPosition;
    private bool playerInSight;
    private float stuckTimer = 0f;
    private float maxStuckTime = 5f;
    private Animator animator;

    private void Start()
    {
        AssignRandomPatrolPoints();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        Transform detectedPlayer = DetectPlayer();

        if (detectedPlayer != null)
        {
            Player = detectedPlayer;

            if (!hasSeenPlayer)
            {
                hasSeenPlayer = true;
                BulletManager.EngageTarget(detectedPlayer);
            }

            memoryTimer = MemoryDuration;
            lastKnownPlayerPosition = Player.position;
        }

        if (hasSeenPlayer)
        {
            PlayerEngagement();
        }
        else
        {
            if (NavMeshAgent != null && NavMeshAgent.enabled && NavMeshAgent.isOnNavMesh)
            {
                if (!NavMeshAgent.pathPending)
                {
                    if (NavMeshAgent.remainingDistance < 0.5f)
                    {
                        stuckTimer += Time.deltaTime;

                        if (stuckTimer >= maxStuckTime)
                        {
                            stuckTimer = 0f;
                            PatrolToNextPoint();
                        }
                    }
                    else
                    {
                        stuckTimer = 0f;
                    }
                }
            }
        }

        if (NavMeshAgent != null && animator != null)
        {
            float moveSpeed = NavMeshAgent.velocity.magnitude;
            animator.SetFloat("Speed", moveSpeed);
            Debug.Log("Animator Speed param: " + animator.GetFloat("Speed"));
        }
    }

    private void PatrolToNextPoint()
    {
        if (PatrolPoints.Length == 0)
        {
            return;
        }
        Vector3 offset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        NavMeshAgent.destination = PatrolPoints[currentPatrolIndex].position + offset;

        currentPatrolIndex = (currentPatrolIndex + 1) % PatrolPoints.Length;
    }

    private Transform DetectPlayer()
    {
        playerInSight = false;
        Collider[]hits = Physics.OverlapSphere(transform.position, DetectionRadius, DetectionLayer);

        foreach (var hit in hits)
        {
            Vector3 directionToTarget = (hit.transform.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

            if (angleToTarget < DetectionAngle /2f)
            {
                if(Physics.Raycast(transform.position + Vector3.up, directionToTarget, out RaycastHit rayHit, DetectionRadius))
                {
                    if (rayHit.transform == hit.transform)
                    {
                        playerInSight = true;
                        return hit.transform;
                    }
                }
            }
        }
        return null;
    }
    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.position, FootstepAudioVolume);
            }
        }
    }
    private void PlayerEngagement()
    {
        

        float distanceToLastKnown = Vector3.Distance(transform.position, lastKnownPlayerPosition);

        if (memoryTimer > 0f)
        {
            memoryTimer -= Time.deltaTime;
        }

        if (distanceToLastKnown > StoppingDistance)
        {
            if (NavMeshAgent != null && NavMeshAgent.enabled && NavMeshAgent.isOnNavMesh)
            {
                NavMeshAgent.isStopped = false;
                NavMeshAgent.SetDestination(lastKnownPlayerPosition);
                BulletManager.Disengage();
            }
            if (animator != null)
            {
                animator.SetBool("IsSearching", false);
                animator.SetBool("IsAttacking", false);
            }
        }

        else
        {
            if (NavMeshAgent != null && NavMeshAgent.enabled && NavMeshAgent.isOnNavMesh)
            {
                NavMeshAgent.isStopped = true;
            }

            Vector3 direction = (lastKnownPlayerPosition - transform.position).normalized;
            direction.y = 0f;

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * RotationSpeed);
            }

            if (Player != null && playerInSight)
            {
                BulletManager.EngageTarget(Player);

                if (animator != null)
                {
                    animator.SetBool("IsAttacking", true);
                }
            }
            else
            {
                BulletManager.Disengage();
                if (animator != null)
                {
                    animator.SetBool("IsAttacking", false);
                }
            }

            if (memoryTimer <= 0f)
            {
                hasSeenPlayer = false;
                Player = null;
                if (NavMeshAgent != null && NavMeshAgent.enabled && NavMeshAgent.isOnNavMesh)
                {
                    NavMeshAgent.isStopped = false;
                }
                BulletManager.Disengage();
                PatrolToNextPoint();
            }
        }
    }

    private void AssignRandomPatrolPoints()
    {
        GameObject[] allPoints = GameObject.FindGameObjectsWithTag("PatrolPoint");

        if (allPoints.Length < 1)
        {
            Debug.LogWarning("Not enough patrol points in the scene!");
            return;
        }
        List<GameObject> shuffled = new List<GameObject>(allPoints);
        for (int i = 0; i < shuffled.Count; i++)
        {
            GameObject temp = shuffled[i];
            int randomIndex = Random.Range(i, shuffled.Count);
            shuffled[i] = shuffled[randomIndex];
            shuffled[randomIndex] = temp;
        }

        PatrolPoints = new Transform[shuffled.Count];
        for (int i = 0; i < shuffled.Count; i++)
        {
            PatrolPoints[i] = shuffled[i].transform;
        }

        currentPatrolIndex = Random.Range(0, PatrolPoints.Length);
    }

    public void ReactToHit(Transform attacker)
    {
        if (attacker == null) return;

        lastKnownPlayerPosition = attacker.position;
        memoryTimer = MemoryDuration;
        hasSeenPlayer = true;
        Player = attacker;

        Vector3 direction = (attacker.position - transform.position).normalized;
        direction.y = 0;
        transform.rotation = Quaternion.LookRotation(direction);

        if (NavMeshAgent != null && NavMeshAgent.enabled && NavMeshAgent.isOnNavMesh)
        {
            NavMeshAgent.isStopped = false;
            NavMeshAgent.SetDestination(lastKnownPlayerPosition);
        }
    }
    private void OnDrawGizmosSelected()
    {
        // Draw detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, DetectionRadius);

        // Draw cone field of view
        Vector3 forward = transform.forward * DetectionRadius;
        Quaternion leftRayRotation = Quaternion.Euler(0, -DetectionAngle / 2f, 0);
        Quaternion rightRayRotation = Quaternion.Euler(0, DetectionAngle / 2f, 0);
        Vector3 leftRay = leftRayRotation * forward;
        Vector3 rightRay = rightRayRotation * forward;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, leftRay);
        Gizmos.DrawRay(transform.position, rightRay);
    }
}

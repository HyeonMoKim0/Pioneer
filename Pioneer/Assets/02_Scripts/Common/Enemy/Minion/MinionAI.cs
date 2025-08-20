using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MinionAI : EnemyBase, IBegin
{
    [Header("���� ������")]
    [SerializeField] private GameObject nestPrefab;

    [Header("�� �ٴ� ���̾�")]
    [SerializeField] private LayerMask groundLayer;

    // �׺� �޽� 
    private NavMeshAgent agent;

    // ���� ���� ����
    public bool isNestCreated = false;
    private float nestCool = 15f;
    private float nestCreationTime = -1f;

    // �ٴ� Ȯ�� ����
    private bool isOnGround = false;

    // ���� ���� ����
    private float lastAttackTime = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        SetAttribute();
        if (agent != null)
        {
            agent.speed = speed;
        }
    }

    void Update()
    {
        fov.DetectTargets(detectMask);
        CheckOnGround();

        if (CanCreateNest())
        {
            CreateNest();
        }
        else if (CanAttack())
        {
            Attack();
        }
        else if (CanMove())
        {
            Move();
        }
    }

    protected override void SetAttribute()
    {
        hp = 20;
        attackDamage = 1;
        speed = 2f;
        detectionRange = 4f;
        attackDelayTime = 2f;
        idleTime = 2f;
        SetMastTarget();
        fov.viewRadius = 2;
    }

    // =============================================================
    // ����
    // =============================================================
    private bool CanCreateNest()
    {
        return isOnGround
            && !isNestCreated
            && Time.time >= nestCreationTime
            && nestCreationTime != -1f
            && !CanAttack();
    }

    void CreateNest()
    {
        Instantiate(nestPrefab, transform.position, Quaternion.identity);
        isNestCreated = true;
    }

    // =============================================================
    // ����
    // =============================================================
    private bool CanAttack()
    {
        return DetectAttackRange().Length > 0 && Time.time >= lastAttackTime + attackDelayTime; ;
    }

    void Attack()
    {
        if (fov.visibleTargets.Count > 0)
        {
            Collider[] detectColliders = DetectAttackRange();

            if (detectColliders.Length > 0)
            {
                currentAttackTarget = FindClosestTargetInAttackRange(detectColliders);
                Debug.Log($"����� Ÿ�� : {currentAttackTarget}");

                if (currentAttackTarget != null)
                {
                    agent.isStopped = true;
                    transform.LookAt(currentAttackTarget.transform);
                    CommonBase targetBase = currentAttackTarget.GetComponent<CommonBase>();
                    Debug.Log($"currentAttackTarget : {currentAttackTarget.gameObject.name}");
                    if (targetBase != null)
                    {
                        targetBase.TakeDamage(attackDamage);
                        lastAttackTime = Time.time;
                        Debug.Log($"���� ���: {currentAttackTarget.name}, ���� HP: {targetBase.CurrentHp}");
                        if (targetBase.IsDead == true)
                        {
                            SetMastTarget();
                        }

                    }
                }
            }
        }
    }

    /// <summary>
    /// ���� ���� ������ ���� ����� �� ã��
    /// </summary>
    /// <returns></returns>
    private GameObject FindClosestTargetInAttackRange(Collider[] detectColliders)
    {
        GameObject closestTarget = null;
        float closestDis = float.MaxValue;

        foreach (var target in detectColliders)
        {
            float dis = Vector3.Distance(transform.position, target.transform.position);
            if (dis < closestDis)
            {
                closestDis = dis;
                closestTarget = target.gameObject;
            }
        }

        return closestTarget;
    }

    // =============================================================
    // �̵�
    // =============================================================
    private bool CanMove()
    {
        return currentAttackTarget != null || fov.visibleTargets.Count > 0;
    }

    void Move()
    {
        Transform moveTarget = currentAttackTarget != null ? currentAttackTarget.transform : null;
        if (fov.visibleTargets.Count > 0)
        {
            moveTarget = FindClosestTargetInDetect(fov.visibleTargets);
        }

        if (moveTarget != null && agent != null)
        {
            Collider col = moveTarget.GetComponent<Collider>();
            Vector3 destination = col != null ? col.ClosestPoint(transform.position) : moveTarget.position;

            if (Vector3.Distance(agent.destination, destination) > 0.5f)
            {
                agent.isStopped = false;
                agent.SetDestination(destination);
            }
        }
    }

    /// <summary>
    /// ���� ����� Ÿ���� ã��
    /// </summary>
    /// <param name="targets"></param>
    /// <returns></returns>
    private Transform FindClosestTargetInDetect(List<Transform> targets)
    {
        Transform closest = null;
        float closestDistance = float.MaxValue;

        foreach (var t in targets)
        {
            float dist = Vector3.Distance(transform.position, t.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closest = t;
            }
        }
        return closest;
    }

    /// <summary>
    /// �� �÷��� ������ �˻�
    /// </summary>
    /// <returns></returns>
    private bool CheckOnGround()
    {
        if (Physics.Raycast(transform.position, Vector3.down, 2f, groundLayer))
        {
            if (!isOnGround)
            {
                nestCool = Random.Range(5f, 15f);
                nestCreationTime = Time.time + nestCool;
                isOnGround = true;
            }
        }
        else
        {
            isOnGround = false;
        }

        return isOnGround;
    }
}
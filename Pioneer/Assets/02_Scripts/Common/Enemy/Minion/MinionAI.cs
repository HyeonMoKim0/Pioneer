using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/* ================================================
0821 �����ϸ� ���� ����
- ���� ���� ����ȭ => DetectAttackRange() ���� ���� ������ Update �ʹݿ� �� �� ȣ���ϰ� ������ �����Ͽ� ��Ȱ��
- Ÿ���� ���� ���� => ������ ���� �� ������ ���� Ÿ������ ��Ƶ� ���� �þ߿� �� ����� ������ Ÿ���� �ٲ������ ������ ����,
                    ���� currentAttackTarget�� ����ִ��� Ȯ�� �� ��������� ���� ���ų� �׾��� �� �þ� ������ ���ο� Ÿ���� ã���� ���� ����
                    -> �׷��� �̷��� �����ϸ� ���밡 Ÿ���϶��� �̻����� 
- ���� �ѹ� �ִ��� ����
- CreateNest�� CanAttack  ���� ������ ���� �ִ��� ���θ� Ȯ��, ������ ���� ���ΰŰ� �ƴϸ� �������� �ƴϴϱ�
================================================ */

public class MinionAI : EnemyBase, IBegin
{
    [Header("���� ������")]
    [SerializeField] private GameObject nestPrefab;

    // �׺� �޽� 
    private NavMeshAgent agent;

    // ���� ���� ����
    public bool isNestCreated = false;
    private float nestCool = 15f;
    private float nestCreationTime = -1f;

    // ���� ���� ����
    private float lastAttackTime = 0f;

    // Ÿ�� ������
    private GameObject mastTarget;  // �⺻ ��ǥ (����)
    private GameObject revengeTarget;   // ���� ������ ��
    // ���� ��ǥ : currentAttackTarget

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        SetAttribute();
        if (agent != null)
        {
            agent.speed = speed;
            agent.stoppingDistance = 0.8f;
        }
    }

    void Update()
    {
        if (!CheckOnGround())
            return;

        fov.DetectTargets(detectMask);

        // ������ Ÿ�� ����
        UpdateTarget();

        Collider[] targetsInAttackRange = DetectAttackRange();
        bool isTargetInAttackRange = currentAttackTarget != null && IsTargetInColliders(currentAttackTarget, targetsInAttackRange);

        if (CanCreateNest(isTargetInAttackRange))
        {
            CreateNest();
        }
        else if (CanAttack(isTargetInAttackRange))
        {
            Attack();
        }
        else if (CanMove())
        {
            Move();
        }
    }

    public override void TakeDamage(int damage, GameObject attacker)
    {
        base.TakeDamage(damage, attacker);

        if (attacker != null && !IsDead)
        {
            revengeTarget = attacker;
            Debug.Log($"{name}��(��) {attacker.name}���� ���ݹ޾� Ÿ���� �����մϴ�!");
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
        mastTarget = SetMastTarget();
        fov.viewRadius = detectionRange;
    }

    private void UpdateTarget()
    {
        /* ===================================
        1. ���� ����� ��ȿ�ϰ� ���� ���� ���� �ִ��� Ȯ��
        2. ���� ���� ���� ���� �ִ��� Ȯ�� (���� ����� ������ Ȯ��)
        3. �⺻ ��ǥ ����� ��ǥ ����
        =================================== */

        if(revengeTarget != null)
        {
            CommonBase targetBase = revengeTarget.GetComponent<CommonBase>();
            
            if(targetBase != null && !targetBase.IsDead && fov.visibleTargets.Contains(revengeTarget.transform))
            {
                currentAttackTarget = revengeTarget;
                return;
            }
            else
            {
                revengeTarget = null;
            }
        }

        Transform closestTarget = FindClosestTargetInDetect(fov.visibleTargets);
        if(closestTarget != null)
        {
            currentAttackTarget = closestTarget.gameObject;
            return;
        }

        currentAttackTarget = mastTarget;
    }

    // =============================================================
    // �ൿ ����
    // =============================================================
    private bool CanCreateNest(bool isAttackable)
    {
        return isOnGround
            && !isNestCreated
            && Time.time >= nestCreationTime
            && nestCreationTime != -1f
            && !isAttackable;
    }

    private bool CanAttack(bool isTargetInAttackRange)
    {
        return currentAttackTarget != null && isTargetInAttackRange && Time.time >= lastAttackTime + attackDelayTime; ;
    }

    private bool CanMove()
    {
        return currentAttackTarget != null;
    }

    // =============================================================
    // ���� ����
    // =============================================================
    void CreateNest()
    {
        Instantiate(nestPrefab, transform.position, Quaternion.identity);
        isNestCreated = true;
    }

    // =============================================================
    // ����
    // =============================================================   
    void Attack()
    {
        if (currentAttackTarget == null)
            return;

        agent.isStopped = true;
        transform.LookAt(currentAttackTarget.transform);

        CommonBase targetBase = currentAttackTarget.GetComponent<CommonBase>();
        if(targetBase != null && !targetBase.IsDead)
        {
            targetBase.TakeDamage(attackDamage, this.gameObject);
            lastAttackTime = Time.time;
        }
    }    

    // =============================================================
    // �̵�
    // =============================================================
    void Move()
    {
        if (currentAttackTarget == null)
            return;

        agent.isStopped = false;
        Vector3 destination = currentAttackTarget.GetComponent<Collider>().ClosestPoint(transform.position);

        if(Vector3.Distance(agent.destination, destination) > 0.5f)
        {
            agent.SetDestination(destination);
        }
    }


    // =============================================================
    // ��ƿ��Ƽ �޼���
    // =============================================================

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

    private bool IsTargetInColliders(GameObject target, Collider[] colliders)
    {
        foreach (var col in colliders)
        {
            if (col.gameObject == target)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// �� �÷��� ������ �˻�
    /// </summary>
    /// <returns></returns>
    protected override bool CheckOnGround()
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
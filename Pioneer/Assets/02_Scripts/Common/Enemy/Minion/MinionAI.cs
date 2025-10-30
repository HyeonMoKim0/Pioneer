using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
    private bool isCurrentAttacking = false;

    // Ÿ�� ������
    // private GameObject mast;  // �⺻ ��ǥ (����)
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

        Debug.Log($"MinionAI currentAttackTarget & isTargetInAttackRange : {this.gameObject.name} {currentAttackTarget.name} & {isTargetInAttackRange}");

        if (CanCreateNest(isTargetInAttackRange))
        {
            CreateNest();
        }
        else if (CanAttack(isTargetInAttackRange))
        {
            StartCoroutine(AttackCoroutine());
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
        maxHp = hp;
        attackDamage = 2;
        attackRange = 1f;
        speed = 2f;
        detectionRange = 1.5f;
        attackDelayTime = 2f;
        idleTime = 2f;
        SetMastTarget();
        fov.viewRadius = detectionRange;
    }

    private void UpdateTarget()
    {
        if (revengeTarget != null)
        {
            CommonBase targetBase = revengeTarget.GetComponent<CommonBase>();

            if (targetBase != null && !targetBase.IsDead && fov.visibleTargets.Contains(revengeTarget.transform))
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
        if (closestTarget != null)
        {
            currentAttackTarget = closestTarget.gameObject;
            return;
        }

        currentAttackTarget = mast;
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
            && !isAttackable
            && GameManager.Instance.LimitsNest();
    }

    private bool CanAttack(bool isTargetInAttackRange)
    {
        Vector3 direction = (currentAttackTarget.transform.position - transform.position).normalized;
        direction.y = 0;
        transform.rotation = Quaternion.LookRotation(direction);

        return currentAttackTarget != null
            && isTargetInAttackRange
            && Time.time >= lastAttackTime + attackDelayTime
            && (!isCurrentAttacking);
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
        Instantiate(nestPrefab, new Vector3(transform.position.x, 0, transform.position.z), Quaternion.identity);
        Debug.Log("DespawnAllEnemies CreateNest ���� ����");
        GameManager.Instance.checkTotalNest++;
        isNestCreated = true;
    }

    // =============================================================
    // ���� : �̹� ������ �������� ������ ��. ���� ���� �ȿ� ���Դٴ� ��
    // =============================================================   
    private IEnumerator AttackCoroutine()
    {
        isCurrentAttacking = true;
        agent.isStopped = true;
        yield return new WaitForSeconds(attackDelayTime);
        yield return Attack();
        agent.speed = speed;
        isCurrentAttacking = false;
    }

    private IEnumerator Attack()
    {
        Debug.Log("�̴Ͼ� Attack �޼��� ����");
        if (currentAttackTarget == null)
            yield break;

        CommonBase targetBase = currentAttackTarget.GetComponent<CommonBase>();
        // Debug.Log($"MinionAI targetBase : {this.name}, {targetBase.name}");
        if (targetBase != null && !targetBase.IsDead)
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

        if (Vector3.Distance(agent.destination, destination) > 0.5f)
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

    private bool IsTargetInColliders(GameObject target, Collider[] colliders)
    {
        Debug.Log(($"MinionAI IsTargetInColliders target : {target.name}"));
        foreach (var col in colliders)
        {
            Debug.Log(($"MinionAI IsTargetInColliders col : {col.name}"));
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
                nestCool = UnityEngine.Random.Range(5f, 15f);
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
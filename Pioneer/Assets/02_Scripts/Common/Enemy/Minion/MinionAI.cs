using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

/*
[Ž��]
- Ÿ�� ������Ʈ ���� ���� �� ����
- ����ִ� ������ ������ 2�� �̸� => ���� ���� (15�� ��� => ��ȸ? �̵�?)
==========================================

1. ���� ���� ���� �ۼ�
- ���� ������ ���� ���� �̴Ͼ� �� ������ �ϳ�
- ������ �������� �����Ǵ� �̴Ͼ� �θ���, �θ��� ������ �ı�
- �������� ������ �̴Ͼ�� �����ʿ��� ������ �̴Ͼ� ������� ������ ���� ����
 
 ==========================================
250812
- ���ο� ��ǥ���� ������ ��� ���� �̵� �������� �����ϰ�
- ���ο� ��ǥ���� ���� ���� �ȿ� �ִٸ� �̵��� ���߰� ����
- ���� ���� ���ε� ���� ���� ���̸� ������ ���ο� ��ǥ�� ��ġ�� �̵�
- ������ ��ǥ���� ���������� ���� �ൿ(���� �������� ����)

+ Idle ��ȸ�Ұ���? �ƴϸ� �� �ڸ� �����Ұ��� (��ȸ�� ��������)
+ �Լ� �� �� ������ �и��ؼ� �ۼ�
+ ���� ���� ����� �ϴ��� Ȯ�ε� �ؾ���
 */

public class MinionAI : EnemyBase, IBegin
{
    [Header("���� ������ ���̾�")]
    [SerializeField] private LayerMask detectLayer;

    [Header("���� ������")]
    [SerializeField] private GameObject nestPrefab;

    [Header("���� �ݶ��̴�")]
    [SerializeField] private GameObject attackCollider;

    private NavMeshAgent agent;

    // 
    private bool isNestCreated = false;
    private float nestCool = 15f;
    private float nestCreationTime = -1f;

    private bool isOnGround = false;

    private Transform currentAttackTarget = null;
    private Vector3 originalDestination;

    void Start()
    {
        SetAttribute();
        agent = GetComponent<NavMeshAgent>();
        if(agent != null)
        {
            agent.speed = speed;
        }
    }

    void Update()
    {       
        fov.DetectTargets(detectLayer);

        //// ���� �ؾ���
        if (targetObject != null)
        {
            // NavMeshAgent ������ ��� ����
            agent.SetDestination(targetObject.transform.position);
        }

        if (CanCreateNest())
        {
            CreateNest();
        }
        else if (CanAttack())
        {
            Attack();
        }
        else if(CanMove())
        {
            Move();
        }        
        else
        {
            Idle();
        }
    }

    protected override void SetAttribute()
    {
        hp = 20;
        attackDamage = 1;
        speed = 2f;
        detectionRange = 4f;
        attackRange = 2f;
        attackDelayTime = 2f;
        idleTime = 2f;

        SetMastTarget();

        fov.viewRadius = attackRange;
    }

    #region �ൿ ���� �˻�
    private bool CanCreateNest()
    {
        return  isOnGround && !isNestCreated && Time.time >= nestCreationTime && nestCreationTime != -1f;
    }

    private bool CanAttack()
    {
        return fov.visibleTargets.Count > 0;
    }

    private bool CanMove()
    {
        if(targetObject != null)
            return true;

        return false;
    }
    #endregion

    // ���� ����
    void CreateNest()
    {
        Instantiate(nestPrefab, transform.position, Quaternion.identity);
        isNestCreated = true;
    }

    // ����
    void Attack()
    {  
        // ���� ���� ���� ���� ������ ������ �����ϴ���?
        if(fov.visibleTargets.Count > 0)
        {
            // ���� ���� �ȿ� �ִ� �ݶ��̴��� ������ ����
            Collider[] detectColliders = DetectAttackRange(attackRange);

            if(detectColliders.Length > 0)
            {
                currentAttackTarget = FindClosestTarget(detectColliders);
            }            
        }
    }

    /// <summary>
    /// ���� ���� ������ ���� ����� �� ã��
    /// </summary>
    /// <returns></returns>
    private Transform FindClosestTarget(Collider[] detectColliders)
    {
        Transform closestTarget = null;
        float closestDis = float.MaxValue;

        foreach (var target in detectColliders) // FOV ����Ʈ�� �ƴ϶� DetectAttackRange���� ��ȯ�� �ݶ��̴� �迭���� ã��
        {
            float dis = Vector3.Distance(transform.position, target.transform.position);
            if (dis < closestDis)
            {
                closestDis = dis;
                closestTarget = target.transform;
            }
        }

        return closestTarget;
    }

    // �̵�
    void Move()
    {
        if (targetObject != null && agent.destination != targetObject.transform.position)
        {
            agent.SetDestination(targetObject.transform.position);
        }
    }

    // ���
    void Idle()
    {
        // ������? ��ȸ? ????????????
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Ground"))
        {
            isOnGround = true;

            if(nestCreationTime == -1f)
            {
                nestCreationTime = Time.time + nestCool;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isOnGround = false;
        }
    }
}

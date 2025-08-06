using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

/*
[Ž��]
- Ÿ�� ������Ʈ ���� ���� �� ����
- ����ִ� ������ ������ 2�� �̸� => ���� ���� (15�� ��� => ��ȸ? �̵�?)

*/
public class MinionAI : EnemyBase, IBegin
{
    [Header("���� ������ ���̾�")]
    [SerializeField] private LayerMask detectLayer;

    [Header("���� ������")]
    [SerializeField] private GameObject nestPrefab;

    private NavMeshAgent agent;

    private int nestCount = 0;
    private float nestCool = 15f;

    public override void Init()
    {
        base.Init();
        SetAttribute();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {       
        fov.DetectTargets(detectLayer);

        if (CanCreateNest())
        {
            CreateNest();
        }
        else if(CanMove())
        {
            Move();
        }
        else if(CanAttack())
        {
            Attack();
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
        // ���� ������ Ȯ�ε� �ؾ���
        return nestCount < 2 && Time.time > nestCool;
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

    }

    // ����
    void Attack()
    {
        Transform closesTarget = null;
        float closesDis = float.MaxValue;

        foreach(var target in fov.visibleTargets)
        {
            float dis = Vector3.Distance(transform.position, target.position);
            if(dis < closesDis)
            {
                closesDis = dis;
                closesTarget = target;
            }
        }

        if(closesTarget != null)
        {
            Vector3 dir = closesTarget.position - transform.position;
            dir.y = 0f;

            if(dir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(dir);

            // ����
        }
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

    }
}

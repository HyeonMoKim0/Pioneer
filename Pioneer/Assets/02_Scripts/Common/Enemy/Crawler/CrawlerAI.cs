using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

/*
{ �Ӽ� }
hp 50
���ݷ� 10
�̵��ӵ� 1
���� ���� 2
���� ���� 4
���� �ð� 2
���� 3

1. Ž��
    - ���� �ڽŰ� ����� ��ġ�� ������Ʈ���� ������� �����ϰ� ���� ���� �ȿ� �� �̻� ��ġ�� ������Ʈ���� ���� ��� ���븦 �ı��Ϸ���
    - Ÿ���� ���� null�̸� ���� 3�� �� �ٽ� Ž��
2. �̵�
    - Ÿ������ �̵� �� ���� ������ ���� ���� ����� �����Ϸ� ��
3. ����
    - ������ 4������ ������ ���� ������ �ϰ� ���� ���� �� �ִ� ��� ��ġ�� ������Ʈ�� �Ǹ� 10 ����
    - ���� �� ���� 3��

==============================================
- ���� Ÿ�� ����Ʈ
- ���� ������ ���� ���� ������� Ÿ�� ����
*/

public class CrawlerAI : EnemyBase, IBegin
{
    // �׺� �޽� 
    private NavMeshAgent agent;

    // ������ ������Ʈ ����� ������ ������ ����Ʈ
    List<Transform> sortedTarget;

    private int closeTarget = 0;

    void Start()
    {
        base.Start();
        agent = GetComponent<NavMeshAgent>();
        SetAttribute();
    }

    void Update()
    {
        fov.DetectTargets(detectMask);

        bool isCanMove = CanMove();
        if (isCanMove == false)
        {
            SetMastTarget();
        }

        if (isCanMove)
        {
            Move();
        }
        else if(CanAttack())
        {
            Attack();
        }
    }

    // �⺻ ����
    protected override void SetAttribute()
    {
        maxHp = 50;
        hp = maxHp;
        attackDamage = 10;
        speed = 1;
        fov.viewRadius = 6;
        attackRange = 4;
        attackDelayTime = 3; 
    }
    
    private bool CanMove()
    {
        return fov.visibleTargets.Any(target => detectMask == (detectMask | (1 << target.gameObject.layer)));
    }

    private bool CanAttack()
    {
        return true;
    }

    private void Move()
    {
        // ���� ����Ʈ�� �ִ� ��ġ�� ������Ʈ �� ���� ����� ������Ʈ���� �����ϱ� �μ����� ������ ������ �������� ����� �� �����Ϸ� ����

        // fov�� ������ ������Ʈ ����� ������ ����
        SortCloseObj();
        currentAttackTarget = sortedTarget[closeTarget].gameObject;
        Vector3 destination = currentAttackTarget.GetComponent<Collider>().ClosestPoint(transform.position);
        if (sortedTarget.Count > 0)
        {
            if (Vector3.Distance(agent.destination, destination) > 0.5f)
            {
                agent.SetDestination(destination);
            }
        }
    }

    private void Attack()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange, detectMask);

        for (int i = 0; i < hitColliders.Length; i++)
        {
            CommonBase targetBase = hitColliders[i].GetComponent<CommonBase>();
            targetBase.TakeDamage(attackDamage, this.gameObject);
        }
    }

    private void SortCloseObj()
    {
        sortedTarget = fov.visibleTargets.OrderBy(target => Vector3.Distance(transform.position, target.transform.position)).ToList();
    }
}

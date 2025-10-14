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
- ���� Ÿ�� ����Ʈ *
- ���� ������ ���� ���� ������� Ÿ�� ����
- �÷��̾����� ���� ������ ���ڱ� ������... ���̷� ..
*/

public class CrawlerAI : EnemyBase, IBegin
{
    // �׺� �޽� 
    private NavMeshAgent agent;

    // ������ ������Ʈ ����� ������ ������ ����Ʈ
    List<Transform> sortedTarget;

    private int closeTarget = 0;

    private GameObject revengeTarget;

    private bool isAttack = false;

    private float attackTimer = 0f;

    void Start()
    {
        base.Start();
        agent = GetComponent<NavMeshAgent>();
        SetAttribute();
    }

    void Update()
    {
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
            return;
        }

        fov.DetectTargets(detectMask);

        if (fov.visibleTargets.Count == 0)
        {
            currentAttackTarget = SetMastTarget();
        }

        if (CanAttack())
        {
            Attack();
        }
        else if (CanMove())
        {
            Move();
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
        attackRange = 2;
        attackDelayTime = 3;
        currentAttackTarget = SetMastTarget();
    }

    private bool CanMove()
    {
        return fov.visibleTargets.Any(target => detectMask == (detectMask | (1 << target.gameObject.layer))) || currentAttackTarget != null;
    }

    private bool CanAttack()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange, detectMask);

        if (hitColliders.Length > 0 && attackTimer <= 0)
            return true;
        else
            return false;
    }

    private void Move()
    {
        // ���� ����Ʈ�� �ִ� ��ġ�� ������Ʈ �� ���� ����� ������Ʈ���� �����ϱ� �μ����� ������ ������ �������� ����� �� �����Ϸ� ����

        // fov�� ������ ������Ʈ ����� ������ ����
        if (fov.visibleTargets.Count > 0)
        {
            SortCloseObj();
            currentAttackTarget = sortedTarget[closeTarget].gameObject;
        }

        Vector3 destination = currentAttackTarget.GetComponent<Collider>().ClosestPoint(transform.position);
        if (Vector3.Distance(agent.destination, destination) > 0.5f)
        {
            agent.SetDestination(destination);
        }
    }

    private void Attack()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange, detectMask);

        for (int i = 0; i < hitColliders.Length; i++)
        {
            GameObject currentObject = hitColliders[i].gameObject;

            UnityEngine.Debug.Log($"[�˻� ����] �̸�: {currentObject.name}, ���̾�: {LayerMask.LayerToName(currentObject.layer)}");

            CommonBase targetBase = currentObject.GetComponent<CommonBase>();

            if (targetBase == null)
            {
                UnityEngine.Debug.LogError($"-> ����: '{currentObject.name}'���� CommonBase ������Ʈ�� ã�� �� �����ϴ�! (targetBase is null)");
            }
            else
            {
                UnityEngine.Debug.Log($"-> ����: '{currentObject.name}'���� CommonBase ������Ʈ�� ã�ҽ��ϴ�.");

                if (targetBase.IsDead)
                {
                    if (fov.visibleTargets.Count > 0)
                    {
                        SortCloseObj();
                        currentAttackTarget = fov.visibleTargets[closeTarget].gameObject;
                    }
                    return;
                }
                targetBase.TakeDamage(attackDamage, this.gameObject);
            }
        }
        attackTimer = attackDelayTime;
    }

    private void SortCloseObj()
    {
        sortedTarget = fov.visibleTargets.OrderBy(target => Vector3.Distance(transform.position, target.transform.position)).ToList();
    }
}
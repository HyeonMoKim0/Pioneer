using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/*
250814
* ���� 1 : ������ �� �� �ϸ� �ְ� ������ �� ���ߴ°� ����
- ���� 2 : ����� ������ �� �� => ���� �׽�Ʈ ���� �÷��̾ CommonBase�� ��� �����ʾ� ��Ȯ�� Ȯ�� �Ұ�
- ���� 3 : ���� ���� ���� �� ��
- ���� 4 : �ٴٿ� ������ �׺�޽ø� ���� �� ���� �ö������ �׺�޽ø� Ű��
- ���� 5 : �� ������ Ȯ���ϴ� �ڵ� ��� ���ʹ̰� ����� �� ���Ƽ� EnemyBase�� �ű��

+ �ڵ尡 �ʹ� ������ �ٽ� ����ϰ� �����غ���
=========================================================================================================
250815
- ���ݵ��� ���ʹ̰� �׾����� ����� Ÿ�� ������ �ȵ� + ����� �̵��� �� ��
- ���� ������ ���� �ȵ�
- ���� ���� ���� ���� Ÿ���� �־ �ϳ��� ������ �ٸ� ���� �� Ÿ���� �ν��ϴ°� �ƴ϶� �ٷ� ����� ���ϴ� ������ ����
*/
public class MinionAI : EnemyBase, IBegin
{
    [Header("���� ������")]
    [SerializeField] private GameObject nestPrefab;

    [Header("�� �ٴ� ���̾�")]
    [SerializeField] private LayerMask groundLayer;

    // �׺� �޽� 
    private NavMeshAgent agent;

    // ���� ���� ����
    private bool isNestCreated = false;
    private float nestCool = 15f;
    private float nestCreationTime = -1f;

    // ���� Ÿ�� ���� ����
    // private Transform currentAttackTarget = null;

    // �ٴ� Ȯ�� ����
    private bool isOnGround = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        SetAttribute();
        if(agent != null)
        {
            agent.speed = speed;
        }
    }

    void Update()
    {      
        // ���̾� ���� ����
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
        else if(CanMove())
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
        fov.viewRadius = attackRange;
    }

    #region ���� ����
    private bool CanCreateNest()
    {
        return isOnGround 
            && !isNestCreated 
            && Time.time >= nestCreationTime 
            && nestCreationTime != -1f
            && !CanAttack();
    }

    // ���� ����
    void CreateNest()
    {
        Instantiate(nestPrefab, transform.position, Quaternion.identity);
        isNestCreated = true;
    }
    #endregion

    #region ����
    private bool CanAttack()
    {
        return DetectAttackRange().Length > 0;
    }

    void Attack()
    {  
        if(fov.visibleTargets.Count > 0)
        {
            // ���� ���� �ȿ� �ִ� �ݶ��̴��� ������ ����
            Collider[] detectColliders = DetectAttackRange();

            // ������ �ڵ�
            Debug.Log($"DetectAttackRange���� {detectColliders.Length}���� �ݶ��̴� ������");
            for (int i = 0; i < detectColliders.Length; i++)
            {
                Debug.Log($"[{i}] �̸�: {detectColliders[i].gameObject.name}, �±�: {detectColliders[i].gameObject.tag}");
            }
            // �������

            if (detectColliders.Length > 0)
            {
                currentAttackTarget = FindClosestTarget(detectColliders);
                Debug.Log($"����� Ÿ�� : {currentAttackTarget}");

                if (currentAttackTarget != null)
                {
                    Debug.Log("���� ����� �� ã��");
                    agent.isStopped = true;
                    Debug.Log("���� ����� �� ã��2");
                    CommonBase targetBase = currentAttackTarget.GetComponent<CommonBase>();
                    Debug.Log($"currentAttackTarget : {currentAttackTarget.gameObject.name}");
                    if (targetBase != null)
                    {
                        Debug.Log("���� ����� �� ã��4");
                        targetBase.TakeDamage(attackDamage);
                        if(targetBase.IsDead == true)
                        {
                            SetMastTarget();
                            agent.SetDestination(currentAttackTarget.transform.position);
                            agent.isStopped = false;
                        }
                        Debug.Log($"���� ���: {currentAttackTarget.name}, ���� HP: {targetBase.CurrentHp}");
                    }
                }
                else
                {
                    Debug.Log("���� ����� �� ã��88");
                    agent.isStopped = false;
                }
            }
        }
    }

    /// <summary>
    /// ���� ���� ������ ���� ����� �� ã��
    /// </summary>
    /// <returns></returns>
    private GameObject FindClosestTarget(Collider[] detectColliders)
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
    #endregion

    #region �̵�
    private bool CanMove()
    {
        return currentAttackTarget != null || fov.visibleTargets.Count > 0;
    }

    void Move()
    {
        if (agent.isStopped) return;

        Transform moveTarget = currentAttackTarget != null ? currentAttackTarget.transform : null;
        if (fov.visibleTargets.Count > 0)
        {
            moveTarget = FindClosestTargetFromList(fov.visibleTargets);
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

    private Transform FindClosestTargetFromList(List<Transform> targets)
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
    #endregion

    // �� �÷��� ������ �˻�
    private bool CheckOnGround()
    {
        // RaycastHit hit;
        if(Physics.Raycast(transform.position, Vector3.down, 2f, groundLayer))
        {
            if(!isOnGround)
            {
                nestCool = Random.Range(5f, 15f);
                nestCreationTime = Time.time + nestCool;
                isOnGround = true;
            }
            Debug.Log("�� ����");
        }
        else
        {
            isOnGround = false;
            Debug.Log("�� �� �ƴ�");
        }

        return isOnGround;
    }
}
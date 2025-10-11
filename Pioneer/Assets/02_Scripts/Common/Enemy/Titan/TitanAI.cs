using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TitanAI : EnemyBase, IBegin
{
    private NavMeshAgent agent;
    private GameObject mastTarget;

    [Header("Ÿ��ź ���� �Ӽ�")]
    [SerializeField] private Transform spriteTransform;
    [SerializeField] private float lungeDuration = 0.2f;

    private bool isAttacking = false;

    #region Unity �⺻ �Լ� (Start, Update)

    void Start()
    {
        base.Start();
        agent = GetComponent<NavMeshAgent>();
        mastTarget = SetMastTarget();

        SetAttribute();
        currentAttackTarget = mastTarget; // �ʱ� ��ǥ�� ����
    }

    void Update()
    {
        if (isAttacking || IsDead) return;

        UpdateTarget();

        if (CanAttack())
        {
            StartCoroutine(AttackRoutine());
        }
        else
        {
            Move();
        }
    }

    #endregion

    #region AI �ൿ �Լ� (Move, Attack, Detect)

    /// <summary>
    ///  AI�� ���� ��ǥ�� �����ϴ� ���ο� �Լ�
    /// </summary>
    private void UpdateTarget()
    {
        // Ÿ���� �ı��Ǿ��ų� ��Ȱ��ȭ�Ǿ����� Ȯ��
        if (currentAttackTarget == null || !currentAttackTarget.activeInHierarchy)
        {
            currentAttackTarget = mastTarget;
        }

        fov.DetectTargets(detectMask);

        GameObject closestObstacle = null;
        float minDistance = float.MaxValue;

        // �þ߿� ���̴� ��� Ÿ�� �� ���� ����� ��ֹ��� ã��
        foreach (Transform target in fov.visibleTargets)
        {
            if (target.gameObject != mastTarget)
            {
                float distance = Vector3.Distance(transform.position, target.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestObstacle = target.gameObject;
                }
            }
        }

        // ���� ����� ��ֹ��� ������ �׳��� ��ǥ��, ������ ���븦 ��ǥ�� ����
        if (closestObstacle != null)
        {
            currentAttackTarget = closestObstacle;
        }
        else
        {
            currentAttackTarget = mastTarget;
        }
    }

    /// <summary>
    ///  isCharging�� ����� �ܼ����� �̵� �Լ�
    /// </summary>
    private void Move()
    {
        agent.speed = speed; // �׻� �Ϲ� �ӵ��� �̵�
        if (currentAttackTarget != null && agent.isOnNavMesh)
        {
            agent.SetDestination(currentAttackTarget.transform.position);
        }
    }

    private bool CanAttack()
    {
        if (currentAttackTarget != null)
        {
            return Vector3.Distance(transform.position, currentAttackTarget.transform.position) <= attackRange;
        }
        return false;
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        agent.isStopped = true;
        agent.updateRotation = false;

        Vector3 lookPos = currentAttackTarget.transform.position;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);

        Debug.Log($"[{currentAttackTarget.name}]���� ����!");
        yield return StartCoroutine(LungeVisualRoutine());

        yield return new WaitForSeconds(attackDelayTime);

        ResetToDefaultState();
    }

    private IEnumerator LungeVisualRoutine()
    {
        Vector3 spriteOriginalPos = spriteTransform.localPosition;
        Vector3 lungeEndPos = spriteOriginalPos + new Vector3(0, 0, attackRange);
        float elapsedTime = 0f;

        while (elapsedTime < lungeDuration)
        {
            spriteTransform.localPosition = Vector3.Lerp(spriteOriginalPos, lungeEndPos, elapsedTime / lungeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        spriteTransform.localPosition = lungeEndPos;

        DealDamage();

        elapsedTime = 0f;
        while (elapsedTime < lungeDuration)
        {
            spriteTransform.localPosition = Vector3.Lerp(lungeEndPos, spriteOriginalPos, elapsedTime / lungeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        spriteTransform.localPosition = spriteOriginalPos;
    }

    #endregion

    #region ���� �Լ� (Attribute, Reset, Damage)

    protected override void SetAttribute()
    {
        base.SetAttribute();
        maxHp = 30;
        hp = maxHp;
        speed = 4f;
        attackRange = 2f;
        attackDamage = 20;
        attackDelayTime = 4f;
        if (fov != null) fov.viewRadius = attackRange;
    }

    private void ResetToDefaultState()
    {
        isAttacking = false;
        currentAttackTarget = mastTarget;
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.updateRotation = true;
        }
    }

    private void DealDamage()
    {
        Collider[] hitColliders = DetectAttackRange();
        foreach (var hit in hitColliders)
        {
            CommonBase targetBase = hit.GetComponent<CommonBase>();
            if (targetBase != null && !targetBase.IsDead)
            {
                targetBase.TakeDamage(attackDamage, gameObject);
            }
        }
    }

    #endregion
}
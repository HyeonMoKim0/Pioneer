using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MarinerBase : CreatureBase
{
    // �¹��� ���� ����
    public LayerMask targetLayer;

    // ���� ����
    public enum CrewState { Wandering, Idle, Attacking, Chasing }
    protected CrewState currentState = CrewState.Wandering;

    // �̵� �� ��� �ð�
    protected float moveDuration = 2f;
    protected float idleDuration = 4f;
    protected float stateTimer = 0f;
    private Vector3 moveDirection;

    // ���� ����
    protected UnityEngine.Transform target;
    protected bool isShowingAttackBox = false;
    protected Coroutine attackRoutine;

    // �߰� �ý��� ���� ����
    protected float chaseRange = 8f;  // �߰� ���� ����
    protected bool isChasing = false; // �߰� ������ Ȯ��
    protected float chaseUpdateInterval = 0.2f; // �߰� ��ǥ ������Ʈ ����
    protected float lastChaseUpdate = 0f;

    // ���� ���� ���� ����
    [Header("���� ����")]
    public bool isRepairing = false;
    protected DefenseObject targetRepairObject;
    protected int repairAmount = 30;
    protected bool isSecondPriorityStarted = false;

    // NavMeshAgent ���� ���
    protected NavMeshAgent agent;

    public override void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = speed;
            agent.acceleration = 12f;
            agent.angularSpeed = 360f;
            agent.stoppingDistance = 0.5f;
        }
        base.Start();
    }

    protected bool IsTargetInFOV()
    {
        if (target == null || fov == null)
            return false;

        fov.DetectTargets(targetLayer);
        return fov.visibleTargets.Contains(target);
    }

    protected virtual void Wander() // ��ȸ
    {
        // NavMesh�� ����� ��ȸ
        if (agent != null && agent.isOnNavMesh)
        {
            if (!agent.hasPath || agent.remainingDistance < 0.5f)
            {
                SetRandomDestination();
            }
        }
        else
        {
            // ���� ��� (NavMesh ���� �� fallback)
            transform.position += moveDirection * speed * Time.deltaTime;
        }

        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            EnterIdleState();
        }
    }

    protected virtual void Idle()
    {
        // Idle ���¿����� NavMesh ����
        if (agent != null && agent.isOnNavMesh)
        {
            agent.ResetPath();
        }

        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            EnterWanderingState();
        }
    }

    protected virtual void EnterWanderingState()
    {
        SetRandomDirection();
        currentState = CrewState.Wandering;
        stateTimer = moveDuration;
        Debug.Log($"{gameObject.name} - ���� �������� �̵� ����");
    }

    protected virtual void EnterIdleState()
    {
        currentState = CrewState.Idle;
        stateTimer = idleDuration;
        if (agent != null && agent.isOnNavMesh)
        {
            agent.ResetPath();
        }
        Debug.Log($"{gameObject.name} - ��� ���·� ��ȯ");
    }

    protected virtual void EnterChasingState()
    {
        currentState = CrewState.Chasing;
        isChasing = true;
        Debug.Log($"{GetCrewTypeName()} {GetMarinerId()}: �߰� ���·� ��ȯ");
    }

    protected void SetRandomDirection()
    {
        float angle = Random.Range(0f, 360f);
        moveDirection = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)).normalized;
    }

    protected void SetRandomDestination()
    {
        if (agent == null || !agent.isOnNavMesh) return;

        Vector3 randomDirection = Random.insideUnitSphere * 5f;
        randomDirection += transform.position;
        randomDirection.y = transform.position.y;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, 5f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    protected bool DetectTarget()
    {
        Collider[] hits = Physics.OverlapBox(
            transform.position,
            new Vector3(attackRange / 2f, 0.5f, attackRange / 2f),
            Quaternion.identity,
            targetLayer
        );

        float minDist = float.MaxValue;
        target = null;

        foreach (var hit in hits)
        {
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                target = hit.transform;
            }
        }

        return target != null;
    }

    protected void LookAtTarget()
    {
        if (target == null) return;

        Vector3 dir = (target.position - transform.position).normalized;
        dir.y = 0f;

        if (dir != Vector3.zero)
            transform.forward = dir;
    }

    protected virtual void ValidateCurrentTarget()
    {
        if (target != null)
        {
            CommonBase targetBase = target.GetComponent<CommonBase>();
            if (targetBase != null && targetBase.IsDead)
            {
                Debug.Log($"{GetCrewTypeName()} {GetMarinerId()}: Ÿ�� {target.name}�� �׾����ϴ�. ���ο� Ÿ���� ã���ϴ�.");
                target = null;
                isChasing = false;
                EnterWanderingState();
            }
        }
    }

    protected virtual void TryFindNewTarget()
    {
        Collider[] hits = Physics.OverlapBox(
            transform.position,
            new Vector3(chaseRange / 2f, 0.5f, chaseRange / 2f),
            Quaternion.identity,
            targetLayer
        );

        float minDist = float.MaxValue;
        UnityEngine.Transform nearestTarget = null;

        foreach (var hit in hits)
        {
            CommonBase targetBase = hit.GetComponent<CommonBase>();
            if (targetBase != null && targetBase.IsDead)
                continue;

            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearestTarget = hit.transform;
            }
        }

        if (nearestTarget != null)
        {
            target = nearestTarget;
            EnterChasingState();
            Debug.Log($"{GetCrewTypeName()} {GetMarinerId()}: {target.name} �߰� ����!");
        }
    }

    protected virtual void HandleChasing()
    {
        if (target == null)
        {
            isChasing = false;
            EnterWanderingState();
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // ���� ���� ���� �ְ� ��ٿ��� �������� ����
        if (distanceToTarget <= attackRange && GetAttackCooldown() <= 0f)
        {
            if (IsTargetInFOV() && attackRoutine == null)
            {
                attackRoutine = StartCoroutine(GetAttackSequence());
            }
        }
        else
        {
            // NavMesh�� ����� �߰�
            ChaseTarget();
        }
    }

    protected virtual void ChaseTarget()
    {
        if (target == null || agent == null || !agent.isOnNavMesh) return;

        if (Time.time - lastChaseUpdate >= chaseUpdateInterval)
        {
            agent.SetDestination(target.position);
            lastChaseUpdate = Time.time;
        }

        LookAtTarget();
    }

    protected virtual void HandleNormalBehavior()
    {
        switch (currentState)
        {
            case CrewState.Wandering:
                Wander();
                break;
            case CrewState.Idle:
                Idle();
                break;
            case CrewState.Chasing:
                HandleChasing();
                break;
            case CrewState.Attacking:
                break;
        }
    }

    protected virtual float GetAttackCooldown()
    {
        return 0f;
    }

    protected virtual IEnumerator GetAttackSequence()
    {
        yield return null;
    }

    // ===== ���� ���� ���� �Լ��� =====
    protected virtual void StartRepair()
    {
        List<DefenseObject> needRepairList = null;
        if (MarinerManager.Instance == null)
            return;
            
        needRepairList = MarinerManager.Instance.GetNeedsRepair();

        for (int i = 0; i < needRepairList.Count; i++)
        {
            DefenseObject obj = needRepairList[i];

            if (MarinerManager.Instance.TryOccupyRepairObject(obj, GetMarinerId()))
            {
                targetRepairObject = obj;

                if (MarinerManager.Instance.CanMarinerRepair(GetMarinerId(), targetRepairObject))
                {
                    Debug.Log($"{GetCrewTypeName()} {GetMarinerId()} ���� ����: {targetRepairObject.name}");
                    isRepairing = true;
                    StartCoroutine(MoveToRepairObject(targetRepairObject.transform.position));
                    return;
                }
                else
                {
                    MarinerManager.Instance.ReleaseRepairObject(obj); // ���� ����
                }
            }
        }

        if (!isSecondPriorityStarted)
        {
            Debug.Log($"{GetCrewTypeName()} ���� ��� ���� -> 2���� �ൿ ����");
            isSecondPriorityStarted = true;
            StartCoroutine(StartSecondPriorityAction());
        }
    }

    protected IEnumerator MoveToRepairObject(Vector3 targetPosition)
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(targetPosition);

            while (!IsArrived())
            {
                yield return null;
            }
        }

        StartCoroutine(RepairProcess());
    }

    protected virtual IEnumerator RepairProcess()
    {
        float repairDuration = 10f;
        float elapsedTime = 0f;

        while (elapsedTime < repairDuration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // �⺻ ���� ������ 100% (�Ϲ� �¹�����)
        bool repairSuccess = GetRepairSuccessRate() > Random.value;
        int actualRepairAmount = repairSuccess ? repairAmount : 0;

        Debug.Log($"{GetCrewTypeName()} {GetMarinerId()} ���� {(repairSuccess ? "����" : "����")}: {targetRepairObject.name}/ ������: {actualRepairAmount}");
        targetRepairObject.Repair(actualRepairAmount);

        isRepairing = false;
        MarinerManager.Instance.UpdateRepairTargets();

        if (GameManager.Instance.TimeUntilNight() <= 30f)
        {
            Debug.Log($"{GetCrewTypeName()} �� ���� �����ൿ ����");
            OnNightApproaching();
            yield break;
        }

        StartRepair();
        MarinerManager.Instance.ReleaseRepairObject(targetRepairObject);
    }

    public virtual IEnumerator StartSecondPriorityAction()
    {
        Debug.Log($"{GetCrewTypeName()} 2���� �ൿ - �⺻ ����");
        yield return new WaitForSeconds(1f);
    }

    // ===== NavMeshAgent ���� ���� �Լ��� =====
    public void MoveTo(Vector3 destination)
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(destination);
        }
    }

    public bool IsArrived()
    {
        if (agent == null || !agent.isOnNavMesh) return true;
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    }

    public IEnumerator MoveToThenReset(Vector3 destination)
    {
        MoveTo(destination);

        while (!IsArrived())
        {
            yield return null;
        }

        if (agent != null && agent.isOnNavMesh)
        {
            agent.ResetPath();
        }
        Debug.Log($"{GetCrewTypeName()} ResetPath ȣ��");
    }

    protected virtual float GetRepairSuccessRate()
    {
        return 1.0f; // �⺻ 100% ������ (�Ϲ� �¹���)
    }

    protected virtual int GetMarinerId()
    {
        return 0; // �⺻��
    }

    protected virtual string GetCrewTypeName()
    {
        return "�¹���"; // �⺻��
    }

    protected virtual void OnNightApproaching()
    {
        Debug.Log($"{GetCrewTypeName()} �⺻ �� ó��");
    }

    protected virtual void OnDrawGizmos()
    {
        if (isShowingAttackBox)
        {
            Gizmos.color = Color.red;
            Vector3 boxCenter = transform.position + transform.forward * 1f;
            Gizmos.matrix = Matrix4x4.TRS(boxCenter, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(2f, 1f, 2f));
        }
    }

}
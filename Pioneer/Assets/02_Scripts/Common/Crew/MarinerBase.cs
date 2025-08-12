using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MarinerBase : CreatureBase
{
    // �¹��� ���� ����
    public LayerMask targetLayer;

    // ���� ����
    public enum CrewState { Wandering, Idle, Attacking }
    protected CrewState currentState = CrewState.Wandering;

    // �̵� �� ��� �ð�
    protected float moveDuration = 2f;
    protected float idleDuration = 4f;
    protected float stateTimer = 0f;
    protected Vector3 moveDirection;

    // ���� ����
    protected UnityEngine.Transform target;
    protected bool isShowingAttackBox = false;
    protected Coroutine attackRoutine;

    // ���� ���� ���� ����
    [Header("���� ����")]
    public bool isRepairing = false;
    protected DefenseObject targetRepairObject;
    protected int repairAmount = 30;
    protected bool isSecondPriorityStarted = false;

    // NavMeshAgent ���� ���
    protected NavMeshAgent agent;

    /// <summary>
    /// �¹��� ���� �ʱ�ȭ
    /// </summary>
    public override void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        base.Start();
    }

    /// <summary>
    /// FOV�� ����� Ÿ���� �þ� ���� �ִ��� Ȯ��
    /// </summary>
    protected bool IsTargetInFOV()
    {
        if (target == null || fov == null)
            return false;

        // FOV���� Ÿ�� ���� ����
        fov.DetectTargets(targetLayer);
        return fov.visibleTargets.Contains(target);
    }

    /// <summary>
    /// ������ �������� ��ȸ
    /// </summary>
    protected virtual void Wander()
    {
        transform.position += moveDirection * speed * Time.deltaTime;
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            EnterIdleState();
        }
    }

    /// <summary>
    /// ��� ����
    /// </summary>
    protected virtual void Idle()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            EnterWanderingState();
        }
    }

    /// <summary>
    /// ��ȸ ���·� ��ȯ
    /// </summary>
    protected virtual void EnterWanderingState()
    {
        SetRandomDirection();
        currentState = CrewState.Wandering;
        stateTimer = moveDuration;
        Debug.Log($"{gameObject.name} - ���� �������� �̵� ����");
    }

    /// <summary>
    /// ��� ���·� ��ȯ
    /// </summary>
    protected virtual void EnterIdleState()
    {
        currentState = CrewState.Idle;
        stateTimer = idleDuration;
        Debug.Log($"{gameObject.name} - ��� ���·� ��ȯ");
    }

    /// <summary>
    /// ������ ���� ����
    /// </summary>
    protected void SetRandomDirection()
    {
        float angle = Random.Range(0f, 360f);
        moveDirection = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)).normalized;
    }

    /// <summary>
    /// ���� ���� �� Ÿ�� ����
    /// </summary>
    protected bool DetectTarget()
    {
        // attackRange ���� ���
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

    /// <summary>
    /// Ÿ���� �ٶ󺸵��� ȸ��
    /// </summary>
    protected void LookAtTarget()
    {
        if (target == null) return;

        Vector3 dir = (target.position - transform.position).normalized;
        dir.y = 0f;

        if (dir != Vector3.zero)
            transform.forward = dir;
    }

    // ===== ���� ���� ���� �Լ��� =====

    /// <summary>
    /// ���� �۾� ����
    /// </summary>
    protected virtual void StartRepair()
    {
        List<DefenseObject> needRepairList = MarinerManager.Instance.GetNeedsRepair();

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

        // ������ �� �ִ� ���� ����� ���� ���
        if (!isSecondPriorityStarted)
        {
            Debug.Log($"{GetCrewTypeName()} ���� ��� ���� -> 2���� �ൿ ����");
            isSecondPriorityStarted = true;
            StartCoroutine(StartSecondPriorityAction());
        }
    }

    /// <summary>
    /// ������ ������Ʈ�� �̵�
    /// </summary>
    protected IEnumerator MoveToRepairObject(Vector3 targetPosition)
    {
        agent.SetDestination(targetPosition);

        while (!IsArrived())
        {
            yield return null;
        }

        StartCoroutine(RepairProcess());
    }

    /// <summary>
    /// ���� ���μ��� (���� Ŭ�������� �������̵� ����)
    /// </summary>
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

    /// <summary>
    /// 2���� �ൿ (���� Ŭ�������� ����)
    /// </summary>
    public virtual IEnumerator StartSecondPriorityAction()
    {
        Debug.Log($"{GetCrewTypeName()} 2���� �ൿ - �⺻ ����");
        yield return new WaitForSeconds(1f);
    }

    // ===== NavMeshAgent ���� ���� �Լ��� =====

    /// <summary>
    /// ������ ��ġ�� �̵�
    /// </summary>
    public void MoveTo(Vector3 destination)
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(destination);
        }
    }

    /// <summary>
    /// �������� �����ߴ��� Ȯ��
    /// </summary>
    public bool IsArrived()
    {
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    }

    /// <summary>
    /// �������� �̵� �� ��� �ʱ�ȭ
    /// </summary>
    public IEnumerator MoveToThenReset(Vector3 destination)
    {
        MoveTo(destination);

        while (!IsArrived())
        {
            yield return null;
        }

        agent.ResetPath();
        Debug.Log($"{GetCrewTypeName()} ResetPath ȣ��");
    }

    /// <summary>
    /// ���� ������ ��ȯ (���� Ŭ�������� �������̵�)
    /// </summary>
    protected virtual float GetRepairSuccessRate()
    {
        return 1.0f; // �⺻ 100% ������ (�Ϲ� �¹���)
    }

    /// <summary>
    /// �¹��� ID ��ȯ (���� Ŭ�������� �������̵�)
    /// </summary>
    protected virtual int GetMarinerId()
    {
        return 0; // �⺻��
    }

    /// <summary>
    /// �¹��� Ÿ�� �̸� ��ȯ (���� Ŭ�������� �������̵�)
    /// </summary>
    protected virtual string GetCrewTypeName()
    {
        return "�¹���"; // �⺻��
    }

    /// <summary>
    /// ���� �ٰ��� �� ó�� (���� Ŭ�������� �������̵�)
    /// </summary>
    protected virtual void OnNightApproaching()
    {
        Debug.Log($"{GetCrewTypeName()} �⺻ �� ó��");
    }

    /// <summary>
    /// ���� �ڽ� ����� ǥ��
    /// </summary>
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

    /// <summary>
    /// ���� ���� ����� ǥ�� (���� ��)
    /// </summary>
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector3(attackRange, 1f, attackRange));
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarinerAI : MonoBehaviour
{
    public enum MarinerState { Wandering, Idle, Attacking }

    // --- ���� ---
    private MarinerState currentState = MarinerState.Wandering;

    // --- ���� ���� ---
    public LayerMask targetLayer;
    public float detectionRange = 3f;
    public float attackInterval = 0.5f;
    private float attackCooldown = 0f;
    private Transform target;

    // --- �� �ൿ ���� ---
    public int marinerId;
    private bool isRepairing = false;
    private DefenseObject targetRepairObject;
    private int repairAmount = 30;
    private bool isSecondPriorityStarted = false;

    // --- �� �ൿ ���� ---
    private float speed = 1f;
    private float moveDuration = 2f;
    private float idleDuration = 4f;
    private float stateTimer = 0f;
    private Vector3 moveDirection;

    private bool isShowingAttackBox = false;
    private float attackVisualDuration = 1f;
    private Coroutine attackRoutine;

    private void Start()
    {
        SetRandomDirection();
        stateTimer = moveDuration;
        Debug.Log("Mariner AI �۵� ����");
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.RegisterMariner(this);

        if (GameManager.Instance.IsDaytime)
        {
            // ��: ���� �� 2���� �ൿ
            if (attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
                attackRoutine = null;
                isShowingAttackBox = false;
            }

            if (!isRepairing)
                StartRepair();

            // ���� �ȱ�/��� ���´� �� ���� ���� (�̵� ����)
        }
        else
        {
            // ��: Mariner AI 
            attackCooldown -= Time.deltaTime;

            if (attackCooldown <= 0f)
            {
                if (DetectTarget())
                {
                    LookAtTarget();

                    if (attackRoutine == null)
                        attackRoutine = StartCoroutine(AttackSequence());
                }

                attackCooldown = attackInterval;
            }

            switch (currentState)
            {
                case MarinerState.Wandering:
                    Wander();
                    break;
                case MarinerState.Idle:
                    Idle();
                    break;
                case MarinerState.Attacking:
                    // ���� ���������� ó��
                    break;
            }
        }
    }

    // --- �� �ൿ �Լ��� ---

    private void StartRepair()
    {
        List<DefenseObject> needRepairList = GameManager.Instance.GetNeedsRepair();

        if (needRepairList.Count > 0)
        {
            targetRepairObject = needRepairList[0];

            if (GameManager.Instance.CanMarinerRepair(marinerId, targetRepairObject))
            {
                Debug.Log("�¹��� ���� ��");
                isRepairing = true;
                StartCoroutine(RepairProcess());
            }
            Debug.Log($"Mariner {marinerId} ���� ���: {targetRepairObject.name}, HP: {targetRepairObject.currentHP}/{targetRepairObject.maxHP}");
        }
        else
        {
            if (!isSecondPriorityStarted)
            {
                Debug.Log("���� ��� ����, 2���� �ൿ ����");
                isSecondPriorityStarted = true;
                StartCoroutine(StartSecondPriorityAction());
            }
        }
    }

    private IEnumerator RepairProcess()
    {
        float repairDuration = 3f;
        float elapsedTime = 0f;

        while (elapsedTime < repairDuration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Debug.Log($"Mariner {marinerId} ���� �Ϸ�: {targetRepairObject.name}, ������: {repairAmount}");
        targetRepairObject.Repair(repairAmount);

        isRepairing = false;
        GameManager.Instance.UpdateRepairTargets();

        if (GameManager.Instance.TimeUntilNight() <= 30f)
        {
            Debug.Log("�� �ð� �ӹ�, �Ͻ� �ൿ ����");
            GameManager.Instance.StoreItemsAndReturnToBase(this);
            yield break;
        }

        StartRepair();
    }

    public IEnumerator StartSecondPriorityAction()
    {
        Debug.Log("�Ϲ� �¹��� 2���� �� �ൿ ����");

        GameObject[] spawnPoints = GameManager.Instance.spawnPoints;
        List<int> triedIndexes = new List<int>();
        int fallbackIndex = (marinerId % 2 == 0) ? 0 : 1;
        int chosenIndex = -1;

        while (triedIndexes.Count < spawnPoints.Length)
        {
            int index = triedIndexes.Count == 0 ? fallbackIndex : Random.Range(0, spawnPoints.Length);

            if (triedIndexes.Contains(index)) continue;

            if (!GameManager.Instance.IsSpawnerOccupied(index))
            {
                GameManager.Instance.OccupySpawner(index);
                chosenIndex = index;
                Debug.Log("������ ���� ����");
                break;
            }
            else
            {
                triedIndexes.Add(index);
                float waitTime = Random.Range(0f, 1f);
                Debug.Log("������ ������, ��� �� ��Ž��");
                yield return new WaitForSeconds(waitTime);
            }
        }

        if (chosenIndex == -1)
        {
            Debug.LogWarning("������ ��� ����, �⺻ ��ġ�� �̵�");
            chosenIndex = fallbackIndex;
        }

        Transform targetSpawn = spawnPoints[chosenIndex].transform;
        yield return StartCoroutine(MoveToTarget(targetSpawn.position));

        if (GameManager.Instance.TimeUntilNight() <= 30f)
        {
            Debug.Log("�� �ð� �ӹ�, 2���� �ൿ �ߴ�");
            GameManager.Instance.ReleaseSpawner(chosenIndex);
            GameManager.Instance.StoreItemsAndReturnToBase(this);
            yield break;
        }

        Debug.Log("10�ʰ� ���� ���");
        yield return new WaitForSeconds(10f);

        GameManager.Instance.CollectResource("wood");
        GameManager.Instance.ReleaseSpawner(chosenIndex);

        var needRepairList = GameManager.Instance.GetNeedsRepair();
        if (needRepairList.Count > 0)
        {
            Debug.Log("���� ��� �߰�, 1���� �ൿ �簳");
            isSecondPriorityStarted = false;
            StartRepair();
        }
        else
        {
            Debug.Log("���� ��� ����, 2���� �ൿ �ݺ�");
            StartCoroutine(StartSecondPriorityAction());
        }
    }

    public IEnumerator MoveToTarget(Vector3 destination, float stoppingDistance = 2f)
    {
        float moveSpeed = 2f;
        while (Vector3.Distance(transform.position, destination) > stoppingDistance)
        {
            Vector3 direction = (destination - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            yield return null;
        }
    }

    // --- �� �ൿ �Լ��� ---

    private void Wander()
    {
        transform.position += moveDirection * speed * Time.deltaTime;
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            Debug.Log("Night Mariner AI �̵� �� ��� ����");
            EnterIdleState();
        }
    }

    private void Idle()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            Debug.Log("Night Mariner AI ��⿡�� �ٽ� �̵� ����");
            EnterWanderingState();
        }
    }

    private void EnterWanderingState()
    {
        SetRandomDirection();
        currentState = MarinerState.Wandering;
        stateTimer = moveDuration;
        Debug.Log("���� �������� �̵� ����");
    }

    private void EnterIdleState()
    {
        currentState = MarinerState.Idle;
        stateTimer = idleDuration;
        Debug.Log("Night Mariner AI ��� ���·� ��ȯ");
    }

    private void SetRandomDirection()
    {
        float angle = Random.Range(0f, 360f);
        moveDirection = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)).normalized;
    }

    private bool DetectTarget()
    {
        Collider[] hits = Physics.OverlapBox(
            transform.position,
            new Vector3(1.5f, 0.5f, 1.5f),
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

    private void LookAtTarget()
    {
        if (target == null) return;
        Vector3 dir = (target.position - transform.position).normalized;
        dir.y = 0f;

        if (dir != Vector3.zero)
            transform.forward = dir;
    }

    private IEnumerator AttackSequence()
    {
        currentState = MarinerState.Attacking;

        Vector3 targetOffset = (target.position - transform.position).normalized;
        Vector3 attackPosition = target.position - targetOffset;
        attackPosition.y = transform.position.y;

        while (Vector3.Distance(transform.position, attackPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, attackPosition, speed * Time.deltaTime);
            yield return null;
        }

        isShowingAttackBox = true;
        yield return new WaitForSeconds(attackVisualDuration);
        isShowingAttackBox = false;

        Vector3 boxCenter = transform.position + transform.forward * 1f;
        Collider[] hits = Physics.OverlapBox(boxCenter, new Vector3(1f, 0.5f, 1f), transform.rotation, targetLayer);

        foreach (var hit in hits)
        {
            Debug.Log($"{hit.name} ���� ���� ��");
        }

        currentState = MarinerState.Wandering;
        stateTimer = moveDuration;
        SetRandomDirection();
        attackRoutine = null;
    }

    private void OnDrawGizmos()
    {
        if (isShowingAttackBox)
        {
            Gizmos.color = Color.red;
            Vector3 boxCenter = transform.position + transform.forward * 1f;
            Gizmos.matrix = Matrix4x4.TRS(boxCenter, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(2f, 1f, 2f));
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector3(3f, 1f, 3f));
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class InfectedMarinerAI : CreatureBase, IBegin
{
    public int marinerId;
    public bool isRepairing = false;
    private DefenseObject targetRepairObject;
    private int repairAmount = 30;

    private bool isSecondPriorityStarted = false;

    private float nightConfusionTime; // ���� ȥ�� �ð�

    private bool isNight = false;
    private bool isConfused = false;
    private bool isNightBehaviorStarted = false;

    private NavMeshAgent agent;

    private void Awake()
    {
        maxHp = 100;
        speed = 1f;
        attackDamage = 6;
        attackRange = 3f;
        attackDelayTime = 1f;

        // CreatureBase�� fov ���� ���
        fov = GetComponent<FOVController>();

        gameObject.layer = LayerMask.NameToLayer("Mariner");
    }

    public override void Init()
    {
        agent = GetComponent<NavMeshAgent>();

        // FOVController �ʱ�ȭ
        if (fov != null)
        {
            fov.Init();
        }

        nightConfusionTime = Random.Range(0f, 30f);
        Debug.Log($"������ �¹��� {marinerId} �ʱ�ȭ - HP: {maxHp}, ���ݷ�: {attackDamage}, �ӵ�: {speed}");
        Debug.Log($"{marinerId} �� ȥ�� �õ尪 ����: {nightConfusionTime:F2}��");

        base.Init();
    }

    private void Update()
    {
        if (IsDead) return;

        if (GameManager.Instance.IsDaytime && !isNightBehaviorStarted)
        {
            isNight = false;

            if (!isRepairing)
            {
                StartRepair();
            }
        }
        else if (!isNight)
        {
            isNight = true;
            StartCoroutine(NightBehaviorRoutine());
        }
    }

    /// <summary>
    /// �� ����
    /// </summary>
    private void StartRepair()
    {
        List<DefenseObject> needRepairList = GameManager.Instance.GetNeedsRepair();

        for (int i = 0; i < needRepairList.Count; i++)
        {
            DefenseObject obj = needRepairList[i];

            if (GameManager.Instance.TryOccupyRepairObject(obj, marinerId))
            {
                targetRepairObject = obj;

                if (GameManager.Instance.CanMarinerRepair(marinerId, targetRepairObject))
                {
                    Debug.Log($"������ �¹��� {marinerId} ���� ����: {targetRepairObject.name}");
                    isRepairing = true;
                    StartCoroutine(MoveToRepairObject(targetRepairObject.transform.position));
                    return;
                }
                else
                {
                    GameManager.Instance.ReleaseRepairObject(obj); // ���� ����
                }
            }
        }

        if (!isSecondPriorityStarted)
        {
            Debug.Log("������ �¹��� ���� ��� ���� -> 2���� �ൿ ����");
            isSecondPriorityStarted = true;
            StartCoroutine(StartSecondPriorityAction());
        }
    }

    private IEnumerator MoveToRepairObject(Vector3 targetPosition)
    {
        // NavMeshAgent�� ���� ��� ��ġ�� �̵�
        agent.SetDestination(targetPosition);

        // �̵��� �Ϸ�� ������ ��ٸ��ϴ�.
        while (!IsArrived())
        {
            yield return null;
        }

        // �̵� �Ϸ� �� ���� �۾��� �����մϴ�.
        StartCoroutine(RepairProcess());
    }

    private IEnumerator RepairProcess()
    {
        float repairDuration = 10f;
        float elapsedTime = 0f;

        while (elapsedTime < repairDuration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        bool repairSuccess = Random.value > 0.7f;

        if (repairSuccess)
        {
            Debug.Log($"Infected Mariner {marinerId} ���� ����: {targetRepairObject.name}/ ������: {repairAmount}");
            targetRepairObject.Repair(repairAmount);
        }
        else
        {
            Debug.Log($"Infected Mariner {marinerId} ���� ����: {targetRepairObject.name}/ ������: {repairAmount}");
            targetRepairObject.Repair(0);
        }

        isRepairing = false;
        GameManager.Instance.UpdateRepairTargets();

        if (GameManager.Instance.TimeUntilNight() <= 30f)
        {
            Debug.Log("������ �¹��� �� ���� �����ൿ ����");
            StoreItemsAndReturnToBase();
            yield break;
        }

        StartRepair();
        GameManager.Instance.ReleaseRepairObject(targetRepairObject);
    }

    public IEnumerator StartSecondPriorityAction()
    {
        Debug.Log("������ AI ������ Ȱ�� ���� ��, ������ �Ĺ� ����.");

        GameObject[] spawnPoints = GameManager.Instance.spawnPoints;
        List<int> triedIndexes = new List<int>();
        int fallbackIndex = (marinerId % 2 == 0) ? 0 : 1; // �ӽ� Ȧ¦ fallback
        int chosenIndex = -1;

        while (triedIndexes.Count < spawnPoints.Length)
        {
            int index = triedIndexes.Count == 0 ? fallbackIndex : Random.Range(0, spawnPoints.Length);

            if (triedIndexes.Contains(index)) continue;

            if (!GameManager.Instance.IsSpawnerOccupied(index))
            {
                GameManager.Instance.OccupySpawner(index);
                chosenIndex = index;
                Debug.Log("���� �ٸ� �¹����� ����� �� ������");
                break;
            }
            else
            {
                triedIndexes.Add(index);
                float waitTime = Random.Range(0f, 1f);
                Debug.Log("�ٸ� �¹����� ���� ���̶� ���� �ð� �� �ٽ� Ž��");
                yield return new WaitForSeconds(waitTime);
            }
        }

        if (chosenIndex == -1)
        {
            Debug.LogWarning("��� �¹����� ����� ������ ó�� ��ġ�� �̵���.");
            chosenIndex = fallbackIndex;
        }

        Transform targetSpawn = spawnPoints[chosenIndex].transform;
        MoveTo(targetSpawn.position);

        while (!IsArrived())
        {
            yield return null;
        }

        if (GameManager.Instance.TimeUntilNight() <= 30f)
        {
            Debug.Log("������ �¹��� �� �ൿ ����");
            GameManager.Instance.ReleaseSpawner(chosenIndex);
            StoreItemsAndReturnToBase();
            yield break;
        }

        Debug.Log("������ �¹��� ��¥ �Ĺ� 10��");
        yield return new WaitForSeconds(10f);

        GameManager.Instance.ReleaseSpawner(chosenIndex);

        var needRepairList = GameManager.Instance.GetNeedsRepair();
        if (needRepairList.Count > 0)// ������� Ȯ��
        {
            Debug.Log("������ �¹��� ���� ��� �߰����� 1���� �ൿ ����");
            isSecondPriorityStarted = false;
            StartRepair();
        }
        else
        {
            Debug.Log("������ �¹��� ���� ��� �̹߰����� 2���� �ൿ ����");
            StartCoroutine(StartSecondPriorityAction());
        }
    }

    private void StoreItemsAndReturnToBase()
    {
        Debug.Log("�¹����� ���������� ������ ����, ����?");
        // TODO: ������ �¹��� ������ ������ �ൿ �߰� ����
    }

    // �� ���� MoveToTarget ���� �� NavMeshAgent ���� �߰�

    public void MoveTo(Vector3 destination)
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(destination);
        }
    }

    public bool IsArrived()
    {
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    }

    /// <summary>
    /// �� ����
    /// </summary>
    private IEnumerator NightBehaviorRoutine() // ȥ�� ����
    {
        isNightBehaviorStarted = true;
        isConfused = true;
        Debug.Log("ȥ�� ���� ����");

        // speed ���� ��� (ȥ�� ���¿����� �� ������)
        float confusedSpeed = speed * 3f; // CreatureBase�� speed * 3
        float escapedTime = 0;

        float angle = Random.Range(0f, 360f);
        Vector3 direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0f, Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;

        while (escapedTime < nightConfusionTime)
        {
            escapedTime += Time.deltaTime;
            transform.position += direction * confusedSpeed * Time.deltaTime;

            yield return null;
        }

        isConfused = false;
        Debug.Log("ȥ�� ���� �� ���� AI�� ����");

        agent.ResetPath(); // �� ���������� ��� �ʱ�ȭ
        ChangeToZombieAI();
    }

    private void ChangeToZombieAI()
    {
        Debug.Log("���� AI��ȯ");

        // ZombieMarinerAI ������Ʈ �߰�
        if (GetComponent<ZombieMarinerAI>() == null)
        {
            ZombieMarinerAI zombieAI = gameObject.AddComponent<ZombieMarinerAI>();
            zombieAI.marinerId = this.marinerId;
        }

        // ���� InfectedMarinerAI ������Ʈ ����
        Destroy(this);
    }

    // ��� �� Ư���� ó���� �ʿ��� ��� �������̵�
    public override void WhenDestroy()
    {
        Debug.Log($"������ �¹��� {marinerId} ���!");
        // ������ �¹��� ��� �� Ư���� ���� �߰� ����
        base.WhenDestroy();
    }
}
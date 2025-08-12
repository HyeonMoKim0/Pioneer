using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class InfectedMarinerAI : MarinerBase, IBegin
{
    // ������ �¹��� ���� ����
    public int marinerId;

    // �� ȥ�� ����
    private float nightConfusionTime; // ���� ȥ�� �ð�
    private bool isNight = false;
    private bool isConfused = false;
    private bool isNightBehaviorStarted = false;

    private void Awake()
    {
        maxHp = 100;
        speed = 1f;
        attackDamage = 6;
        attackRange = 3f;
        attackDelayTime = 1f;

        fov = GetComponent<FOVController>();

        gameObject.layer = LayerMask.NameToLayer("Mariner");
    }

    public override void Start()
    {
        if (fov != null)
        {
            fov.Start();
        }

        nightConfusionTime = Random.Range(0f, 30f);
        Debug.Log($"������ �¹��� {marinerId} �ʱ�ȭ - HP: {maxHp}, ���ݷ�: {attackDamage}, �ӵ�: {speed}");
        Debug.Log($"{marinerId} �� ȥ�� �õ尪 ����: {nightConfusionTime:F2}��");

        base.Start();
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
    /// ������ �¹����� 2���� �ൿ (��¥ �Ĺ�)
    /// </summary>
    public override IEnumerator StartSecondPriorityAction()
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

            if (!MarinerManager.Instance.IsSpawnerOccupied(index))
            {
                MarinerManager.Instance.OccupySpawner(index);
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

        UnityEngine.Transform targetSpawn = spawnPoints[chosenIndex].transform;
        MoveTo(targetSpawn.position);

        while (!IsArrived())
        {
            yield return null;
        }

        if (GameManager.Instance.TimeUntilNight() <= 30f)
        {
            Debug.Log("������ �¹��� �� �ൿ ����");
            MarinerManager.Instance.ReleaseSpawner(chosenIndex);
            StoreItemsAndReturnToBase(); // ������ �¹��� ���� ó��
            yield break;
        }

        Debug.Log("������ �¹��� ��¥ �Ĺ� 10��");
        yield return new WaitForSeconds(10f);

        MarinerManager.Instance.ReleaseSpawner(chosenIndex);

        var needRepairList = MarinerManager.Instance.GetNeedsRepair();
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

    /// <summary>
    /// ������ �¹��� ������ ó�� (������ ����)
    /// </summary>
    private void StoreItemsAndReturnToBase()
    {
        Debug.Log("�¹����� ���������� ������ ����, ����?");
        // TODO: ������ �¹��� ������ ������ �ൿ �߰� ����
    }

    /// <summary>
    /// �� ȥ�� �ൿ
    /// </summary>
    private IEnumerator NightBehaviorRoutine()
    {
        isNightBehaviorStarted = true;
        isConfused = true;
        Debug.Log("ȥ�� ���� ����");

        float escapedTime = 0;

        float angle = Random.Range(0f, 360f);
        Vector3 direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0f, Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;

        while (escapedTime < nightConfusionTime)
        {
            escapedTime += Time.deltaTime;
            transform.position += direction * speed * Time.deltaTime;

            yield return null;
        }

        isConfused = false;
        Debug.Log("ȥ�� ���� �� ���� AI�� ����");

        agent.ResetPath();
        ChangeToZombieAI();
    }

    /// <summary>
    /// ���� AI�� ��ȯ
    /// </summary>
    private void ChangeToZombieAI()
    {
        Debug.Log("���� AI��ȯ");

        if (GetComponent<ZombieMarinerAI>() == null)
        {
            ZombieMarinerAI zombieAI = gameObject.AddComponent<ZombieMarinerAI>();
            zombieAI.marinerId = this.marinerId;
        }

        Destroy(this);
    }

    /// <summary>
    /// ������ �¹����� 30% ���� ������ (70% ����)
    /// </summary>
    protected override float GetRepairSuccessRate()
    {
        return 0.3f; // 30% ������
    }

    /// <summary>
    /// �¹��� ID ��ȯ
    /// </summary>
    protected override int GetMarinerId()
    {
        return marinerId;
    }

    /// <summary>
    /// �¹��� Ÿ�� �̸� ��ȯ
    /// </summary>
    protected override string GetCrewTypeName()
    {
        return "�����¹���";
    }

    /// <summary>
    /// ���� �ٰ��� �� ó�� (������ �¹��� ����)
    /// </summary>
    protected override void OnNightApproaching()
    {
        StoreItemsAndReturnToBase();
    }
}
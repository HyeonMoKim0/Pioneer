using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarinerAI : MonoBehaviour
{
    public int marinerId;
    public bool isRepairing = false;
    private DefenseObject targetRepairObject;
    private int repairAmount = 30;
    private bool isSecondPriorityStarted = false;

    private void Update()
    {
        if (!isRepairing && GameManager.Instance.IsDaytime)
        {
            StartRepair();
        }
    }

    private void StartRepair()
    {
        List<DefenseObject> needRepairList = GameManager.Instance.GetNeedsRepair();

        if (needRepairList.Count > 0)
        {
            targetRepairObject = needRepairList[0]; // �ӽ÷� index 0�� �׽�Ʈ ����

            if (GameManager.Instance.CanMarinerRepair(marinerId, targetRepairObject))
            {
                Debug.Log("�¹��� ���� ��");
                isRepairing = true;
                StartCoroutine(RepairProcess());
            }
            Debug.Log($"Mariner {marinerId} ������ ������Ʈ : {targetRepairObject.name}, ���� HP: {targetRepairObject.currentHP}/{targetRepairObject.maxHP}");
        }
        else
        {
            if (!isSecondPriorityStarted)
            {
                Debug.Log("���� ������Ʈ �������� 2���� �ൿ ����");
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

        Debug.Log($"Mariner {marinerId} ���� �Ϸ�: {targetRepairObject.name}/ ������: {repairAmount}");
        targetRepairObject.Repair(repairAmount);

        isRepairing = false;
        GameManager.Instance.UpdateRepairTargets();

        if (GameManager.Instance.TimeUntilNight() <= 30f)
        {
            Debug.Log("�Ϲ� �¹��� �� ���� �����ൿ ����");
            GameManager.Instance.StoreItemsAndReturnToBase(this); // �ӽ� ���� �ʿ�
            yield break;
        }

        StartRepair();
    }

    public IEnumerator StartSecondPriorityAction()
    {
        Debug.Log("�Ϲ� �¹��� 2���� �� �ൿ ����");

        GameObject[] spawnPoints = GameManager.Instance.spawnPoints;
        List<int> triedIndexes = new List<int>();
        int fallbackIndex = (marinerId % 2 == 0) ? 0 : 1; // �ӽ÷� �����ʴ� 0 �� 1�� Ȧ¦ ������ ���߿�?
        int chosenIndex = -1;

        while (triedIndexes.Count < spawnPoints.Length)
        {
            int index = triedIndexes.Count == 0 ? fallbackIndex : Random.Range(0, spawnPoints.Length);
            // ���� 0�� 1�� ��� �� ���߿� ������ ���� ������ ����

            if (triedIndexes.Contains(index)) continue; // �̹� �õ��� �����ʴ� �Ƕ�

            if (!GameManager.Instance.IsSpawnerOccupied(index)) // �� ���� ��
                // ���õ� �����ʰ� �̹� �ٸ� ������ �����ߴ°�? �÷ο���Ʈ Ȯ��
            {
                GameManager.Instance.OccupySpawner(index);
                chosenIndex = index;
                Debug.Log("���� �ٸ� �¹����� ����� �� ������");
                
                break;
            }
            else // ������
            {
                triedIndexes.Add(index);
                float waitTime = Random.Range(0f, 1f);
                Debug.Log("�ٸ� �¹����� ���� ���̶� ���� �ð� �� �ٽ� Ž��");
                yield return new WaitForSeconds(waitTime);
            }
        }

        if (chosenIndex == -1) // ���� ó�� �ʿ��ұ�?
        {
            Debug.LogWarning("��� �¹����� ����� ������ ó�� ��ġ�� �̵���.");
            chosenIndex = fallbackIndex; // ù ��ġ�� �̵�
        }

        Transform targetSpawn = spawnPoints[chosenIndex].transform;
        yield return StartCoroutine(MoveToTarget(targetSpawn.position));

        if (GameManager.Instance.TimeUntilNight() <= 30f)
        {
            Debug.Log("�¹��� �� �ൿ ����");
            GameManager.Instance.ReleaseSpawner(chosenIndex);
            GameManager.Instance.StoreItemsAndReturnToBase(this);
            yield break;
        }

        Debug.Log("�¹��� 10�� ���� ����");
        yield return new WaitForSeconds(10f);

        GameManager.Instance.CollectResource("wood"); // ��¸�
        GameManager.Instance.ReleaseSpawner(chosenIndex);

        var needRepairList = GameManager.Instance.GetNeedsRepair();
        if (needRepairList.Count > 0)// ������� Ȯ��
        {
            Debug.Log("�¹��� ���� ��� �߰����� 1���� �ൿ ����");
            isSecondPriorityStarted = false;
            StartRepair();
        }
        else
        {
            Debug.Log("�¹��� ���� ��� �̹߰����� 2���� �ൿ ����");
            StartCoroutine(StartSecondPriorityAction());
        }
    }

    public IEnumerator MoveToTarget(Vector3 destination, float stoppingDistance = 2f) // 2,2,2?? ���� ����?
    {
        float speed = 2f;
        while (Vector3.Distance(transform.position, destination) > stoppingDistance) // 2M ������ �̵�
        {
            Vector3 direction = (destination - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
            yield return null;
        }
    }
}
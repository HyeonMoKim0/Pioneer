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

    private void StartRepair() // 1���� �ൿ: HP�� 50% ������ ��ġ�� ������Ʈ ���� (3�� ���� ����)
    {
        List<DefenseObject> needRepairList = GameManager.Instance.GetRepairTargetsNeedingRepair();

        if (needRepairList.Count > 0)
        {
            targetRepairObject = needRepairList[0];
            Debug.Log($"[Mariner {marinerId}] ������ ������Ʈ ����: {targetRepairObject.name}, ���� HP: {targetRepairObject.currentHP}/{targetRepairObject.maxHP}");

            if (GameManager.Instance.CanMarinerRepair(marinerId, targetRepairObject))
            {
                Debug.Log($"[Mariner {marinerId}] ���� ����: {targetRepairObject.name}");
                isRepairing = true;
                StartCoroutine(RepairProcess());
            }
            else
            {
                Debug.Log($"[Mariner {marinerId}] ������ġ�� �����ϴ�");
            }
        }
        else
        {
            if (!isSecondPriorityStarted)
            {
                Debug.Log($"[Mariner {marinerId}] ������ ������Ʈ�� �������� 2���� �ൿ ����");
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

        Debug.Log($"[Mariner {marinerId}] ���� �� �Դϴ� : {targetRepairObject.name}/ ������: {repairAmount}");
        targetRepairObject.Repair(repairAmount);

        isRepairing = false;

        GameManager.Instance.UpdateRepairTargets();
        StartRepair();
    }

    private IEnumerator StartSecondPriorityAction() // 2���� �ൿ: �ٴ� ������ �Ĺ� (10�� ������ �ൿ ��, ȹ��)
    {
        Debug.Log($"[Mariner {marinerId}] 2���� �ൿ���� ������ �Ĺ� ����");

        yield return new WaitForSeconds(10f);

        Debug.Log($"[Mariner {marinerId}] �Ĺ� �Ϸ� �� ������ ȹ��");

        var needRepairList = GameManager.Instance.GetRepairTargetsNeedingRepair();

        if (needRepairList.Count > 0)
        {
            Debug.Log($"[Mariner {marinerId}] ���� ��� �߰� �� ������ ����");
            isSecondPriorityStarted = false;
            StartRepair();
        }
        else
        {
            Debug.Log($"[Mariner {marinerId}] ���� ��� ���� �� �ٽ� ������ �Ĺ�");
            StartCoroutine(StartSecondPriorityAction());
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFishing : MonoBehaviour
{
    [System.Serializable]
    public struct FishingDropItem
    {
        public SItemTypeSO itemData;
        public float dropProbability;
    }

    [Header("���� ������ ��� ���̺�")]
    public List<FishingDropItem> dropItemTable;

    [Header("���� ������")]
    public SItemTypeSO treasureItem;

    private Coroutine fishingLoopCoroutine;

    private void Awake()
    {

    }

    public void StartFishingLoop()
    {
        if(fishingLoopCoroutine == null)
        {
            fishingLoopCoroutine = StartCoroutine(FishingLoop());
        }
    }

    public void StopFishingLoop()
    {
        if (fishingLoopCoroutine != null)
        {
            StopCoroutine(fishingLoopCoroutine);
            fishingLoopCoroutine = null;
            Debug.Log("���� �ߴ�");
        }
    }

    private IEnumerator FishingLoop()
    {        
        while (true)
        {
            Debug.Log("���� ����");
            yield return new WaitForSeconds(2f);
            SItemTypeSO caughtItem = GetItem();
            if(caughtItem != null)
            {                
                SItemStack itemStack = new SItemStack(caughtItem.id, 1);
                InventoryManager.Instance.Add(itemStack);
                PlayerStatsLevel.Instance.AddExp(GrowStatType.Fishing, 5);
                Debug.Log($"������ ȹ��: {caughtItem.typeName}, ����ġ +{5}");
            }
            else
            {
                Debug.LogError("������ ȹ�濡 �����߽��ϴ�. ��� ���̺��� Ȯ�����ּ���.");
            }

            Debug.Log("���� ��");
        }
    }

    private SItemTypeSO GetItem()
    {
        Debug.Log("������ ��� ����");
        float totalProbability = 0f;
        // 1. ��ü ����ġ �� ���
        for (int i = 0; i < dropItemTable.Count; i++)
        {
            totalProbability += dropItemTable[i].dropProbability;
        }

        if (totalProbability <= 0)
        {
            return dropItemTable[0].itemData;
        }
        Debug.Log($"����ġ ��� �� : {totalProbability}");
        // 2. 0 ~ ��ü ����ġ���� ���� ���� �̱�
        float randomNum = Random.Range(0f, totalProbability);
        Debug.Log("���� �� �ɸ�");
        Debug.Log($"randomNum : {randomNum}");
        // 3. ���� ���ڰ� ���� �������� ����ġ ���� ������ ��÷
        foreach (var item in dropItemTable)
        {
            if(randomNum <= item.dropProbability)
            {
                return item.itemData;
            }
            // 4. ��÷�����ʾ����� ���� ������ ����ġ�� ���� ���� ���������� �Ѿ
            randomNum -= item.dropProbability;
        }
        
        return dropItemTable[dropItemTable.Count - 1].itemData;
    }
}
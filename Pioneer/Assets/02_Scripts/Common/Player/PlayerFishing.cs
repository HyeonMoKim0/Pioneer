using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFishing : MonoBehaviour
{
    public static PlayerFishing instance;

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

    private int fishingExp = 5;

    private void Awake()
    {
        instance = this;
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

                if(caughtItem == treasureItem)
                {
                    TreasureBoxManager.instance.GetBox();
                    fishingExp = 10;
                }
                else
                {
                    fishingExp = 5;
                    InventoryManager.Instance.Add(itemStack);
                }

                PlayerStatsLevel.Instance.AddExp(GrowStatType.Fishing, fishingExp);
                Debug.Log($">> PlayerFishing.FishingLoop() ������ ȹ��: ���� {caughtItem.id}, �̸� {caughtItem.typeName}, ����ġ +{fishingExp}");

                (float extraItemChance, float treasureChestChance) chances = PlayerStatsLevel.Instance.FishingChance();

                if(Random.Range(0f, 1f) < chances.extraItemChance)
                {
                    if (caughtItem == treasureItem)
                    {
                        TreasureBoxManager.instance.GetBox();
                    }
                    else
                    {
                        InventoryManager.Instance.Add(itemStack);
                    }
                    Debug.Log($"<color=cyan>[���� ���� ���ʽ�!]</color> {caughtItem.typeName}��(��) �߰��� ȹ���߽��ϴ�! (Ȯ��: {chances.extraItemChance * 100:F2}%)");
                }

                if(Random.Range(0f, 1f) < chances.treasureChestChance)
                {
                    if(treasureItem != null)
                    {
                        //SItemStack treasureItemStack = new SItemStack(treasureItem.id, 1);
                        //InventoryManager.Instance.Add(treasureItemStack);

                        TreasureBoxManager.instance.GetBox();
                        Debug.Log($"<color=yellow>[���� ���� ���ʽ�!]</color> �������ڸ� �߰��� ȹ���߽��ϴ�! (Ȯ��: {chances.treasureChestChance * 100:F2}%)");
                    }
                }

                // �ٵ��̺�Ʈ ������ ��� �߰� ������ ȹ�� 
                if(OceanEventManager.instance.currentEvent is OceanEventWaterBloom)
                {
                    OceanEventWaterBloom waterBloomEnvent = OceanEventManager.instance.currentEvent as OceanEventWaterBloom;

                    SItemTypeSO bonusItem = waterBloomEnvent.GetMoreItem();

                    if(bonusItem != null)
                    {
                        SItemStack waterBloombonusItemStack = new SItemStack(bonusItem.id, 1);
                        InventoryManager.Instance.Add(waterBloombonusItemStack);
                    }
                }
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
        // 2. 0 ~ ��ü ����ġ���� ���� ���� �̱�
        float randomNum = Random.Range(0f, totalProbability);
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
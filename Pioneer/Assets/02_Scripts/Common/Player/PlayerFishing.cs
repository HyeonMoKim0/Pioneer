using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFishing : MonoBehaviour
{
    [System.Serializable]
    public struct FishingDropItem
    {
        public SItemStack itemStack;
        public float dropProbability;
    }

    [Header("���� ������ ��� ���̺�")]
    public List<FishingDropItem> dropItemTable;

    [Header("���� ������ ����ġ")]
    private int fishingExp = 5;
    private int treasureChestExp = 10;

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
            GetItem();
            // PlayerStatsLevel.Instance.AddExp(); => ������ �����Ǹ� �ش� �����ۿ����� ����ġ �ο�?
            Debug.Log("���� ��");
        }
    }

    private void GetItem()
    {
        float totalProbability = 0f;
        // 1. ��ü ����ġ �� ���
        for (int i = 0; i < dropItemTable.Count; i++)
        {
            totalProbability += dropItemTable[i].dropProbability;
        }
        // 2. 0 ~ ��ü ����ġ���� ���� ���� �̱�
        float randomNum = Random.Range(0f, totalProbability);
        // 3. ���� ���ڰ� ���� �������� ����ġ ���� ������ ��÷
        foreach(var item in dropItemTable)
        {
            if(randomNum <= item.dropProbability)
            {
                // ������ ȹ�� ����
                Debug.Log("������ ȹ��");
                return;
            }
            // 4. ��÷�����ʾ����� ���� ������ ����ġ�� ���� ���� ���������� �Ѿ
            randomNum -= item.dropProbability;
        }
    }
}
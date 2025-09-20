using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFishing : MonoBehaviour
{
    [Header("Item List")]
    public List<SItemStack> fishingItemList = new List<SItemStack>();

    private Coroutine fishingLoopCoroutine;

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
            yield return new WaitForSeconds(4f);
            // PlayerStatsLevel.Instance.AddExp(); => ������ �����Ǹ� �ش� �����ۿ����� ����ġ �ο�?
            Debug.Log("���� ��");
        }
    }

    private void GetItem()
    {
        // ����...
    }
}

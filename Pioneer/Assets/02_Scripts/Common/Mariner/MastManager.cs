using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MastManager : MonoBehaviour
{
    public static MastManager Instance;

    [Header("���� ����")]
    public int currentDeckCount = 0;
    public LayerMask platformLayerMask; // �÷��� ���̾��ũ

    [Header("������ ID ����")]
    public int woodItemID = 30001; // �볪�� ������ ID
    public int clothItemID = 30003; // õ ������ ID

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        UpdateCurrentDeckCount();
    }

    public int GetItemCount(int itemID)
    {
        if (InventoryManager.Instance == null) return 0; // �κ����� ���

        int count = 0;
        foreach (var item in InventoryManager.Instance.itemLists)
        {
            if (item != null && item.id == itemID)
            {
                count += item.amount;
            }
        }
        return count;
    }

    // �κ��丮���� Ư�� ������ �Ҹ� (���� ��ȭ��)
    public bool ConsumeItems(int itemID, int amount)
    {
        if (InventoryManager.Instance == null) return false;
        if (GetItemCount(itemID) < amount) return false;

        int remainingToConsume = amount;

        for (int i = 0; i < InventoryManager.Instance.itemLists.Count && remainingToConsume > 0; i++)
        {
            var item = InventoryManager.Instance.itemLists[i];
            if (item != null && item.id == itemID)
            {
                int consumeFromSlot = Mathf.Min(item.amount, remainingToConsume);
                item.amount -= consumeFromSlot;
                remainingToConsume -= consumeFromSlot;

                if (item.amount <= 0)
                {
                    InventoryManager.Instance.itemLists[i] = null;
                }
            }
        }

        return remainingToConsume == 0;
    }

    // ���� ���� ���� ������Ʈ (CreateObject���� ȣ��)
    public void UpdateCurrentDeckCount()
    {
        Collider[] platformColliders = Physics.OverlapSphere(Vector3.zero, 1000f, platformLayerMask);
        currentDeckCount = platformColliders.Length;

        Debug.Log($"=== ���� ī��Ʈ ����� ===");
        Debug.Log($"���̾��ũ ��: {platformLayerMask.value}");
        Debug.Log($"�˻��� �ݶ��̴� ��: {platformColliders.Length}");
        Debug.Log($"���� ���� ����: {currentDeckCount}");

        // �� ���� ���� ���
        for (int i = 0; i < platformColliders.Length; i++)
        {
            Debug.Log($"���� {i}: {platformColliders[i].name} at {platformColliders[i].transform.position}");
        }
    }

    public void DecrementDeckCount(int destroyedCount)
    {
        currentDeckCount -= destroyedCount;

        if (currentDeckCount < 0)
        {
            currentDeckCount = 0;
        }

    }
    // ���ӿ��� ó��
    public void GameOver()
    {
        Debug.Log("���ӿ���! ���밡 �ı��Ǿ����ϴ�.");
        Time.timeScale = 0f;
    }
}
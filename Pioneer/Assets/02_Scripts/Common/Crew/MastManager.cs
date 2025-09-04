using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ���� �ý��� ������ (�̱���)
public class MastManager : MonoBehaviour
{
    public static MastManager Instance;

    [Header("���� ����")]
    public int currentDeckCount = 0;
    public GameObject deckPrefab; // PF_ItemDeck ������

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
        // ���۽� ���� ���� ���� ���
        UpdateCurrentDeckCount();
    }

    // �κ��丮���� Ư�� ������ ���� Ȯ��
    public int GetItemCount(int itemID)
    {
        if (InventoryManager.Instance == null) return 0;

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

    // �κ��丮���� Ư�� ������ �Ҹ�
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

    // ���� ���� ���� ������Ʈ (��ġ/���Žÿ��� ȣ��)
    public void UpdateCurrentDeckCount()
    {
        GameObject[] decks = GameObject.FindGameObjectsWithTag("Deck");
        currentDeckCount = decks.Length;
        Debug.Log($"���� ���� ����: {currentDeckCount}");
    }

    // ���� ��ġ �������� Ȯ��
    public bool CanBuildDeck(MastSystem mast)
    {
        int maxDecks = mast.GetMaxDeckCount();
        return currentDeckCount < maxDecks && GetItemCount(woodItemID) >= 30 && GetItemCount(clothItemID) >= 15;
    }

    // ���� �Ǽ�
    public bool BuildDeck(MastSystem mast, Vector3 position)
    {
        if (!CanBuildDeck(mast))
        {
            if (currentDeckCount >= mast.GetMaxDeckCount())
            {
                mast.ShowMessage("������ ���̻� ��ġ�� �� ����.", 4f);
            }
            else
            {
                mast.ShowMessage($"��ᰡ �����մϴ�.", 3f);
            }
            return false;
        }

        // �ڿ� �Ҹ�
        if (!ConsumeItems(woodItemID, 30) || !ConsumeItems(clothItemID, 15))
        {
            mast.ShowMessage("������ �Ҹ� �����߽��ϴ�.", 3f);
            return false;
        }

        // ���� ����
        GameObject newDeck = Instantiate(deckPrefab, position, Quaternion.identity);
        newDeck.layer = LayerMask.NameToLayer("Platform"); // ���̾�� ���� (���� ���̾�� �°� ����)

        // ���� �� ������Ʈ
        UpdateCurrentDeckCount();

        Debug.Log("���� �Ǽ� �Ϸ�!");

        // �κ��丮 UI ���ΰ�ħ
        if (InventoryUiMain.instance != null)
        {
            InventoryUiMain.instance.IconRefresh();
        }

        return true;
    }

    // ���ӿ��� ó��
    public void GameOver()
    {
        Debug.Log("���ӿ���! ���밡 �ı��Ǿ����ϴ�.");
        Time.timeScale = 0f;
    }
}
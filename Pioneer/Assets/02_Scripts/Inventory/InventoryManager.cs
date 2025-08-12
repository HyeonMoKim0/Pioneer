using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public SItemStack mouseInventory;
    public List<SItemStack> itemLists;
    public Dictionary<int, SItemStack> fastSearch;
    [SerializeField] int inventoryCount;
    [SerializeField] Transform positionDrop;
    [SerializeField] Vector3 dropOffset = new Vector3(1, -0.8f, -1);

    public int Get(int id)
    {
        int sum = 0;
        for (int index = 0; index < itemLists.Count; ++index)
        {
            if (itemLists[index] != null &&
                itemLists[index].id == id)
            {
                sum += itemLists[index].amount;
            }
        }

        return sum;
    }

    public void MouseSwitch(int index)
    {
        if (mouseInventory != null && itemLists[index] != null && mouseInventory.id == itemLists[index].id)
        {
            itemLists[index].amount += mouseInventory.amount;
            mouseInventory = null;
            SafeClean();
            return;
        }

        SItemStack temp = itemLists[index];
        itemLists[index] = mouseInventory;
        mouseInventory = temp;
    }

    public void MouseSplit(int index)
    {
        if (itemLists[index] == null)
        {
            return;
        }
        if (mouseInventory != null)
        {
            MouseSwitch(index);
            return;
        }

        int mSlotNum = itemLists[index].amount / 2;
        int mMouseNum = itemLists[index].amount - mSlotNum;

        itemLists[index].amount = mSlotNum;
        mouseInventory = new SItemStack(itemLists[index].id, mMouseNum);

        SafeClean();
    }

    public void MouseDrop()
    {
        Debug.Log($">> InventoryManager.MouseDrop() : ȣ���");

        // ItemDropManager.instance.Drop(mouseInventory, positionDrop.position);
        ItemDropManager.instance.Drop(mouseInventory, ThisIsPlayer.Player.transform.position + dropOffset);

        mouseInventory = null;
    }

    public void RemoveMouseItem()
    {
        mouseInventory = null;
    }

    public void MouseSingle(int index)
    {
        // ���콺�� ����ְ� �κ��� �������� �ִ°��� ������ ��
        // ���콺�� �����ϰ� �κ��� �� ������ ������ ��

        if (mouseInventory != null && itemLists[index] != null && (mouseInventory.id != itemLists[index].id))
        {
            return;
        }
        // ���� ���� �������� ���콺 ���� ������ �� , ctrl�� ���� ���·� ��Ŭ�� �� �� �� �� �� ĭ�� ��������.
        else if (mouseInventory != null)
        {
            if (itemLists[index] == null)
            {
                itemLists[index] = new SItemStack(mouseInventory.id, 1);
            }
            else
            {
                itemLists[index].amount++;
            }
            mouseInventory.amount--;
            
        }
        else if (itemLists[index] != null)
        {
            mouseInventory = new SItemStack(itemLists[index].id, 1);
            itemLists[index].amount--;
        }

        SafeClean();
    }

    public void Add(SItemStack item)
    {
        // ���� �������� ������ �ش� ������ ����
        // ���� �������� ���ų�, ���� ���Ⱑ �Ǹ� ���Ӱ� ����
        if (item.amount < 1) return;

        int firstEmpty = -1;
        for (int inventoryIndex = 0; inventoryIndex < itemLists.Count; ++inventoryIndex)
        {
            if (itemLists[inventoryIndex] == null)
            {
                if (firstEmpty == -1) firstEmpty = inventoryIndex;
                continue;
            }

            if (itemLists[inventoryIndex].id == item.id)
            {
                itemLists[inventoryIndex].amount += item.amount;
                InventoryUiMain.instance.IconRefresh();
                return;
            }
        }
        if (firstEmpty == -1)
        {
            ItemDropManager.instance.Drop(mouseInventory, positionDrop.transform.position);
            return;
        }
        else
        {
            itemLists[firstEmpty] = new SItemStack(item.id, item.amount);
        }
        InventoryUiMain.instance.IconRefresh();
    }

    public void Remove(params SItemStack[] removeTargets)
    {
        for (int targetIndex = 0; targetIndex < removeTargets.Length; targetIndex++)
        {
            int targetAmount = removeTargets[targetIndex].amount;

            for (int inventoryIndex = itemLists.Count - 1; inventoryIndex >= 0; --inventoryIndex)
            {
                if (itemLists[inventoryIndex] == null) continue;

                if (removeTargets[targetIndex].id == itemLists[inventoryIndex].id)
                {
                    if (targetAmount > itemLists[inventoryIndex].amount)
                    {
                        targetAmount -= itemLists[inventoryIndex].amount;
                        itemLists[inventoryIndex] = null;
                        continue;
                    }
                    itemLists[inventoryIndex].amount -= targetAmount;
                    break;
                }
            }
        }
        SafeClean();
    }

    public void SortSelf()
    {
        // ������ ��ħ
        // �׵� ������ �߰�

        for (int index = 9; index < inventoryCount; index++)
        {
            if (itemLists[index] == null) continue;

            for (int x = index + 1; x < inventoryCount; ++x)
            {
                if (itemLists[x] == null) continue;

                if (itemLists[index].id == itemLists[x].id)
                {
                    itemLists[index].amount += itemLists[x].amount;
                    itemLists[x] = null;
                }
            }
        }
        SafeClean();
        List<SItemStack> list = new List<SItemStack>();
        for (int index = 9; index < inventoryCount; index++)
        {
            if (itemLists[index] == null) continue;
            list.Add(itemLists[index]);
            itemLists[index] = null;
        }
        list = list
            .OrderBy(w => ItemTypeManager.Instance.itemTypeSearch[w.id].categories)
            .ThenBy(w => ItemTypeManager.Instance.itemTypeSearch[w.id].typeName, StringComparer.Create(
            new CultureInfo("ko-KR"), ignoreCase: false)).ToList();
        for (int index = 0; index < list.Count; index++)
        {
            itemLists[index + 9] = (list[index]);
        }
        SafeClean();
    }

    private void SafeClean()
    {
        for (int index = 0; index < inventoryCount; ++index)
        {
            if (itemLists[index] == null)
            {
                continue;
            }
            if (itemLists[index].amount < 1)
            {
                itemLists[index] = null;
            }
        }
        if (mouseInventory != null && mouseInventory.amount < 1)
        {
            mouseInventory = null;
        }
    }

    private void Awake()
    {
        Instance = this;

        itemLists = new List<SItemStack>();
        fastSearch = new Dictionary<int, SItemStack>();

        for (int i = 0; i < inventoryCount; ++i)
        {
            itemLists.Add(null);
            // Debug.Log($"awake : {itemLists[i].id}");
        }
        Demo();

        mouseInventory = null;
    }

    private void Demo()
    {
        Add(new SItemStack(30001, 10));
        Add(new SItemStack(30002, 10));

        itemLists[0] = new SItemStack(30002, 100);
        itemLists[1] = new SItemStack(100, 100);
        itemLists[2] = new SItemStack(101, 100);
        itemLists[3] = new SItemStack(102, 100);
        itemLists[4] = new SItemStack(103, 100);
        itemLists[4] = new SItemStack(30001, 200);
    }
}

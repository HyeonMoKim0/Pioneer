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
        ItemDropManager.instance.Drop(mouseInventory, positionDrop.position);

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

        if (fastSearch.ContainsKey(item.id) == false)
        {
            itemLists.Add(item);
            fastSearch.Add(item.id, item);
            return;
        }
        fastSearch[item.id].amount += item.amount;
    }

    public void Remove(params SItemStack[] removeTargets)
    {
        for (int index = 0; index < removeTargets.Length; index++)
        {
            fastSearch[removeTargets[index].id].amount -= removeTargets[index].amount;
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

        List<SItemStack> list = new List<SItemStack>();
        for (int index = 9; index < inventoryCount; index++)
        {
            if (itemLists[index] == null) continue;
            list.Add(itemLists[index]);
            itemLists[index] = null;
        }
        list.OrderBy(w => ItemTypeManager.Instance.itemTypeSearch[w.id].typeName, StringComparer.Create(
            new CultureInfo("ko-KR"), ignoreCase: false));
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
    }

    private void Demo()
    {
        Add(new SItemStack(30001, 100));
        Add(new SItemStack(30002, 100));

        itemLists[0] = new SItemStack(30002, 100);
        itemLists[1] = new SItemStack(100, 100);
        itemLists[2] = new SItemStack(101, 100);
        itemLists[3] = new SItemStack(102, 100);
        itemLists[4] = new SItemStack(103, 100);
    }
}

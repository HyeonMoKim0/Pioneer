using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SItemStack
{
    public int id;
    public int amount;
    public int duability; // ������
    //public bool isCanStack;
    public bool isUseCoroutineEnd = true; // �ش� Ŭ������ �������� ����. SItemTypeSO �� �ܺ� Ŭ������ ����

    public SItemTypeSO itemBaseType => ItemTypeManager.Instance.FindType(this);

    public virtual int GetID() => id;
    //public virtual bool IsCanStack() => isCanStack;

    public SItemStack(SItemStack from)
    {
        this.id = from.id;
        this.amount = from.amount;
        this.duability = from.duability;
    }
    public SItemStack(int id, int amount)
    {
        this.id = id;
        this.amount = amount;
    }
    public SItemStack(int id, int amount, int duability)
    {
        this.id = id;
        this.amount = amount;
        this.duability = duability;

	}

    public static bool IsEmpty(SItemStack target)
    {
        if (target == null) return true;
        if (target.id == 0) return true;
        return false;
    }
    public SItemStack Copy()
    {
        return new SItemStack(id, amount, duability);
    }

    // �������� �ִ� �������� ������ ������ ����Ű�� �Ͱ� � �������� ���̴��� ���ϰ� ����Ǿ����� ������ ���Դϴ�.
    public virtual void Use()
    {
        // ���� �������� ���

        // 
    }
}


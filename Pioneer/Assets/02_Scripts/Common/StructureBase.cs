using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

/// <summary>
/// ����ü�� �� �� �ִ� ����� ���� ���� �����ϱ� �ϴ� �־��, �ٵ� �ϴ� ������ ����ֱ� �� ���� 
/// </summary>
public class StructureBase : CommonBase
{
    [field: SerializeField]
    public bool isUsing { get; private set; }


    void Start()
    {
        
    }

    public void Repair()
    {
        // ����
    }



    #region ��ȣ�ۿ� ������ ������Ʈ�� ����� ��
    public virtual void Interactive()
    {

    }

    public virtual void Use()
    {
        // ������� �� ����


        isUsing = true;
    }

    public virtual void UnUse()
    {
        // ��� �������� �� ����
        isUsing = false;
    }
    #endregion
}
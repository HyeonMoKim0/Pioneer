using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

/// <summary>
/// ����ü�� �� �� �ִ� ����� ���� ���� �����ϱ� �ϴ� �־��, �ٵ� �ϴ� ������ ����ֱ� �� ���� 
/// </summary>
public class StructureBase : CommonBase
{
    public bool isUsing { get; private set; }

    void Start()
    {
        
    }

    public void Repair()
    {
        // ����
        Debug.Log(MethodBase.GetCurrentMethod().Name);
    }



    #region ��ȣ�ۿ� ������ ������Ʈ�� ����� ��
    public virtual void Interactive()
    {
        Debug.Log(MethodBase.GetCurrentMethod().Name);
    }

    public virtual void Use()
    {
        // ������� �� ����
        Debug.Log(MethodBase.GetCurrentMethod().Name);


        isUsing = true;
    }

    public virtual void UnUse()
    {
        // ��� �������� �� ����
        Debug.Log(MethodBase.GetCurrentMethod().Name);

        isUsing = false;
    }
    #endregion
}
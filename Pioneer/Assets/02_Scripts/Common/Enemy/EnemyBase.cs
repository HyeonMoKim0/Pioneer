using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class EnemyBase : CreatureBase, IBegin
{
    [Header("�⺻ �Ӽ�")]
    protected float idleTime;
    protected GameObject targetObject;
    protected float detectionRange;
    protected LayerMask detectMask;

    /// <summary>
    /// �Ӽ� ������ �� �Ҵ�
    /// </summary>
    protected virtual void SetAttribute()
    {

    }

    /// <summary>
    /// ���� Ÿ������ ����
    /// </summary>
    protected void SetMastTarget()
    {
        targetObject = GameObject.FindGameObjectWithTag("Engine");
    }
}
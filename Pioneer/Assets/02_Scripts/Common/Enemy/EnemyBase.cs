using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class EnemyBase : CreatureBase, IBegin
{
    [Header("�⺻ �Ӽ�")]
    protected GameObject targetObject;
    protected float detectionRange;
    protected LayerMask detectMask;

    // ���� Ÿ������ ����
    protected void SetMastTarget()
    {
        targetObject = GameObject.FindGameObjectWithTag("Engine");
    }
}
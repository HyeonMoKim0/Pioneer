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

    [SerializeField] private Vector3 attackBoxCenterOffset;

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

    /// <summary>
    /// ���� ���� �� ��� �ݶ��̴��� ã�� �迭�� ��ȯ
    /// </summary>
    protected Collider[] DetectAttackRange(float attackRange)
    {
        Vector3 boxCenter = transform.position + transform.forward * attackBoxCenterOffset.z + transform.up * attackBoxCenterOffset.y;
        Vector3 halfBoxSize = new Vector3(1f, 1f, attackRange / 2);
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            9
        
        Collider[] hitColliders = Physics.OverlapBox(boxCenter, halfBoxSize, transform.rotation, detectMask);

        return hitColliders;
    }
}
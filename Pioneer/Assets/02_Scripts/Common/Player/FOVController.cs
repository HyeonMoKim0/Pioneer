using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

// 250805 : �þ� ������ �ʿ��� �� ��ũ��Ʈ���� DetectTargets �Լ��� ������ ���̾ �Ű������� �����Ͽ� ȣ���Ͽ� ����ϵ��� ����

public class FOVController : MonoBehaviour, IBegin
{
    [Header("�þ� ����(��)")]
    public float viewRadius = 10f;

    [Header("�þ� ��")]
    [Range(0, 360)]
    public float viewAngle = 360f;

    [Header("�þ� ���� ����")]
    private float detectionInterval = 0.2f;

    [Header("��ֹ� ���̾� ����")]
    private LayerMask obstacleMask;

    public List<Transform> visibleTargets = new List<Transform>();

    public virtual void Start()
    {
        obstacleMask = LayerMask.GetMask("Obstacle"); // ���̾� �̸� ���� �ʿ�
    }

    /// <summary>
    /// 1.���� ���� �ȿ��� ��� ã��
    /// 2. �þ߰� ���� �ִ��� Ȯ��
    /// 3. ��ֹ� �ִ��� ����ĳ��Ʈ �˻�
    /// </summary>
    /// <param name="targetLayer">Ž���� ������Ʈ�� ���̾�</param>
    public void DetectTargets(LayerMask targetLayer)
    {
        visibleTargets.Clear();
        Collider[] targetsInRange = Physics.OverlapSphere(transform.position, viewRadius, targetLayer);

        for (int i = 0; i < targetsInRange.Length; i++)
        {
            Transform target = targetsInRange[i].transform;
            Vector3 dirTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirTarget) < viewAngle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, dirTarget, distanceToTarget, obstacleMask))
                {
                    visibleTargets.Add(target);
                }
            }
        }
    }

    #region ������ ����� �׸���
    private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
            angleInDegrees += transform.eulerAngles.y;

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    private void OnDrawGizmosSelected()
    {
        // ���� �ݰ� (���� ����)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        // �þ߰� �� �� ��
        Vector3 leftBoundary = DirFromAngle(-viewAngle / 2, false);
        Vector3 rightBoundary = DirFromAngle(viewAngle / 2, false);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewRadius);

        // �ٶ󺸴� ���� ���� (��Ȯ�� forward)
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * viewRadius);
    }
    #endregion
}

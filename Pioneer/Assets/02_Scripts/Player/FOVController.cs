using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class FOVController : MonoBehaviour
{
    [Header("�þ� ����(��)")]
    [SerializeField] private float viewRadius = 10f;

    [Header("�þ� ��")]
    [Range(0, 360)]
    [SerializeField] private float viewAngle = 45f;

    [Header("�þ� ���� ����")]
    [SerializeField] private float detectionInterval = 0.2f;

    [Header("���̾� ����")]
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private LayerMask obstacleMask;

    public List<Transform> visibleTargets = new List<Transform>();

    private void Start()
    {
        StartCoroutine(DetectRatgets());
    }

    IEnumerator DetectRatgets()
    {
        while(true)
        {
            yield return new WaitForSeconds(detectionInterval);

            DetectTargets();
        }
    }

    // ���� ���� �ȿ��� ��� ã��
    // �þ߰� ���� �ִ��� Ȯ��
    // ��ֹ� �ִ��� ����ĳ��Ʈ �˻�
    private void DetectTargets()
    {
        visibleTargets.Clear();
        Collider[] targetsInRange = Physics.OverlapSphere(transform.position, viewRadius, enemyMask);

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

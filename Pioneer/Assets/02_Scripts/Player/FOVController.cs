using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class FOVController : MonoBehaviour
{
    [Header("�þ� ���� (��)")]
    [SerializeField] private float viewRadius = 10f;

    [Header("�þ� ��")]
    [SerializeField] private float viewAngle = 60f;

    [Header("�þ� ���� ����")]
    [SerializeField] private float detectionInterval = 0.2f;

    [Header("���̾� ����")]
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private LayerMask obstacleMask;

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

    }
}

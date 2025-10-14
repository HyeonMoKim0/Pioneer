using UnityEngine;

public class EnemyBase : CreatureBase, IBegin
{
    [Header("�⺻ �Ӽ�")]
    protected float idleTime;
    // public GameObject targetObject;
    public GameObject currentAttackTarget;
    protected float detectionRange;

    [Header("������ �� ���̾�")]
    [SerializeField] protected LayerMask detectMask;

    [Header("�� �ٴ� ���̾�")]
    [SerializeField] protected LayerMask groundLayer;

    // �ٴ� Ȯ�� ����
    protected bool isOnGround = false;

    // ���� �ڽ� �߽� ������ ����
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
    protected GameObject SetMastTarget()
    {
        GameObject mast = GameObject.FindGameObjectWithTag("Mast");
        return mast;
    }

    /// <summary>
    /// ���� ���� �� ��� �ݶ��̴��� ã�� �迭�� ��ȯ
    /// </summary>
    protected Collider[] DetectAttackRange()
    {
        Vector3 boxCenter = transform.position
            + transform.right * attackBoxCenterOffset.x
            + transform.forward * attackBoxCenterOffset.z
            + transform.up * attackBoxCenterOffset.y;
        Vector3 halfBoxSize = new Vector3(0.25f, 0.25f, attackRange / 2f);

        // Debug.Log($"DetectMask: {detectMask}, BoxCenter: {boxCenter}, HalfSize: {halfBoxSize}");

        return Physics.OverlapBox(boxCenter, halfBoxSize, transform.rotation, detectMask);
    }

    /// <summary>
    /// �� �÷��� ������ �˻�
    /// </summary>
    /// <returns></returns>
    protected virtual bool CheckOnGround()
    {
        if (Physics.Raycast(transform.position, Vector3.down, 2f, groundLayer))
        {
            if (!isOnGround)
            {
                isOnGround = true;
            }
        }
        else
        {
            isOnGround = false;
        }

        return isOnGround;
    }

    //================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        // DetectAttackRange()�� �����ϰ� �߽� ���
        // float debugAttackRange = 5f; // Ȯ�ο�, ���� �׽�Ʈ�� ���� ����
        Vector3 boxCenter = transform.position
            + transform.right * attackBoxCenterOffset.x
            + transform.forward * attackBoxCenterOffset.z
            + transform.up * attackBoxCenterOffset.y;

        Vector3 halfBoxSize = new Vector3(0.25f, 0.25f, attackRange / 2f);

        // ȸ�� ����
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(boxCenter, transform.rotation, Vector3.one);
        Gizmos.matrix = rotationMatrix;

        // OverlapBox�� ������ ũ���� �ڽ� �׸���
        Gizmos.DrawWireCube(Vector3.zero, halfBoxSize * 2); // halfSize * 2 = ��ü ũ��
    }
}
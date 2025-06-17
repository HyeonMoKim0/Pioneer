using UnityEngine;

public class UnitBase : MonoBehaviour
{
    [Header("Sprite ����")]
    [SerializeField] private Transform spritePivot;    // SpriteRenderer�� �޸� �ڽ�
    [SerializeField] private float flipThreshold = 0.5f;

    [Header("ī�޶� �ٶ󺸱� ����")]
    [SerializeField] private float lookOffset = -7f;   // ī�޶� ��ġ���� �󸶳� ���� �ٶ���

    #region Bounce ���� �ٽ� �ϰ� ������ Ű��~~~ 1
    // [Header("Bounce ����")]
    // [SerializeField] private float bounceHeight = 0.25f;
    // [SerializeField] private float bounceSpeed  = 5f;
    // private float baseY;
    #endregion

    private Transform cameraTransform;
    private Vector3 lastPosition;
    private Vector3 originalScale;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        lastPosition = transform.position;

        // SpritePivot�� ���� ������ ����
        originalScale = spritePivot.localScale;

        #region Bounce ���� �ٽ� �ϰ� ������ Ű��~~~ 2
        // baseY = spritePivot.localPosition.y;
        #endregion
    }

    private void Update()
    {
        // 1) ī�޶� + ������ �ٶ󺸱� (Pitch + Yaw)
        if (cameraTransform != null)
        {
            Vector3 targetPos = cameraTransform.position + Vector3.up * lookOffset;
            transform.LookAt(targetPos);
        }

        // 2) �̵� ���� ���
        Vector3 moveDir = (transform.position - lastPosition) / Time.deltaTime;

        // 3) �¿� Flip (���� ������ ����)
        if (Mathf.Abs(moveDir.x) > flipThreshold)
        {
            Vector3 s = originalScale;
            s.x = Mathf.Abs(originalScale.x) * (moveDir.x > 0 ? 1 : -1);
            spritePivot.localScale = s;
        }

        #region Bounce ���� �ٽ� �ϰ� ������ Ű��~~~ 3
        /*
        // �̵� ���� ���� ���Ʒ��� ���� Ƣ�� ȿ��
        Vector3 spritePos = spritePivot.localPosition;
        if (Mathf.Abs(moveDir.x) > flipThreshold)
        {
            float jump = Mathf.Abs(Mathf.Sin(Time.time * bounceSpeed));
            spritePos.y = baseY + jump * bounceHeight;
        }
        else
        {
            spritePos.y = baseY;
        }
        spritePivot.localPosition = spritePos;
        */
        #endregion

        // 4) ���� ��ġ ����
        lastPosition = transform.position;
    }
}

using UnityEngine;

public class UnitBase : MonoBehaviour, IBegin
{
    [Header("Sprite ����")]
    [SerializeField] private Transform spritePivot;    // SpriteRenderer�� �޸� �ڽ�
    [SerializeField] private float flipThreshold = 0.5f;

    [Header("ī�޶� �ٶ󺸱� ����")]
    [SerializeField] private float lookOffset = -7f;   // ī�޶� ��ġ���� �󸶳� ���� �ٶ���

    private Transform cameraTransform;
    private Vector3 lastPosition;
    private Vector3 originalScale;

    public void Init()
    {
        cameraTransform = Camera.main.transform;
        lastPosition = transform.position;

        // SpritePivot�� ���� ������ ����
        Debug.Log($">> localScale = {spritePivot.localScale}");
        originalScale = spritePivot.localScale;
    }

    private void Update()
    {
        // 1) spritePivot�� ī�޶� ������ �ٶ󺸰�
        if (cameraTransform != null && spritePivot != null)
        {
            spritePivot.forward = cameraTransform.forward;
        }

        // 2) �̵� ���� ���
        Vector3 moveDir = (transform.position - lastPosition) / Time.deltaTime;

        // 3) �¿� Flip (���� ������ ����)
        if (Mathf.Abs(moveDir.x) > flipThreshold)
        {
            Vector3 s = originalScale;
            Debug.Log($">> s = {s}");
            s.x = Mathf.Abs(originalScale.x) * (moveDir.x > 0 ? -1 : 1);
            Debug.Log($">> s.x = {s.x}");
            spritePivot.localScale = s;
            Debug.Log($">> localScale = {spritePivot.localScale}");
        }

        // 4) ���� ��ġ ����
        lastPosition = transform.position;
    }
}

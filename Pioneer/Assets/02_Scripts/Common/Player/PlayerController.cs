using UnityEngine;
using UnityEngine.UI;

// PlayerController: �Է¸� ó���ؼ� �ٸ� ��ũ��Ʈ�� ��� ������
public class PlayerController : MonoBehaviour
{
    private PlayerCore playerCore;
    private PlayerFishing playerFishing;

    private Vector3 lastMoveDirection;

    [Header("���� 1�� �ٴ� Ȯ�� ��� ")]
    public float rayOffset;
    public LayerMask seaLayer;
    public LayerMask groundLayer;
    public bool isSeaInFront = false;
    public GameObject fishingUI;
    LayerMask combinedMask;

    [Header("���� 2�� �ٴ� Ȯ�� ��� ")]
    public Vector3 checkBoxCenter;
    public Vector3 checkBoxHalfExtents;
    public float checkBoxOffset;

    private float currentChargeTime;
    private bool isCharging;
    public float ChargeTime;
    public Slider chargeSlider;

    void Awake()
    {
        playerCore = GetComponent<PlayerCore>();
        playerFishing = GetComponent<PlayerFishing>();
        // playerAttack = GetComponentInChildren<PlayerAttack>();

        combinedMask = seaLayer | groundLayer;
    }

    private void Start()
    {
        
    }

    void Update()
    {
        // ���� 1�� �ٴ� Ȯ�� ���
        isSeaInFront = CheckSea();

        if (isSeaInFront)
        {
            fishingUI.gameObject.SetActive(true);

            if (Input.GetKeyDown(KeyCode.Q))
            {
                isCharging = true;
                currentChargeTime = 0f;
            }

            // q�� 1���̻� ���� ��� ���� ���� ���� ��ġ
            if (Input.GetKey(KeyCode.Q))
            {
                currentChargeTime += Time.deltaTime;
                playerCore.SetState(PlayerCore.PlayerState.ChargingFishing);

                if (currentChargeTime >= ChargeTime)
                {
                    Debug.Log("���� ����!");
                    playerCore.SetState(PlayerCore.PlayerState.ActionFishing);
                    playerFishing.StartFishingLoop();
                    isCharging = false;
                }
            }

            if (isCharging && Input.GetKeyUp(KeyCode.Q))
            {
                isCharging = false;
                currentChargeTime = 0f;
                playerCore.SetState(PlayerCore.PlayerState.Default);
            }

            chargeSlider.value = currentChargeTime / ChargeTime;
        }
        else
        {
            playerCore.SetState(PlayerCore.PlayerState.Default);
            fishingUI.gameObject.SetActive(false);
        }

        // move
        if (playerCore.currentState == PlayerCore.PlayerState.Default)
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");
            Vector3 moveInput = new Vector3(moveX, 0, moveY);
            Vector3 moveDirection = moveInput.normalized;
            playerCore.Move(moveInput);

            if (moveDirection != Vector3.zero)
            {
                lastMoveDirection = moveDirection;
            }

            if (Input.GetMouseButtonDown(0))
            {
                playerCore.Attack();
            }
        }
    }

    
    private bool CheckSea()
    {
        // �ٴ� üũ 1�� ���
        Vector3 startRayPoint = transform.position + lastMoveDirection * rayOffset;
        Vector3 rayDir = Vector3.down;

        Debug.DrawRay(startRayPoint, rayDir, Color.white);
        if (Physics.Raycast(startRayPoint, rayDir, out RaycastHit hit, combinedMask))
        {
            int hitLayer = hit.collider.gameObject.layer;

            if ((seaLayer.value & (1 << hitLayer)) > 0)
                return true;
            else
                return false;
        }
        else
            return false;

        /*// �ٴ� üũ 2�� ���
        return Physics.CheckBox(transform.position + Vector3.down * checkBoxOffset, checkBoxHalfExtents, Quaternion.identity, seaLayer);*/
    }

    /*/// <summary>
    /// Scene �信 �ٴ� ���� �ڽ��� �׸��ϴ�.
    /// </summary>
    private void OnDrawGizmos()
    {
        // ������ ���� ���� ���� �׸����� �Ͽ� ������ ������ �����մϴ�.
        if (!Application.isPlaying)
            return;

        // 1. �߽���(Center)�� Physics.CheckBox�� ������ ������ checkBoxCenter�� �����մϴ�.
        Vector3 gizmoCenter = transform.position + Vector3.down * checkBoxOffset;

        // 2. ũ��(Size)�� Physics.CheckBox�� �����ϰ� �����մϴ�.
        // Physics.CheckBox�� ũ���� '����(HalfExtents)'�� ����ϰ�,
        // Gizmos.DrawCube�� '��ü(Full)' ũ�⸦ ����ϹǷ� 2�� �����ݴϴ�.
        Vector3 gizmoSize = checkBoxHalfExtents * 2;

        // 3. ����(Color)�� ���� üũ ����� isSeaInFront ������ �����մϴ�.
        // isSeaInFront�� true�̸� �Ķ���, false�̸� ȸ������ ǥ�õ˴ϴ�.
        Gizmos.color = isSeaInFront ? Color.blue : Color.white;

        // 4. Gizmos.DrawCube �Լ��� �ڽ��� ���� �׸��ϴ�.
        // CheckBox�� ȸ������ Quaternion.identity(ȸ�� ����)�̹Ƿ�, �� ����� �� �����ϰ� �������Դϴ�.
        Gizmos.DrawCube(gizmoCenter, gizmoSize);
    }*/
}

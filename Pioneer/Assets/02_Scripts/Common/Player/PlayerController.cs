using UnityEngine;
using UnityEngine.UI;

// PlayerController: �Է¸� ó���ؼ� �ٸ� ��ũ��Ʈ�� ��� ������
// TODO : �ڵ� ���� �ʿ� �� �ʿ��� ���� �� �޼��� ����
public class PlayerController : MonoBehaviour
{
    private PlayerCore playerCore;
    private PlayerFishing playerFishing;
    private GameManager gameManager;

    // �̵� ����
    private Vector3 lastMoveDirection;

    [Header("���� �ٴ� Ȯ�� ���� ����")]
    public float rayOffset;
    public float ChargeTime;
    public bool isSeaInFront = false;
    public LayerMask seaLayer;
    public LayerMask groundLayer;
    public GameObject fishingUI;
    public GameObject fishingCencleUI;
    public Slider chargeSlider;
    public Slider cencleChargeSlider;

    private LayerMask combinedMask;
    private float currentChargeTime;
    private bool isCharging;

    [SerializeField] private float fishingCancelDelay = 1.0f;
    private float cancelDelayTimer;


    void Awake()
    {
        playerCore = GetComponent<PlayerCore>();
        playerFishing = GetComponent<PlayerFishing>();
        gameManager = GetComponent<GameManager>();

        combinedMask = seaLayer | groundLayer;
    }

    private void Start()
    {
        
    }

    void Update()
    {
        // �ٴ� üũ
        isSeaInFront = CheckSea();

        switch (playerCore.currentState)
        {
            case PlayerCore.PlayerState.Default:
                // �̵�, ����, ���� ����
                HendleDefault();
                break;
            case PlayerCore.PlayerState.ChargingFishing:
                // ���� ����
                HendleCharging();
                break;
            case PlayerCore.PlayerState.ActionFishing:
                // ���� ���� ����
                HendleFishing();
                break;
        }
    }

    private void HendleDefault()
    {
        // �̵�
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        Vector3 moveInput = new Vector3(moveX, 0, moveY);
        Vector3 moveDirection = moveInput.normalized;
        playerCore.Move(moveInput);

        if (moveDirection != Vector3.zero)
        {
            lastMoveDirection = moveDirection;
        }

        /*// ����
        if (Input.GetMouseButtonDown(0))
        {
            playerCore.Attack();
        }*/

        // ���� ���� ���� Ȯ���ϰ� ���� ���� ��ȯ?
        if(isSeaInFront) // + ������ && gameManager.currentGameTime < dayDuration?
        {
            fishingUI.gameObject.SetActive(true);

            if(Input.GetKeyDown(KeyCode.Q))
            {
                isCharging = true;
                currentChargeTime = 0f;
                chargeSlider.value = 0f;
                playerCore.SetState(PlayerCore.PlayerState.ChargingFishing);
            }
        }
        else
        {
            fishingUI.gameObject.SetActive(false);
            fishingCencleUI.gameObject.SetActive(false);
        }
    }

    private void HendleCharging()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            currentChargeTime += Time.deltaTime;
            chargeSlider.value = currentChargeTime / ChargeTime;

            if(currentChargeTime >= ChargeTime)
            {
                isCharging = false;
                playerCore.SetState(PlayerCore.PlayerState.ActionFishing);
                playerFishing.StartFishingLoop();
                cancelDelayTimer = fishingCancelDelay;
                currentChargeTime = 0f;
                chargeSlider.value = 0f;
                fishingUI.gameObject.SetActive(false);
                fishingCencleUI.gameObject.SetActive(true);
            }
        }

        if(Input.GetKeyUp(KeyCode.Q))
        {
            isCharging = false;
            currentChargeTime = 0f;
            chargeSlider.value = 0f;
            playerCore.SetState(PlayerCore.PlayerState.Default);
        }
    }

    private void HendleFishing()
    {
        if (cancelDelayTimer > 0)
        {
            cancelDelayTimer -= Time.deltaTime;
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            currentChargeTime = 0f;
            chargeSlider.value = 0f;
            chargeSlider.gameObject.SetActive(true);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            currentChargeTime += Time.deltaTime;
            cencleChargeSlider.value = currentChargeTime / ChargeTime;

            if (currentChargeTime >= ChargeTime)
            {
                Debug.Log("���� �ߴ�!");
                playerFishing.StopFishingLoop();
                playerCore.SetState(PlayerCore.PlayerState.Default);
                currentChargeTime = 0f;
                cencleChargeSlider.value = 0f;
                fishingCencleUI.gameObject.SetActive(false);
            }
        }

        if (Input.GetKeyUp(KeyCode.Q))
        {
            currentChargeTime = 0f;
            cencleChargeSlider.value = 0f;
        }
    }

    
    private bool CheckSea()
    {
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
    }

    /// <summary>
    /// �ǰ� ������ ���ð� ���� ��ҵǾ��� �� UI�� ���¸� �����ϴ� �Լ�
    /// </summary>
    public void CancelFishing()
    {
        isCharging = false;
        currentChargeTime = 0f;

        if (chargeSlider != null) 
            chargeSlider.value = 0f;
        if (cencleChargeSlider != null)
            cencleChargeSlider.value = 0f;

        if (fishingCencleUI != null) 
            fishingCencleUI.gameObject.SetActive(false);

        if (isSeaInFront && fishingUI != null)
        {
            fishingUI.gameObject.SetActive(true);
        }
    }
}

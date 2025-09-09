using System.Collections;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

/* PlayerStats (CreatureBase ���) : ü��, ���ݷ� ���� �ٽ� ���� �� TakeDamage ���� ��� ����
 * [�־�� �� ����]
int hp = 100;					// ü��
int fullness = 100;				// ������
int mental = 100;					// ���ŷ�
int attackDamage = 2; 				// ���ݷ�
float beforeAttackDelay = 0.6f;		// ���� �� ���� �ð� 
float AttackCooldown = 0.4f;			// ���� �� ���� �ð�
float totalAttackTime = 1.0f;			// �� ���� �ð�
int attackPerSecond = 1;			// �ʴ� ���� ���� Ƚ��
float attackRange = 0.4f;			// ���� �Ÿ�
============================================================================================
- �̵�
- ����
- ������
- ���ŷ�
- ü�� ���̴� �Լ� + ü�� �ö󰡴� �Լ�
=============================================================================================
25.09.07 ���� ��
    - ���� ���� ���� �� ��� ������ ������
    - ���ŷ� ����
    - �������ͽ� ���� ����
25.09.09
    - �÷��̾� �� �ٴ� ������ �� ������ �س����� 
    - ���ŷ� ����
    - �������ͽ� ���� ����
    - ������ �� ���ŷ� �ּ�, �ִ� ���� �ɾ�α�
 */

public class PlayerCore : CreatureBase, IBegin
{
    // ��ü �ý��� ����

        // ������ ������ (fullness ���� ���� ���� ����)
    public enum FullnessState
    {
        Full,       // ��θ� (80 ~ 100)
        Normal,     // ���� (30 ~ 79)
        Hungry,     // ����� (1 ~ 29)
        Starving    // ���ָ� (0)
    }
    
        // ������ ����  
    int currentfullness;         
    int maxFullness = 100;
    int minFullness = 0;
    FullnessState currentFullnessState;
    int fullnessStarvingMax = 100;
    private Coroutine starvationCoroutine;

    [Header("������ ����")]
    [SerializeField] private float fullnessDecreaseTime = 5f;    // ������ �⺻ ���� �ӵ�(�ð�)
    [SerializeField] private float fullnessModifier = 1.3f;      // ������ ���� �ӵ� ������ => 30%

        // ���ŷ� ����
    int currentmental;         
    int maxMental = 100;
    int minMental = 0;
    bool isDrunk = false;
    private Coroutine enemyExistCoroutine;

    [Header("���ŷ� ����")]
    [SerializeField] private float existEnemyMenetalCool = 2f;

    [Header("���� ����")]
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private float attackHeight = 1.0f;
    [SerializeField] private LayerMask enemyLayer;

    private Rigidbody playerRb;
    private bool isAttacking = false;

    private float defaultSpeed;

    void Awake()
    {
        playerRb = GetComponent<Rigidbody>();
        SetSetAttribute();
    }
    
    void Start()
    {
        StartCoroutine(FullnessSystemCoroutine());
    }

    void Update()
    {
        fov.DetectTargets(enemyLayer);
    }

    // =============================================================
    // �������ͽ� ���� �� ����
    // =============================================================
    void SetSetAttribute()
    {
        maxHp = 100;
        hp = maxHp;                 // ü��
        speed = 4.0f;               // �̵� �ӵ�
        defaultSpeed = speed;
        currentfullness = 80;              // ������ (���� �� 80)
        currentmental = maxMental;         // ���ŷ� (���� �� 100)
        attackDamage = 2;           // ���ݷ�
        attackDelayTime = 0.4f;     // ���� ��Ÿ��
        //attackRange = 0.4f;       // ���� ���� (�̹� attack box ũ�⸦ 0.4�� �����ص�)
    }

    // =============================================================
    // �̵�
    // =============================================================
    public void Move(Vector3 moveInput)
    {
        Vector3 moveVelocity = moveInput.normalized * speed;

        playerRb.velocity = new Vector3(moveVelocity.x, playerRb.velocity.y, moveVelocity.z);
    }

    // =============================================================
    // ����
    // =============================================================
    public void Attack()
    {
        if (isAttacking) return;
        StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            Vector3 dir = (hit.point - transform.position).normalized;
            dir.y = 0f;
            transform.rotation = Quaternion.LookRotation(dir);

            Vector3 position = transform.position + dir * 0.5f;
            position.y = attackHeight;
            playerAttack.transform.position = position;
            playerAttack.transform.rotation = Quaternion.LookRotation(dir);

            // TODO: ���� �ִϸ��̼� ���� �ð� �߰��ؾ� ��!!!!!!!!!!! (0.6��)
            playerAttack.gameObject.SetActive(true);
            playerAttack.damage = this.attackDamage;
        }

        // ���� �ִϸ��̼� ���� ���� �ð� (0.4��)
        yield return new WaitForSeconds(attackDelayTime);

        playerAttack.gameObject.SetActive(false);

        isAttacking = false;
    }

    /* =============================================================
       { ������ }
    - ���۽� 80���� ����, �ִ� 100 �ּ� 0
    - ���� �ð� 5�ʿ� �� ���� 1�� ����
    - �÷��̾� ü���� 50% �̸��̸� ���� �ӵ� 30% ���� 
        - 100 ~ 80 ��θ� ���� : �ӵ� 20% ����
        - 79 ~ 30 ��θ� ���� ����
        - 29 ~ 1 ����� ���� : �ӵ� 30% ����
        - 0 ���ָ� ���� : ü���� �� �� 1�� ���� (�ִ� 100��)
    - ���� ������ ���� �ּ� 5 ~ 80���� ���� ����
        - ���� ������ �������� �˾ƾ� �� ��?
    ====================================
    25.09.07 : ������ ���ָ� �ڷ�ƾ ����
    ============================================================= */

    // �ʴ� ������ 1�� ���� Start �Լ����� ���� (�ڷ�ƾ)
    private IEnumerator FullnessSystemCoroutine()
    {
        while(true)
        {
            float currentDecreaseTime = fullnessDecreaseTime;
            if (hp < maxHp * 0.5f)
            {
                currentDecreaseTime = fullnessDecreaseTime / fullnessModifier;
            }

            yield return new WaitForSeconds(currentDecreaseTime);

            if(currentfullness > 0)
            {
                currentfullness--;
                UpdateFullnessState();
            }
            Debug.Log($"���ָ� ��ġ : {currentfullness}");
        }
    }

    /// <summary>
    /// ������ ��ġ�� ���� ���� ���� �Լ�
    /// </summary>
    private void UpdateFullnessState()
    {
        FullnessState fullnessState;

        if (currentfullness >= 80)
            fullnessState = FullnessState.Full;
        else if (currentfullness >= 30)
            fullnessState = FullnessState.Normal;
        else if (currentfullness >= 1)
            fullnessState = FullnessState.Hungry;
        else
            fullnessState = FullnessState.Starving;

        if(fullnessState != currentFullnessState)
        {
            currentFullnessState = fullnessState;

            switch(currentFullnessState)
            {
                case FullnessState.Full:
                    speed = defaultSpeed * 1.2f;
                    break;
                case FullnessState.Hungry:
                    speed = defaultSpeed * 0.7f;
                    break;
                default:
                    speed = defaultSpeed;
                    break;
            }

            if(currentFullnessState == FullnessState.Starving)      // ���ָ� �����϶�
            {
                if(starvationCoroutine == null)
                    starvationCoroutine = StartCoroutine(StarvingDamageCorountine());
            }
            else                                                    // ���ָ� ���°� �ƴҶ�
            {
                if (starvationCoroutine != null)
                {
                    StopCoroutine(starvationCoroutine);
                    starvationCoroutine = null;
                }
            }
        }
    }

    // �ʴ� ü�� 1�� �����ϴ� ���ָ� �Լ� (�ڷ�ƾ)
    private IEnumerator StarvingDamageCorountine()
    {        
        Debug.Log("���ָ� ���� : ü�� ���� ����");
        for(int i = 0; i < fullnessStarvingMax; i++)
        {
            yield return new WaitForSeconds(1f);
            TakeDamage(1, this.gameObject);
        }        
    }

    // public ���� ���� ����� ȣ���� �� �ִ� �Լ� �߰�
    public void EatFood(int increase)
    {
        currentfullness += increase;
    }

    /* =============================================================
        { ���ŷ� }
    - ���۽� 100���� ����, 0 ~ 100 ������ ���� ����
    - ���ŷ� 40 ~ 100 : ȿ�� ����
    - ���ŷ� 0 ~ 39 : ���ݷ�, ��ġ �۾� �뼺�� Ȯ��, ��å�� �ý��� ���� ����

    [���� ����]
    - ������ ��뿡 ���� 5 ~ 80���� ���� ����
    - ���� ���� �� 10�� ���� (���� ��� ����)

    [���� ����]
    - �÷��̾� �ݰ� 2M �� ���ʹ̰� ������ ��� 2�ʴ� 1�� ����
    - ���ʹ̿��� ���� ���� ��� ���� 1ȸ�� 3�� ���� (�ݰ� �� ���ʹ� ���� ���ǰ� ��ø ����)
    - �¹��� AI ����� ���� ���ŷ��� 20% ����

    [���� ����]
    - ������ �� ���� ���ø� ���� ���°� ��
    - ���� ���� : ���ŷ� ���� �� ���� �Ұ�, �����
    ============================================================= */

    void UpdateMental(int increase)
    {
        if(isDrunk)
            return;

        currentmental += increase;
        currentmental = Mathf.Clamp(currentmental, minMental, maxMental);


    }

    // ���ŷ� �ø��� ������ ���� ȣ��
    public void UseMentalItem(int increase)
    {

    }

    /// <summary>
    /// �ݰ� 2m ���� ���ʹ̰� ���� ���θ� Ȯ���ϰ� ���ŷ� ���� �ڷ�ƾ ���� �� �ߴ� �Լ�
    /// </summary>
    public void NearEnemy()
    {
        if (fov.visibleTargets.Count > 0 && enemyExistCoroutine == null)
        {
            enemyExistCoroutine = StartCoroutine(EnemyExist());
        }
        else if(fov.visibleTargets.Count == 0 && enemyExistCoroutine != null)
        {
            StopCoroutine(EnemyExist());
            enemyExistCoroutine = null;
        }
    }

    /// <summary>
    /// ���ʹ� ����� 2�ʿ� �� �� ���ŷ� ����
    /// </summary>
    /// <returns></returns>
    private IEnumerator EnemyExist()
    {
        while(true)
        {
            yield return new WaitForSeconds(existEnemyMenetalCool);
            currentmental--;
        }        
    }
}
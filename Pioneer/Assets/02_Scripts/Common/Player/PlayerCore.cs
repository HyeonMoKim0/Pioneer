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
 */

public class PlayerCore : CreatureBase, IBegin
{
    // ��ü �ý��� ����
    int fullness;       // ������ ����    
    int maxFullness = 100;
    float fullnessTimer = 0f;
    float fullnessDecrease = 5f;

    int mental;         // ���ŷ� ����
    int maxMental = 100;

    [Header("���� ����")]
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private float attackDuration = 0.2f; // ���� ���� ���� �ð�
    [SerializeField] private float attackHeight = 1.0f;

    private Rigidbody playerRb;
    private bool isAttacking = false;

    void Awake()
    {
        playerRb = GetComponent<Rigidbody>();
        SetSetAttribute();
    }

    void Update()
    {
        
    }

    // =============================================================
    // �������ͽ� ���� �� ����
    // =============================================================
    void SetSetAttribute()
    {
        maxHp = 100;
        hp = maxHp;                   // ü��
        speed = 4.0f;               // �̵� �ӵ�
        fullness = maxFullness;     // ������
        mental = maxMental;         // ���ŷ�
        attackDamage = 2;           // ���ݷ�
        attackDelayTime = 0.4f;     // ���� ��Ÿ��
        attackRange = 0.4f;         // ���� ����
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

            playerAttack.gameObject.SetActive(true);
            playerAttack.damage = this.attackDamage;
        }

        yield return new WaitForSeconds(attackDuration);

        playerAttack.gameObject.SetActive(false);

        isAttacking = false;
    }

    // =============================================================
    // ������
    // =============================================================


    // =============================================================
    // ���ŷ�
    // =============================================================
}
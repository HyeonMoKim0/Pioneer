using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("�⺻ �Ӽ�")]
    public int hp;
    public int maxHp; // �ִ� ü�� �߰�
    public int attackPower;
    public float speed;
    public int detectionRange;
    public int attackRange;
    public float attackVisualTime;
    public float restTime;

    [Header("����")]
    public GameObject targetObject;
    public EnemyState currentState;

    [Header("��� ����")]
    public bool destroyOnDeath = true;
    public float deathDelay = 1.0f;
    public GameObject deathEffect; // ��� �� ����Ʈ

    // �̺�Ʈ �ý���
    public System.Action<EnemyBase> OnDeath;
    public System.Action<EnemyBase, int> OnDamageTaken;
    public System.Action<EnemyBase> OnHealthChanged;

    // ���� �÷���
    protected bool isDead = false;
    protected bool isInvulnerable = false;

    protected virtual void Awake()
    {
        SetAttribute();
        InitializeHealth();
    }

    protected virtual void SetAttribute()
    {
        // �⺻�� ���� (�ڽ� Ŭ�������� �������̵�)
    }

    private void InitializeHealth()
    {
        if (maxHp <= 0)
        {
            maxHp = hp; // maxHp�� �������� �ʾҴٸ� ���� hp�� ����
        }
        hp = maxHp; // ������ �� �ִ� ü������ ����
    }

    #region ������ �� ü�� �ý���

    /// <summary>
    /// �������� �޴� �޼���
    /// </summary>
    /// <param name="damage">���� ������</param>
    /// <param name="source">�������� �� ������Ʈ (�ݰ� � ���)</param>
    public virtual void TakeDamage(int damage, GameObject source = null)
    {
        // �̹� �׾��ų� ���� ���¸� ������ ����
        if (isDead || isInvulnerable)
            return;

        // ������ ����
        hp -= damage;
        hp = Mathf.Max(0, hp); // ü���� ������ ���� �ʵ���

        UnityEngine.Debug.Log($"{gameObject.name}��(��) {damage} �������� �޾ҽ��ϴ�. ���� ü��: {hp}/{maxHp}");

        // �̺�Ʈ ȣ��
        OnDamageTaken?.Invoke(this, damage);
        OnHealthChanged?.Invoke(this);

        // ��� üũ
        if (hp <= 0)
        {
            Die();
        }
        else
        {
            // ������ ���� (�ڽ� Ŭ�������� �������̵� ����)
            OnDamageReaction(damage, source);
        }
    }

    /// <summary>
    /// ü���� ȸ���ϴ� �޼���
    /// </summary>
    /// <param name="healAmount">ȸ���� ü��</param>
    public virtual void Heal(int healAmount)
    {
        if (isDead)
            return;

        hp += healAmount;
        hp = Mathf.Min(hp, maxHp); // �ִ� ü���� �ʰ����� �ʵ���

        UnityEngine.Debug.Log($"{gameObject.name}��(��) {healAmount} ü���� ȸ���߽��ϴ�. ���� ü��: {hp}/{maxHp}");

        OnHealthChanged?.Invoke(this);
    }

    /// <summary>
    /// �������� �޾��� ���� ���� (�ڽ� Ŭ�������� �������̵�)
    /// </summary>
    /// <param name="damage">���� ������</param>
    /// <param name="source">������ �ҽ�</param>
    protected virtual void OnDamageReaction(int damage, GameObject source)
    {
        // �⺻�����δ� �ƹ��͵� ���� ����
        // �ڽ� Ŭ�������� Ư���� ���� ���� ���� (��: �ݰ�, ���� ��)
    }

    #endregion

    #region ��� ó��

    /// <summary>
    /// ��� ó�� �޼���
    /// </summary>
    protected virtual void Die()
    {
        if (isDead)
            return;

        isDead = true;
        currentState = EnemyState.Dead;

        UnityEngine.Debug.Log($"{gameObject.name}��(��) ����߽��ϴ�.");

        // ��� �̺�Ʈ ȣ��
        OnDeath?.Invoke(this);

        // ��� �� Ư���� ó�� (�ڽ� Ŭ�������� �������̵� ����)
        OnDeathBehavior();

        // ��� ����Ʈ ���
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, transform.rotation);
        }

        // ������Ʈ �ı� �Ǵ� ��Ȱ��ȭ
        if (destroyOnDeath)
        {
            StartCoroutine(DestroyAfterDelay());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// ��� �� Ư���� �ൿ (�ڽ� Ŭ�������� �������̵�)
    /// </summary>
    protected virtual void OnDeathBehavior()
    {
        // �⺻�����δ� �ƹ��͵� ���� ����
        // �ڽ� Ŭ�������� Ư���� ��� ȿ�� ���� ����
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(deathDelay);
        Destroy(gameObject);
    }

    #endregion

    #region ���� �� ��ƿ��Ƽ �޼���

    /// <summary>
    /// ���� ü�� ���� ��ȯ (0.0 ~ 1.0)
    /// </summary>
    public float GetHealthRatio()
    {
        return maxHp > 0 ? (float)hp / maxHp : 0f;
    }

    /// <summary>
    /// ���� ����ִ��� Ȯ��
    /// </summary>
    public bool IsAlive()
    {
        return !isDead && hp > 0;
    }

    /// <summary>
    /// ���� ���� ����
    /// </summary>
    /// <param name="invulnerable">���� ����</param>
    /// <param name="duration">���� ���� �ð� (0�̸� �������� ������ ������)</param>
    public void SetInvulnerable(bool invulnerable, float duration = 0f)
    {
        isInvulnerable = invulnerable;

        if (invulnerable && duration > 0f)
        {
            StartCoroutine(RemoveInvulnerabilityAfterDelay(duration));
        }
    }

    private IEnumerator RemoveInvulnerabilityAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isInvulnerable = false;
    }

    /// <summary>
    /// ü���� �ִ�� ȸ��
    /// </summary>
    public void FullHeal()
    {
        Heal(maxHp - hp);
    }

    /// <summary>
    /// ������ ����Ű�� �޼���
    /// </summary>
    public void Kill()
    {
        hp = 0;
        Die();
    }

    #endregion

}
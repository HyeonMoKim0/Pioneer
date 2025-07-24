using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("�⺻ �Ӽ�")]
    public int hp;
    public int maxHp;
    public int attackPower;
    public float speed;
    public int detectionRange;
    public int attackRange;
    public float attackVisualTime;
    public float restTime;
    protected GameObject targetObject;

    private GameObject attacker;

    private bool isDead = false;

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
        hp -= damage;

        if (hp <= 0)
        {
            Die();
        }

        attacker = source;
    }

    /// <summary>
    /// ü���� ȸ���ϴ� �޼���
    /// </summary>
    /// <param name="healAmount">ȸ���� ü��</param>
    public virtual void Heal(int healAmount)
    {
        hp += healAmount;

        if (hp > maxHp)
        {
            hp = maxHp;
        }
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
    public virtual void Die()
    {
        if (isDead)
            return;

        isDead = true;
        Destroy(gameObject, 1f);
    }

    #endregion

    #region ���� �� ��ƿ��Ƽ �޼���

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
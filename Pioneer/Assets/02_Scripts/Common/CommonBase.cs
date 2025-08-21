using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �ֻ��� �θ� ��ũ��Ʈ
public class CommonBase : MonoBehaviour, IBegin
{
    public int hp;
    public int maxHp;
    public bool IsDead = false;
    public GameObject attacker = null;

    public int CurrentHp => hp;

    /*public virtual void Start()
    {
        hp = maxHp;
    }*/

    // ===============
    void Start()
    {
        hp = maxHp;
    }
    // ===============

    void Update()
    {
        
    }

    // ������ �޴� �Լ�
    public virtual void TakeDamage(int damage, GameObject attacker)
    {
        if (IsDead) return;

        hp -= damage;
        Debug.Log($"������ {damage} ����. ���� HP: {hp}");

        this.attacker = attacker;

        if (hp <= 0)
        {
            IsDead = true;
            WhenDestroy();
        }
    }

    // ��������� ȣ���ϴ� ���� (����ü�� ��� ������� ��)
    public virtual void WhenDestroy()
    {
        Debug.Log($"{gameObject.name} ������Ʈ �ı�");
        Destroy(gameObject);
    }
}

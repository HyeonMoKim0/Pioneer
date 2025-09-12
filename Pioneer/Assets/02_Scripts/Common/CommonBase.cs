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
        Debug.Log(gameObject.name + "�� " + damage + "�� �������� �Ծ����ϴ�! ���� ü��: " + hp);

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
        if (GameManager.Instance == null) return; // �� ����

        if (gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            GameManager.Instance.TriggerGameOver();
            return;
        }
        else if (gameObject.layer == LayerMask.NameToLayer("Mariner"))
        {
            GameManager.Instance.MarinerDiedCount();
        }
        Destroy(gameObject);
    }
}

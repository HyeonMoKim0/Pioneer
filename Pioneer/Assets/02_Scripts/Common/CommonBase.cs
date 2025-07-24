using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �ֻ��� �θ� ��ũ��Ʈ
public class CommonBase : MonoBehaviour, IBegin
{
    int hp;
    int maxHp;

    public virtual void Init()
    {
        
    }

    void Update()
    {
        
    }

    // ������ �޴� �Լ�
    public virtual void TakeDamage(int damage)
    {

    }

    // ��������� ȣ���ϴ� ���� (����ü�� ��� ������� ��)
    public virtual void WhenDestroy()
    {

    }
}

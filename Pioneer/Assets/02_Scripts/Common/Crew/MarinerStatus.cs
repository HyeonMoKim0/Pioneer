/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MarinerStatus : MonoBehaviour, IBegin
{
    public int maxHP = 70;
    public int currentHP;
    public int attackPower = 6; // �⺻ ���ݷ��� 6

    public bool IsDead = false;
    public bool IsConfused = false;

    private void Init()
    {
        currentHP = maxHP;
    }

    public void UpdateStatus()
    {
        if (currentHP <= 0 && !IsDead)
        {
            Die();
        }
    }

    public void Die()
    {
        IsDead = true;
        Debug.Log("�¹��� ���");
        Destroy(gameObject);
    }
}*/
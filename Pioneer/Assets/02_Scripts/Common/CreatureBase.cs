using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureBase : CommonBase
{
    public FOVController fov;   // �þ� ���� = ���� ����

    public float speed;
    public int attackDamage;
    public float attackRange;
    public float attackDelayTime;

    void Init()
    {
        fov = GetComponent<FOVController>();
    }

    // �ӽ� ��ŸƮ
    void Start()
    {
        // fov = GetComponent<FOVController>();
    }
}

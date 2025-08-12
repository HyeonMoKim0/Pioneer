using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEvent : MonoBehaviour, IBegin
{
    public static PlayerEvent instance;

    // ���� �޾��� �� ȣ��� �̺�Ʈ
    public event System.Action OnDamaged;

    // ���Ϳ��� ���ݹ޴� �Լ� (����)
    public void TakeDamage(int amount)
    {
        Debug.Log($"�÷��̾ {amount} �������� �޾ҽ��ϴ�.");

        // ������ ó�� ����...

        // �����ڿ��� �˸�
        OnDamaged();
    }


    private void Awake()
    {
        instance = this;
        OnDamaged += () => { };
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

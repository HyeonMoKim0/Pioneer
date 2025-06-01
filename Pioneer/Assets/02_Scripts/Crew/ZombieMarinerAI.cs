using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// ���� ���� ����� �þ߾ȿ� ������ �� ���� ���� �̱���
/// </summary>
public class ZombieMarinerAI : MonoBehaviour
{
    public enum ZombieState { Wandering, Idle } // ������ , ���
    private ZombieState currentState = ZombieState.Wandering;

    private float speed = 1f;
    private float hp = 40f;
    private float moveDuration = 2f;
    private float idleDuration = 4f;

    private float stateTimer = 0f;
    private Vector3 moveDirection;

    private void Start()
    {
        InitZombieStats();
        SetRandomDirection();
        stateTimer = moveDuration;
        Debug.Log("���� �¹��� �۵� ��");
    }

    private void InitZombieStats()
    {
        if (hp > 40f)
        {
            Debug.Log("���� AI HP �ڵ� ����");
            hp = 40f;
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            case ZombieState.Wandering:
                Wander();
                break;

            case ZombieState.Idle:
                Idle();
                break;
        }
    }

    /// <summary>
    /// �̵� -> ��� -> �̵� , �÷��̾� �߽߰� ����
    /// </summary>

    private void Wander()
    {
        transform.position += moveDirection * speed * Time.deltaTime;
        stateTimer -= Time.deltaTime;

        Debug.DrawRay(transform.position, moveDirection * 2f, Color.green); // �̵� ���� �ð�ȭ

        if (stateTimer <= 0f)
        {
            Debug.Log("���� AI �̵� �� ��� ����");
            EnterIdleState();
        }
    }

    private void Idle()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            Debug.Log("���� AI ��⿡�� �ٽ� �̵� ����");
            EnterWanderingState();
        }
    }

    private void EnterWanderingState()
    {
        SetRandomDirection();
        currentState = ZombieState.Wandering;
        stateTimer = moveDuration;
        Debug.Log("���� �������� �̵� ����");
    }

    private void EnterIdleState()
    {
        currentState = ZombieState.Idle;
        stateTimer = idleDuration;
        Debug.Log("���� AI ��� ���·� ��ȯ");
    }


    /// <summary>
    /// ���� ���� ����
    /// </summary>

    private void SetRandomDirection()
    {
        float angle = Random.Range(0f, 360f); // ���� ���� �� MOVE
        moveDirection = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)).normalized;
    }




}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionAI : EnemyBase
{
    [Header("Ž�� �� ���� �ݶ��̴�")]
    [SerializeField] private Collider detectCollider;
    [SerializeField] private Collider attackCollider;

    // ���� ���� ����
    [Header("���� ����")]
    [SerializeField] private GameObject nestPrefab; // ���� ������

    public GameObject[] spawnNestList;

    private int maxNestCount = 2;
    private int currentSpawnNest = 0;
    private float spawnNestCoolTime = 15f;
    private float nestSpawnTime = 0f;
    private int spawnNestSlot = 0;

    // Behavior Tree Runner
    private BehaviorTreeRunner BTRunner = null;

    private void Awake()
    {
        base.Awake();
        BTRunner = new BehaviorTreeRunner(SettingBt());

        spawnNestList = new GameObject[maxNestCount];
    }

    private void Update()
    {
        BTRunner.Operate();
    }

    /// <summary>
    /// ���� �� ����
    /// </summary>
    protected override void SetAttribute()
    {
        hp = 20;
        attackPower = 1;
        speed = 2.0f;
        detectionRange = 4;
        attackRange = 2;
        attackVisualTime = 1.0f;  // ���� �ð�
        restTime = 2.0f;
        targetObject = GameObject.FindGameObjectWithTag("Engine");
    }

    private int FindEmptyNestSlot()
    {
        for(int i = 0; i < spawnNestList.Length; i++)
        {
            if (spawnNestList[i] == null)
                return i;
        }

        return -1;
    }

    private

    /// <summary>
    /// ���� ��ȯ
    /// </summary>
    /// <returns></returns>
    INode.ENodeState SpawnNest()
    {
        if(currentSpawnNest >= maxNestCount || Time.time - nestSpawnTime < spawnNestCoolTime)
        {
            spawnNestSlot = FindEmptyNestSlot();
            if(spawnNestSlot == -1)
            {
                return INode.ENodeState.Failure;
            }

            GameObject spawnNest = Instantiate(nestPrefab, transform.position, transform.rotation);

            currentSpawnNest++;
            spawnNestList[spawnNestSlot] = spawnNest;

            nestSpawnTime = Time.time;

            return INode.ENodeState.Success;
        }
        return INode.ENodeState.Failure;
    }


    /// <summary>
    /// �̵�
    /// </summary>
    /// <returns></returns>
    INode.ENodeState Movement()
    {
        return INode.ENodeState.Running;
    }

    /// <summary>
    /// ����
    /// </summary>
    /// <returns></returns>
    INode.ENodeState Attack()
    {
        return INode.ENodeState.Failure;
    }

    INode SettingBt()
    {
        return new SelecterNode(new List<INode>
        {
            new SequenceNode(new List<INode>
            {
                new ActionNode(() => SpawnNest())
            }),

            new SequenceNode(new List<INode>
            {
                new ActionNode(() => Movement())
            }),

            new SequenceNode(new List<INode>
            {
                new ActionNode(() => Attack())
            })
        });
    }
}

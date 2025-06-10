using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
[������ĥ ���]
** Ÿ�� ������Ʈ => ���� ��ǥ�� ����
    - ó�� => ����
    - Ÿ�� ������Ʈ�� �ٲ���ٰ� �������� �ٽ� �������� ����

- �̴Ͼ� �÷��̾� ���ݽ� �̴Ͼ� ü�� ���̵��� ����

- ���� ���� �� ���� ���� �߰� 
    - ����, Ÿ�� ������Ʈ, ��ġ�� ������Ʈ �� => �Ƹ� ���̾�� ���� �� �� ����, ���� ���̾� ���� �����ϵ��� ����
- Ÿ�� ������Ʈ ������ �̵� ����
    - �̵� �� ������Ʈ ������ null�� �� �� �̵� �����
    - �̵� �� Ÿ�� ������Ʈ�� ���� ���� �ȿ� ������ ���� �ൿ ����
    - �̵� �� Ÿ�� ������Ʈ ���� ������Ʈ���� ������ �޾��� ��� Ÿ�� ������Ʈ�� �ش� ������Ʈ�� ������
        - ���� ������ (4x1x4 => 2x1x2)�� ����
        - ������ ����� ��ġ�� 0.2�� ���� �̵�
            - (2x1x2)�� ������ ���� ���� ���� ���� ������ Ÿ�� ������Ʈ�� �ִٸ�
                - �ش� ������Ʈ ��ġ �������� �ٶ󺸱� �� ���� �ൿ ����
        - ������ ����� ���ٸ�(�� ã�Ҵٸ�) �̵� �ൿ ����
- ����
    - ���� �ൿ ���� Ÿ�� ������Ʈ�� null�̵Ǹ� �������� ��ǥ ����
    - ���� ����
        - �̴Ͼ��� �� �������� (2x1x1) �������� ���簢�� ������ attackvisualTime �ð����� ������ (�ٴڿ���? �ݶ��̴� ��������?)
        - ���� ���� ���� �ִٸ� ���ݷ�(attackPower)��ŭ ���ظ� ���� (���ʹ̿� �ٴ� ����)
        - ������ ������ ���� (���� �ӵ� ��ŭ ?�̶�µ� �ϴ� ����..) �� �� �̵� ����
 */

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

    #region ����
    /// <summary>
    /// ���� �迭 �� �ε��� ã��
    /// </summary>
    /// <returns></returns>
    private int FindEmptyNestSlot()
    {
        for(int i = 0; i < spawnNestList.Length; i++)
        {
            if (spawnNestList[i] == null)
                return i;
        }

        return -1;
    }

    /// <summary>
    /// �迭���� ���� ����
    /// </summary>
    private void PopNestList()
    {
        for(int i = 0; i < spawnNestList.Length; i++)
        {
            if(spawnNestList[i] == null && currentSpawnNest > 0)
            {
                currentSpawnNest--;
            }
        }
    }

    /// <summary>
    /// ���� ��ȯ
    /// </summary>
    /// <returns></returns>
    INode.ENodeState SpawnNest()
    {
        if(Time.time - nestSpawnTime < spawnNestCoolTime)
        {
            return INode.ENodeState.Failure;
        }

        if(currentSpawnNest >= maxNestCount)
        {
            return INode.ENodeState.Failure;
        }

        spawnNestSlot = FindEmptyNestSlot();
        if (spawnNestSlot == -1)
        {
            return INode.ENodeState.Failure;
        }

        GameObject spawnNest = Instantiate(nestPrefab, transform.position, transform.rotation);

        currentSpawnNest++;
        spawnNestList[spawnNestSlot] = spawnNest;

        nestSpawnTime = Time.time;

        return INode.ENodeState.Success;
    }
    #endregion


    #region ����

    private void SetDetectRange()
    {

    }

    private void DetectTarget()
    {
        if ()
        {
            
        }
    }

    #endregion

    /// <summary>
    /// �̵�
    /// </summary>
    /// <returns></returns>
    INode.ENodeState Movement()
    {
        // Ÿ�� ������Ʈ ������ null�� ���ٸ� �ٽ� Ž��? -> ��ȹ������ �׷��� ���������� ������ ��ǥ�� �Ѱ���.
        if(targetObject == null)
        {
            return INode.ENodeState.Failure;
        }



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

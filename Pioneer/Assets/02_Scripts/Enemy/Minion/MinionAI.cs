using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI;

public class MinionAI : EnemyBase
{
    NavMeshAgent agent;

    [Header("Ÿ�� ���̾� ����")]
    [SerializeField] private LayerMask targetLayers;

    // ����
    [Header("���� ��Ÿ�� ����")]
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private GameObject attackRangeObject;

    private bool hasTargetPosition = false; // ���� ��ġ ���� ���� Ȯ�� ����
    private bool isAttack = false; // ���� ���ߴ���?
    private float closeTargetDistance; // ���� ����� Ÿ�ٰ��� �Ÿ�
    private float counterAttackTimer = 0f; // ���� ���� ������ ���� ��
    private float counterAttackDuration = 1f; // ���� ���� ������ ���� �ð�
    private float rotationSpeed = 10f;
    private float lastAttackTime = 0f;
    private bool IsOnCooldown => Time.time < lastAttackTime + attackCooldown;
    private float CooldownRemaining => Mathf.Max(0f, (lastAttackTime + attackCooldown) - Time.time);
    private Vector3 targetPosition; // ���� ��ġ�� ������ ����
    private Collider closeTarget; // ���� ����� Ÿ��
    private GameObject attacker; // �� ������ ��
    private GameObject lastTargetObj;
    private GameObject engineObject; 

    // ���� �ð�ȭ
    private float attackVisualTimer = 0f;
    private bool attackSuccess = false;

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

        agent = GetComponent<NavMeshAgent>();

        BTRunner = new BehaviorTreeRunner(SettingBt());

        spawnNestList = new GameObject[maxNestCount];
    }

    private void Update()
    {
        // �ൿ �켱 ���� �Ǵ�
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
        SetTargetObj();
    }

    private void SetTargetObj()
    {
        targetObject = GameObject.FindGameObjectWithTag("Engine");
    }

    /// <summary>
    /// ���� �޾�����
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="source"></param>
    protected override void OnDamageReaction(int damage, GameObject source)
    {
        base.OnDamageReaction(damage, source);

        attacker = source;
        isAttack = true;
    }

    private void ResetAttackVariable()
    {
        isAttack = false;
        attacker = null;
        counterAttackTimer = 0f;
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
    #endregion

    #region ����
    /// <summary>
    /// ���� ���� ���� ������ ���� ����� ��ġ�� ���ʹ̸� ��ȯ ��
    /// </summary>
    /// <returns></returns>
    private Collider DetectTarget()
    {
        Vector3 boxSize = new Vector3(detectionRange / 2f, 1f, detectionRange / 2f);
        Vector3 boxCenter = transform.position + Vector3.forward * 1f;

        Collider[] detectColliders = Physics.OverlapBox(boxCenter, boxSize, transform.rotation, targetLayers);

        if (detectColliders.Length == 0)
            return null;

        closeTargetDistance = Mathf.Infinity;
        closeTarget = null;

        foreach(Collider collider in detectColliders)
        {
            float distance = Vector3.Distance(transform.position, collider.transform.position);
            
            if(distance <  closeTargetDistance)
            {
                closeTargetDistance = distance;
                closeTarget = collider;
            }
        }

        return closeTarget;
    }
    #endregion   

    #region ����
    // ���� ���� �ð�ȭ
    private IEnumerator VisualizeAttackRange()
    {
        isAttack = true;

        if (attackRangeObject != null)
            attackRangeObject.SetActive(true);

        yield return new WaitForSeconds(attackVisualTime);        

        attackSuccess = AttackTarget();

        if (attackSuccess)
        {
            lastAttackTime = Time.time;
            UnityEngine.Debug.Log($"[����] ����! ���� ���ݱ��� {attackCooldown}��");
        }

        if (attackRangeObject != null)
            attackRangeObject.SetActive(false);

        yield return new WaitForSeconds(CooldownRemaining);
        isAttack = false;
    }

    private bool AttackTarget()
    {
        if (targetObject == null)
        {
            SetTargetObj();  // Ÿ���� null�̸� �������� ��ǥ �缳��
            return false;
        }

        Vector3 directionToTarget = targetObject.transform.position - transform.position;
        directionToTarget.y = 0f;  // Y���� 0���� ������ ���� ȸ���� ����
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f); // ȸ�� �ӵ� ����


        Vector3 boxSize = new Vector3(attackRange, 1f, 1f);
        Vector3 boxCenter = transform.position + transform.forward * (attackRange / 2f);

        // box �ݶ��̴� �ȿ��� ���� ����
        Collider[] hits = Physics.OverlapBox(boxCenter, boxSize, transform.rotation, targetLayers);

        bool hitTarget = false;

        foreach (var hit in hits)
        {
            switch (hit.gameObject.layer)
            {
                case int layer when layer == LayerMask.NameToLayer("Player"):
                    if (PlayerController.Instance != null)
                    {
                        PlayerController.Instance.TakeDamage(attackPower); // �޼���� ����
                        UnityEngine.Debug.Log("[����] ������ ���� �Ϸ�!");
                        hitTarget = true;
                    }
                    else
                    {
                        UnityEngine.Debug.LogError("[����] PlayerController�� ã�� �� �����ϴ�!");
                    }
                    break;
                case int layer when layer == LayerMask.NameToLayer("Mariner"):
                    // hitTarget = true;
                    break;
                default:
                    break;
            }
        }

        return hitTarget;
    }

    private bool IsTargetInAttackRange()
    {
        if (targetObject == null) return false;

        float distance = Vector3.Distance(transform.position, targetObject.transform.position);
        return distance <= attackRange;
    }    
    #endregion

    #region �̵�
    private void SetNewTargetPosition()
    {
        if (targetObject != null)
        {
            // Ÿ�� �ֺ� ���� ��ġ ����
            Vector3 targetPos = targetObject.transform.position;
            Vector3 randomOffset = GetRandomHorizontalOffset(2f);
            targetPosition = targetPos + randomOffset;
        }
        else
        {
            // ���� �ֺ� ���� ��ġ ���� (ĳ�õ� ���� ���� ���)
            if (engineObject == null)
            {
                engineObject = GameObject.FindGameObjectWithTag("Engine");
            }

            if (engineObject != null)
            {
                Vector3 enginePos = engineObject.transform.position;
                Vector3 randomOffset = GetRandomHorizontalOffset(2f);
                targetPosition = enginePos + randomOffset;
            }
        }
    }

    private Vector3 GetRandomHorizontalOffset(float radius)
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * radius;
        randomDirection.y = 0f; // ���� �̵���
        return randomDirection;
    }

    private void HandleRotation()
    {
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Vector3 direction = agent.velocity.normalized;
            direction.y = 0f;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }
    }
    #endregion

    #region �ൿ
    /// <summary>
    /// �̵� �� ���� �������� 
    /// </summary>
    /// <returns></returns>
    INode.ENodeState CounterAttack()
    {
        if (!isAttack || attacker == null)
            return INode.ENodeState.Failure;

        counterAttackTimer += Time.deltaTime;

        if (counterAttackTimer < counterAttackDuration)
        {
            transform.position = Vector3.MoveTowards(transform.position, attacker.transform.position, speed * Time.deltaTime);
            return INode.ENodeState.Running;
        }

        Vector3 boxsize = new Vector3(1f, 0.5f, 1f);
        Vector3 boxCenter = transform.position + Vector3.up * 1f;

        Collider[] hits = Physics.OverlapBox(boxCenter, boxsize / 2f, transform.rotation, targetLayers);

        foreach (var hit in hits)
        {
            if (hit.gameObject == attacker)
            {
                Vector3 lookDir = attacker.transform.position;
                lookDir.y = transform.position.y; // ���� ȸ���� ����
                transform.LookAt(lookDir);

                targetObject = attacker;

                ResetAttackVariable();
                return INode.ENodeState.Success;
            }
        }

        ResetAttackVariable();

        SetTargetObj();

        return INode.ENodeState.Failure;
    }

    /// <summary>
    /// ���� ��ȯ
    /// </summary>
    /// <returns></returns>
    INode.ENodeState SpawnNest()
    {
        if (Time.time - nestSpawnTime < spawnNestCoolTime)
        {
            return INode.ENodeState.Failure;
        }

        if (currentSpawnNest >= maxNestCount)
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

    /// <summary>
    /// �̵�
    /// </summary>
    /// <returns></returns>
    INode.ENodeState Movement()
    {     
        targetObject = DetectTarget()?.gameObject;
        // Ÿ�� ������Ʈ ������ null�� ���ٸ� �ٽ� Ž��? -> ��ȹ������ �׷��� ���������� ������ ��ǥ�� �Ѱ���.
        if(targetObject == null)
        {
            SetTargetObj();
        }

        if (targetObject == null)
        {
            return INode.ENodeState.Failure;
        }

        if (!hasTargetPosition || targetObject != lastTargetObj)
        {
            SetNewTargetPosition();
            lastTargetObj = targetObject;
            hasTargetPosition = true;
        }    
        
        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
        {
            hasTargetPosition = false; // ���ο� ��ġ �缳���� ����
            return INode.ENodeState.Success;
        }

        if (agent.velocity.sqrMagnitude > 0.1f) // �̵� ���� ���� ȸ��
        {
            Vector3 direction = agent.velocity.normalized; // agent�� �ӵ��� ���� ȸ��
            direction.y = 0f; // y���� ���� ȸ���� ����
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f); // ȸ�� �ӵ�
        }

        agent.SetDestination(targetPosition);
        HandleRotation();

        return INode.ENodeState.Running;
    }    

    /// <summary>
    /// ����
    /// </summary>
    /// <returns></returns>
    INode.ENodeState Attack()
    {
        if (!IsTargetInAttackRange())
        {
            return INode.ENodeState.Failure;
        }

        if (IsOnCooldown)
        {
            // UnityEngine.Debug.Log($"[����] ��Ÿ�� ��... ���� �ð�: {CooldownRemaining:F1}��");
            return INode.ENodeState.Running; // �Ǵ� Failure (���� �����ο� ����)
        }

        // ���� ����
        StartCoroutine(VisualizeAttackRange());

        if (attackSuccess)
        {
            UnityEngine.Debug.Log($"[����] ����! ���� ���ݱ��� {attackCooldown}��");
            return INode.ENodeState.Success;
        }
        else
        {
            UnityEngine.Debug.Log("[����] ���� - Ÿ���� ã�� �� ����");
            return INode.ENodeState.Failure;
        }
    }   
    #endregion

    #region �ൿ Ʈ�� ����
    INode SettingBt()
    {
        return new SelecterNode(new List<INode>
        {
            // 1����: ���ݴ����� �� �ݰ�
            new ActionNode(() => CounterAttack()),
            
            // 2����: ���� ������ Ÿ���� ������ ����
            new ActionNode(() => Attack()),
            
            // 3����: ���� ���� (Ȯ����)
            new ActionNode(() => SpawnNest()),
            
            // 4����: �̵� (�⺻ �ൿ)
            new ActionNode(() => Movement())
        });
    }
    #endregion

    #region ����� �� �ð�ȭ
    private void OnDrawGizmosSelected()
    {
        // ���� ���� - ȸ���� �簢��
        if (detectionRange > 0)
        {
            Gizmos.color = Color.yellow;
            Vector3 boxCenter = transform.position + Vector3.up * 0.5f;
            Vector3 boxSize = new Vector3(detectionRange, 1f, detectionRange);

            // ȸ�� ��Ʈ���� ����
            Gizmos.matrix = Matrix4x4.TRS(boxCenter, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, boxSize);
            Gizmos.matrix = Matrix4x4.identity; // ��Ʈ���� ����
        }

        // ���� ���� - ȸ���� �簢��
        if (attackRange > 0)
        {
            Gizmos.color = Color.red;
            Vector3 boxCenter = transform.position + transform.forward * (1f / 2f) + Vector3.up * 0.5f;
            Vector3 boxSize = new Vector3(attackRange, 1f, 1f);

            // ȸ�� ��Ʈ���� ����
            Gizmos.matrix = Matrix4x4.TRS(boxCenter, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, boxSize);
            Gizmos.matrix = Matrix4x4.identity; // ��Ʈ���� ����
        }
    }
    #endregion
}

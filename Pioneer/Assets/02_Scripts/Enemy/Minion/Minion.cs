using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Minion : EnemyBase
{
    [Header("�̴Ͼ� ���� ����")]
    public GameObject nestPrefab;  // ���� ������
    public LayerMask targetLayer;  // Ž���� ���̾�
    public LayerMask unitLayer;    // ���� ���̾�
    public LayerMask installationLayer; // ��ġ�� ������Ʈ ���̾�

    [Header("�ൿ ����")]
    [SerializeField] private float detectionCheckInterval = 0.2f; // Ž�� üũ ����
    [SerializeField] private float nestSpawnCooldown = 15f; // ���� ��ȯ ��ٿ�
    [SerializeField] private float counterAttackSpeed = 10f; // �ݰ� �� �̵� �ӵ�

    // ���� ������
    private int currentSpawnNest = 0;  // ��ȯ�� ���� ��
    private GameObject attacker = null;  // �ڽ��� ������ ������Ʈ
    private bool isUnderAttack = false;  // ���ݹް� �ִ��� ����
    private float lastSpawnTime = 0f;  // ���� ��ȯ �ð� ����
    private bool isCooldown = false;  // 1���� �ൿ �Ұ� ���� üũ
    private bool hasDetectionRadar = false;  // ���� ���̴� Ȱ��ȭ ����
    private Vector3 currentDetectionSize = Vector3.zero;  // ���� ���� ����
    private List<GameObject> spawnedNests = new List<GameObject>();  // ��ȯ�� ������
    private bool isAttacking = false;  // ���� ������ ����
    private float lastAttackTime = 0f;  // ������ ���� �ð�
    private float lastDetectionCheck = 0f; // ������ Ž�� üũ �ð�

    // ���� �ð�ȭ ����
    private GameObject attackPreview;
    private Renderer attackPreviewRenderer;

    // �ൿ Ʈ�� ����
    private BehaviorTreeRunner _BTRunner = null;

    private void Awake()
    {
        base.Awake();
        SetupAttackPreview();
        _BTRunner = new BehaviorTreeRunner(SettingBt());
    }

    private void Start()
    {
        // ���� ���̴� �ʱ� ����
        UpdateDetectionRadar(detectionRange);
    }

    private void Update()
    {
        _BTRunner.Operate();
        CleanupDeadNests();
        CheckCooldownStatus();
    }

    private void OnDestroy()
    {
        // �޸� ����
        if (attackPreview != null)
        {
            DestroyImmediate(attackPreview);
        }
    }

    protected override void SetAttribute()
    {
        hp = 20;
        attackPower = 1;
        speed = 2.0f;
        detectionRange = 4;
        attackRange = 2;
        attackVisualTime = 1.0f;  // ���� �ð�
        restTime = 2.0f;
        targetObject = GameObject.FindGameObjectWithTag("Engine");  // �⺻ ��ǥ�� ����
    }

    #region �ʱ�ȭ �� ���� �޼���

    private void SetupAttackPreview()
    {
        attackPreview = GameObject.CreatePrimitive(PrimitiveType.Cube);
        attackPreview.name = "AttackPreview_" + gameObject.name;
        attackPreview.transform.SetParent(transform);
        attackPreview.transform.localPosition = Vector3.forward * 1f;
        attackPreview.transform.localScale = new Vector3(2f, 1f, 1f);

        attackPreviewRenderer = attackPreview.GetComponent<Renderer>();
        attackPreviewRenderer.material.color = Color.red;
        attackPreview.SetActive(false);

        // �ݶ��̴� ����
        Collider previewCollider = attackPreview.GetComponent<Collider>();
        if (previewCollider != null)
        {
            DestroyImmediate(previewCollider);
        }
    }

    private void CleanupDeadNests()
    {
        spawnedNests.RemoveAll(nest => nest == null);
        currentSpawnNest = spawnedNests.Count;
    }

    private void CheckCooldownStatus()
    {
        // ��ٿ� ���� üũ
        if (Time.time - lastSpawnTime >= nestSpawnCooldown && isCooldown)
        {
            isCooldown = false;
            UnityEngine.Debug.Log("���� ��ȯ ��ٿ� ����");
        }
    }

    private void UpdateDetectionRadar(float range)
    {
        hasDetectionRadar = true;
        currentDetectionSize = new Vector3(range, 1f, range);
    }

    #endregion

    #region �ൿ Ʈ�� �׼ǵ�

    // 1���� �ൿ: ���� ��ȯ
    INode.ENodeState SpawnNest()
    {
        if (currentSpawnNest < 2 && !isCooldown && nestPrefab != null)
        {
            // ���� ����
            GameObject nest = Instantiate(nestPrefab, transform.position, transform.rotation);
            spawnedNests.Add(nest);
            currentSpawnNest++;

            lastSpawnTime = Time.time;
            isCooldown = true;

            // ���� ���̴� Ȱ��ȭ
            UpdateDetectionRadar(detectionRange);

            UnityEngine.Debug.Log($"���� ��ȯ! �� ����: {currentSpawnNest}");
            return INode.ENodeState.Success;
        }

        return INode.ENodeState.Failure;
    }

    // 2���� �ൿ: �̵�
    INode.ENodeState Movement()
    {
        // Ÿ���� ������ ����
        if (targetObject == null)
        {
            return INode.ENodeState.Failure;
        }

        // ���� ���̴��� ���ο� Ÿ�� Ž�� (���� ����ȭ�� ���� �ֱ������θ� üũ)
        if (hasDetectionRadar && Time.time - lastDetectionCheck >= detectionCheckInterval)
        {
            GameObject detectedTarget = DetectInRadar();
            if (detectedTarget != null)
            {
                UpdateDetectionRadar(2f); // ���� ���� ���
                targetObject = detectedTarget;
            }
            lastDetectionCheck = Time.time;
        }

        // Ÿ���� ���� �̵�
        Vector3 direction = (targetObject.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        transform.LookAt(targetObject.transform.position);

        // ���� ������ �����ߴ��� üũ
        float distanceToTarget = Vector3.Distance(transform.position, targetObject.transform.position);
        if (distanceToTarget <= attackRange)
        {
            hasDetectionRadar = false;  // ���� ���̴� ��Ȱ��ȭ
            return INode.ENodeState.Success;  // �������� ��ȯ
        }

        return INode.ENodeState.Running;
    }

    // 3���� �ൿ: ����
    INode.ENodeState Attack()
    {
        if (targetObject == null)
        {
            return INode.ENodeState.Failure;
        }

        float distanceToTarget = Vector3.Distance(transform.position, targetObject.transform.position);
        if (distanceToTarget > attackRange)
        {
            // Ÿ���� ���� ������ ������� �ٽ� �̵�
            UpdateDetectionRadar(2f);
            return INode.ENodeState.Failure;
        }

        if (!isAttacking && Time.time - lastAttackTime >= restTime)
        {
            StartCoroutine(PerformAttack());
            return INode.ENodeState.Success;
        }

        return isAttacking ? INode.ENodeState.Running : INode.ENodeState.Failure;
    }

    // ���� �ൿ: �ݰ�
    INode.ENodeState CounterAttack()
    {
        if (!isUnderAttack || attacker == null)
            return INode.ENodeState.Failure;

        // ���� ���̴� ����
        if (!hasDetectionRadar)
        {
            UpdateDetectionRadar(2f);
        }

        // ������ ��ġ�� ������ �̵�
        Vector3 direction = (attacker.transform.position - transform.position).normalized;
        transform.position += direction * speed * counterAttackSpeed * Time.deltaTime;
        transform.LookAt(attacker.transform.position);

        // �����ڰ� ���� ���̴� ���� �ִ��� üũ
        if (IsTargetInRadar(attacker))
        {
            StartCoroutine(PerformCounterAttack());
            ResetCounterAttackState();
            return INode.ENodeState.Success;
        }

        return INode.ENodeState.Running;
    }

    #endregion

    #region Ž�� �� Ÿ����

    // ���� ���̴� �� Ÿ�� Ž��
    private GameObject DetectInRadar()
    {
        Vector3 detectionCenter = transform.position;
        Collider[] detectedTargets = Physics.OverlapBox(
            detectionCenter,
            currentDetectionSize / 2,
            Quaternion.identity,
            unitLayer | targetLayer | installationLayer
        );

        if (detectedTargets.Length > 0)
        {
            return FindClosestTarget(detectedTargets);
        }
        return null;
    }

    // Ÿ���� ���� ���̴� ���� �ִ��� üũ
    private bool IsTargetInRadar(GameObject target)
    {
        if (target == null || !hasDetectionRadar)
            return false;

        Vector3 detectionCenter = transform.position;
        Bounds detectionBounds = new Bounds(detectionCenter, currentDetectionSize);

        return detectionBounds.Contains(target.transform.position);
    }

    // ���� ����� ��ǥ ã��
    private GameObject FindClosestTarget(Collider[] targets)
    {
        GameObject closestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider target in targets)
        {
            // �ڱ� �ڽŰ� ���� �±״� ����
            if (target.gameObject == gameObject || target.CompareTag("Enemy"))
                continue;

            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = target.gameObject;
            }
        }

        return closestTarget;
    }

    // ���� �����ϰ� ��ǥ ����
    INode.ENodeState Detect()
    {
        // ���� ���� ���� �ٸ� Ÿ���� �ִ��� ���� Ȯ��
        GameObject detectedTarget = DetectInRadar();

        if (detectedTarget != null)
        {
            // ���� ���� ���� Ÿ���� ������ �켱������ ����
            targetObject = detectedTarget;
        }
        else
        {
            // ���� ���� ���� Ÿ���� ������ �⺻ �������� ����
            GameObject engine = GameObject.FindGameObjectWithTag("Engine");
            if (engine != null)
            {
                targetObject = engine;
            }
        }

        return targetObject != null ? INode.ENodeState.Success : INode.ENodeState.Failure;
    }

    #endregion

    #region ���� �ý���

    // ���� ���� �ڷ�ƾ
    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Ÿ�� �������� �ٶ󺸱�
        if (targetObject != null)
        {
            transform.LookAt(targetObject.transform.position);
        }

        // ���� ���� �̸�����
        attackPreview.SetActive(true);
        yield return new WaitForSeconds(attackVisualTime);

        // ���� ����
        attackPreview.SetActive(false);
        DealDamageInRange();

        // ���� ����
        yield return new WaitForSeconds(restTime);

        isAttacking = false;
    }

    // �ݰ� ���� �ڷ�ƾ
    private IEnumerator PerformCounterAttack()
    {
        if (attacker == null) yield break;

        // ������ �������� �ٶ󺸱�
        transform.LookAt(attacker.transform.position);

        // ���� ���� �̸�����
        attackPreview.SetActive(true);
        yield return new WaitForSeconds(attackVisualTime);

        // ���� ����
        attackPreview.SetActive(false);
        DealDamageInRange();

        // ���ݼӵ���ŭ ������
        yield return new WaitForSeconds(restTime);
    }

    // ���� ���� �� ���� ó��
    private void DealDamageInRange()
    {
        Vector3 attackCenter = transform.position + transform.forward * 1f;
        Vector3 attackSize = new Vector3(1f, 1f, 2f);

        Collider[] hits = Physics.OverlapBox(attackCenter, attackSize / 2, transform.rotation);

        foreach (Collider hit in hits)
        {
            if (hit.gameObject != gameObject && !hit.CompareTag("Enemy"))
            {
                // ���� ó�� ����
                var health = hit.GetComponent<EnemyBase>();
                if (health != null)
                {
                    health.TakeDamage(attackPower);
                }

                UnityEngine.Debug.Log($"���� ����: {hit.name}");
            }
        }
    }

    #endregion

    #region ���� ����

    private void ResetCounterAttackState()
    {
        isUnderAttack = false;
        attacker = null;
    }

    // ������ ���� �������̵� - �ݰ� ���� ����
    protected override void OnDamageReaction(int damage, GameObject source)
    {
        base.OnDamageReaction(damage, source);

        if (source != null)
        {
            isUnderAttack = true;
            attacker = source;
            UnityEngine.Debug.Log($"�̴Ͼ��� ���ݹ���! ������: {source.name}, �ݰ� �غ�");
        }
    }

    // ��� �� Ư���� �ൿ - ��ȯ�� ������ ����
    protected override void OnDeathBehavior()
    {
        base.OnDeathBehavior();

        // ��ȯ�� ������ ����
        foreach (GameObject nest in spawnedNests)
        {
            if (nest != null)
            {
                Destroy(nest);
            }
        }
        spawnedNests.Clear();

        UnityEngine.Debug.Log("�̴Ͼ� ��� - ��ȯ�� �������� ��� �����߽��ϴ�.");
    }

    #endregion

    #region �ൿ Ʈ�� ����

    // Behavior Tree ���� - �ùٸ� �켱������ ����
    INode SettingBt()
    {
        return new SelecterNode(new List<INode>
        {
            // ���� �ൿ: �ݰ� (�ֿ켱)
            new SequenceNode(new List<INode>
            {
                new ActionNode(() => CounterAttack())
            }),

            // 1����: ���� ��ȯ
            new SequenceNode(new List<INode>
            {
                new ActionNode(() => SpawnNest())
            }),

            // 2����: Ž�� �� �̵�
            new SequenceNode(new List<INode>
            {
                new ActionNode(() =>
                {
                    Detect();
                    return Movement();
                })
            }),

            // 3����: ����
            new SequenceNode(new List<INode>
            {
                new ActionNode(() => Attack())
            })
        });
    }

    #endregion

    #region ����� �� �ð�ȭ

    // ������ ���� ���� ǥ��
    private void OnDrawGizmosSelected()
    {
        // ���� ����
        if (hasDetectionRadar)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, currentDetectionSize);
        }

        // ���� ����
        Gizmos.color = Color.red;
        Vector3 attackCenter = transform.position + transform.forward * 1f;
        Gizmos.DrawWireCube(attackCenter, new Vector3(2f, 1f, 1f));

        // Ÿ�� ���ἱ
        if (targetObject != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetObject.transform.position);
        }
    }

    #endregion
}
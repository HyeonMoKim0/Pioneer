﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(FOVController))]
public class MarinerAI : MarinerBase, IBegin
{
    [System.Serializable]
    public struct ItemDrop
    {
        public int itemID;
        public float probability;
    }

    private static readonly ItemDrop[] FixedItemDrops = new ItemDrop[]
    {
        new ItemDrop { itemID = 30001, probability = 0.20f },
        new ItemDrop { itemID = 30002, probability = 0.15f },
        new ItemDrop { itemID = 30003, probability = 0.15f },
        new ItemDrop { itemID = 30004, probability = 0.10f },
        new ItemDrop { itemID = 30005, probability = 0.10f },
        new ItemDrop { itemID = 30006, probability = 0.075f },
        new ItemDrop { itemID = 30007, probability = 0.0525f },
        new ItemDrop { itemID = 30008, probability = 0.0525f },
        new ItemDrop { itemID = 30009, probability = 0.06f },
        new ItemDrop { itemID = 40009, probability = 0.06f }
    };

    public int marinerId;

    private float attackCooldown = 0f;
    private float attackInterval = 0.5f;

    private bool isRegistered = false;
    private bool lastDaytimeState = false;
    private bool hasInitializedDaytimeState = false;

    private void Awake()
    {
        maxHp = 100;
        hp = 100;
        speed = 2f;
        attackDamage = 6;
        attackRange = 4f;
        attackDelayTime = 1f;

        fov = GetComponent<FOVController>();

        gameObject.layer = LayerMask.NameToLayer("Mariner");
        targetLayer = LayerMask.GetMask("Enemy");
    }

    public override void Start()
    {
        base.Start();  // 먼저 호출 (NavMeshAgent 설정)

        SetRandomDirection();
        stateTimer = moveDuration;
        fov.Start();
    }

    private void Update()
    {
        if (GameManager.Instance == null || MarinerManager.Instance == null) return;

        if (!isRegistered)
        {
            MarinerManager.Instance.RegisterMariner(this);
            isRegistered = true;
        }

        bool currentDaytimeState = GameManager.Instance.IsDaytime;

        if (!hasInitializedDaytimeState)
        {
            lastDaytimeState = currentDaytimeState;
            hasInitializedDaytimeState = true;
        }

        if (currentDaytimeState != lastDaytimeState)
        {
            OnTimeStateChanged(currentDaytimeState);
            lastDaytimeState = currentDaytimeState;
        }

        if (currentDaytimeState)
        {
            HandleDaytimeBehavior();
        }
        else
        {
            HandleNightCombat();
        }
    }

    private void ResetAgentPath()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.ResetPath();
        }
    }

    private void UpdateTargetDetection()
    {
        ValidateCurrentTarget();
        if (target == null)
        {
            TryFindNewTarget();
        }
    }

    private void HandlePostCombatAction()
    {
        if (GameManager.Instance.IsDaytime)
        {
            Debug.Log($"승무원 {marinerId}: 전투 종료, 수리 재개");
            // 즉시 1순위 행동 시작
            StartRepair();
        }
        else
        {
            EnterWanderingState();
        }
    }

    private bool CheckSecondPriorityActionCancellation(string context)
    {
        if (!isSecondPriorityStarted)
        {
            Debug.Log($"승무원 {marinerId}: {context} 작업 취소로 2순위 행동 중단");
            return true;
        }
        return false;
    }

    private void OnTimeStateChanged(bool isDaytime)
    {
        if (isDaytime)
        {
            CleanupNightCombat();
            TransitionToDaytime();
        }
        else
        {
            TransitionToNight();
        }
    }

    private void CleanupNightCombat()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
            isShowingAttackBox = false;
        }

        if (isChasing)
        {
            isChasing = false;
            target = null;
            ResetAgentPath();
        }
    }

    private void TransitionToDaytime()
    {
        isSecondPriorityStarted = false;
        CancelCurrentRepair();
        Debug.Log($"승무원 {marinerId}: 낮 행동 모드로 전환");
        StartRepair();
    }

    private void TransitionToNight()
    {
        if (isRepairing || isSecondPriorityStarted)
        {
            CancelCurrentRepair();
            isSecondPriorityStarted = false;
            ResetAgentPath();
        }
    }

    private void HandleDaytimeBehavior()
    {
        if (IsDead) return;

        // 전투 중이 아닐 때만 타겟 탐지
        if (!isChasing && target == null)
        {
            ValidateCurrentTarget();
            if (target == null)
            {
                TryFindNewTarget();
            }
        }

        if (target != null)
        {
            if (isRepairing)
            {
                Debug.Log($"승무원 {marinerId}: 적 발견으로 수리 완전 취소");
                CancelCurrentRepair();
            }

            if (isSecondPriorityStarted)
            {
                Debug.Log($"승무원 {marinerId}: 적 발견으로 파밍 중단");
                CancelSecondPriorityAction();
            }

            attackCooldown -= Time.deltaTime;

            if (isChasing && target != null)
            {
                HandleChasing();
            }
            else if (target != null)
            {
                EnterChasingState();
            }
        }
        else
        {
            if (!isRepairing && !isSecondPriorityStarted)
            {
                StartRepair();
            }
        }
    }

    private void CancelCurrentRepair()
    {
        if (isRepairing)
        {
            isRepairing = false;
            StopAllCoroutines();

            if (targetRepairObject != null)
            {
                MarinerManager.Instance.ReleaseRepairObject(targetRepairObject);
                targetRepairObject = null;
            }

            ResetAgentPath();
            Debug.Log($"승무원 {marinerId}: 수리 취소");
        }
    }

    private void CancelSecondPriorityAction()
    {
        if (isSecondPriorityStarted)
        {
            isSecondPriorityStarted = false;
            StopAllCoroutines();
            ResetAgentPath();
            Debug.Log($"승무원 {marinerId}: 2순위 작업 취소");
        }
    }

    private void HandleNightCombat()
    {
        if (IsDead) return;

        attackCooldown -= Time.deltaTime;

        // 전투 중이 아닐 때만 타겟 탐지
        if (!isChasing && target == null)
        {
            ValidateCurrentTarget();
            if (target == null)
            {
                TryFindNewTarget();
            }
        }

        if (isChasing && target != null)
        {
            HandleChasing();
        }
        else
        {
            HandleNormalBehavior();
        }
    }

    protected override float GetAttackCooldown()
    {
        return attackCooldown;
    }

    protected override IEnumerator GetAttackSequence()
    {
        return MarinerAttackSequence();
    }

    private IEnumerator MarinerAttackSequence()
    {
        currentState = CrewState.Attacking;

        if (target == null)
        {
            attackRoutine = null;
            isChasing = false;
            HandlePostCombatAction();
            yield break;
        }

        ResetAgentPath();
        LookAtTarget();

        isShowingAttackBox = true;
        yield return new WaitForSeconds(attackDelayTime);
        isShowingAttackBox = false;

        PerformMarinerAttack();
        attackCooldown = attackInterval;

        // 공격 후 타겟 상태 확인
        if (target != null)
        {
            CommonBase targetBase = target.GetComponent<CommonBase>();
            if (targetBase != null && !targetBase.IsDead)
            {
                Debug.Log($"승무원 {marinerId}: 공격 완료, 추격 재개");
                EnterChasingState();
            }
            else
            {
                // 적이 죽었으므로 즉시 1순위 행동으로 전환
                Debug.Log($"승무원 {marinerId}: 적 처치 완료");
                target = null;
                isChasing = false;
                attackRoutine = null;
                HandlePostCombatAction();
                yield break;
            }
        }
        else
        {
            // 적이 없어졌으므로 즉시 1순위 행동으로 전환
            Debug.Log($"승무원 {marinerId}: 적 소실");
            isChasing = false;
            attackRoutine = null;
            HandlePostCombatAction();
            yield break;
        }

        attackRoutine = null;
    }

    private void PerformMarinerAttack()
    {
        Vector3 boxCenter = transform.position + transform.forward * 1f;
        Collider[] hits = Physics.OverlapBox(boxCenter, new Vector3(1f, 0.5f, 1f), transform.rotation, targetLayer);

        foreach (var hit in hits)
        {
            Debug.Log($"승무원이 {hit.name} 공격 범위 내 감지");

            CommonBase targetBase = hit.GetComponent<CommonBase>();
            if (targetBase != null)
            {
                targetBase.TakeDamage(attackDamage, this.gameObject);
                Debug.Log($"승무원이 {hit.name}에게 {attackDamage}의 데미지를 입혔습니다.");
            }
        }
    }

    public override IEnumerator StartSecondPriorityAction()
    {
        // 인벤토리 체크 - 7개 이상이면 보관함으로 이동
        MarinerInventory inventory = GetComponent<MarinerInventory>();
        if (inventory != null && inventory.ShouldMoveToStorage())
        {
            Debug.Log($"승무원 {marinerId}: 인벤토리가 가득함 ({inventory.GetAllItem()}개) - 보관함으로 이동");

            // 보관함 찾기
            GameObject storage = GameObject.FindWithTag("Engine");
            if (storage != null)
            {
                // NavMeshAgent로 보관함으로 이동
                if (agent != null && agent.isOnNavMesh)
                {
                    agent.SetDestination(storage.transform.position);

                    // 보관함에 도착할 때까지 대기
                    while (!IsArrived())
                    {
                        if (!isSecondPriorityStarted)
                        {
                            yield break;
                        }

                        if (GameManager.Instance.TimeUntilNight() <= 30f)
                        {
                            OnNightApproaching();
                            yield break;
                        }
                        yield return null;
                    }

                    Debug.Log($"승무원 {marinerId}: 보관함에 도착 - 아이템 저장");

                    // 보관함에 아이템 저장
                    var storageInventory = storage.GetComponent<InventoryBase>();
                    if (storageInventory != null)
                    {
                        inventory.TransferAllItemsToStorage(storageInventory);
                    }
                    else // 보관함 구현 후 삭제? or 에러처리? 
                    {
                        Debug.LogWarning("보관함에 InventoryBase가 없음 - 아이템 제거, 보관함 구현 후 삭제?");

                        List<SItemStack> itemsToRemove = new List<SItemStack>();
                        for (int i = 0; i < inventory.itemLists.Count; i++)
                        {
                            if (inventory.itemLists[i] != null)
                            {
                                itemsToRemove.Add(new SItemStack(inventory.itemLists[i].id, inventory.itemLists[i].amount));
                            }
                        }

                        if (itemsToRemove.Count > 0)
                        {
                            inventory.Remove(itemsToRemove.ToArray());
                        }
                    }

                    Debug.Log($"승무원 {marinerId}: 보관함 저장 완료 - 1순위 행동 재확인");

                    // 보관함 저장 후 1순위 행동(수리) 재확인
                    isSecondPriorityStarted = false;
                    StartRepair();
                    yield break;
                }
            }
            else
            {
                Debug.LogWarning($"승무원 {marinerId}: 보관함을 찾을 수 없음 - 3초간 랜덤 이동 후 재시도");

                SetRandomDestination();

                yield return new WaitForSeconds(3f); // 움직이고 3초 대기

                isSecondPriorityStarted = false;
                StartRepair();
                yield break;
            }
        }
        else
        {
            // 기존 파밍 시스템
            Debug.Log($"승무원 {marinerId}: 개인 경계 탐색 및 파밍 시작");
            yield return StartCoroutine(MoveToMyEdgeAndFarm());

            var needRepairList = MarinerManager.Instance.GetNeedsRepair();
            if (needRepairList.Count > 0)
            {
                isSecondPriorityStarted = false;
                StartRepair();
            }
            else
            {
                StartCoroutine(StartSecondPriorityAction());
            }
        }
    }

    protected override float GetRepairSuccessRate()
    {
        return 1.0f; // 100% 성공률
    }

    protected override void OnNightApproaching()
    {
        MarinerManager.Instance.StoreItemsAndReturnToBase(this);
    }

    public override void WhenDestroy()
    {
        GameManager.Instance.MarinerDiedCount();
        base.WhenDestroy();
    }

    protected override void OnPersonalFarmingCompleted()
    {
        int acquiredItemID = GetRandomItemIDByProbability(FixedItemDrops);

        MarinerInventory inventory = GetComponent<MarinerInventory>();

        if (inventory != null)
        {
            bool result = inventory.AddItem(acquiredItemID, 1);

            // 바디이벤트 녹조로 얻는 추가 아이템 획득 
            if (OceanEventManager.instance.currentEvent is OceanEventWaterBloom)
            {
                OceanEventWaterBloom waterBloomEnvent = OceanEventManager.instance.currentEvent as OceanEventWaterBloom;

                SItemTypeSO bonusItem = waterBloomEnvent.GetMoreItem();

                if (bonusItem != null)
                {
                    inventory.AddItem(bonusItem.id, 1);
                }
            }

            Debug.Log($"AddItem 결과: {result}, 획득 아이템 ID: {acquiredItemID}");
        }

        Debug.Log($"승무원 {marinerId}: 개인 경계에서 자원 수집 완료");
    }

    private int GetRandomItemIDByProbability(ItemDrop[] dropList)
    {
        float randomValue = Random.value; // 0.0에서 1.0 사이의 무작위 값
        float cumulativeProbability = 0f;

        foreach (var drop in dropList)
        {
            cumulativeProbability += drop.probability;

            if (randomValue <= cumulativeProbability)
            {
                return drop.itemID;
            }
        }

        Debug.LogError("확률 계산 오류: 아이템이 선택되지 않았습니다. 첫 번째 아이템 반환.");
        return dropList.Length > 0 ? dropList[0].itemID : 0;
    }
}
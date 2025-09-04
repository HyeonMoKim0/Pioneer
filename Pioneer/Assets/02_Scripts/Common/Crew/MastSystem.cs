using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MastSystem : CommonBase
{
    [Header("���� ����")]
    public int mastLevel = 1;
    public float interactionRange = 3f;
    public LayerMask playerLayer;

    [Header("�⺻ UI")]
    public GameObject mastUI;
    public GameObject interactionPrompt; // EŰ ������Ʈ
    public GameObject messagePanel; // �޽��� �г�
    public TextMeshProUGUI messageText; // �޽��� �ؽ�Ʈ
    public Button closeButton; // X �ݱ� ��ư

    [Header("��� UI")]
    public Image material1Image; // �볪�� �̹���
    public Image material2Image; // õ �̹���
    public TextMeshProUGUI material1CountText; // �볪�� ���� (n/30)
    public TextMeshProUGUI material2CountText; // õ ���� (n/15)

    [Header("��ȭ ���� UI")]
    public TextMeshProUGUI currentStageText; // ���� 1�ܰ�
    public Image nextStageImage; // 2�ܰ� �̹���
    public TextMeshProUGUI enhanceEffectText; // ��ȭ ȿ�� ����
    public Button enhanceButton; // ��ȭ�ϱ� ��ư

    private bool playerInRange = false;
    private bool isUIOpen = false;
    private Coroutine messageCoroutine;
    private Coroutine warningCoroutine;

    void Start()
    {
        SetMastLevel(mastLevel);
        hp = maxHp;

        if (mastUI) mastUI.SetActive(false);

        if (enhanceButton) enhanceButton.onClick.AddListener(EnhanceMast);
        if (closeButton) closeButton.onClick.AddListener(CloseUI);
    }

    void Update()
    {
        CheckPlayerDistance();
        HandleInput();
        UpdateUI();
        CheckMastCondition();
    }

    void SetMastLevel(int level)
    {
        mastLevel = Mathf.Clamp(level, 1, 2);
        maxHp = mastLevel == 1 ? 500 : 1000;
        if (hp > maxHp) hp = maxHp;
    }

    // �ִ� ���� ���� ��ȯ
    public int GetMaxDeckCount()
    {
        return mastLevel == 1 ? 30 : 50;
    }

    // �÷��̾� �Ÿ� üũ
    void CheckPlayerDistance()
    {
        Collider[] playersInRange = Physics.OverlapSphere(transform.position, interactionRange, playerLayer);
        bool wasInRange = playerInRange;
        playerInRange = playersInRange.Length > 0;

        if (wasInRange != playerInRange)
        {
            if (!playerInRange && isUIOpen)
            {
                CloseUI();
            }
            ShowInteractionPrompt(playerInRange);
        }
    }

    void HandleInput()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (isUIOpen)
                CloseUI();
            else
                OpenUI();
        }
    }

    void ShowInteractionPrompt(bool show)
    {
        if (interactionPrompt)
            interactionPrompt.SetActive(show);
    }

    void OpenUI()
    {
        isUIOpen = true;
        if (mastUI) mastUI.SetActive(true);

        ShowInteractionPrompt(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void CloseUI()
    {
        isUIOpen = false;
        if (mastUI) mastUI.SetActive(false);

        if (playerInRange)  
        {
            ShowInteractionPrompt(true);  
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // UI ������Ʈ
    void UpdateUI()
    {
        if (!isUIOpen) return;

        // ���� �ܰ� �ܰ� ǥ��
        if (currentStageText) currentStageText.text = $"���� {mastLevel}�ܰ�";

        // ��ȭ ȿ�� ����
        if (enhanceEffectText)
        {
            if (mastLevel == 1)
                enhanceEffectText.text = "�ִ� ���� ��ġ ������\n30������ 50����\n�����մϴ�";
            else
                enhanceEffectText.text = "�̹� �ִ� �ܰ��Դϴ�";
        }

        // ��� ���� ������Ʈ
        int currentWood = MastManager.Instance.GetItemCount(MastManager.Instance.woodItemID);
        int currentCloth = MastManager.Instance.GetItemCount(MastManager.Instance.clothItemID);

        // �볪�� ���� ǥ�� (�����ϸ� ������)
        if (material1CountText)
        {
            material1CountText.text = $"{currentWood}/30";
            material1CountText.color = currentWood >= 30 ? Color.white : Color.red;
        }

        // õ ���� ǥ�� (�����ϸ� ������)
        if (material2CountText)
        {
            material2CountText.text = $"{currentCloth}/15";
            material2CountText.color = currentCloth >= 15 ? Color.white : Color.red;
        }

        // ��ȭ ��ư Ȱ��ȭ/��Ȱ��ȭ
        if (enhanceButton)
        {
            bool canEnhance = mastLevel < 2 && currentWood >= 30 && currentCloth >= 15;
            enhanceButton.interactable = canEnhance;
        }
    }

    // ���� ���� üũ
    void CheckMastCondition()
    {
        float hpPercentage = (float)hp / maxHp;

        // 50% ������ �� ��� �޽���
        if (hpPercentage <= 0.5f && hpPercentage > 0f)
        {
            if (warningCoroutine == null)
            {
                warningCoroutine = StartCoroutine(ShowWarningMessage());
            }
        }
        else if (warningCoroutine != null)
        {
            StopCoroutine(warningCoroutine);
            warningCoroutine = null;
        }
    }

    // ��� �޽��� �ڷ�ƾ
    IEnumerator ShowWarningMessage()
    {
        while (true)
        {
            ShowMessage("���밡 �Ҿ����� ���δ�.", 4f);
            yield return new WaitForSeconds(10f);
        }
    }
    public void ShowMessage(string message, float duration)
    {
        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);

        messageCoroutine = StartCoroutine(ShowMessageCoroutine(message, duration));
    }
    IEnumerator ShowMessageCoroutine(string message, float duration)
    {
        if (messagePanel) messagePanel.SetActive(true);
        if (messageText) messageText.text = message;

        yield return new WaitForSeconds(duration);

        if (messagePanel) messagePanel.SetActive(false);
        messageCoroutine = null;
    }

    // ��ȭ�ϱ�
    void EnhanceMast()
    {
        int currentWood = MastManager.Instance.GetItemCount(MastManager.Instance.woodItemID);
        int currentCloth = MastManager.Instance.GetItemCount(MastManager.Instance.clothItemID);

        if (mastLevel >= 2)
        {
            ShowMessage("�̹� �ִ� �ܰ��Դϴ�.", 3f);
            return;
        }

        if (currentWood < 30 || currentCloth < 15)
        {
            ShowMessage("��ᰡ �����մϴ�.", 3f);
            return;
        }

        if (!MastManager.Instance.ConsumeItems(MastManager.Instance.woodItemID, 30) ||
            !MastManager.Instance.ConsumeItems(MastManager.Instance.clothItemID, 15))
        {
            ShowMessage("������ �Ҹ� �����߽��ϴ�.", 3f);
            return;
        }

        SetMastLevel(mastLevel + 1);
        hp = maxHp;

        ShowMessage("���밡 ��ȭ�Ǿ����ϴ�!", 3f);

        if (InventoryUiMain.instance != null)
        {
            InventoryUiMain.instance.IconRefresh();
        }
    }

    public override void TakeDamage(int damage, GameObject attacker)
    {
        if (IsDead) return;

        hp -= damage;
        Debug.Log($"���밡 ������ {damage} ����. ���� HP: {hp}");
        this.attacker = attacker;

        if (hp <= 0)
        {
            hp = 0;
            IsDead = true;
            WhenDestroy();
        }
    }

    public override void WhenDestroy()
    {
        Debug.Log("���� �ı��� - ���ӿ���");
        MastManager.Instance.GameOver();
    }

}
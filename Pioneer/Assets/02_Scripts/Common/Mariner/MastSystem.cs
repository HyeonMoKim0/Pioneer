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

    [Header("ù ��° UI - �⺻ ����")]
    public GameObject mastUI; // ù ��° UI 
    public TextMeshProUGUI hpPercentageText; // ������ % ǥ��
    public Button upgradeMenuButton; // ���׷��̵� �޴��� ���� ��ư
    public Button closeButton; // �ݱ� ��ư

    [Header("�� ��° UI - ��ȭ ��")]
    public GameObject upgradeUI; // �� ��° UI (��ȭ ��)
    public Image material1Image; // �볪�� �̹���
    public Image material2Image; // õ �̹���
    public TextMeshProUGUI material1CountText; // �볪�� ���� (n/30)
    public TextMeshProUGUI material2CountText; // õ ���� (n/15)
    public TextMeshProUGUI currentStageText; // ���� �ܰ� ǥ��
    public Image nextStageImage; // 2�ܰ� �̹���
    public TextMeshProUGUI enhanceEffectText; // ��ȭ ȿ�� ����
    public Button enhanceButton; // ��ȭ�ϱ� ��ư
    public Button backButton; // �ڷΰ��� ��ư

    [Header("�޽��� �ý���")]
    public GameObject messagePanel; // �޽��� �г�
    public TextMeshProUGUI messageText; // �޽��� �ؽ�Ʈ

    private bool playerInRange = false;
    private bool isUIOpen = false;
    private bool isUpgradeMenuOpen = false; // ���׷��̵� �޴� ����
    private Coroutine messageCoroutine;
    private Coroutine warningCoroutine;

    void Start()
    {
        SetMastLevel(mastLevel);
        hp = maxHp;

        if (mastUI) mastUI.SetActive(false);
        if (upgradeUI) upgradeUI.SetActive(false);

        if (upgradeMenuButton) upgradeMenuButton.onClick.AddListener(OpenUpgradeMenu);
        if (enhanceButton) enhanceButton.onClick.AddListener(EnhanceMast);
        if (closeButton) closeButton.onClick.AddListener(CloseAllUI);
        if (backButton) backButton.onClick.AddListener(BackToMainUI);
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

    public int GetMaxDeckCount()
    {
        return mastLevel == 1 ? 30 : 50;
    }

    void CheckPlayerDistance()
    {
        Collider[] playersInRange = Physics.OverlapSphere(transform.position, interactionRange, playerLayer);
        bool wasInRange = playerInRange;
        playerInRange = playersInRange.Length > 0;

        // ���� ��Ż�� ��� UI �ݱ�
        if (wasInRange && !playerInRange)
        {
            CloseAllUI();
        }
    }


    void HandleInput()
    {
        if (playerInRange && Input.GetMouseButtonDown(1)) // ��Ŭ��
        {
            if (!isUIOpen)
                OpenUI();
        }
    }

    void OpenUI()
    {
        isUIOpen = true;
        isUpgradeMenuOpen = false;
        if (mastUI) mastUI.SetActive(true);
        if (upgradeUI) upgradeUI.SetActive(false);
    }

    void OpenUpgradeMenu()
    {
        Debug.Log("OpenUpgradeMenu ȣ���");
        isUpgradeMenuOpen = true;
        if (mastUI)
        {
            mastUI.SetActive(false);
            Debug.Log("mastUI ��Ȱ��ȭ");
        }
        if (upgradeUI)
        {
            upgradeUI.SetActive(true);
            Debug.Log("upgradeUI Ȱ��ȭ");
        }
        else
        {
            Debug.LogError("upgradeUI�� null.");
        }
    }

    void BackToMainUI()
    {
        isUpgradeMenuOpen = false;
        if (mastUI) mastUI.SetActive(true);
        if (upgradeUI) upgradeUI.SetActive(false);
    }

    void CloseAllUI()
    {
        isUIOpen = false;
        isUpgradeMenuOpen = false;
        if (mastUI) mastUI.SetActive(false);
        if (upgradeUI) upgradeUI.SetActive(false);
    }

    void UpdateUI()
    {
        if (!isUIOpen) return;

        if (!isUpgradeMenuOpen)
        {
            if (hpPercentageText)
            {
                float hpPercentage = (float)hp / maxHp * 100f;
                hpPercentageText.text = $"������: {hpPercentage:F0}%";
            }
        }
        else
        {
            if (currentStageText)
            {
                if (mastLevel == 1)
                    currentStageText.text = "���� �ܰ�: 2�ܰ�";
                else
                    currentStageText.text = "�ִ� �ܰ�";
            }

            if (enhanceEffectText)
            {
                if (mastLevel == 1)
                    enhanceEffectText.text = "�ִ� ���� ��ġ ������\n30������ 50����\n�����մϴ�";
                else
                    enhanceEffectText.text = "�̹� �ִ� �ܰ��Դϴ�";
            }

            int currentWood = InventoryManager.Instance.Get(MastManager.Instance.woodItemID);
            int currentCloth = InventoryManager.Instance.Get(MastManager.Instance.clothItemID);

            if (material1CountText)
            {
                material1CountText.text = $"{currentWood}/30";
                material1CountText.color = currentWood >= 30 ? Color.white : Color.red;
            }

            if (material2CountText)
            {
                material2CountText.text = $"{currentCloth}/15";
                material2CountText.color = currentCloth >= 15 ? Color.white : Color.red;
            }

            if (enhanceButton)
            {
                bool canEnhance = mastLevel < 2 && currentWood >= 30 && currentCloth >= 15;
                enhanceButton.interactable = canEnhance;
            }
        }
    }

    void CheckMastCondition()
    {
        float hpPercentage = (float)hp / maxHp;

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

    void EnhanceMast()
    {
        if (mastLevel >= 2)
        {
            ShowMessage("�̹� �ִ� �ܰ��Դϴ�.", 3f);
            return;
        }

        const int requiredWood = 30;
        const int requiredCloth = 15;

        // MastManager�� ������ ID�� ���ǵǾ� ���� 30001 : ������ 30003 : õ����
        int woodId = MastManager.Instance.woodItemID;
        int clothId = MastManager.Instance.clothItemID;

        // �κ��丮�� ��ᰡ ������� Ȯ��
        if (InventoryManager.Instance.Get(woodId) < requiredWood ||
            InventoryManager.Instance.Get(clothId) < requiredCloth)
        {
            ShowMessage("��ᰡ �����մϴ�.", 3f);
            return;
        }

        // Remove �޼��带 �� ���� ȣ���Ͽ� ��� ��Ḧ �Ҹ��մϴ�.
        InventoryManager.Instance.Remove(
            new SItemStack(woodId, requiredWood),
            new SItemStack(clothId, requiredCloth)
        );

        SetMastLevel(mastLevel + 1);
        hp = maxHp;
        ShowMessage("���밡 ��ȭ�Ǿ����ϴ�", 3f);

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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerGameOver();
        }
    }

}
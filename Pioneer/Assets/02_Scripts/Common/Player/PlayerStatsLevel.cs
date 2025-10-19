using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using UnityEngine;

// PlayerLevelSystem : ����, ����, ä�� �� ������ ����ġ ����
// TODO : ���ø� ������ ����, ������ ����ġ ȹ�� �ڵ� �߰� �Ϸ�, ���� ����ġ �߰��ؾ���

#region ���� �������ͽ� ��ȹ ���
/* =============================================================
- switch���� ����Ʈ ������� �ٲ���� 

    [[ ���� ���� ]] => ���ʹ� ���̽����� �ؾ��ϳ�..
- ���ʹ̸� �� �� ���� ����������, ����ġ ȹ�� *
    
    { ���ʹ� óġ�� ȹ�� ������ ����ġ �� }
    * ���� : 3
    * �̴Ͼ� : 5
    * ���� �¹��� : 5                => ���� �¹����� ���ʹ̰� �ƴ϶� ���� �߰� ���� �����ҵ�..
    * Ÿ��ź : 8
    * ũ�ѷ� : 10
     
    { ���� ���� }
    * 0 : ȿ�� ����
    * 1 : ���ݷ� 10% ��� / ���� ������ ������ ���ҷ� -0.1
    * 2 : ���ݷ� 15% ��� / ���� ������ ������ ���ҷ� -0.3
    * 3 : ���ݷ� 20% ��� / ���� ������ ������ ���ҷ� -0.5
    * 4 : ���ݷ� 25% ��� / ���� ������ ������ ���ҷ� -0.8
    * 5 : ���ݷ� 30% ��� / ���� ������ ������ ���ҷ� -1.0
==================================================================   
==================================================================  
    [[ ������ ]] => ���������� �����ؾ��ҵ�..
- ��ġ�� ������Ʈ�� ��ġ �Ϸ� ������ craftExp ȹ��
- �Ϲ� ������ ���� �Ϸ�� craftExp ȹ�� *
    - ���۵� �� �ǹ��� �����ؾ���
    - �����ϴµ� �ʿ��� �������� 1�̻� �Ҹ�
    
    { ���� �����ǿ� ���� ����ġ �� }
    * �Ϲ� ��� ������ : 5
    * �Һ��� ������ : 10
    * ��ġ�� ������Ʈ : 15
    * ���� : 4
    
    { ������ ���� } 
    * 0 : ȿ�� ����
    * 1 : �뼺�� Ȯ�� 5%
    * 2 : �뼺�� Ȯ�� 10%
    * 3 : �뼺�� Ȯ�� 15%
    * 4 : �뼺�� Ȯ�� 20%
    * 5 : �뼺�� Ȯ�� 30%
    
    // ���� ������!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    // �뼺�� �ý��� �����ؾ���;
    = �뼺���̶�? = 
    - ������ ���۽� 1�� �� ȹ��
    - ���۽� �Ҹ��ؾ� �� ��� ������ 40% ���̹�..?
==================================================================    
==================================================================  
    [[ ���� ���� ]] => ���ô�..?
- �÷��̾ ���� ���ø� ���� �Ϲ� �������� ȹ���� ��� gratheringExp ȹ��

    { ���ø� ���� ������ ȹ�濡 ���� ����ġ �� }
    * ���÷� ���� �� �ִ� ������ : 5
    * �������� : 10
    
    { ���� ���� }
    * 0 : ȿ�� ����
    * 1 : 5% Ȯ���� �ڿ� 1�� �߰� ȹ��
    * 2 : 7% Ȯ���� �ڿ� 1�� �߰� ȹ��
    * 3 : 10% Ȯ���� �ڿ� 1�� �߰� ȹ�� / 30% Ȯ���� �������� 1�� ȹ�� (���÷� �������ڸ� ���� ���� �� ����
    * 4 : 12% Ȯ���� �ڿ� 1�� �߰� ȹ�� / 40% Ȯ���� �������� 1�� ȹ�� (���÷� �������ڸ� ���� ���� �� ����
    * 5 : 15% Ȯ���� �ڿ� 1�� �߰� ȹ�� / 50% Ȯ���� �������� 1�� ȹ�� (���÷� �������ڸ� ���� ���� �� ����
============================================================= */
#endregion

public enum GrowStatType
{
    Combat,         // ����
    Crafting,       // ����
    Fishing,        // ����
}

[System.Serializable]
public class GrowState
{
    public GrowStatType Type;      // �������ͽ� ����
    public int level;               // ���� ����
    public float currentExp;        // ���� �������� ����ġ ��
    public int[] maxExp;            // ������ ���� ����ġ �ִ밪

    public GrowState(GrowStatType type, int[] maxExp)
    {
        this.Type = type;
        this.maxExp = maxExp;
        this.level = 0;
        this.currentExp = 0;
    }
}

// ���������� ���� Ȯ�� switch �κ� ����Ʈ�� �����ϱ�
public class PlayerStatsLevel : MonoBehaviour
{
    public static PlayerStatsLevel Instance { get; private set; }

    public Dictionary<GrowStatType, GrowState> growStates = new Dictionary<GrowStatType, GrowState>();

    public PlayerCore player;

    public List<(float attack, float durability)> combatList 
        = new List<(float attack, float durability)> { (0f, 0f), (0.10f, -0.1f), (0.15f, -0.3f), (0.20f, -0.5f), (0.25f, -0.8f), (0.30f, -1) };
    public List<float> craftingList = new List<float> { 0f, 0.05f, 0.10f, 0.15f, 0.20f, 0.30f };
    public List<(float count, float chest)> fishingList 
        = new List<(float count, float chest)> { (0.0f, 0f), (0.05f, 0f), (0.1f, 0.3f), (0.12f, 0.4f), (0.15f, 0.5f) };

    public static event Action<GrowStatType> StatLevelUp;
     

    // =============== ������ �ν�����â���� ������ ����ġ�� ���̵��� ==================
    [SerializeField] private List<GrowState> growStateForInspector;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(Instance);

            player = GetComponent<PlayerCore>();

        InitGrowState();
    }

    // =============== ������ �ν�����â���� ������ ����ġ�� ���̵��� ==================
    private void Update()
    {
        if (Application.isEditor)
        {
            growStateForInspector = new List<GrowState>(growStates.Values);
        }

        if (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(KeyCode.F8)) growStates[GrowStatType.Combat].level++;
            if (Input.GetKeyDown(KeyCode.F9)) growStates[GrowStatType.Crafting].level++;
            if (Input.GetKeyDown(KeyCode.F10)) growStates[GrowStatType.Fishing].level++;


		}


    }

    // �������ͽ� �ʱ� ���� ����
    void InitGrowState()
    {
        growStates.Clear();
        growStates.Add(GrowStatType.Combat, new GrowState(GrowStatType.Combat, new int[] { 50, 100, 150, 200, 250 }));
        growStates.Add(GrowStatType.Crafting, new GrowState(GrowStatType.Crafting, new int[] { 50, 100, 150, 200, 250 }));
        growStates.Add(GrowStatType.Fishing, new GrowState(GrowStatType.Fishing, new int[] { 50, 100, 150, 200, 250 }));
    }

    /// <summary>
    /// ����ġ ȹ��
    /// </summary>
    /// <param name="type">�������ͽ� ����</param>
    /// <param name="amount">����ġ ��</param>
    public void AddExp(GrowStatType type, int amount)
    {
        UnityEngine.Debug.Log($"AddExp() ����");
        GrowState growState = growStates[type];

        if (growState.level >= growState.maxExp.Length)
            return;

        growState.currentExp += amount;

        while (growState.level < growState.maxExp.Length && growState.currentExp >= growState.maxExp[growState.level])
        {
            growState.currentExp -= growState.maxExp[growState.level];
            growState.level++;
            UnityEngine.Debug.Log($"{type} ������ -> {growState.level}");

            // switch ������ ����
            if (type == GrowStatType.Combat)
            {
                CombatLevelUp(type); // ���� �� ����
            }
            // ===========================================
            StatLevelUp?.Invoke(type); // ui ������Ʈ �̺�ư
        }
        UnityEngine.Debug.Log($"{type} ���� ����ġ {amount} ȹ��");
    }

    /// <summary>
    /// [[ ���� ]] ������ �� ȿ�� ����
    /// </summary>
    /// <param name="type"></param>
    /// ȣ�� ���� : ����ġ�� ��� ���� && ���� �� / not ���ݷ��� ��
    private void CombatLevelUp(GrowStatType type)
    {
        int combatLevel = growStates[GrowStatType.Combat].level;
        float increaseAttackDamage = 0f;
        if(combatLevel >= 0 && combatLevel < combatList.Count)
        {
            increaseAttackDamage = combatList[combatLevel].attack;
        }

        player.duabilityReducePrevent += combatList[combatLevel].durability;

        int prevDamage = (int)player.handAttackCurrentValueRaw.weaponDamage;

        player.handAttackCurrentValueRaw.weaponDamage =
            Mathf.RoundToInt(prevDamage * (1 + increaseAttackDamage)); // ���� ���� ���� ���� ����

		//player.attackDamage = Mathf.RoundToInt(player.attackDamage * (1 + increaseAttackDamage));
	}

    /// <summary>
    /// [[ ������ (������ ����) ]] Ȯ�� ����
    /// </summary>
    /// <returns></returns>
    public float CraftingChance()
    {
        int level = growStates[GrowStatType.Crafting].level;
        float greatSuccessChance = 0f;
        if (level >= 0 &&  level < craftingList.Count)
        {
            greatSuccessChance = craftingList[level];
        }         

        return PlayerCore.Instance.IsMentalDebuff() ? (greatSuccessChance * 6) / 10 : greatSuccessChance;
    }

    /// <summary>
    /// [[ ���� ]] �Ĺ� ��� �� �������� �߰� ȹ�� Ȯ�� ����
    /// </summary>
    /// <returns></returns>
    public (float count, float chest) FishingChance()      // C#�� Ʃ���̶�� ����� ����
    {
        int level = growStates[GrowStatType.Fishing].level;
        if (level >= 0 && level < fishingList.Count)
        {
            return fishingList[level]; // ���� ���
        }

        return (0.0f, 0f); // �̰� �Ƹ� ������
    }




}

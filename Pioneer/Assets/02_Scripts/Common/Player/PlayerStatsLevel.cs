using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

// PlayerLevelSystem : ����, ����, ä�� �� ������ ����ġ ����

/* =============================================================
    [[ ���� ���� ]] => ���ʹ� ���̽����� �ؾ��ϳ�..
- ���ʹ̰� �÷��̾� ���� �������� ���� ����Ҷ� CombatExp ȹ�� (��Ÿ)
    - ���ʹ� ��ũ��Ʈ���� attacker == Player && hp <= 0 �϶� ����ġ �ִ� �Լ� ȣ��
- ���ʹ� hp�� �÷��̾ ���� �������� 40% �̻� ������� CombatExp ȹ�� -> �׷���... �ʹ�.. ����� �ʳ�..? ����� �ʹ� ���� �����ϴµ�..?
    
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
- �Ϲ� ������ ���� �Ϸ�� craftExp ȹ��
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

public enum GrowStateType
{
    Combat,         // ����
    Crafting,       // ����
    Fishing,        // ����
}

[System.Serializable]
public class GrowState
{
    public GrowStateType Type;      // �������ͽ� ����
    public int level;               // ���� ����
    public float currentExp;        // ���� �������� ����ġ ��
    public int[] maxExp;            // ������ ���� ����ġ �ִ밪

    public GrowState(GrowStateType type, int[] maxExp)
    {
        this.Type = type;
        this.maxExp = maxExp;
        this.level = 0;
        this.currentExp = 0;
    }
}

public class PlayerStatsLevel : MonoBehaviour
{
    public static PlayerStatsLevel instance { get; private set; }

    public Dictionary<GrowStateType, GrowState> growStates = new Dictionary<GrowStateType, GrowState>();

    public PlayerCore player;

    private void Awake()
    {
        if(instance == null)
            instance = this;
        else
            Destroy(instance);

            player = GetComponent<PlayerCore>();

        InitGrowState();
    }

    // �������ͽ� �ʱ� ���� ����
    void InitGrowState()
    {
        growStates.Add(GrowStateType.Combat, new GrowState(GrowStateType.Combat, new int[] { 50, 100, 150, 200, 250 }));
        growStates.Add(GrowStateType.Crafting, new GrowState(GrowStateType.Crafting, new int[] { 50, 100, 150, 200, 250 }));
        growStates.Add(GrowStateType.Fishing, new GrowState(GrowStateType.Fishing, new int[] { 50, 100, 150, 200, 250 }));
    }

    /// <summary>
    /// ����ġ ȹ��
    /// </summary>
    /// <param name="type">�������ͽ� ����</param>
    /// <param name="amount">����ġ ��</param>
    public void AddExp(GrowStateType type, int amount)
    {
        GrowState growState = growStates[type];

        if (growState.level >= growState.maxExp.Length)
            return;

        growState.currentExp += amount;

        while (growState.level < growState.maxExp.Length && growState.currentExp >= growState.maxExp[growState.level])
        {
            growState.currentExp -= growState.maxExp[growState.level];
            growState.level++;
            Debug.Log($"{type} ������ -> {growState.level}");

            CombatLevelUpBuff(type);
        }
    }
    

    // [[ ���� ]] ������ �� ȿ�� ����
    private void CombatLevelUpBuff(GrowStateType type)
    {
        float increaseAttackDamage = 0f;
        if (type == GrowStateType.Combat)
        {
            // ������ ���� ���ݷ� ���, ���� ������ ������ ���ҷ� ����
            switch(growStates[GrowStateType.Combat].level)
            {
                case 1:
                    increaseAttackDamage = 0.1f;
                    break;
                case 2:
                    increaseAttackDamage = 0.15f;
                    break;
                case 3:
                    increaseAttackDamage = 0.20f;
                    break;
                case 4:
                    increaseAttackDamage = 0.25f;
                    break;
                case 5:
                    increaseAttackDamage = 0.30f;
                    break;
            }

            player.attackDamage = Mathf.RoundToInt(player.attackDamage * (1 + increaseAttackDamage));
        }
    }

    // [[ ������ ]] �뼺�� (����) Ȯ�� ��ȯ
    public float CraftingLevelUpBuff()
    {
        switch (growStates[GrowStateType.Crafting].level)
        {
            case 1:
                return 0.05f;
            case 2:
                return 0.1f;
            case 3:
                return 0.15f;
            case 4:
                return 0.2f;
            case 5:
                return 0.3f;
            default:
                return 0f;
        }
    }

    // [[ ���� ]] �߰� ȹ�� �� ���� ���� �߰� ȹ�� Ȯ��
    public (float count, float chest) FishingLevelUpBuff()
    {
        switch(growStates[GrowStateType.Fishing].level)
        {
            case 1:
                return (0.05f, 0f);
            case 2:
                return (0.07f, 0f);
            case 3:
                return (0.1f, 0.3f);
            case 4:
                return (0.12f, 0.4f);
            case 5:
                return (0.15f, 0.5f);
            default:
                return (0.0f, 0f);
        }
    }
}

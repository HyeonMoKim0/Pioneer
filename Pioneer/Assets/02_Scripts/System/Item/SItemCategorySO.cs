using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "ItemCategory", menuName = "ScriptableObjects/Items/ItemCategory", order = 1)]
public class SItemCategorySO : ScriptableObject
{
    public int typeInt; // ���� ī�װ� ������ �ٲ�� �ϴ� ���°� ������ ������ �̷� ���� �����մϴ�.
    public ETypes categoryType;
    public Sprite categorySprite;
}

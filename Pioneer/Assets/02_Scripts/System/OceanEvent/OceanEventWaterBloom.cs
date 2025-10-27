using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
[ �ٴ��̺�Ʈ - ���� ]
- �Ϸ����� �����

- �ٴٿ��� �Ĺֽ� 80% Ȯ���� ������ �߰� ȹ��
- �յ� ���� Ȯ���� �ٴ� �Ĺ����� ���� �� �ִ� ��� ������ �� 1�� �߰� ȹ��
*/

public class OceanEventWaterBloom : OceanEventBase
{
    [SerializeField] private int getMoreProbability = 80;

    List<PlayerFishing.FishingDropItem> getMoreDropItems;

    public SItemTypeSO GetMoreItem()
    {
        getMoreDropItems = PlayerFishing.instance.dropItemTable;

        if (Random.Range(0, 100) < getMoreProbability)
        {
            int randomIndex = Random.Range(0, getMoreDropItems.Count);

            PlayerFishing.FishingDropItem bonusItem = getMoreDropItems[randomIndex];
            
            return bonusItem.itemData;
        }
        
        return null;
    }
}
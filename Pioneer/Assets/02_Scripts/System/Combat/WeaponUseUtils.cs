using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponUseUtils
{
    public static IEnumerator AttackCoroutine(CommonBase userGameObject, SItemStack itemWithState, SItemWeaponTypeSO data)
    {
        // �ֵθ��� �ִϸ��̼�
        // �÷��̾� �ൿ ����ŬŸ��
        // �÷��̾� Ŭ�� ���� ����
        // ���� ���� ����
        // ������ ���
        // ������ �κ��丮 ������Ʈ

        // ��� ����
        // ���� �ִϸ��̼� �ڵ� �ֱ�
        yield return new WaitForSeconds(data.weaponAnimation);
        // ��� ����

        // �÷��̾� Ŭ�� ����
        Ray m_rayFromMouse = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit m_hitOnMap;
        Vector3 direction;
        LayerMask mapMaskLayer = LayerMask.NameToLayer("MouseClickArea");

        // ���� ���� ����
        if (Physics.Raycast(
            m_rayFromMouse.origin,
            m_rayFromMouse.direction,
            out m_hitOnMap,
            maxDistance: 200.0f,
            mapMaskLayer))
        {
            m_hitOnMap.point = new Vector3(
                m_hitOnMap.point.x,
                userGameObject.transform.position.y,
                m_hitOnMap.point.z);

            // ... RayCastHit m_hitOnMap�� ������ Ȱ���� �ڵ�
            direction = (m_hitOnMap.point - userGameObject.transform.position).normalized;
        }




        // ������ ���
        itemWithState.duability -= data.duabilityRedutionPerHit;

        // �κ��丮 ������Ʈ 
        InventoryUiMain.instance.IconRefresh();

        // ���� ������
        yield return new WaitForSeconds(data.weaponDelay);
        //


    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkMariner : MonoBehaviour
{
    string[] curseList = new string[]
    {
        "�̱����� ��",
        "��°�� �� �� ä �� ���� �ִ°���?",
        "���ܿ�",
        "�� �̻� ���ư� �� ����",
        "�� �׷���?",
        "�̹� �ʾ���",
        "�ʵ� �׷��� �ɰž�",
        "�ʵ� �� �� ���Ҿ�",
        "�� �ʿ��� ����"
    };

    public void Curse() => GuiltyCanvas.instance.CurseView(SelectCurse());
    public string SelectCurse() => curseList[Random.Range(0, curseList.Length)];

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($">> DarkMariner.OnCollisionEnter(Collision collision) : ȣ���, �浹ü {collision.collider.name}");

        if (ThisIsPlayer.IsThisPlayer(collision))
        {
            Debug.Log($">> DarkMariner.OnCollisionEnter(Collision collision) => Player");

            Curse();
        }
    }
}

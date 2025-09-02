using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDeckDisconnect : MonoBehaviour
{
	public static ItemDeckDisconnect instance;

	private void Awake()
	{
		if (instance != null && instance != this)
		{
			Destroy(gameObject);  // �ߺ� ����
			return;
		}
		instance = this;
	}

	// ���� �ı� : BFS�� ������ �� + ������ �ı��� ������ ��� �ٴ� �˻�
	public void DestroyItemDeck()
    {
		  
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// ���� �ı� : BFS�� ������ �� + ������ �ı��� ������ ��� �ٴ� �˻�
public class ItemDeckDisconnect : MonoBehaviour
{
	public static ItemDeckDisconnect instance;
	public bool IsConnected { get; private set; }

	[SerializeField] private GameObject mast;
	[SerializeField] private GameObject worldSpace;

    private void Awake()
	{
		if (instance != null && instance != this)
		{
			Destroy(gameObject);  // �ߺ� ����
			return;
		}
		instance = this;
	}


	public void ScanConnected()
	{

	}

	public void DestroyDeck()
    {
		  
    }
}

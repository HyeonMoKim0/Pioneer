using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���� �������� ����
/// </summary>
[System.Serializable]
class DeckInfo
{
	public GameObject obj;
	public bool isConnected;	
	//public bool IsConnected { get; private set; }
}


/// <summary>
/// ���� �ı� : BFS�� ������ �� + ������ �ı��� ������ ��� �ٴ� �˻�
/// </summary>
public class ItemDeckDisconnect : MonoBehaviour
{
	public static ItemDeckDisconnect instance;

	[SerializeField] private Transform mast;
	[SerializeField] private GameObject worldSpace;
	[SerializeField] private LayerMask deckLayer;
	[SerializeField] private Vector2 gridSize = new(1, 1);

	private Dictionary<Vector2Int, DeckInfo> decks = new();
	private Vector2Int[] DIR4 = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

	private void Awake()
	{
		deckLayer = LayerMask.GetMask("Platform");
	}

	private bool IsInLayer(int layer)
	{
		return layer == deckLayer;
	}

	private void InitScan()
	{
		decks.Clear();
		var all = FindObjectsOfType<Transform>(false);

		foreach(var d in all)
		{
			GameObject go = d.gameObject;

			if (!IsInLayer(go.layer)) continue;

			var 
		}
	}

}

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���� �������� ����
/// </summary>
[System.Serializable]
class DeckInfo
{
	public Vector2Int coord;        // ��ǥ
	public GameObject obj;          // ������ ���� ������Ʈ
	public bool isConnected;        // ����Ǿ��ִ����� ����
}

/// <summary>
/// ���� �ı� �ý���:
/// - �� ���� �� "Platform" ���̾��� ��� ���� �ڵ� ����
/// - ���� ��ġ/�ı� ���� BFS�� ����� ���� ���� ����
/// - isConnected = true/false �� ǥ��
/// </summary>
public class ItemDeckDisconnect : MonoBehaviour
{
	public static ItemDeckDisconnect instance;

	[SerializeField] private Transform mast;                    // ����
	[SerializeField] private GameObject worldSpace;             // ���� �θ� ������Ʈ
	[SerializeField] private LayerMask deckLayer;               // "Platform" ���̾�
	[SerializeField] private Vector2 gridSize = new(1, 1);      // ��ǥ ���� ����
	[SerializeField] private List<DeckInfo> deckLists = new();  // ����Ʈ Ȯ�� ��

	private Dictionary<Vector2Int, DeckInfo> decks = new();
	private readonly Vector2Int[] DIR4 = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

	private void Awake()
	{
		instance = this;
		deckLayer = LayerMask.GetMask("Platform"); // "Platform" ���̾� ����
		InitScan();
		UpdateConnectivity();
	}

	/// <summary>
	/// Ư�� GameObject�� deckLayer�� ���� �ִ��� üũ
	/// </summary>
	private bool IsInLayer(GameObject go)
	{
		return (deckLayer & (1 << go.layer)) != 0;
	}

	/// <summary>
	/// �� �� ��� Platform ���̾� ���� �ڵ� ��ĵ
	/// </summary>
	private void InitScan()
	{
		decks.Clear();
		var all = FindObjectsOfType<Transform>(false);

		foreach (var d in all)
		{
			GameObject go = d.gameObject;
			if (!IsInLayer(go)) continue;

			var coord = WorldToCoord(go.transform.position);
			if (decks.ContainsKey(coord)) continue;

			decks.Add(coord, new DeckInfo { coord = coord, obj = go, isConnected = false });
		}
	}

	/// <summary>
	/// BFS�� ����� ���� ���θ� ��� �� decks[*].isConnected ����
	/// </summary>
	public void UpdateConnectivity()
	{
		// 1. ��ü �ʱ�ȭ
		foreach (var kv in decks)
			kv.Value.isConnected = false;

		if (decks.Count == 0 || mast == null) return;

		// 2. ���밡 �׻� ��Ȯ�� �߾� ���� ����� ���� �� mast.position�� �����Ͽ� ��Ʈ ���
		var root = WorldToCoord(mast.position);
		if (!decks.ContainsKey(root))
		{
			// ���� ���⼭ ���ٰ� ������ gridSize/��ġ�� ��߳� ��.
			Debug.Log($"[ItemDeckDisconnect] ��Ʈ ��ǥ {root}�� ������ �����ϴ�. gridSize/��ġ Ȯ�� �ʿ�.");
			RefreshDebugView(); 
			return;
		}

		// 3. BFS Ž�� ����!@!!!!!!!!!!!!!!!!!!!!!
		var visited = new HashSet<Vector2Int>();
		var q = new Queue<Vector2Int>();
		visited.Add(root);
		q.Enqueue(root);

		while (q.Count > 0)
		{
			var cur = q.Dequeue();
			if (!decks.ContainsKey(cur)) continue;

			decks[cur].isConnected = true;

			foreach (var d in DIR4)
			{
				var next = cur + d;
				if (visited.Contains(next)) continue;
				if (decks.ContainsKey(next))
				{
					visited.Add(next);
					q.Enqueue(next);
				}
			}
		}

		RefreshDebugView();
	}

	/// <summary>
	/// ������� ���� ����(GameObject) ����Ʈ ��ȯ
	/// </summary>
	public List<GameObject> GetDisconnectedDecks()
	{
		var list = new List<GameObject>();
		foreach (var kv in decks)
			if (!kv.Value.isConnected)
				list.Add(kv.Value.obj);
		return list;
	}

	public void RefreshDebugView()
	{
		deckLists.Clear(); 
		foreach (var kv in decks)
		{
			deckLists.Add(new DeckInfo
			{
				coord = kv.Key,
				obj = kv.Value.obj,
				isConnected = kv.Value.isConnected
			});
		}
	}

	// -------------------- ���� ��ƿ --------------------
	private Vector2Int WorldToCoord(Vector3 pos)
	{
		int cx = Mathf.RoundToInt(pos.x / Mathf.Max(0.0001f, gridSize.x));
		int cy = Mathf.RoundToInt(pos.z / Mathf.Max(0.0001f, gridSize.y));
		return new Vector2Int(cx, cy);
	}

	private void OnDrawGizmos()
	{
		if (decks == null) return;
		foreach (var kv in decks)
		{
			if (kv.Value?.obj == null) continue;
			Gizmos.color = kv.Value.isConnected ? Color.green : Color.red;
			Gizmos.DrawSphere(kv.Value.obj.transform.position + Vector3.up * 0.5f, 0.2f);
		}
	}
}

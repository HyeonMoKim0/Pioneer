using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���� �������� ����
/// </summary>
[System.Serializable]
class DeckInfo
{
    public Vector2Int coord;
    public GameObject obj;
    public bool isConnected;
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

    [SerializeField] private Transform mast;                // ����
    [SerializeField] private GameObject worldSpace;         // ���� �θ� ������Ʈ
    [SerializeField] private LayerMask deckLayer;           // "Platform" ���̾�
    [SerializeField] private Vector2 gridSize = new(2, 2);  // ��ǥ ���� ����
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

            decks.Add(coord, new DeckInfo { obj = go, isConnected = false });
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

        // 2. ����� ���� ����� ���� ã��
        if (!TryGetNearestDeckToMast(out var root)) return;

        // 3. BFS Ž�� ����!!!!!!!!!!!!!
        var visited = new HashSet<Vector2Int>();
        var q = new Queue<Vector2Int>();
        visited.Add(root);
        q.Enqueue(root);

        while (q.Count > 0)
        {
            Debug.Log("�׽�Ʈ��3333");
            var cur = q.Dequeue();
            if (!decks.ContainsKey(cur)) continue;

            decks[cur].isConnected = true;

            foreach (var d in DIR4)
            {
                Debug.Log("�׽�Ʈ��4444");
                var nxt = cur + d;
                if (visited.Contains(nxt)) continue;
                if (decks.ContainsKey(nxt))
                {
                    Debug.Log("�׽�Ʈ��5555");
                    visited.Add(nxt);
                    q.Enqueue(nxt);
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
	private bool TryGetNearestDeckToMast(out Vector2Int coord)
    {
		coord = default;
        float best = float.MaxValue;
        bool found = false;

        foreach (var kv in decks)
        {
            float d = (kv.Value.obj.transform.position - mast.position).sqrMagnitude;
            if (d < best)
            {
                best = d;
                coord = kv.Key;
                found = true;
            }
        }
        return found;
    }

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
			Gizmos.color = kv.Value.isConnected ? Color.green : Color.red;
			Gizmos.DrawSphere(kv.Value.obj.transform.position + Vector3.up * 0.5f, 0.2f);
		}
	}

}

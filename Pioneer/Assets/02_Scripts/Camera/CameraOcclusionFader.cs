using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(1000)] // CinemachineBrain ���� �� ����ǵ���(�ɼ�)
public class CameraOcclusionFader : MonoBehaviour
{
    [Header("Targets")]
    public Transform player;
    public LayerMask occluderMask;

    [Header("Fade Settings")]
    [Range(0f, 1f)] public float fadeAlpha = 0.35f; // ��ǥ ������
    [Range(0f, 1f)] public float minAlpha = 0.30f; // ���� �ּ� ����(�Ⱥ��� ������ �������� �ʰ�)
    public float fadeSpeed = 10f;
    public float sphereRadius = 0.35f;

    // ���ɿ� NonAlloc ���� (�ʿ�� ũ�� Ű���)
    const int MaxHits = 64;
    static readonly RaycastHit[] _hitsBuffer = new RaycastHit[MaxHits];

    readonly Dictionary<Renderer, float> _current = new();   // ���� ���� �� ����
    readonly HashSet<Renderer> _hitsThisFrame = new();       // �̹� �����ӿ� �ɸ� ������
    readonly List<Renderer> _toRestore = new();              // ���� ���� �ӽ� ����Ʈ
    MaterialPropertyBlock _mpb;

    void Awake()
    {
        _mpb = new MaterialPropertyBlock();
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        _hitsThisFrame.Clear();

        // ī�޶� �� �÷��̾� �������� ĳ��Ʈ
        var camPos = transform.position;
        var dir = player.position - camPos;
        var dist = dir.magnitude;
        if (dist <= 0.001f) return;

        var ray = new Ray(camPos, dir / dist);

        // NonAlloc ĳ��Ʈ (GC ����)
        int hitCount = Physics.SphereCastNonAlloc(
            ray, sphereRadius, _hitsBuffer, dist,
            occluderMask, QueryTriggerInteraction.Ignore);

        // ���� �ݶ��̴����� ��� Renderer ����
        for (int i = 0; i < hitCount; i++)
        {
            var col = _hitsBuffer[i].collider;
            // �ڽı��� ��� ���� (�޽ÿ� �ڽ��� ������ ĳ�� ���� ���)
            var renderers = col.GetComponentsInChildren<Renderer>(includeInactive: false);
            for (int r = 0; r < renderers.Length; r++)
            {
                var rend = renderers[r];
                _hitsThisFrame.Add(rend);
                if (!_current.ContainsKey(rend)) _current[rend] = 1f; // ���� ��Ͻ� �⺻ ���� 1
            }
        }

        // 1) �ɸ� �������� ��ǥ ���ı��� ����
        foreach (var rend in _hitsThisFrame)
        {
            float a0 = GetAlpha(rend);
            float a1 = Mathf.MoveTowards(a0, fadeAlpha, fadeSpeed * Time.deltaTime);
            ApplyAlpha(rend, a1);
        }

        // 2) �� �ɸ� �������� 1.0���� ���� (������ ��ųʸ� ����)
        _toRestore.Clear();
        foreach (var kv in _current)
        {
            var rend = kv.Key;
            if (_hitsThisFrame.Contains(rend)) continue;

            float a0 = kv.Value;
            float a1 = Mathf.MoveTowards(a0, 1f, fadeSpeed * Time.deltaTime);
            ApplyAlpha(rend, a1);

            if (Mathf.Approximately(a1, 1f))
                _toRestore.Add(rend);
        }
        for (int i = 0; i < _toRestore.Count; i++)
            _current.Remove(_toRestore[i]);
    }

    float GetAlpha(Renderer r)
        => _current.TryGetValue(r, out float a) ? a : 1f; // �⺻�� 1�� ����

    void ApplyAlpha(Renderer r, float a)
    {
        // �ּ�/�ִ� ���� ���� (���� ���� ����)
        a = Mathf.Clamp(a, minAlpha, 1f);
        _current[r] = a;

        // 2D ��������Ʈ�� ���(���� Ȯ��): SpriteRenderer.color
        if (r is SpriteRenderer sr)
        {
            var c = sr.color;
            c.a = a;
            sr.color = c;
            return;
        }

        // �Ϲ� Renderer: ��Ƽ���� ����(����޽�)���� MPB ����
        // ����: Opaque ��Ƽ������ ���İ� �������� �ݿ����� ����(Transparent/��� ���̴� �ʿ�)
        var mats = r.sharedMaterials;
        int matCount = mats != null ? mats.Length : 0;
        for (int i = 0; i < matCount; i++)
        {
            var m = mats[i];
            if (m == null) continue;

            bool wrote = false;

            // URP Lit �迭
            if (m.HasProperty("_BaseColor"))
            {
                // ���� �� ���� + ���ĸ� ��ü
                Color c = m.GetColor("_BaseColor");
                c.a = a;
                _mpb.Clear();
                _mpb.SetColor("_BaseColor", c);
                r.SetPropertyBlock(_mpb, i);
                wrote = true;
            }
            // ���Ž� �Ǵ� Ŀ���� �÷�
            else if (m.HasProperty("_Color"))
            {
                Color c = m.GetColor("_Color");
                c.a = a;
                _mpb.Clear();
                _mpb.SetColor("_Color", c);
                r.SetPropertyBlock(_mpb, i);
                wrote = true;
            }

            // � ������Ƽ�� ����ٸ�(���̴��� ���ĸ� ���� �� ��) - �ƹ��͵� ���� ����
            // �ʿ��: ����� �α׸� �־� ���� ��Ƽ���� ���� ����
            // if (!wrote) Debug.Log($"[OcclusionFader] No _BaseColor/_Color on {r.name} (mat:{m.name})");
        }
    }
}

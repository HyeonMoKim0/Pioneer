// EnemySpawnIntro.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawnIntro : MonoBehaviour
{
    [Header("Intro")]
    [Min(0f)] public float fadeDuration = 1.0f;
    public bool playOnEnable = true;
    [Tooltip("���̵� �� �̵� ����")]
    public bool stopAgentDuringIntro = true;

    // ���� ĳ��
    SpriteRenderer sprite;             // ù ��° �ڽ� "2D Sprite"�� SR
    NavMeshAgent agent;                // �׻� ����
    readonly List<Behaviour> toToggleBehaviours = new();
    readonly List<Collider> toToggleColliders = new();
    readonly List<Renderer> toToggleRenderers = new(); // SpriteRenderer ����
    bool cached;

    void Awake()
    {
        Cache();
    }

    void OnEnable()
    {
        if (playOnEnable) StartCoroutine(IntroCo());
    }

    /// <summary>���� ���� �������� ȣ���ϰ� ���� ��</summary>
    public void TriggerNow() => StartCoroutine(IntroCo());

    void Cache()
    {
        if (cached) return;
        cached = true;

        agent = GetComponent<NavMeshAgent>();

        // === ù ��° �ڽ� "2D Sprite" ���� ===
        if (transform.childCount > 0)
        {
            var child = transform.GetChild(0);
            sprite = child.GetComponent<SpriteRenderer>();
        }

        if (sprite == null)
            Debug.LogWarning($"[EnemySpawnIntro] '{name}'�� ù ��° �ڽ� SpriteRenderer�� �����ϴ�. (\"2D Sprite\" ���¸� ���)");

        // ����/���� Behaviour ���� (NavMeshAgent, �ڽ� ����)
        var behaviours = GetComponentsInChildren<Behaviour>(true);
        foreach (var b in behaviours)
        {
            if (b == null) continue;
            if (ReferenceEquals(b, this)) continue;
            if (b is NavMeshAgent) continue;     // �䱸����: Agent�� ����
            toToggleBehaviours.Add(b);
        }

        // Collider�� ����
        GetComponentsInChildren(true, toToggleColliders);

        // �ٸ� Renderer�� ��� ���� (SpriteRenderer�� ���̵�θ� ó��)
        var renderers = GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            if (sprite != null && ReferenceEquals(r, sprite)) continue;
            toToggleRenderers.Add(r);
        }
    }

    IEnumerator IntroCo()
    {
        Cache();

        // 0) ���� ���� ����
        foreach (var b in toToggleBehaviours) if (b) b.enabled = false;
        foreach (var c in toToggleColliders) if (c) c.enabled = false;
        foreach (var r in toToggleRenderers) if (r) r.enabled = false;

        if (stopAgentDuringIntro && agent) agent.isStopped = true;

        // ��������Ʈ ���� 0���� ����
        SetAlpha(0f);

        // 1) ���̵� 0��1
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = (fadeDuration <= 0f) ? 1f : Mathf.Clamp01(t / fadeDuration);
            SetAlpha(a);
            yield return null;
        }
        SetAlpha(1f);

        // 2) ��� Ȱ��ȭ
        foreach (var r in toToggleRenderers) if (r) r.enabled = true;
        foreach (var c in toToggleColliders) if (c) c.enabled = true;
        foreach (var b in toToggleBehaviours) if (b) b.enabled = true;

        if (stopAgentDuringIntro && agent) agent.isStopped = false;
    }

    void SetAlpha(float a)
    {
        if (!sprite) return;
        var c = sprite.color;
        c.a = a;
        sprite.color = c;
    }
}

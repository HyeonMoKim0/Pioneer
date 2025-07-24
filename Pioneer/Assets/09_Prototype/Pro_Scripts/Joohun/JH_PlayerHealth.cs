using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JH_PlayerHealth : MonoBehaviour, IBegin
{
    [Header("Health Settings")]
    public int maxHP = 100;
    private int currentHP;

    [Header("Respawn Settings")]
    public float invincibleDuration = 2f; // �����ð�
    public float respawnDelay = 15f; // ������ �ð�
    public Vector3 respawnPosition = new Vector3(0f, -1f, 0f);

    private bool isDead = false;
    private bool isInvincible = false;

    void Init()
    {
        currentHP = maxHP;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) // �÷��̾� ������ �׽�Ʈ�� ����
        {
            TakeDamage(30);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible) return;

        currentHP -= damage;
        Debug.Log($"[TakeDamage] Took {damage} damage. Current HP: {currentHP}");

        if (currentHP <= 0)
        {
            Debug.Log("Player die");
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    private IEnumerator InvincibilityCoroutine() // �����ð�
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibleDuration);
        isInvincible = false;
    }

    private void Die()
    {
        isDead = true;

        JH_GameManager.Instance.RespawnPlayer(gameObject, respawnPosition, respawnDelay);

        gameObject.SetActive(false);
    }

    public void Revive()
    {
        currentHP = maxHP;
        isDead = false;
        isInvincible = false;

        Debug.Log(" Player respawn");
    }

    public bool IsDead()
    {
        return isDead;
    }
}

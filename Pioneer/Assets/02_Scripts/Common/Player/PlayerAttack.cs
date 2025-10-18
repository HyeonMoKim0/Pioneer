using UnityEngine;

public class PlayerAttack : MonoBehaviour, IBegin
{
    public int damage;
    public Collider attackCollider;

    private void Awake()
    {
        // ���� ���� �� Ȯ���ϰ� ��Ȱ��ȭ
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<CreatureBase>()?.TakeDamage(damage, this.gameObject);
            // ����ġ ����
            PlayerStatsLevel.Instance.AddExp(GrowStatType.Combat, damage);
            UnityEngine.Debug.Log($"AddExp() ȣ��");
        }
    }

    public void EnableAttackCollider()
    {
        if (attackCollider != null)
        {
            UnityEngine.Debug.Log($">> PlayerAttack.EnableAttackCollider() ȣ��");

            attackCollider.enabled = true;
        }
    }

    public void DisableAttackCollider()
    {
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
        }
    }

    public void SetAttackRange(float range)
    {
        Vector3 v = transform.localScale;
        v.z = range;
        transform.localScale = v;
    }
}

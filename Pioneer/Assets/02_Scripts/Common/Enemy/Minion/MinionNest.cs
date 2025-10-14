using System.Collections;
using UnityEngine;

public class MinionNest : EnemyBase
{
    [Header("���� �̴Ͼ� ����")]
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private int maxMinionCount = 1;

    [Header("���� �ð� ����")]
    [SerializeField] float initDelay = 5.0f;

    void OnEnable()
    {
        StartCoroutine(SpawnMinionRoutine());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    protected override void SetAttribute()
    {
        base.SetAttribute();
        maxHp = 10;
        hp = maxHp;
    }

    IEnumerator SpawnMinionRoutine()
    {
        yield return new WaitForSeconds(initDelay);

        for(int i = 0; i < maxMinionCount; i++)
        {
            Instantiate(minionPrefab, transform.position, Quaternion.identity);

            if(i < maxMinionCount - 1)
            {
                yield return new WaitForSeconds(initDelay);
            }
        }

        Destroy(gameObject);
    }
}

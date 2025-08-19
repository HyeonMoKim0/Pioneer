using System.Collections;
using UnityEngine;

/* ================================
2508118
- ������ �̴Ͼ� 2������ �����ϱ⸸ ��
- ������ 10�� ü���� ���� �̴Ͼ� ���� ��Ÿ���� 5��
- �̴Ͼ� 2������ �����ϰ� ���� �ı�

- ���� ������ �̴Ͼ���� ���� �������� �� ������ �¾.
- ���� ������ �̴Ͼ���� �� �۵������ʴ°� ����..?
================================ */


public class MinionNest : EnemyBase
{
    [Header("���� �̴Ͼ� ����")]
    [SerializeField] private GameObject minionPrefab;
    private int maxMinionCount = 2;

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

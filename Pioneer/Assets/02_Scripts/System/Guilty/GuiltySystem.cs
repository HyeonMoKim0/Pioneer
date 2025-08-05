using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GuiltySystem : MonoBehaviour, IBegin
{
    public static GuiltySystem instance;

    public bool canUseESC = false;

    

    [Header("Dark Object")]
    public Vector3 forwardVector; // ī�޶� �缱���� ��ġ�� ���, �̴� �߿��մϴ�.
    public Vector3 rightVector;
    [SerializeField] GameObject prefab;
    [SerializeField] GameObject player;
    [SerializeField] Transform pivot;
    [SerializeField] Vector2 size;
    [SerializeField] float darkObjectLifeTime;
    [SerializeField] float darkObjectTerm;
    private Coroutine darkObjectCoroutine;
    private Vector3[] mEdgePoints;
    private Vector3 mForwardVector;
    private Vector3 mRightVector;
    private Vector2 mSize;
    private int deadCount = 0;
    private int maxAttackWeight = 20; // ������ ���۷��� : https://www.notion.so/2025e8a380a580c7abe6c8c80736cb6e?v=2025e8a380a580feb76f000c763770ff&p=1e970641e0a78013a100caebc2a28a4d&pm=s
    private int currentAttackWeight = 0; // ������ ���۷��� : https://www.notion.so/2025e8a380a580c7abe6c8c80736cb6e?v=2025e8a380a580feb76f000c763770ff&p=1e970641e0a78013a100caebc2a28a4d&pm=s
    private int level = 0;

    public void ChangeWeight(int value)
    {
        currentAttackWeight += value;
        currentAttackWeight = Mathf.Clamp(currentAttackWeight, 0, maxAttackWeight);

        switch (currentAttackWeight)
        {
            case >= 16: level = 4; break;
            case >= 12: level = 3; break;
            case >= 8: level = 2; break;
            case >= 4: level = 1; break;
            default: level = 0; break;
        }

        if (level >= 4)
        {
            canUseESC = false;
        }

        if (level >= 1 && darkObjectCoroutine == null)
        {
            darkObjectCoroutine = StartCoroutine(CoroutineDarkObject());
        }
    }

    public void CrewDead()
    {
        deadCount++;
        ChangeWeight(2);
    }
    public void TimeReachedToDayTime() => ChangeWeight(-1);
    public void Drink() => ChangeWeight(-2);

    private void Awake()
    {
        instance = this;

        mForwardVector = forwardVector.normalized;
        mRightVector = rightVector.normalized;
        mSize = size / 2;
        Vector3 mHalf = size / 2;

        mEdgePoints = new Vector3[4]
        {
            new Vector3(size.x / 2, player.transform.position.y, size.y / 2),
            new Vector3(-size.x / 2, player.transform.position.y, size.y / 2),
            new Vector3(size.x / 2, player.transform.position.y, -size.y / 2),
            new Vector3(-size.x / 2, player.transform.position.y, -size.y / 2)
        };
    }

    // Start is called before the first frame update
    void Init()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.X) && Input.GetKeyDown(KeyCode.K))
        {
            CrewDead();
        }
    }

    private void SpawnDarkObject()
    {
        Vector3 mEdge1 = Vector3.zero;
        Vector3 mEdge2 = Vector3.zero;
        switch (Random.Range(0, 4))
        {
            case 0: mEdge1 = mEdgePoints[0]; mEdge2 = mEdgePoints[1]; break;
            case 1: mEdge1 = mEdgePoints[0]; mEdge2 = mEdgePoints[2]; break;
            case 2: mEdge1 = mEdgePoints[1]; mEdge2 = mEdgePoints[3]; break;
            case 3: mEdge1 = mEdgePoints[2]; mEdge2 = mEdgePoints[3]; break;
            default: break;
        }

        GameObject darkObject = Instantiate(prefab,
            Vector3.Lerp(mEdge1, mEdge2, Random.Range(0f, 1f)), Quaternion.identity);
        darkObject.transform.LookAt(player.transform.position);
        Destroy(darkObject, darkObjectLifeTime);
    }

    IEnumerator CoroutineDarkObject()
    {
        while (true)
        {
            yield return new WaitForSeconds(darkObjectTerm);

            float mProbability = 0.0f;
            switch (level)
            {
                case 1: mProbability = 0.4f; break;
                case 2: mProbability = 0.5f; break;
                case 3: mProbability = 0.5f; break;
                case 4: mProbability = 0.7f; break;
                default: break;
            }

            if (Random.Range(0.0f, 1.0f) < mProbability)
            {
                SpawnDarkObject();
            }

        }
    }
}

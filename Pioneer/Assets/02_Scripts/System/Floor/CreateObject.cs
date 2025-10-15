using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;


#warning TODO : CreateObject ������ �ʿ�
// ���� : ���콺 ���� -> �Ǽ� ���� ���� -> �̵� -> ��ġ
// �ʿ� : �����ǿ��� ���� ���� ���� -> ���� ��ư ���� -> ���� UI ���� -> �Ǽ� UI ��ȯ -> ���콺 ���� -> �Ǽ� ���� ���� -> �̵� -> �ð� �Ҹ� �� ���ع��� �ʴ��� �׻� üũ -> ������ �Ҹ� -> ��ġ

public class CreateObject : MonoBehaviour, IBegin
{
    public enum CreationType { Platform, Wall, Door, Barricade, CraftingTable , Ballista, Trap, Lantern }

    [System.Serializable]
    public class CreationList
    {
        public GameObject platform;
        public GameObject wall;
        public GameObject door;
        public GameObject barricade;
        public GameObject craftingTable;
        public GameObject ballista;
        public GameObject trap;
        public GameObject lantern;
    }

    public static CreateObject instance;

    [Header("�⺻ ����")]
    [SerializeField] private Transform worldSpaceParent;
    private Transform playerTrans;
    private Camera mainCamera;

    [Header("��ġ ������Ʈ ����")]
    public CreationType creationType;
    [SerializeField] private float maxDistance;
    [SerializeField] private LayerMask platformLayer;
    [SerializeField] private LayerMask creationLayer;
    [SerializeField] private Color rejectColor;
    [SerializeField] private Color permitColor;
    [SerializeField] private CreationList creationList;
    private GameObject onHand;
    private GameObject tempObj;
    private Renderer creationRender;
    private Dictionary<CreationType, GameObject> creationDict = new Dictionary<CreationType, GameObject>();
    private int rotateN = 0;

    [Header("�׺�޽� ����")]
    [SerializeField] private NavMeshSurface navMeshSurface;
    [SerializeField] private float stopDistance = 1.5f;
    private NavMeshAgent playerAgent;

    private void Awake()
    {
        Debug.Log($">> CreateObject : {gameObject.name}");
		instance = this;

        mainCamera = Camera.main;
        playerTrans = transform;
        playerAgent = GetComponent<NavMeshAgent>();

        creationDict.Add(CreationType.Platform,         creationList.platform);
        creationDict.Add(CreationType.Wall,             creationList.wall);
        creationDict.Add(CreationType.Door,             creationList.door);
        creationDict.Add(CreationType.Barricade,        creationList.barricade);
        creationDict.Add(CreationType.CraftingTable,    creationList.craftingTable);
        creationDict.Add(CreationType.Ballista,         creationList.ballista);
        creationDict.Add(CreationType.Trap,             creationList.trap);
        creationDict.Add(CreationType.Lantern,          creationList.lantern);

        CreateObjectInit();
    }

    //�ܺο��� 'creationType' ���� �� 'Init'�޼��� ȣ���Ͽ� �ʱ�ȭ
    public void CreateObjectInit()
    {
        rotateN = 0;

        onHand = Instantiate(creationDict[creationType], worldSpaceParent);
        onHand.transform.localRotation = Quaternion.identity;
        onHand.transform.localPosition = Vector3.zero;
        onHand.layer = 0;

        creationRender = onHand.GetComponent<Renderer>();
        onHand.GetComponent<Collider>().isTrigger = true;
    }

    private void Start()
    {
        //ExitInstallMode(); // ���� ���� �� ��ġ ��� OFF
    }

    private void Update()
    {
        if (onHand == null) return;

        CheckCreatable();
        Trim();

        if (tempObj != null && (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0))
            CancelInstall();
    }

    //��ǥ ����
    private Vector3 SnapToGrid(Vector3 worldPos)
    {
        float cellSize = 1f;
        int x = Mathf.RoundToInt(worldPos.x / cellSize);
        int z = Mathf.RoundToInt(worldPos.z / cellSize);
        return new Vector3(x * cellSize, 0f, z * cellSize);
    }

    //��ġ ���� ���� �Ǻ�
    private void CheckCreatable()
    {
        #region UI ������ ��ġ���� ���� ��������� ������ �ʰ� ó���� 
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            onHand.SetActive(false);    // ������ ����
            return;                     // UI Ŭ�� ���̸� ��ġ/�̵�/ȸ�� ���� ����
        }
        else
        {
            onHand.SetActive(true);     // UI���� ����� �ٽ� ���̰�
        }
        #endregion

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        //���콺 ��ġ�κ��� y = 0�� ���� ��ǥ ���ϱ�
        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 mouseWorldPos = ray.GetPoint(enter);
            Vector3 localPos = SnapToGrid(worldSpaceParent.InverseTransformPoint(mouseWorldPos));
            onHand.transform.localPosition = localPos;
            Vector3 worldPos = onHand.transform.position;
            onHand.transform.position += Vector3.up * 0.01f;

            if (CheckNear(worldPos))
            {
                creationRender.material.color = permitColor;

                if (Input.GetMouseButtonDown(0))
                {
                    MoveToCreate(worldPos, localPos);
                }
            }
            else
            {
                creationRender.material.color = rejectColor;
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            rotateN++;
            onHand.transform.localRotation = Quaternion.Euler(new Vector3(0f, 90f * rotateN, 0f));
        }
    }

    //�ֺ� �˻�
    private bool CheckNear(Vector3 center)
    {
        float[] xArr; //x��ġ
        float[] zArr; //y��ġ
        float[] xSign; //x��ȣ
        float[] zSign; //y��ȣ

        //maxDistance���� �ָ� ��ġ �Ұ���
        if (Vector3.SqrMagnitude(center - SnapToGrid(playerTrans.position)) > Mathf.Pow(maxDistance, 2))
        {
            return false;
        }

        switch (creationType)
        {
            case CreationType.Platform:
                // ���� �߰�
                if (MastManager.Instance != null)
                {
                    int currentDeckCount = MastManager.Instance.currentDeckCount;
                    int maxDeckCount = 30; // 1���� �ִ� ����

                    // ���� ������ ���� �ִ� ���� Ȯ��
                    MastSystem[] masts = FindObjectsOfType<MastSystem>();
                    if (masts.Length > 0)
                    {
                        maxDeckCount = masts[0].GetMaxDeckCount();
                    }

                    // �ִ� ���� �ʰ� �� ��ġ �Ұ�
                    if (currentDeckCount >= maxDeckCount)
                    {
                        Debug.Log($"���� ��ġ �Ұ�: {currentDeckCount}/{maxDeckCount}�� (�ִ� ����)");
                        return false;
                    }
                }
                // �������

                //1.414213 * 0.5
                xArr = new float[]{ 0.707106f, 0.707106f, -0.707106f, -0.707106f };
                zArr = new float[]{ 0.707106f, -0.707106f, -0.707106f, 0.707106f };

                //���콺 ��ġ�� �÷��� ������ ��ġ �Ұ�
                if (Physics.CheckBox(center, new Vector3(0.99f, 0.5f, 0.99f), Quaternion.Euler(new Vector3(0f, 45f, 0f)), platformLayer))
                {
                    return false;
                }

                //���콺 ��ġ ���� 4���⿡ ������ü(1.98, 1, 0.48) ������ �÷��� ������ ��ġ ����
                for (int i = 0; i < 4; i++)
                {
                    Vector3 offset = new Vector3(xArr[i], 0f, zArr[i]);
                    Vector3 origin = center + offset;
                    Vector3 halfSize = new Vector3(0.99f, 0.5f, 0.249f);
                    Quaternion orientation = Quaternion.Euler(new Vector3(0f, 45f * i + 45f, 0f));

					// ���⿡�� ����!!!!!!!!!!!!!!!! �ٴڳ��� ������������ ���ǹ�!!!!!!!!!!!!!!!!!!!!!!!!!
					if (Physics.CheckBox(origin, halfSize, orientation, platformLayer))
                        return true;
                    //else
                        //ItemDeckDisconnect.instance.DestroyDeck();
                }

                //�� �� ��ġ �Ұ�
                return false;

            case CreationType.Wall:
            case CreationType.Barricade:
                xArr = new float[] { -1.060659f, -0.353553f, 0.353553f, 1.060659f };
                zArr = new float[] { -1.060659f, -0.353553f, 0.353553f, 1.060659f };

                for (int i = 0; i < xArr.Length; i++)
                {
                    float angle = onHand.transform.rotation.y;
                    int zIndex = rotateN % 2 == 0 ? i : xArr.Length - 1 - i;

                    Vector3 offset = new Vector3(xArr[i], 0f, zArr[zIndex]);
                    Vector3 origin = center + offset;
                    Vector3 halfSize = new Vector3(0.49f, 0.5f, 0.49f);
                    Quaternion orientation = Quaternion.Euler(new Vector3(0f, 45f, 0f));

                    //������Ʈ�� ��ġ�� ��ġ�� �÷����� ������ ��ġ �Ұ�
                    if (!Physics.CheckBox(origin, halfSize, orientation, platformLayer))
                    {
                        Debug.Log("�÷��� ����");
                        return false;
                    }
                    //���콺 ��ġ�� ������Ʈ�� ������ ��ġ �Ұ�
                    if (Physics.CheckBox(origin, halfSize, orientation, creationLayer))
                    {
                        Debug.Log("������Ʈ ����");
                        return false;
                    }
                }
                return true;

            case CreationType.Door:
                xArr = new float[] { 0.353553f, 1.060659f };
                zArr = new float[] { 0.353553f, 1.060659f };
                xSign = new float[] { -1f, -1f, 1f, 1f };
                zSign = new float[] { -1f, 1f, 1f, -1f };

                for (int i = 0; i < xArr.Length; i++)
                {
                    float angle = onHand.transform.rotation.y;

                    Vector3 offset = new Vector3(xArr[i] * xSign[rotateN % 4], 0f, zArr[i] * zSign[rotateN % 4]);
                    Vector3 origin = center + offset;
                    Vector3 halfSize = new Vector3(0.49f, 0.5f, 0.49f);
                    Quaternion orientation = Quaternion.Euler(new Vector3(0f, 45f, 0f));

                    //������Ʈ�� ��ġ�� ��ġ�� �÷����� ������ ��ġ �Ұ�
                    if (!Physics.CheckBox(origin, halfSize, orientation, platformLayer))
                    {
                        Debug.Log("�÷��� ����");
                        return false;
                    }
                    //���콺 ��ġ�� ������Ʈ�� ������ ��ġ �Ұ�
                    if (Physics.CheckBox(origin, halfSize, orientation, creationLayer))
                    {
                        Debug.Log("������Ʈ ����");
                        return false;
                    }
                }
                return true;

            case CreationType.CraftingTable:
                xArr = new float[] { -0.353553f, 0.353553f };
                zArr = new float[] { -0.353553f, 0.353553f };

                for (int i = 0; i < xArr.Length; i++)
                {
                    float angle = onHand.transform.rotation.y;
                    int zIndex = rotateN % 2 == 0 ? i : xArr.Length - 1 - i;

                    Vector3 offset = new Vector3(xArr[i], 0f, zArr[zIndex]);
                    Vector3 origin = center + offset;
                    Vector3 halfSize = new Vector3(0.49f, 0.5f, 0.49f);
                    Quaternion orientation = Quaternion.Euler(new Vector3(0f, 45f, 0f));

                    //������Ʈ�� ��ġ�� ��ġ�� �÷����� ������ ��ġ �Ұ�
                    if (!Physics.CheckBox(origin, halfSize, orientation, platformLayer))
                    {
                        Debug.Log("�÷��� ����");
                        return false;
                    }
                    //���콺 ��ġ�� ������Ʈ�� ������ ��ġ �Ұ�
                    if (Physics.CheckBox(origin, halfSize, orientation, creationLayer))
                    {
                        Debug.Log("������Ʈ ����");
                        return false;
                    }
                }
                return true;

            case CreationType.Ballista:
                xArr = new float[] { 0f, 0.707106f, 1.414213f, 0.707106f, 0f, -0.707106f, -1.414213f, -0.707106f, 0f };
                zArr = new float[] { 0f, 0.707106f, 0f, -0.707106f, -1.414213f, -0.707106f, 0f, 0.707106f, 1.414213f };

                for (int i = 0; i < xArr.Length; i++)
                {
                    float angle = onHand.transform.rotation.y;

                    Vector3 offset = new Vector3(xArr[i], 0f, zArr[i]);
                    Vector3 origin = center + offset;
                    Vector3 halfSize = new Vector3(0.49f, 0.5f, 0.49f);
                    Quaternion orientation = Quaternion.Euler(new Vector3(0f, 45f, 0f));

                    //������Ʈ�� ��ġ�� ��ġ�� �÷����� ������ ��ġ �Ұ�
                    if (!Physics.CheckBox(origin, halfSize, orientation, platformLayer))
                    {
                        Debug.Log("�÷��� ����");
                        return false;
                    }
                    //���콺 ��ġ�� ������Ʈ�� ������ ��ġ �Ұ�
                    if (Physics.CheckBox(origin, halfSize, orientation, creationLayer))
                    {
                        Debug.Log("������Ʈ ����");
                        return false;
                    }
                }
                return true;

            case CreationType.Trap:
                xArr = new float[] { 0.353553f, 0.353553f, -0.353553f, -0.353553f };
                zArr = new float[] { 0.353553f, -0.353553f, -0.353553f, 0.353553f };

                for (int i = 0; i < xArr.Length; i++)
                {
                    float angle = onHand.transform.rotation.y;

                    Vector3 offset = new Vector3(xArr[i], 0f, zArr[i]);
                    Vector3 origin = center + offset;
                    Vector3 halfSize = new Vector3(0.49f, 0.5f, 0.49f);
                    Quaternion orientation = Quaternion.Euler(new Vector3(0f, 45f, 0f));

                    //������Ʈ�� ��ġ�� ��ġ�� �÷����� ������ ��ġ �Ұ�
                    if (!Physics.CheckBox(origin, halfSize, orientation, platformLayer))
                    {
                        Debug.Log("�÷��� ����");
                        return false;
                    }
                    //���콺 ��ġ�� ������Ʈ�� ������ ��ġ �Ұ�
                    if (Physics.CheckBox(origin, halfSize, orientation, creationLayer))
                    {
                        Debug.Log("������Ʈ ����");
                        return false;
                    }
                }
                return true;

            case CreationType.Lantern:
                xArr = new float[] { 0f };
                zArr = new float[] { 0f };

                for (int i = 0; i < xArr.Length; i++)
                {
                    float angle = onHand.transform.rotation.y;

                    Vector3 offset = new Vector3(xArr[i], 0f, zArr[i]);
                    Vector3 origin = center + offset;
                    Vector3 halfSize = new Vector3(0.49f, 0.5f, 0.49f);
                    Quaternion orientation = Quaternion.Euler(new Vector3(0f, 45f, 0f));

                    //������Ʈ�� ��ġ�� ��ġ�� �÷����� ������ ��ġ �Ұ�
                    if (!Physics.CheckBox(origin, halfSize, orientation, platformLayer))
                    {
                        Debug.Log("�÷��� ����");
                        return false;
                    }
                    //���콺 ��ġ�� ������Ʈ�� ������ ��ġ �Ұ�
                    if (Physics.CheckBox(origin, halfSize, orientation, creationLayer))
                    {
                        Debug.Log("������Ʈ ����");
                        return false;
                    }
                }
                return true;

        }

        return false;
    }

    //�����Ϸ� �̵�
    private void MoveToCreate(Vector3 world, Vector3 local)
    {
        if (tempObj != null)
        {
            Destroy(tempObj);
        }

        //���� �� ��ġ ǥ��
        tempObj = Instantiate(creationDict[creationType], worldSpaceParent);
        tempObj.transform.localPosition = new Vector3(local.x, 0f, local.z);
        tempObj.transform.rotation = onHand.transform.rotation;
        tempObj.GetComponent<Collider>().isTrigger = true;
        tempObj.GetComponent<Renderer>().material.color = permitColor;

        Vector3 dir = (world - playerTrans.position).normalized;
        Vector3 stopPos = world - dir * stopDistance;

        playerAgent.isStopped = false;
        playerAgent.SetDestination(stopPos);
    }

    //���� �Ϸ� ����
    private void Trim()
    {
        if (tempObj == null) return;

        float dist = Vector3.Distance(playerAgent.transform.position, tempObj.transform.position);
        if (dist < 2.0f)
        {
            // ���⼭ �ð��� �Ҹ��� �� ������ ���Ѿƾ� ��.


            tempObj.GetComponent<Collider>().isTrigger = false;
            tempObj.GetComponent<Renderer>().material.color = Color.white;

            navMeshSurface.BuildNavMesh();

            //���� �߰�
            if (creationType == CreationType.Platform && MastManager.Instance != null)
            {
                MastManager.Instance.UpdateCurrentDeckCount();
                Debug.Log($"���� ���� ����: {MastManager.Instance.currentDeckCount}");
            }
            //�������

            Debug.Log($"[��ġ �Ϸ��] �Ÿ�: {dist}");

            playerAgent.ResetPath();
            playerAgent.isStopped = false;

            tempObj = null;
        }
 
    }



    //�������� ���� ������ ĵ��
    void CancelInstall()
    {
        playerAgent.isStopped = true;
        playerAgent.ResetPath();
        Destroy(tempObj);
        tempObj = null;
    }

    public void EnterInstallMode(SInstallableObjectDataSO installableSO)
    {
        if (onHand != null)
        {
            Destroy(onHand);
            onHand = null;
        }

        if (!playerAgent.enabled)
            playerAgent.enabled = true;

        // ��ġ Ÿ�� ����
        creationType = (CreationType)(int)installableSO.installType;

        Debug.Log($"[��ġ��� ����] ���õ� ������Ʈ: {installableSO.name}");

        CreateObjectInit(); // �� ������ ������Ʈ ����
    }

    public void ExitInstallMode()
    {
        // ������ ������Ʈ ����
        if (onHand != null)
        {
            Destroy(onHand);
            onHand = null;
        }

        // �ӽ� ���� ������Ʈ ���� (�̵� �� ��ġ ����� ������Ʈ)
        if (tempObj != null)
        {
            Destroy(tempObj);
            tempObj = null;
        }

        // NavMeshAgent ���� �ʱ�ȭ
        playerAgent.ResetPath();
        playerAgent.isStopped = true;

        Debug.Log("[��ġ ��� �����]");
    }
}
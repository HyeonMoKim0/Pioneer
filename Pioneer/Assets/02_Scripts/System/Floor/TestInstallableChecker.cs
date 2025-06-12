// ===================================================================================================
// �÷��̾ �ٴ� ��ġ ����� ���� ���, ������ ��ġ�� �̵� �� ��ġ
// ���� ���� �� ��� ��ҵǸ�, ���� Ÿ���� ���� ��쿡�� ��ġ ����
// ===================================================================================================

using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using TMPro;

public class TestInstallableChecker : MonoBehaviour
{
    #region Ȥ�� �𸣴ϱ� ���ܵα�~ 
    //public GameObject previewFloorPrefab;

    //public Material validMaterial;
    //public Material invalidMaterial;
    //public Material placedMaterial;

    //public float maxPlaceDistance;

    //public GameObject warningText;
    //public float warningDuration = 1.5f;
    //private Coroutine warningCoroutine;

    //private GameObject currentPreview;
    //private Renderer previewRenderer;
    //private Vector3 targetPosition;

    //private const float positionOffset = 0.001f;

    //// ��ġ ��� �̵� ������ ����
    //private bool isMovingToInstallPoint = false;
    //private Vector3 destinationQueued;
    #endregion

    public Camera mainCamera;
    public Transform worldSpaceParent;

    public NavMeshSurface navMeshSurface;
    public NavMeshAgent playerAgent;
    public float stopDistance = 2f;

    public LayerMask installableLayer;
    public LayerMask blockLayer;

    public bool isMovingToInstall = false;
    public float viewRange = 10f;
    private Vector3 installPosition;

    public SInstallableObjectDataSO currentObject;
    private GameObject previewObject;
    private Renderer previewRenderer;
    private Color defaultColor;


    void Start()
    {
        #region �̰͵� ����α�~
        //currentPreview = Instantiate(previewFloorPrefab, worldSpaceParent);
        //currentPreview.transform.localRotation = Quaternion.identity;
        //currentPreview.transform.localPosition = Vector3.zero;

        //previewRenderer = currentPreview.GetComponent<Renderer>();

        //Collider previewCollider = currentPreview.GetComponent<Collider>();
        //if (previewCollider != null)
        //    previewCollider.isTrigger = true;

        //if (previewRenderer == null)
        //    Debug.LogError("�����信 Renderer�� �����ϴ�.");
        #endregion

        if (mainCamera == null)
            mainCamera = Camera.main;

        previewObject = Instantiate(currentObject.prefab, worldSpaceParent);
        previewObject.transform.localPosition = Vector3.zero;

        previewRenderer = previewObject.GetComponent<Renderer>();

        Collider previewCollider = previewObject.GetComponent<Collider>();
        if (previewCollider != null)
            previewCollider.isTrigger = true;
    }

    void Update()
    {
        HandlePreview();
        CheckArrivalAndInstall();

        // ��ġ ���� �÷��̾� ���� ���� �� ��ġ ��� ���
        if (isMovingToInstall)
        {
            Vector3 moveInput = GetComponent<JH_PlayerMovement>().moveInput;

            if (moveInput != Vector3.zero)
            {
                CancelInstall();
            }
        }
    }

    // ���� ��������� �Ϸ�~~!!! ����, Start, Update���� �Ϸ�~~~@!!!!!!


    void HandlePreview()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, installableLayer, QueryTriggerInteraction.Collide))
        {
            Vector3 snappedPos = SnapToGrid(hit.point);
            installPosition = snappedPos;

            #region ���͸� ����
            //snappedPos += new Vector3(positionOffset, positionOffset, -positionOffset);
            //currentPreview.transform.localPosition = snappedPos;
            //currentPreview.SetActive(true);
            #endregion

            bool canPlace = IsPlaceable(snappedPos);
            ApplyPreviewColor(canPlace ? Color.green : Color.red);

            if (Input.GetMouseButtonDown(0))
            {
                if (canPlace && !isMovingToInstall)
                {
                    StartMovingToInstall(snappedPos);
                }
                else if (!canPlace)
                {
                    ShowWarningText();
                }
            }
        }
        else
        {
            previewObject.SetActive(false);
            #region �̰� ���߿� �����ϱ�~~ 
            //warningText.SetActive(false);
            #endregion
        }
    }

    void StartMovingToInstall(Vector3 snappedPos)
    {
        if (playerAgent == null || !playerAgent.isOnNavMesh)
            return;

        Vector3 worldTarget = worldSpaceParent.TransformPoint(snappedPos);

        // ���� ��� �� stopDistance �տ��� ����
        Vector3 directionToTarget = (worldTarget - transform.position).normalized;
        Vector3 stopBeforeTarget = worldTarget - directionToTarget * stopDistance;

        playerAgent.isStopped = false;
        playerAgent.SetDestination(stopBeforeTarget);

        // �ٲ�� �� ���� ����!!!! Vector3 ������ ���� ������ �ϳ�?
        installPosition = snappedPos;
        isMovingToInstall = true;

        Debug.Log("��ġ ���� �α����� �̵� ����");
    }

    void CheckArrivalAndInstall()
    {
        if (!isMovingToInstall) return;

        bool arrived = !playerAgent.pathPending &&
                       playerAgent.remainingDistance <= playerAgent.stoppingDistance;

        if (arrived)
        {
            InstallTile(installPosition);

            isMovingToInstall = false;
            installPosition = Vector3.zero;

            playerAgent.ResetPath();
            playerAgent.isStopped = false;

            Debug.Log("��ġ �Ϸ� �� ���� �ʱ�ȭ");
        }
    }

    void InstallTile(Vector3 localPosition)
    {
        GameObject tile = Instantiate(previewObject, worldSpaceParent);
        tile.transform.localPosition = localPosition;
        tile.transform.localRotation = Quaternion.identity;

        Renderer r = tile.GetComponent<Renderer>();
        if (r == null && r.material == null)
            Debug.LogError("Renderer Ȥ�� Material �ٽ� Ȯ�����ּ���!!!@!! Ȯ���϶� ���̾�!!@@!!!!!");

        Collider c = tile.GetComponent<Collider>();
        if (c != null)
            c.isTrigger = false;

        if (navMeshSurface != null)
            navMeshSurface.BuildNavMesh();

        tile.name = $"Tile ({localPosition.x}, {localPosition.y}, {localPosition.z})";
        Debug.Log($"��ġ �Ϸ�: {localPosition}");
    }

    bool IsPlaceable(Vector3 snappedPos)
    {
        float distance = Vector3.Distance(transform.position, worldSpaceParent.TransformPoint(snappedPos));
        if (distance > viewRange)
            return false;

        Vector3 worldSnappedPos = worldSpaceParent.TransformPoint(snappedPos);
        Collider[] overlaps = Physics.OverlapBox(worldSnappedPos, Vector3.one * 0.45f, Quaternion.identity, blockLayerMask, QueryTriggerInteraction.Ignore);
        if (overlaps.Length > 0)
            return false;

        // �ٴ� ���Ἲ üũ (�����¿�)
        Vector3[] directions = {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right
        };

        float checkDistance = 1f;
        foreach (Vector3 dir in directions)
        {
            Vector3 checkPos = worldSnappedPos + dir * checkDistance;
            if (Physics.CheckBox(checkPos, Vector3.one * 0.45f, Quaternion.identity, blockLayerMask, QueryTriggerInteraction.Ignore))
            {
                return true;
            }
        }

        return false;
    }

    void CancelInstall()
    {
        playerAgent.isStopped = true;
        playerAgent.ResetPath();
        isMovingToInstallPoint = false;
        destinationQueued = Vector3.zero;

        Debug.Log("�÷��̾� ���ۿ� ���� ��ġ ����� ��ҵ�");
    }

    Vector3 SnapToGrid(Vector3 worldPos)
    {
        float cellSize = 1f;
        Vector3 localPos = worldSpaceParent.InverseTransformPoint(worldPos);
        int x = Mathf.RoundToInt(localPos.x / cellSize);
        int z = Mathf.RoundToInt(localPos.z / cellSize);
        return new Vector3(x * cellSize, 0f, z * cellSize);
    }

    void ShowWarningText()
    {
        if (warningCoroutine != null)
            StopCoroutine(warningCoroutine);

        warningText.SetActive(true);
        warningCoroutine = StartCoroutine(HideWarningTextAfterDelay());
    }

    IEnumerator HideWarningTextAfterDelay()
    {
        yield return new WaitForSeconds(warningDuration);
        warningText.SetActive(false);
        warningCoroutine = null;
    }










    void ApplyPreviewColor(Color color)
    {
        if (previewRenderer != null)
            previewRenderer.material.color = color;
    }
}
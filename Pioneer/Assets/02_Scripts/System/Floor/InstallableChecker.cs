using TMPro;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class InstallableChecker : MonoBehaviour
{
    [Header("��ġ ����")]
    public Camera mainCamera;
    public GameObject previewFloorPrefab;
    public LayerMask installableLayer;
    public LayerMask blockLayerMask;
    public Material validMaterial;
    public Material invalidMaterial;
    public Material placedMaterial;
    public Transform player;
    public float maxPlaceDistance;
    public Transform worldSpaceParent;
    public GameObject warningText;
    public float warningDuration = 1.5f;
    private Coroutine warningCoroutine;

    [Header("NavMesh ����")]
    public NavMeshSurface navMeshSurface;
    public NavMeshAgent playerAgent;

    private GameObject currentPreview;
    private Renderer previewRenderer;
    private Vector3 targetPosition;

    private const float positionOffset = 0.001f;

    private bool isMovingToInstallPoint = false;
    private Vector3 destinationQueued;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (worldSpaceParent == null)
        {
            Debug.LogError("WorldSpace �θ� �Ҵ����ּ���.");
            return;
        }

        currentPreview = Instantiate(previewFloorPrefab, worldSpaceParent);
        currentPreview.transform.localRotation = Quaternion.identity;
        currentPreview.transform.localPosition = Vector3.zero;

        previewRenderer = currentPreview.GetComponent<Renderer>();

        Collider previewCollider = currentPreview.GetComponent<Collider>();
        if (previewCollider != null)
            previewCollider.isTrigger = true;

        if (previewRenderer == null)
            Debug.LogError("�����信 Renderer�� �����ϴ�.");
    }

    void Update()
    {
        HandlePreview();
        CheckArrivalAndInstall();
    }

    void HandlePreview()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, installableLayer, QueryTriggerInteraction.Collide))
        {
            Vector3 snappedPos = SnapToGrid(hit.point);
            targetPosition = snappedPos;

            snappedPos += new Vector3(positionOffset, positionOffset, -positionOffset);
            currentPreview.transform.localPosition = snappedPos;
            currentPreview.SetActive(true);

            bool canPlace = IsPlaceable(snappedPos);
            previewRenderer.material = canPlace ? validMaterial : invalidMaterial;

            if (canPlace)
            {
                if (Input.GetMouseButtonDown(0) && !isMovingToInstallPoint)
                {
                    StartMovingToInstall(snappedPos);
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    ShowWarningText();
                }
            }
        }
        else
        {
            currentPreview.SetActive(false);
            warningText.SetActive(false);
        }
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


    void StartMovingToInstall(Vector3 snappedPos)
    {
        if (playerAgent == null || !playerAgent.isOnNavMesh)
            return;

        Vector3 worldTarget = worldSpaceParent.TransformPoint(snappedPos);

        playerAgent.isStopped = false;
        playerAgent.SetDestination(worldTarget);

        destinationQueued = snappedPos;
        isMovingToInstallPoint = true;

        Debug.Log("��ġ �������� �̵� ����");
    }


    void CheckArrivalAndInstall()
    {
        if (!isMovingToInstallPoint) return;

        // ���� �Ǵ�
        bool arrived = !playerAgent.pathPending &&
                       playerAgent.remainingDistance <= playerAgent.stoppingDistance;

        if (arrived)
        {
            InstallTile(destinationQueued);

            // ���� �ʱ�ȭ
            isMovingToInstallPoint = false;
            destinationQueued = Vector3.zero;

            playerAgent.isStopped = true;
            playerAgent.ResetPath();

            Debug.Log("���� �� ��ġ �� ���� �ʱ�ȭ �Ϸ�");
        }
    }


    void InstallTile(Vector3 localPosition)
    {
        GameObject tile = Instantiate(previewFloorPrefab, worldSpaceParent);
        tile.transform.localPosition = localPosition;
        tile.transform.localRotation = Quaternion.identity;

        Renderer r = tile.GetComponent<Renderer>();
        if (r != null && placedMaterial != null)
            r.material = placedMaterial;

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
        float distance = Vector3.Distance(player.position, worldSpaceParent.TransformPoint(snappedPos));
        if (distance > maxPlaceDistance)
            return false;

        Vector3 worldSnappedPos = worldSpaceParent.TransformPoint(snappedPos);
        Collider[] overlaps = Physics.OverlapBox(worldSnappedPos, Vector3.one * 0.45f, Quaternion.identity, blockLayerMask, QueryTriggerInteraction.Ignore);
        return overlaps.Length == 0;
    }

    Vector3 SnapToGrid(Vector3 worldPos)
    {
        float cellSize = 1f;
        Vector3 localPos = worldSpaceParent.InverseTransformPoint(worldPos);
        int x = Mathf.RoundToInt(localPos.x / cellSize);
        int z = Mathf.RoundToInt(localPos.z / cellSize);
        return new Vector3(x * cellSize, 0f, z * cellSize);
    }
}

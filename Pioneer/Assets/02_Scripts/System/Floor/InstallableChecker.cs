using UnityEngine;

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

    // ���� ����
    private GameObject currentPreview;
    private Renderer previewRenderer;
    private Vector3 targetPosition;

    // ��ġ ������ �� (���͸� ����)
    private const float positionOffset = 0.001f;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (worldSpaceParent == null)
        {
            Debug.LogError("WorldSpace �θ� �Ҵ����ּ���.");
            return;
        }

        // ������ ������Ʈ ���� �� �ʱ�ȭ
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
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, installableLayer, QueryTriggerInteraction.Collide))
        {
            Vector3 snappedPos = SnapToGrid(hit.point);
            targetPosition = snappedPos;

            // ���͸� ������ �̼� ����
            snappedPos.x += positionOffset;
            snappedPos.y += positionOffset;
            snappedPos.z -= positionOffset;

            currentPreview.transform.localPosition = snappedPos;
            currentPreview.SetActive(true);

            if (IsPlaceable(snappedPos))
            {
                previewRenderer.material = validMaterial;

                if (Input.GetMouseButtonDown(0))
                {
                    InstallTile();
                }
            }
            else
            {
                previewRenderer.material = invalidMaterial;
            }
        }
        else
        {
            currentPreview.SetActive(false);
        }
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

    void InstallTile()
    {
        GameObject tile = Instantiate(previewFloorPrefab, worldSpaceParent);
        tile.transform.localPosition = targetPosition;
        tile.transform.localRotation = Quaternion.identity;

        Renderer r = tile.GetComponent<Renderer>();
        if (r != null && placedMaterial != null)
            r.material = placedMaterial;

        Collider c = tile.GetComponent<Collider>();
        if (c != null)
            c.isTrigger = false; // �浹 ���� ���·� ��ȯ, �⺻ ������ isTrigger = true�̱� ����

        tile.name = $"Tile ({targetPosition.x}, {targetPosition.y}, {targetPosition.z})";
        Debug.Log($"��ġ �Ϸ�: {targetPosition}");
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

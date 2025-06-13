using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class TestInstallableChecker : MonoBehaviour
{
    [Header("ī�޶��÷��̾�")]
    [SerializeField] Camera mainCamera;
    [SerializeField] Transform player;
    [SerializeField] float maxPlaceDistance = 5f;

    [Header("���̾� ����ũ")]
    [SerializeField] LayerMask installableLayerMask; // Raycast ��
    [SerializeField] LayerMask blockLayerMask;       // �浹 �˻��

    [Header("NavMesh �̵�")]
    [SerializeField] NavMeshSurface navMeshSurface;
    [SerializeField] float stopDistance = 1.5f;

    [Header("���� SO")]
    [SerializeField] SInstallableObjectDataSO currentData;

    [Header("������ �θ�")]
    [SerializeField] Transform previewParent;

    // ��Ÿ�ӿ� �����ϴ� ��Ƽ����
    Material _previewValidMat;
    Material _previewInvalidMat;

    GameObject _previewInstance;
    NavMeshAgent _agent;
    Vector3 _queuedLocalPos;
    bool _isMovingToInstall;

    void Start()
    {
        if (!mainCamera) mainCamera = Camera.main;
        _agent = GetComponent<NavMeshAgent>();
        SetupPreviewMats();
        CreatePreviewInstance();
    }

    void Update()
    {
        if (currentData == null) return;

        UpdatePreview();
        CheckArrival();

        // �̵� ���� �Է� �� ��ġ ���
        if (_isMovingToInstall &&
            (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0))
        {
            CancelInstallation();
        }
    }

    // 1) SO.previewMaterial ������� Valid/Invalid ��Ƽ���� ����
    void SetupPreviewMats()
    {
        var baseMat = currentData.previewMaterial;
        _previewValidMat = new Material(baseMat);
        _previewInvalidMat = new Material(baseMat);

        // ���� ������� (0.5 ���� ����)
        _previewValidMat.color = new Color(0f, 1f, 0f, 0.5f);
        _previewInvalidMat.color = new Color(1f, 0f, 0f, 0.5f);
    }

    // 2) ������ �ν��Ͻ� ���� (ó���� �� ��)
    void CreatePreviewInstance()
    {
        if (_previewInstance != null) Destroy(_previewInstance);
        _previewInstance = Instantiate(currentData.prefab, previewParent);
        _previewInstance.transform.localRotation = Quaternion.identity;
        _previewInstance.SetActive(false);

        // Ʈ���ŷ� ���� ���� �浹 ����
        foreach (var c in _previewInstance.GetComponentsInChildren<Collider>())
            c.isTrigger = true;
    }

    // 3) �� ������ ���콺 ��ġ �� ������ ����
    void UpdatePreview()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, installableLayerMask))
        {
            _previewInstance.SetActive(false);
            return;
        }

        // �׸��� ����
        Vector3 localHit = previewParent.InverseTransformPoint(hit.point);
        Vector3 snapped = new Vector3(
            Mathf.Round(localHit.x),
            0,
            Mathf.Round(localHit.z)
        );
        _previewInstance.transform.localPosition = snapped + Vector3.up * currentData.yOffset;
        _previewInstance.SetActive(true);

        // ��ġ ���� ���� �Ǵ�
        bool canPlace = CanPlaceAt(_previewInstance.transform.position);
        ApplyPreviewMat(canPlace);

        // Ŭ�� �� �̵� �Ǵ� ����
        if (Input.GetMouseButtonDown(0) && !_isMovingToInstall)
        {
            if (canPlace) BeginMoveToInstall(snapped);
            else Debug.LogWarning("��ġ�� �� ���� ��ġ�Դϴ�.");
        }
    }

    // 4) ��ġ ���� ���� (�Ÿ� + OverlapBox + Floor ����)
    bool CanPlaceAt(Vector3 worldPos)
    {
        if (Vector3.Distance(player.position, worldPos) > maxPlaceDistance)
            return false;

        if (Physics.OverlapBox(
                worldPos,
                currentData.size * 0.5f,
                Quaternion.identity,
                blockLayerMask
            ).Length > 0)
            return false;

        if (currentData.type == InstallableType.Floor)
        {
            Vector3[] dirs = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
            foreach (var d in dirs)
            {
                Vector3 nb = worldPos + Vector3.Scale(d, currentData.size);
                if (Physics.CheckBox(nb, currentData.size * 0.5f, Quaternion.identity, blockLayerMask))
                    return true;
            }
            return false;
        }

        return true;
    }

    // 5) ������ ��Ƽ���� ����
    void ApplyPreviewMat(bool valid)
    {
        var mat = valid ? _previewValidMat : _previewInvalidMat;
        foreach (var r in _previewInstance.GetComponentsInChildren<Renderer>())
        {
            var arr = new Material[r.sharedMaterials.Length];
            for (int i = 0; i < arr.Length; i++) arr[i] = mat;
            r.materials = arr;
        }
    }

    // 6) NavMeshAgent �̵� ����
    void BeginMoveToInstall(Vector3 localPos)
    {
        if (!_agent.isOnNavMesh) return;
        _queuedLocalPos = localPos;
        _isMovingToInstall = true;

        Vector3 worldTarget = previewParent.TransformPoint(localPos + Vector3.up * currentData.yOffset);
        Vector3 dir = (worldTarget - player.position).normalized;
        Vector3 stopPt = worldTarget - dir * stopDistance;

        _agent.isStopped = false;
        _agent.SetDestination(stopPt);
    }

    // 7) ���� üũ �� ��ġ �ڷ�ƾ ����
    void CheckArrival()
    {
        if (!_isMovingToInstall) return;
        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            StartCoroutine(DoBuild(_queuedLocalPos));
            _isMovingToInstall = false;
        }
    }

    IEnumerator DoBuild(Vector3 localPos)
    {
        yield return new WaitForSeconds(currentData.buildTime);

        // ���� ��ġ �ν��Ͻ� ����
        Vector3 placePos = previewParent.TransformPoint(localPos + Vector3.up * currentData.yOffset);
        var placed = Instantiate(currentData.prefab, placePos, Quaternion.identity, previewParent);

        // �ݶ��̴� ����
        foreach (var c in placed.GetComponentsInChildren<Collider>())
            c.isTrigger = false;

        // SO.defaultMaterial�� ��Ƽ���� �ϰ� ����
        foreach (var r in placed.GetComponentsInChildren<Renderer>())
        {
            var mats = new Material[r.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++)
                mats[i] = currentData.defaultMaterial;
            r.sharedMaterials = mats;
        }

        // NavMesh ����
        navMeshSurface?.BuildNavMesh();
    }

    void CancelInstallation()
    {
        _agent.isStopped = true;
        _agent.ResetPath();
        _isMovingToInstall = false;
        Debug.Log("��ġ ��ҵ�");
    }
}

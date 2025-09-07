using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TPlayerController : CreatureBase
{
    Rigidbody rb;
    Vector3 playerDir;
    Vector3 lastPosition;
    
    public Vector3 moveInput { get; private set; }
    public float moveThreshold = 0.01f;
    public Collider[] interactableCol;
    public LayerMask interactableLayer;
    
    private Vector3 centerVec;
    private float centerVecY = 1f;
    private RigidbodyConstraints rbConstraints;

    void Awake()
    {
        hp = 100;
    }

    void Start()
    {
        
    }

    /// <summary>
    /// TODO: ���� �ʿ� 
    /// ����� �߸���Ÿ ��ȣ�ۿ��ϸ�, �÷��̾ ���� ������ �� ���� �Ǿ��־
    /// Rigidbody�� �ı��ϴ� ������ �ۼ��ص�����,
    /// ���� ��ȹ�� ����� ���ɼ��� ���� ������ �� �κ��� ���Ѻ��ٰ� �����ϰ� ������ �ʿ䰡 ����
    /// </summary>
    private void OnEnable()
    {
        if (TryGetComponent(out Rigidbody rigidbody))
        {
            rb = rigidbody;
        }
        else
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.constraints = rbConstraints;
        }
        lastPosition = transform.position;
    }

    private void Update()
    {
        Move();

        centerVec = transform.position;
        centerVec.y += centerVecY;
        interactableCol = Physics.OverlapSphere(centerVec, fov.viewRadius, interactableLayer, QueryTriggerInteraction.Ignore);

        if (interactableCol.Length > 0)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                interactableCol[0].TryGetComponent<Ballista>(out var ballista);
                rbConstraints = rb.constraints;
                Destroy(rb);

                foreach (Behaviour component in GetComponents<Behaviour>())
                {
                    if (component is MeshFilter || component is MeshRenderer || component is Transform || component == null)
                        continue;

                    component.enabled = false;
                }
                GetComponent<CapsuleCollider>().enabled = false;
                ballista?.Use(gameObject);
            }
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        Vector3 velocity = new Vector3(moveInput.x * speed, rb.velocity.y, moveInput.z * speed);
        // if (GuiltySystem.instance.IsSlowed) velocity *= 0.5f;

        rb.velocity = velocity;
    }

    void Move()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        moveInput = new Vector3(moveX, 0f, moveZ).normalized;
    }

    bool IsMove()
    {
        float dist = Vector3.Distance(transform.position, lastPosition);
        bool moved = dist > moveThreshold;
        if (moved) lastPosition = transform.position;
        return moved;
    }

    private void OnDrawGizmos()
    {
        foreach (var collider in interactableCol)
        {
            if (collider == null) continue;

            Gizmos.color = Color.green;
            Vector3 center = collider.transform.position;
            center.y = centerVecY;
            Gizmos.DrawSphere(center, 0.5f);
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.AI; // NavMeshAgent�� ������� ������, ������Ʈ�� �ִٸ� �ʿ��մϴ�.

[RequireComponent(typeof(Rigidbody))] // Rigidbody�� ����ϹǷ� ������Ʈ ����
public class TitanAI : EnemyBase, IBegin
{
    private NavMeshAgent agent;


    // AI�� ���� ���¸� ��Ȯ�ϰ� ����
    private enum State
    {
        MovingToTarget,
        Attacking
    }

    private State currentState;
    private Rigidbody rb;
    private GameObject mastObject;

    void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody>();

        // Rigidbody ����: ��ҿ��� ���� ������ ���� �ʵ��� isKinematic = true
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate; // �ε巯�� �������� ����

        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = false;

        SetAttribute();
    }

    void Update()
    {
        // �̵� ������ ���� Ÿ���� �����ϰ� �����Դϴ�.
        if (currentState == State.MovingToTarget)
        {
            fov.DetectTargets(detectMask);
            MoveToTarget();

            // �þ߿� Ÿ���� ������ '����' ���·� ��ȯ�ϰ� ���� �ڷ�ƾ�� '�� ����' ȣ���մϴ�.
            if (fov.visibleTargets.Count > 0)
            {
                Transform targetToAttack = fov.visibleTargets[0];
                StartCoroutine(RushAttackSequence(targetToAttack));
            }
        }
        // ���� ������ ���� �ڷ�ƾ�� ��� ���� ó���ϹǷ� Update������ �ƹ��͵� ���� �ʽ��ϴ�.
    }

    protected override void SetAttribute()
    {
        base.SetAttribute();
        maxHp = 30;
        attackDamage = 20;
        speed = 4;
        attackRange = 4;
        attackDelayTime = 4; // ��ü ���� ��Ÿ��
        mastObject = SetMastTarget();
        currentAttackTarget = mastObject;

        fov.viewRadius = 1f;

        // �ʱ� ���¸� '�̵�'���� ����
        currentState = State.MovingToTarget;
    }

    // ��ֹ��� �����ϰ� ���������� �̵��ϴ� �Լ� (����� ��û��� ����)
    private void MoveToTarget()
    {
        if (currentAttackTarget != null)
        {
            // ��ǥ ��ġ�� y���� ���� y������ ����
            Vector3 targetPosition = new Vector3(
               currentAttackTarget.transform.position.x,
               transform.position.y,
               currentAttackTarget.transform.position.z);

            Vector3 nextPosition = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            // �� �� ���� "NavMesh �ٴ��� ����� �ʰ�" ���ݴϴ�.
            agent.Warp(nextPosition);

            // ȸ�� ������ ����
            Vector3 direction = targetPosition - transform.position;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
            }
        }
    }

    // ������ ��ü ������ ó���ϴ� �ڷ�ƾ
    private IEnumerator RushAttackSequence(Transform targetToAttack)
    {
        // 1. ���¸� '���� ��'���� �ٲ㼭 Update�� �̵� ������ �����ϴ�.
        currentState = State.Attacking;

        Vector3 directionToTarget = (targetToAttack.position - transform.position).normalized;
        directionToTarget.y = 0; // Y�� ȸ�� ����
        transform.rotation = Quaternion.LookRotation(directionToTarget);

        // 2. ���� ������ (����: ���� �� �غ� �ð�)
        Debug.Log("Ÿ�� ����! ������ �غ��մϴ�...");
        yield return new WaitForSeconds(1f);

        // --- ������� ���� ������ ����˴ϴ� ---

        // 3. MoveTowards�� �̿��� ����
        Debug.Log("����!");

        float dashDuration = 0.2f; // ������ �ɸ��� �ð� (ª������ ����)
        float dashSpeed = attackRange / dashDuration; // ��ǥ �Ÿ��� �ð����� ���� ��Ȯ�� �ӵ� ���

        // ���� ���� ��ġ�� ��ǥ ��ġ ���
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = transform.position + transform.forward * attackRange;
        targetPosition.y = transform.position.y; // Y�� ����

        float elapsedTime = 0f;
        while (elapsedTime < dashDuration)
        {
            // ��ǥ ��ġ�� ���� ���� �ӵ��� �̵�
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, dashSpeed * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null; // ���� �����ӱ��� ���
        }

        // ���� ������ ���� �������� ��ġ�� ��Ȯ�� ������
        // transform.position = targetPosition;

        // --- ���� ���� ���� ---

        // 4. ���⿡ ���� �������� �ִ� ���� ���� (��: OverlapSphere�� �ֺ� ����)
        Debug.Log("���� �߻�! " + attackDamage + " ����!");

        // 5. ���� ���� ������ �ð���ŭ ��� (�ĵ�: ���� �� ��Ÿ��)
        // 1�� ���� + 0.3�� ���� �ð��� ������ ������ �ð�
        yield return new WaitForSeconds(attackDelayTime - 1.3f);

        // 6. �ٽ� �̵� ���·� ����
        Debug.Log("���� �Ϸ�. �ٽ� �̵��մϴ�.");
        currentState = State.MovingToTarget;
    }
}
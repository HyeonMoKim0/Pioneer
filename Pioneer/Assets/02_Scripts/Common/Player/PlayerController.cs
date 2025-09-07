using UnityEngine;

// PlayerController: �Է¸� ó���ؼ� �ٸ� ��ũ��Ʈ�� ��� ������
public class PlayerController : MonoBehaviour
{
    private PlayerCore playerCore;
    private PlayerAttack playerAttack;

    void Awake()
    {
        playerCore = GetComponent<PlayerCore>();
        playerAttack = GetComponentInChildren<PlayerAttack>();
    }

    void Update()
    {
        // move
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        playerCore.Move(new Vector3(moveX, 0, moveY));

        if(Input.GetMouseButtonDown(0))
        {
            playerCore.Attack();
        }
    }
}

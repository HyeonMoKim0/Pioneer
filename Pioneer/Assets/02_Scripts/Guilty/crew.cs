using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class crew : MonoBehaviour
{
    public Transform player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //���� ������
        float r = Random.Range(-180f, 181f) * Time.deltaTime;
        //���� ���ϱ�
        transform.Rotate(0, r, 0);
        //�÷��̾� �Ĵٺ���
        transform.LookAt(player);
    }
}

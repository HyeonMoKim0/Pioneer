using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGetNoticeUI : MonoBehaviour
{
    public static ItemGetNoticeUI Instance;

    // ���� 4��¥�� ����Ʈ
    // ����Ʈ ���� => ��Ÿ���� / �������
    // �̹� �� �� -> ���� ���� �������(�ʿ��� ��ŭ��) -> ����Ʈ -> ��Ÿ����

    public GameObject prefab;

    public List<ItemGetNoticeSingleUI> uiList;

    public void Add(SItemStack item)
    {
        //if ()
    }

    private void Awake()
    {
        Instance = this;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

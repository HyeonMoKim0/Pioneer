using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ��� �޼����� �������� �Ű����� ������ ���� Ŭ�����Դϴ�.
// �Ű����� �ʹ� ������ Ŭ������ ����ü�� ������ ���� �ֽ��ϴ�.
[Serializable]
public class ArgumentGeometry
{
    public GameObject parent;
    public int index;
    public int rowCount;
    public Vector2 delta2D;
    public Vector2 start2D;
    public Vector2 size;
}
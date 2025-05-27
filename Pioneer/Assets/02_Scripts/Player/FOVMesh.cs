using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class FOVMesh : MonoBehaviour
{
    [Header("�þ� ���� (Degree)")]
    public float viewAngle = 90f;

    [Header("�þ� �Ÿ�")]
    public float viewRadius = 10f;

    [Header("Ray ���� (Ŭ���� �ε巯��)")]
    public int rayCount = 50;

    [Header("��ֹ� ���̾�")]
    public LayerMask obstacleMask;

    private Mesh mesh;

    private Vector3 origin;

    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        origin = Vector3.zero;
    }

    private void LateUpdate()
    {
        origin = transform.position;
        DrawFieldOfView();
    }

    private void DrawFieldOfView()
    {
        float angleStep = viewAngle / rayCount;
        float startAngle = transform.eulerAngles.y - viewAngle / 2f;

        List<Vector3> viewPoints = new List<Vector3>();

        for (int i = 0; i <= rayCount; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector3 dir = DirFromAngle(angle);

            RaycastHit hit;
            Vector3 point;

            if (Physics.Raycast(origin, dir, out hit, viewRadius, obstacleMask))
            {
                // ��ֹ��� ����� ��
                point = hit.point;
            }
            else
            {
                // ��ֹ��� ������ �ִ� �Ÿ�����
                point = origin + dir * viewRadius;
            }

            viewPoints.Add(point);
        }

        // �޽� ����
        int vertexCount = viewPoints.Count + 1; // +1�� origin
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = transform.InverseTransformPoint(origin); // ���� ��ǥ�� ���� ����

        for (int i = 0; i < viewPoints.Count; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);
        }

        int triIndex = 0;
        for (int i = 0; i < vertexCount - 2; i++)
        {
            triangles[triIndex] = 0;
            triangles[triIndex + 1] = i + 1;
            triangles[triIndex + 2] = i + 2;
            triIndex += 3;
        }

        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    private Vector3 DirFromAngle(float angleInDegrees)
    {
        // Y�� ���� ȸ�� ���� (XZ ���)
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}

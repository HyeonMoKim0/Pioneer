using UnityEngine;

public class UnitBase : MonoBehaviour
{
    void LateUpdate()
    {
        var cam = Camera.main;
        if (!cam) return;

        // ī�޶��� ������ XZ ������� ���� �� Y�ุ ���� ������
        Vector3 fwd = cam.transform.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 1e-6f) return;
        fwd.Normalize();

        transform.rotation = Quaternion.LookRotation(fwd, Vector3.up) * Quaternion.Euler(0f, 0f, 0f);
    }
}

using UnityEngine;


[CreateAssetMenu(fileName = "InstallableObject", menuName = "ScriptableObjects/Installables/InstallableObjects")]
public class SInstallableObjectDataSO : SItemTypeSO
{
	[Header("��ġ ������ �� ����")]
	public GameObject prefab;                  // ��ġ ��� ������
	public Vector3 size = Vector3.one;         // ��ġ ������ Overlap ũ��

    [Header("��� Ȯ��")]
	public float maxHp = 20f;                  // ������
	public float buildTime = 2f;               // ��ġ �ð�
}

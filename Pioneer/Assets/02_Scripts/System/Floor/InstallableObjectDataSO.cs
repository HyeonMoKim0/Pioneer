using UnityEngine;

public enum InstallableType 
{ 
	Floor, 
	Object 
}

[CreateAssetMenu(fileName = "InstallableObject", menuName = "Installables/InstallableObject")]
public class InstallableObjectDataSO : ScriptableObject // ������ Ÿ�� ���, ���� ���� �� ��
{
	public InstallableType type; 
	public GameObject prefab;               
	public Vector3 size = Vector3.one;
	// �ٴ�����, ������Ʈ������ ���� y�� �������� �ٸ� �� ������ ����ϴ� ����
	public float yOffset = 0f;
	static readonly public Color defaultColor = Color.white;
	static readonly public Color validColor = Color.green;
	static readonly public Color invalidColor = Color.red;
}

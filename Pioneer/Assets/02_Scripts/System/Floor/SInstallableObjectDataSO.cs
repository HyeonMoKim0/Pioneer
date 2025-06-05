using UnityEngine;

public enum InstallableType 
{ 
	Floor, 
	Object 
}

[CreateAssetMenu(fileName = "InstallableObject", menuName = "ScriptableObjects/Installables/InstallableObjects")]
public class SInstallableObjectDataSO : SItemTypeSO
{
	[Header("��ġ Ÿ��")]
	public InstallableType type;               // Floor �Ǵ� Object

	[Header("��ġ ������ �� ����")]
	public GameObject prefab;                  // ��ġ ��� ������
	public Vector3 size = Vector3.one;         // ��ġ ������ Overlap ũ��
	public float yOffset = 0f;                 // ��ġ ���� ������
											   // (�ٴ����� ������Ʈ����, ������Ʈ ���̰� ������������� ���� �ٸ� �� ����)
    [Header("��� Ȯ��")]
	public float maxHp = 20f;                  // ������
	public float buildTime = 2f;               // ��ġ �ð�

    [Header("��Ƽ���� ����")]
    public Material defaultMaterial;		   // ���� ��Ƽ������ ���� �ٲٴ� ������ �ϸ�
    public Material previewMaterial;		   // �̹� ��ġ�Ǿ� �ִ� ��Ƽ���� ���� �������� ��ġ ���� ���ο� ���� ���� �ٲ�׿�~
											   // ���� �ٺ���׿�~
}

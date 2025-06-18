using UnityEngine;


[CreateAssetMenu(fileName = "InstallableObject", menuName = "ScriptableObjects/Installables/InstallableObjects")]
public class SInstallableObjectDataSO : SItemTypeSO
{
    public enum CreationType { Platform, Wall, Door, Barricade, CraftingTable, Ballista, Trap, Lantern }

    [Header("��ġ Ÿ��")]
    public CreationType installType;

    [Header("��ġ ������ �� ����")]
	public GameObject prefab;                  // ��ġ ��� ������
	public Vector3 size = Vector3.one;         // ��ġ ������ Overlap ũ��

    [Header("��� Ȯ��")]
	public float maxHp = 20f;                  // ������
	public float buildTime = 2f;               // ��ġ �ð�
}

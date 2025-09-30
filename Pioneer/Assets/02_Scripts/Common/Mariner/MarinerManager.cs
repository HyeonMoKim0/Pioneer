using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarinerManager : MonoBehaviour
{
    public static MarinerManager Instance;

    [Header("���� ����(���� �ʿ�)")]
    public float infectionStartTime = 180f;
    public float infectionInterval = 10f;
    private bool infectionStarted = false;

    private List<MarinerAI> allMariners = new List<MarinerAI>();
    private List<DefenseObject> repairTargets = new List<DefenseObject>();
    private HashSet<int> occupiedSpawners = new HashSet<int>();
    private Dictionary<int, int> repairOccupancy = new Dictionary<int, int>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        UpdateRepairTargets();
    }

    private void Update()
    {
        if (!infectionStarted && GameManager.Instance.currentGameTime >= infectionStartTime)
        {
            infectionStarted = true;
            StartCoroutine(InfectMarinersOneByOne());
        }
    }


    /// <summary>
    /// �¹������� �ϳ��� ������Ű�� �ڷ�ƾ
    /// </summary>
    private IEnumerator InfectMarinersOneByOne()
    {
        Debug.Log("���� ���μ��� ���۵�");

        var marinerQueue = new List<MarinerAI>(allMariners);

        foreach (var mariner in marinerQueue)
        {
            if (mariner != null)
            {
                InfectMariner(mariner);
                yield return new WaitForSeconds(infectionInterval);
            }
        }

        Debug.Log("��� �¹��� ���� �Ϸ�");
    }

    /// <summary>
    /// ���� �¹����� ������Ű�� �Լ�
    /// </summary>
    private void InfectMariner(MarinerAI mariner)
    {
        if (mariner == null) return;

        Debug.Log($"���� �߻�: �¹��� {mariner.marinerId}");

        InfectedMarinerAI infected = mariner.GetComponent<InfectedMarinerAI>();
        if (infected != null)
        {
            infected.enabled = true;
            infected.marinerId = mariner.marinerId;
        }

        mariner.enabled = false;
    }

    /// <summary>
    /// �¹����� ����ϴ� �Լ�
    /// </summary>
    public void RegisterMariner(MarinerAI mariner)
    {
        if (!allMariners.Contains(mariner))
        {
            allMariners.Add(mariner);
        }
    }

    /// <summary>
    /// ���� ��� ����� ������Ʈ�ϴ� �Լ�
    /// </summary>
    public void UpdateRepairTargets()
    {
        repairTargets.Clear();
        DefenseObject[] defenseObjects = FindObjectsOfType<DefenseObject>();

        foreach (var obj in defenseObjects)
        {
            if (obj.currentHP < obj.maxHP * 0.5f)
            {
                repairTargets.Add(obj);
                Debug.Log($"���� ��� �߰�: {obj.name}/ HP: {obj.currentHP}/{obj.maxHP}");
            }
        }
    }

    /// <summary>
    /// ������ �ʿ��� ������Ʈ ����� ��ȯ�ϴ� �Լ�
    /// </summary>
    public List<DefenseObject> GetNeedsRepair()
    {
        List<DefenseObject> needRepair = new List<DefenseObject>();
        foreach (var obj in repairTargets)
        {
            if (obj.currentHP < obj.maxHP * 0.5f)
                needRepair.Add(obj);
        }
        return needRepair;
    }

    /// <summary>
    /// �¹����� ������ �� �ִ��� Ȯ���ϴ� �Լ�
    /// </summary>
    public bool CanMarinerRepair(int marinerId, DefenseObject target)
    {
        return true;
    }

    /// <summary>
    /// �¹����� �������� �����ϰ� ���ҷ� ���ư��� �Լ�
    /// </summary>
    public void StoreItemsAndReturnToBase(MarinerAI mariner)
    {
        Debug.Log($"�¹��� [{mariner.marinerId}] ������ ���� �� ���� ����");

        if (HasStorage())
        {
            Vector3 dormPosition = new Vector3(0f, 0f, 0f); // ����
            mariner.StartCoroutine(mariner.MoveToThenReset(dormPosition));
        }
        else
        {
            Debug.Log("������ ���� ");
            mariner.StartCoroutine(mariner.StartSecondPriorityAction());
        }
    }

    /// <summary>
    /// ����Ұ� �ִ��� Ȯ���ϴ� �Լ�
    /// </summary>
    public bool HasStorage()
    {
        return true;
    }

   /* /// <summary>
    /// �����ʰ� �����Ǿ� �ִ��� Ȯ���ϴ� �Լ�
    /// </summary>
    public bool IsSpawnerOccupied(int index)
    {
        return occupiedSpawners.Contains(index);
    }

    /// <summary>
    /// �����ʸ� �����ϴ� �Լ�
    /// </summary>
    public void OccupySpawner(int index)
    {
        occupiedSpawners.Add(index);
    }

    /// <summary>
    /// ������ ������ �����ϴ� �Լ�
    /// </summary>
    public void ReleaseSpawner(int index)
    {
        occupiedSpawners.Remove(index);
    }*/

    /// <summary>
    /// ���� ������Ʈ�� �����Ǿ� �ִ��� Ȯ���ϴ� �Լ�
    /// </summary>
    public bool IsRepairObjectOccupied(DefenseObject obj)
    {
        return repairOccupancy.ContainsKey(obj.GetInstanceID());
    }

    /// <summary>
    /// ���� ������Ʈ ������ �õ��ϴ� �Լ�
    /// </summary>
    public bool TryOccupyRepairObject(DefenseObject obj, int marinerId)
    {
        int id = obj.GetInstanceID();
        if (!repairOccupancy.ContainsKey(id))
        {
            repairOccupancy[id] = marinerId;
            return true;
        }
        return false;
    }

    /// <summary>
    /// ���� ������Ʈ ������ �����ϴ� �Լ�
    /// </summary>
    public void ReleaseRepairObject(DefenseObject obj)
    {
        int id = obj.GetInstanceID();
        if (repairOccupancy.ContainsKey(id))
            repairOccupancy.Remove(id);
    }
}

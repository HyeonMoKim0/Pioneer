using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

// ��� ���� Ui�� ���⼭ �ذ��մϴ�.
// ������ ������ �ش� ������Ʈ�� �����ؼ� �����մϴ�.
public class InGameUI : MonoBehaviour, IBegin
{
    static public InGameUI instance;

    [Header("���� UI ���ӿ�����Ʈ")]
    public GameObject gameObjectBarChart;
    public GameObject gameObjectGuiltyBarChart;
    public GameObject gameObjectBuffEffect;
    public GameObject gameObjectItemGet;
    public GameObject gameObjectClock;
    public GameObject gameObjectRepair;
    public GameObject defaultCraftUI;
    public GameObject defaultCraftUiSubPivot;
    public GameObject makeshiftCraftUI;
    public GameObject gameObjectStatus;
    public GameObject gameObjectInventory;
    public GameObject ManuUI;
    public GameObject ManuDenyUI;
    [Header("���� UI ���� Ŭ����")]
    public CraftUiMain mainCraft;
    public MakeshiftCraftUiMain makeshiftCraft;
    public DefaultFabrication currentFabricationUi;

    public List<GameObject> currentOpenedUI = new List<GameObject>();
    private List<GameObject> mainCraftSelectUi;

    //Coroutine coroutineDenyESC = null;
    float denyUiEndTime = 0.0f;
    float denyUiLifeTime = 2.0f;
    bool isCraftButtonExist = false;

    private void Awake()
    {
        instance = this;
        mainCraftSelectUi = new List<GameObject>();

    }

    // Start is called before the first frame update
    void Start()
    {
        Show(makeshiftCraftUI);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UseESC();
        }
        
        if (Time.time < denyUiEndTime)
        {
            ManuDenyUI.SetActive(true);
        }
        else
        {
            ManuDenyUI.SetActive(false);
        }
    }

    public void ShowDefaultCraftUI()
    {
        CommonUI.instance.CloseTab(mainCraft.ui);
        Clear();
        // ���⼭ ����

        if (isCraftButtonExist == false)
        {
            isCraftButtonExist = true;
            for (int index = 0; index < ItemCategoryManager.Instance.categories.Count; ++index)
            {
                ArgumentGeometry geometryCategoryButton = new ArgumentGeometry()
                {
                    parent = defaultCraftUiSubPivot,
                    index = index,
                    rowCount = 1,
                    delta2D = new Vector2(0, -100),
                    start2D = new Vector2(0, 0),
                    size = new Vector2(100, 100)
                };
                ArgumentGeometry geometryCraftSelectCategory = new ArgumentGeometry()
                {
                    parent = defaultCraftUI,
                    index = 0,
                    rowCount = 1,
                    delta2D = new Vector2(0, 100),
                    start2D = new Vector2(-600, 300)
                };
                ArgumentGeometry geometryItemSelectButton = new ArgumentGeometry()
                {
                    parent = defaultCraftUI,
                    index = -1,
                    rowCount = 1,
                    delta2D = new Vector2(0, -100),
                    start2D = new Vector2(-600, 225),
                };

                Debug.Assert(defaultCraftUiSubPivot != null);
                Debug.Assert(ItemCategoryManager.Instance != null);
                Debug.Assert(ItemCategoryManager.Instance.categories != null);
                Debug.Assert(ItemCategoryManager.Instance.categories[index] != null);
                Debug.Assert(mainCraft != null);
                Debug.Assert(mainCraft.ui != null);
                Debug.Assert(geometryCategoryButton != null);
                Debug.Assert(geometryItemSelectButton != null);
                CommonUI.instance.ShowCategoryButton(
                    defaultCraftUiSubPivot,
                    ItemCategoryManager.Instance.categories[index],
                    mainCraft.ui,
                    geometryCategoryButton,
                    geometryCraftSelectCategory,
                    geometryItemSelectButton,
                    mainCraftSelectUi);
            }




            //CommonUI.instance.ShowCategoryButton(defaultCraftUI, );

        }

        Show(defaultCraftUI);
        currentFabricationUi = mainCraft.ui;
    }

    public void CloseDefaultCraftUI()
    {
        CommonUI.instance.CloseTab(makeshiftCraft.ui);
        Clear();
        Show(makeshiftCraftUI);
        InventoryUiMain.instance.IconRefresh();
        currentFabricationUi = makeshiftCraft.ui;
    }

    public void Show(GameObject UiGo)
    {
        UiGo.SetActive(true);
        currentOpenedUI.Add(UiGo);
    }

    public void Clear()
    {
        foreach (GameObject go in currentOpenedUI)
        {
            go.SetActive(false);
        }
    }

    public void UseESC()
    {
        if (defaultCraftUI.activeInHierarchy)
        {
        
            CloseDefaultCraftUI();
            return;
        }

        if (ManuUI.activeInHierarchy)
        {
            ManuUI.SetActive(false);
        }
        else
        {
            if (GuiltySystem.instance.canUseESC)
            {
                ManuUI.SetActive(true);
            }
            else
            {
                denyUiEndTime = Time.time + denyUiLifeTime;
            }
        }
    }
}

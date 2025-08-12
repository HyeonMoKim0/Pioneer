using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUiMain : MonoBehaviour, IBegin
{
    static public InventoryUiMain instance;
    
    public List<ItemSlotUI> currentSelectedSlot;
    [SerializeField] ItemSlotUI mouseUI;
    [SerializeField] List<GameObject> slotGameObjects;
    [SerializeField] List<GameObject> inventorySlot;
    [SerializeField] List<GameObject> quickSlot;
    [SerializeField] GameObject imageMouseHoldingItem;
    [SerializeField] GameObject windowMouse;
    [SerializeField] Canvas canvas;
    [SerializeField] TextMeshProUGUI windowMouseText;
    [SerializeField] Button trashButton;
    [SerializeField] Sprite trashOpen;
    [SerializeField] Sprite trashClose;
    RectTransform followUiRect1;
    RectTransform followUiRect2;
    ItemSlotUI[] itemSlotUIs;

    public void InventoryExpand(bool value)
    {
        foreach (GameObject i in inventorySlot)
        {
            i.SetActive(value);
        }
    }

    public void HideWindow()
    {
        windowMouse.SetActive(false);
    }

    public void ShowWindow()
    {
        if (currentSelectedSlot.Count == 0)
        {
            windowMouse.SetActive(false);
            return;
        }

        windowMouse.SetActive(true);
        SItemStack mItemStack = InventoryManager.Instance.itemLists[currentSelectedSlot[0].index];

        if (mItemStack == null || mItemStack.id == 0)
        {
            windowMouse.SetActive(false);
            return;
        }

        //Debug.Log($">> ������ ���� : {currentSelectedSlot[0].index} / {mItemStack.id} {mItemStack.amount}");

        windowMouseText.text = GetInfomation(mItemStack);
    }

    public void ClickSlot(int index)
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            InventoryManager.Instance.MouseSplit(index);
        }
        else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            InventoryManager.Instance.MouseSingle(index);
        }
        else
        {
            InventoryManager.Instance.MouseSwitch(index);
        }

        mouseUI.Show(InventoryManager.Instance.mouseInventory);
        itemSlotUIs[index].Show(InventoryManager.Instance.itemLists[index]);
    }
    public void ClickOut()
    {
        if (SItemStack.IsEmpty(InventoryManager.Instance.mouseInventory))
        {
            Debug.Log($">> InventoryUiMain.ClickOut() : �������� ��� �ֽ��ϴ�.");
            return;
        }

        Debug.Log($">> {gameObject.name} -> InventoryUiMain.ClickOut() : ������ ��� {InventoryManager.Instance.mouseInventory.id} / {InventoryManager.Instance.mouseInventory.amount}");
        Debug.Log($">> {gameObject.name} -> InventoryUiMain.ClickOut() : ������ ���1");
        InventoryManager.Instance.MouseDrop();
        Debug.Log($">> {gameObject.name} -> InventoryUiMain.ClickOut() : ������ ���2");
        mouseUI.Clear();
        Debug.Log($">> {gameObject.name} -> InventoryUiMain.ClickOut() : ������ ���3");

        IconRefresh();
    }

    public void Sort()
    {
        InventoryManager.Instance.SortSelf();
        IconRefresh();
    }
    public void Remove()
    {
        InventoryManager.Instance.RemoveMouseItem();
        mouseUI.Clear();
    }

    private void Awake()
    {
        instance = this;

        itemSlotUIs = new ItemSlotUI[slotGameObjects.Count];
        for (int index = 0; index < slotGameObjects.Count; ++index)
        {
            itemSlotUIs[index] = slotGameObjects[index].GetComponent<ItemSlotUI>();
        }
        currentSelectedSlot = new List<ItemSlotUI>();
    }

    // Start is called before the first frame update
    public void Init()
    //void Start()
    {
        followUiRect1 = imageMouseHoldingItem.GetComponent<RectTransform>();
        followUiRect2 = windowMouse.GetComponent<RectTransform>();

        IconRefresh();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out mMousePos
        );

        followUiRect1.anchoredPosition = mMousePos;
        followUiRect2.anchoredPosition = mMousePos + new Vector2(50, 50);

        ShowWindow();
    }

    string GetInfomation(SItemStack target)
    {
        SItemTypeSO info = ItemTypeManager.Instance.itemTypeSearch[target.id];

        string categoriesName = "";
        switch (info.categories)
        {
            case EDataType.CommonResource: categoriesName = "���� �ڿ�"; break;
            case EDataType.WeaponItem: categoriesName = "���� ������"; break;
            case EDataType.NormalItem: categoriesName = "�Ϲ� ������"; break;
            case EDataType.ConsumeItem: categoriesName = "�Ҹ� ������"; break;
            case EDataType.BuildObject: categoriesName = "��ġ�� ������Ʈ"; break;
            case EDataType.Recipe: categoriesName = "���� ������"; break;
            case EDataType.Unit: categoriesName = "����"; break;
            default: break;
        }

        return $"{info.typeName}\n{categoriesName}\n{info.infomation}";
    }

    public void IconRefresh()
    {
        for (int index = 0; index < slotGameObjects.Count; ++index)
        {
            //if (InventoryManager.Instance.itemLists[index] == null) continue;

            slotGameObjects[index].GetComponent<ItemSlotUI>().Show(
                InventoryManager.Instance.itemLists[index]);
        }
        mouseUI.Show(InventoryManager.Instance.mouseInventory);
    }
}

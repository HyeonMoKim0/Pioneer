using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     �ش� Ŭ������ �������� ������ �ۺ� ���� �޼���� �ٲ���� ���Դϴ�. �ڵ� �ݺ��� ���ϱ� ���� Ŭ�����Դϴ�.
/// </summary>
public class CommonUI : MonoBehaviour, IBegin
{
    public static CommonUI instance;

    [SerializeField] GameObject prefabItemButton;
    [SerializeField] GameObject prefabCraftSelectTopButton;
    [SerializeField] GameObject prefabCraftSelectItemButton;
    [SerializeField] GameObject prefabItemCategoryButton;
    [SerializeField] Sprite imageEmpty;
    [SerializeField] bool isDebugging;
    Coroutine currentCraftCoroutine;
    SItemRecipeSO currentRecipe;

    bool m_sCurrentCrafting = false;
    public bool IsCurrentCrafting
    {
        get
        {
            return m_sCurrentCrafting;
        }
        private set
        {
            if (isDebugging)
            {
                Debug.Log($">> CommonUI.IsCurrentCrafting.set : m_sCurrentCrafting : {m_sCurrentCrafting} -> {value}");
            }
            m_sCurrentCrafting = value;
        }
    }

    // ������ ���ϸ� �������� ���������� ������ �� �ִ��� �ƴ����� �������°��� �Ȱ��ٰ� ��
    // - ������ �� �ִ°�? -> ������ ������ �Ŵ���
    // - ���� â ����

    // ������ ���� â�� �������ݴϴ�
    // DefaultFabrication ui : ���� â ���ӿ�����Ʈ�� ������Ʈ �Դϴ�.
    // SItemRecipeSO recipe : �����Ϸ��� �������Դϴ�.
    // InventoryBase inventory : ������ �κ��丮�� ������� ���� �������Դϴ�. �Ϲ������� �÷��̾��� �κ��丮�� ���پ��ϴ�
    // GameObject[] outsideGameObjectCraftButtonsWithImage : �̹����� ������ �ִ� ���ӿ�����Ʈ�� ����̸�, �ش� ���ӿ�����Ʈ�� �������� ���� �� �ִ��� �ƴ��� ���θ� �����ֱ� �����Դϴ�. �� ����� �������ϰ� �ؾ� �ϰŵ��
    public void UpdateCraftWindowUi(DefaultFabrication ui, SItemRecipeSO recipe, InventoryBase inventory, GameObject[] outsideGameObjectCraftButtonsWithImage)
    {
        currentRecipe = recipe;
        SItemTypeSO recipeResultType = ItemTypeManager.Instance.itemTypeSearch[recipe.result.id];
        ui.craftName.text = recipeResultType.typeName;
        ui.craftLore.text = recipeResultType.infomation;

        for (int rIndex = 0; rIndex < 3; rIndex++)
        {
            ui.materialPivots[rIndex].SetActive(false);
            //ui.materialEachText[rIndex].text = "";
            ui.materialEachText[rIndex].enabled = false;
            //ui.materialIconImage[rIndex].sprite = instance.imageEmpty;
            ui.materialIconImage[rIndex].enabled = false;
        }

        Vector3 mPositionPivot = Vector3.zero;
        switch (recipe.input.Length)
        {
            case 1: mPositionPivot = new Vector3(0, 100, 0); break;
            case 2: mPositionPivot = new Vector3(-112.5f, 100, 0); break;
            case 3: mPositionPivot = new Vector3(-225f, 100, 0); break;
            default: break;
        }

        Vector3 delta = new Vector3(225, 0, 0);
        for (int rIndex = 0; rIndex < recipe.input.Length; rIndex++)
        {
            ui.materialPivots[rIndex].SetActive(true);
            ui.materialPivots[rIndex].GetComponent<RectTransform>().anchoredPosition
                = mPositionPivot + rIndex * delta;

            ui.materialEachText[rIndex].enabled = true;
            ui.materialIconImage[rIndex].enabled = true;

            int need = recipe.input[rIndex].amount;
            int has = inventory.Get(recipe.input[rIndex].id);

            ui.materialEachText[rIndex].text = $"{has}/{need}";
            ui.materialIconImage[rIndex].sprite = ItemTypeManager.Instance.itemTypeSearch[recipe.input[rIndex].id].image;
        }

        mSetButtonAvailable(ui.craftButton.gameObject.GetComponent<UnityEngine.UI.Image>(), recipe);

        // ���� �ð� ǥ��
        ui.timeLeft.text = $"{recipe.time}s";
        ui.craftButtonWord.text = DefaultFabrication.CraftStart;

        // ũ����Ʈ ��ư ���� ��ġ
        ui.craftButton.onClick.RemoveAllListeners();
        ui.craftButton.onClick.AddListener(() =>
        {
            if (ItemRecipeManager.Instance.CanCraftInInventory(recipe.result.id) == false) return;
            if (currentCraftCoroutine != null)
            {
                StopCoroutine(currentCraftCoroutine);
            }

            // ���� �ð� Ÿ�� ������ + ���� �Ϸ� �� �� ������ �� �ִ��� ������Ʈ
            // �ٸ� �Ǽ� �������ΰ�� �ٸ� ������ ����
            // Debug.Log($">> CommonUI.UpdateCraftWindowUi(DefaultFabrication ui, SItemRecipeSO recipe, InventoryBase inventory, GameObject[] outsideGameObjectCraftButtonsWithImage) : IsCurrentCrafting = {IsCurrentCrafting}");
            if (IsCurrentCrafting)
            {
                StopCraft(ui);
                ui.timeLeft.text = $"{recipe.time}s";
            }
            else if (recipe.resultBuildingOrNull == null)
            {
                currentCraftCoroutine = StartCoroutine(CraftCoroutine(recipe, outsideGameObjectCraftButtonsWithImage, ui));
            }
            // �ǹ� ������ ���
            else
            {
                // ���� â�� ����
                // ��� ��ġ�� �̵�
                // �ð� �Ҹ�
                // ���� ������ ��� ����

                // ���⼭ ���๰ ����
                CreateObject.instance.EnterInstallMode(recipe.resultBuildingOrNull);


            }
        });
    }

#warning TODO : ������ ��ġ ����
    // ī�װ� UI
    // GameObject parent : ��ư���� �θ� ���ӿ�����Ʈ�Դϴ�
    // SItemCategorySO category : ī�װ� ��ũ���ͺ� ������Ʈ�Դϴ�
    // DefaultFabrication ui : ���� â ���ӿ�����Ʈ�� ������Ʈ �Դϴ�.
    // ArgumentGeometry geometryCategoryButton : ī�װ� ��ư�� �������� ��ġ ����� ���� �Ű������Դϴ�
    // ArgumentGeometry geometryCraftSelectCategory, : ���� ������ ī�װ� �׸��� �������� ��ġ ����� ���� �Ű������Դϴ�
    // ArgumentGeometry geometryCraftSelectButton : ���� ���� ��ư�� �������� ��ġ ����� ���� �Ű������Դϴ�
    // List<GameObject> prevCraftSelectButton : ���� ���� ���� UI�� ����� ���� �Ű������Դϴ�. �ش� ������ ���Ӱ� ������� ���� ���� ���ӿ�����Ʈ���� ���ҷ� ���ɴϴ�
    public Button ShowCategoryButton(GameObject parent, SItemCategorySO category, DefaultFabrication ui,
        ArgumentGeometry geometryCategoryButton,
        ArgumentGeometry geometryCraftSelectCategory,
        ArgumentGeometry geometryCraftSelectButton,
        List<GameObject> prevCraftSelectButton)
    {
        // 1. ī�װ� �̹��� ��ư

        // �����Ǵ� ���ٴٵ�Ŵ��� ������ �ش� �׸��� ��� ī�װ��� �����Ǹ� ������
        // ��ư�� ������, ShowItemButton�� ������ ȣ����

        // ��ư ��ġ
        GameObject categoryButtonObject = Instantiate(prefabItemCategoryButton, parent.transform);
        RectTransform rectTransform = categoryButtonObject.GetComponent<RectTransform>();
        //rectTransform.sizeDelta = size;
        SetPosition(
            categoryButtonObject,
            geometryCategoryButton.parent,
            geometryCategoryButton.index,
            geometryCategoryButton.rowCount,
            geometryCategoryButton.delta2D,
            geometryCategoryButton.start2D);
        rectTransform.sizeDelta = geometryCategoryButton.size;
        // ��ư �̹��� ��ġ
        categoryButtonObject.GetComponent<UnityEngine.UI.Image>().sprite = category.categorySprite;

        // ��ư ���� ��ġ
        Button categoryButton = categoryButtonObject.GetComponent<Button>();
        categoryButton.onClick.AddListener(() =>
        {
            // 2. ���� ���� ��ư��
            // �ش� ��ư�� ������ ���� ���� UI�� ��
            // ���� ���� ������ �� ������
            foreach (GameObject prevUi in prevCraftSelectButton) Destroy(prevUi);
            ui.gameObject.SetActive(false);

            // ���� ���� ī�װ� �׸�
            GameObject craftSelectCategory = Instantiate(prefabCraftSelectTopButton, parent.transform);
            craftSelectCategory.transform.parent = geometryCraftSelectCategory.parent.transform;
            craftSelectCategory.transform.localPosition = geometryCraftSelectCategory.start2D;
            prevCraftSelectButton.Add(craftSelectCategory);
            CraftItemSelectTop craftSelectCategoryUi = craftSelectCategory.GetComponent<CraftItemSelectTop>();
            craftSelectCategoryUi.categoryImage.sprite = category.categorySprite;
            craftSelectCategoryUi.categoryName.text = category.categoryName;

            // ���� ���� ��ư ��ȯ
            for (int index = 0; index < category.recipes.Count; index++)
            {
                GameObject m_one = Instantiate(prefabCraftSelectItemButton, parent.transform);

                prevCraftSelectButton.Add(m_one);
                // ������ ��������
                SItemRecipeSO recipe = category.recipes[index];
                SItemTypeSO recipeResultType = ItemTypeManager.Instance.itemTypeSearch[recipe.result.id];
                // ��ư ��ġ
                SetPosition(
                    m_one,
                    geometryCraftSelectButton.parent,
                    index,
                    1,
                    -new Vector2(0, m_one.GetComponent<RectTransform>().sizeDelta.y),
                    geometryCraftSelectButton.start2D);
                CraftItemSelectSingle m_oneUi = m_one.GetComponent<CraftItemSelectSingle>();

                m_oneUi.image.sprite = ItemTypeManager.Instance.itemTypeSearch[category.recipes[index].result.id].image;
                m_oneUi.itemName.text = ItemTypeManager.Instance.itemTypeSearch[category.recipes[index].result.id].typeName;

                // ��ư ���� ��ġ
                //Button craftSelectItemButtons = categoryButtonObject.GetComponent<Button>();
                m_oneUi.button.onClick.AddListener(() =>
                {
                    ui.gameObject.SetActive(true);
                    UpdateCraftWindowUi(ui, recipe, InventoryManager.Instance, new GameObject[] { m_one });
                });
            }

        });
        return categoryButton;
    }

    // ���� ���� UI
    public Button ShowItemButton(GameObject parent, SItemRecipeSO recipe, DefaultFabrication ui,
        int index, int rowCount, Vector2 delta, Vector2 start, Vector2 size)
    {
        SItemTypeSO recipeResultType = ItemTypeManager.Instance.itemTypeSearch[recipe.result.id];

        // ��ư ��ġ
        GameObject itemButtonGameObject = Instantiate(instance.prefabItemButton, parent.transform);
        RectTransform rectTransform = itemButtonGameObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = size;
        SetPosition(itemButtonGameObject, parent, index, rowCount, delta, start);

        // ��ư ���뼺 ǥ��
        mSetButtonAvailable(itemButtonGameObject.GetComponent<UnityEngine.UI.Image>(), recipe);

        // ��ư �̹��� ��ġ
        itemButtonGameObject.GetComponent<UnityEngine.UI.Image>().sprite =
            ItemTypeManager.Instance.itemTypeSearch[recipe.result.id].image;

        // ��ư ���� ��ġ
        Debug.Assert(itemButtonGameObject != null);
        Debug.Assert(itemButtonGameObject.GetComponent<Button>() != null);
        Button itemButton = itemButtonGameObject.GetComponent<Button>();


        itemButton.onClick.AddListener(() => // ��ư Ŭ�� ��
        {
            ui.gameObject.SetActive(true);
            UpdateCraftWindowUi(ui, recipe, InventoryManager.Instance, new GameObject[] { itemButtonGameObject });
        });
        return itemButton;
    }




    // ������ ��ư





    public Button ShowSelectButton()
    {
        return null;
    }

    private static void mSetButtonAvailable(Image buttonImage, SItemRecipeSO pRecipe)
    {
        UnityEngine.Color buttonColor = buttonImage.color;
        if (ItemRecipeManager.Instance.CanCraftInInventory(pRecipe.result.id))
        {
            buttonColor.a = 1.0f;
        }
        else
        {
            buttonColor.a = 0.5f;
        }
        buttonImage.color = buttonColor;
    }

    private static void SetPosition(GameObject target, GameObject parent, int index, int rowCount, Vector2 delta, Vector2 start)
    {
        int xPos = index % rowCount;
        int yPos = index / rowCount;

        target.transform.position = parent.transform.position + new Vector3(start.x, start.y, 0.0f) + new Vector3(delta.x * xPos, delta.y * yPos);
    }

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator CraftCoroutine(SItemRecipeSO recipe, GameObject[] itemButtonGameObject, DefaultFabrication ui)
    {
        // �Է� �ð���ŭ ����
        // ������ ����
        IsCurrentCrafting = true;
        float leftTime = recipe.time;
        ui.craftButtonWord.text = DefaultFabrication.CraftEnd;

        while (leftTime > 0.0f)
        {
            ui.timeLeft.text = $"{leftTime}s";
            leftTime -= Time.deltaTime;
            yield return null;
        }
        Craft(recipe, itemButtonGameObject, ui);
        ui.timeLeft.text = $"���� �Ϸ�";
        ui.craftButtonWord.text = DefaultFabrication.CraftStart;
        InventoryUiMain.instance.IconRefresh();
        IsCurrentCrafting = false;
        // ��⿡ ����ġ �߰� ����
        PlayerStatsLevel.Instance.AddExp(GrowStatType.Crafting, currentRecipe.exp);
    }

    public void Craft(SItemRecipeSO recipe, GameObject[] itemButtonGameObject, DefaultFabrication ui)
    {
        InventoryManager.Instance.Add(recipe.result);
        InventoryManager.Instance.Remove(recipe.input);

        for (int rIndex = 0; rIndex < recipe.input.Length; rIndex++)
        {
            int need = recipe.input[rIndex].amount;
            int has = InventoryManager.Instance.Get(recipe.input[rIndex].id);

            ui.materialEachText[rIndex].text = $"{has}/{need}";
        }

        for (int buttonIndex = 0; buttonIndex < itemButtonGameObject.Length; buttonIndex++)
        {
            mSetButtonAvailable(itemButtonGameObject[buttonIndex].GetComponent<UnityEngine.UI.Image>(), recipe);
        }
        mSetButtonAvailable(ui.craftButton.gameObject.GetComponent<UnityEngine.UI.Image>(), recipe);
    }

    public void StopCraft(DefaultFabrication ui)
    {
        StopCoroutine(currentCraftCoroutine);
        currentCraftCoroutine = null;
        IsCurrentCrafting = false;
        ui.craftButtonWord.text = DefaultFabrication.CraftStart;
        ui.timeLeft.text = $"{currentRecipe.time}s";
    }

    public void CloseTab(DefaultFabrication ui)
    {
        if(IsCurrentCrafting) StopCraft(ui);
        ui.gameObject.SetActive(false);
    }
}

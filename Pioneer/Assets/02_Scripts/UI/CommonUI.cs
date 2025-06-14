using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class CommonUI : MonoBehaviour
{
    public static CommonUI instance;

    [SerializeField] GameObject prefabItemButton;
    [SerializeField] Sprite imageEmpty;

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
        itemButton.onClick.AddListener(() =>
        {
            // ��� �����ִ� ����
            ui.craftName.text = recipeResultType.typeName;
            ui.craftLore.text = recipeResultType.infomation;

            for (int rIndex = 0; rIndex < 3; rIndex++)
            {
                //ui.materialEachText[rIndex].text = "";
                ui.materialEachText[rIndex].enabled = false;
                //ui.materialIconImage[rIndex].sprite = instance.imageEmpty;
                ui.materialIconImage[rIndex].enabled = false;
            }




            for (int rIndex = 0; rIndex < recipe.input.Length; rIndex++)
            {
                ui.materialEachText[rIndex].enabled = true;
                ui.materialIconImage[rIndex].enabled = true;

                int need = recipe.input[rIndex].amount;
                int has = InventoryManager.Instance.Get(recipe.input[rIndex].id);

                ui.materialEachText[rIndex].text = $"{has}/{need}";
                ui.materialIconImage[rIndex].sprite = ItemTypeManager.Instance.itemTypeSearch[recipe.input[rIndex].id].image;
            }

            mSetButtonAvailable(ui.craftButton.gameObject.GetComponent<UnityEngine.UI.Image>(), recipe);

            // ũ����Ʈ ��ư ���� ��ġ
            ui.craftButton.onClick.RemoveAllListeners();
            ui.craftButton.onClick.AddListener(() =>
            {
                if (ItemRecipeManager.Instance.CanCraftInInventory(recipe.result.id) == false) return;

                InventoryManager.Instance.Add(recipe.result);
                InventoryManager.Instance.Remove(recipe.input);

                for (int rIndex = 0; rIndex < recipe.input.Length; rIndex++)
                {
                    int need = recipe.input[rIndex].amount;
                    int has = InventoryManager.Instance.Get(recipe.input[rIndex].id);

                    ui.materialEachText[rIndex].text = $"{has}/{need}";
                }

                mSetButtonAvailable(itemButtonGameObject.GetComponent<UnityEngine.UI.Image>(), recipe);
                mSetButtonAvailable(ui.craftButton.gameObject.GetComponent<UnityEngine.UI.Image>(), recipe);
            });
        });
        return itemButton;
    }

    private static void mSetButtonAvailable(Image buttonImage, SItemRecipeSO pRecipe)
    {
        Color buttonColor = buttonImage.color;
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
}

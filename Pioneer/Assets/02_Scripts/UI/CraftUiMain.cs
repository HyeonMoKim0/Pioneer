using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CraftUiMain : MonoBehaviour
{
    public static CraftUiMain instance;

    [Header("prefab")]
    public GameObject prefabCraftItemButton;

    [Header("UI")]
    public UnityEngine.UI.Button rightCraftButton;
    public UnityEngine.UI.Image material1iconImage;
    public UnityEngine.UI.Image material2iconImage;
    public UnityEngine.UI.Image material3iconImage;
    public TextMeshProUGUI craftName;
    public TextMeshProUGUI material1eaText;
    public TextMeshProUGUI material2eaText;
    public TextMeshProUGUI material3eaText;
    public TextMeshProUGUI craftLore;
    public GameObject pivotItem;
    public Vector3 startPos;
    public float xTerm;
    public float yTerm;

    private SItemRecipe currentSelectedRecipe;
    private TextMeshProUGUI[] materialEachText;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        //textMeshProUGUI.tex

        ItemRender(); // �ܺ� ������Ʈ�� �����ϹǷ� �ݵ�� �����ũ�� �ƴ� ��Ÿ�忡 �־�� �մϴ�.
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ItemRender()
    {
        void mButtonAvailable(GameObject target, SItemRecipe pRecipe)
        {
            Image buttonImage = target.GetComponent<Image>();
            Color buttonColor = buttonImage.color;
            if (ItemRecipeManager.Instance.CanCraftInInventory(pRecipe.result.id))
            {
                buttonColor.a = 0.5f;
            }
            else
            {
                buttonColor.a = 1.0f;
            }
            buttonImage.color = buttonColor;
        }


        // �������� ������.
        for (int index = 0; index < ItemRecipeManager.Instance.recipes.Count; ++index)
        {
            SItemRecipe recipe = ItemRecipeManager.Instance.recipes[index];
            currentSelectedRecipe = recipe;
            SItemType recipeResult = ItemTypeManager.Instance.itemTypeSearch[recipe.result.id];

            int xPos = index % 4;
            int yPos = index / 4;

            GameObject buttonObject = Instantiate(prefabCraftItemButton, pivotItem.transform);
            buttonObject.transform.position = startPos + new Vector3(xTerm * xPos, yTerm * yPos);

            UnityEngine.UI.Button button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(() => // ���� ������ �������� ������ �� �����ֱ�.
            {
                // �����Ǹ� �����ִ� ���ٽ�

                // ��� �����ֱ�
                craftName.text = recipeResult.name;
                craftLore.text = recipeResult.infomation;

                // ������ �����ֱ�
                for (int rIndex = 0; rIndex < 3; ++rIndex)
                {
                    materialEachText[rIndex].text = "";
                }
                for (int rIndex = 0; rIndex < recipe.input.Length; ++rIndex)
                {
                    int need = recipe.input[rIndex].amount;
                    int has = InventoryManager.Instance.Get(recipe.input[rIndex].id);

                    materialEachText[rIndex].text = $"{need}/{has}";
                }
#warning ������ ������ ��� �̹��� �����ִ� ���


                // ���� ũ������ ��ư �۾�
#warning ���� ��ư �۾� ������ ��
                // ��ư�� ������ ��, ������ ���� �������� �Ǵ�
                // �׵� ������ ���� �� ����
                rightCraftButton.onClick.AddListener(() =>
                {
                    
                });



            });

            // ������ ���� �������� �Ǵ�
            mButtonAvailable(buttonObject, recipe);

        }



    }

}

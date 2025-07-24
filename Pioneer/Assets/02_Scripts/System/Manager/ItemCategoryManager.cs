using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCategoryManager : MonoBehaviour, IBegin
{
    public static ItemCategoryManager Instance;

    public List<SItemCategorySO> categories;
    public Dictionary<int, SItemCategorySO> itemCategoriesSearchInt;
    public Dictionary<ETypes, SItemCategorySO> itemCategoriesSearchEnum;

    // ����� ������ �ʰ�, �������� �׽�Ʈ�� �ӽ� �Լ��� ���� ī�װ��� �ְ� ���� ��� �� �Լ��� ����Ͻÿ�.
    private void Add(SItemCategorySO category)
    {
        categories.Add(category);
        itemCategoriesSearchInt.Add(category.typeInt, category);
        itemCategoriesSearchEnum.Add(category.categoryType, category);
    }

    private void Awake()
    {
        Instance = this;

        //types = new List<SItemTypeSO>();
        itemCategoriesSearchInt = new Dictionary<int, SItemCategorySO>();
        itemCategoriesSearchEnum = new Dictionary<ETypes, SItemCategorySO>();
        InspectorRegister();

        //Demo();
    }

    // Start is called before the first frame update
    private void Init()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void InspectorRegister()
    {
        foreach (SItemCategorySO one in categories)
        {
            itemCategoriesSearchInt.Add(one.typeInt, one);
            itemCategoriesSearchEnum.Add(one.categoryType, one);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Finalfactory.Tagger;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    public GameObject ButtonPrefab;
    public GameObject ItemPrefab;
    public Sprite[] Sprites;
    public GameObject Selecton;
    public GameObject Grid;
    
    // Start is called before the first frame update
    void Start()
    {
        var fruit = TaggerSystem.Data.GetOrAddGroup("Fruit");
        fruit.Add("Banana");
        fruit.Add("Cherry");
        fruit.Add("Grapes");
        fruit.Add("Lemon");
        fruit.Add("Mulberry");
        fruit.Add("Orange");
        fruit.Add("Pear");
        fruit.Add("Pineapple");
        fruit.Add("Strawberry");
        fruit.Add("Watermelon");
        
        var vegetable = TaggerSystem.Data.GetOrAddGroup("Vegetable");
        vegetable.Add("Carrot");
        vegetable.Add("Pepper");
        vegetable.Add("Radish");
        vegetable.Add("Mushroom");
        
        var color = TaggerSystem.Data.GetOrAddGroup("Color");
        color.Add("Yellow");
        color.Add("Brown");
        color.Add("Orange");
        color.Add("Red");
        color.Add("Green");
        color.Add("Purple");
        color.Add("White");
        
        var foodType = TaggerSystem.Data.GetOrAddGroup("FoodType");
        foodType.Add("Fruit");
        foodType.Add("Vegetable");
        foodType.Add("Seafood");
        foodType.Add("Baked");
        foodType.Add("Dairy");
        foodType.Add("Part");
        foodType.Add("Drink");
        foodType.Add("Raw");
        foodType.Add("Cooked");
        
        //Banana
        SpawnItem(0).AddTags("Banana", "Fruit", "Yellow");
        //Bread
        SpawnItem(1).AddTags("Bread", "Brown", "Baked");
        //Carrot
        SpawnItem(2).AddTags("Carrot", "Vegetable", "Orange");
        //Cheese
        SpawnItem(3).AddTags("Cheese", "Yellow", "Dairy");
        //Cherry
        SpawnItem(4).AddTags("Cherry", "Fruit", "Red");
        //Fish Cooked
        SpawnItem(5).AddTags("Fish", "Seafood", "Cooked");
        //Fish Raw
        SpawnItem(6).AddTags("Fish", "Seafood", "Raw");
        //Fish Tail
        SpawnItem(7).AddTags("Fish", "Seafood", "Part");
        //Grapes Green
        SpawnItem(8).AddTags("Grapes", "Fruit", "Green");
        //Grapes Purple
        SpawnItem(9).AddTags("Grapes", "Fruit", "Purple");
        //Lemon
        SpawnItem(10).AddTags("Lemon", "Fruit", "Yellow");
        //Meat Cooked
        SpawnItem(11).AddTags("Meat", "Cooked");
        //Meat Raw
        SpawnItem(12).AddTags("Meat", "Raw");
        //Mulberry
        SpawnItem(13).AddTags("Mulberry", "Fruit", "Purple");
        //Mushroom
        SpawnItem(14).AddTags("Mushroom", "Vegetable", "White");
        //Nut
        SpawnItem(15).AddTags("Nut", "Brown");
        //Orange
        SpawnItem(16).AddTags("Orange", "Fruit", "Orange");
        //Pear
        SpawnItem(17).AddTags("Pear", "Fruit", "Green");
        //Pepper Green
        SpawnItem(18).AddTags("Pepper", "Vegetable", "Green");
        //Pepper Red
        SpawnItem(19).AddTags("Pepper", "Vegetable", "Red");
        //Pepper Yellow
        SpawnItem(20).AddTags("Pepper", "Vegetable", "Yellow");
        //Pie
        SpawnItem(21).AddTags("Pie", "Baked");
        //Pineapple
        SpawnItem(22).AddTags("Pineapple", "Fruit", "Yellow");
        //Padish
        SpawnItem(23).AddTags("Radish", "Vegetable", "Red");
        //Strawberry
        SpawnItem(24).AddTags("Strawberry", "Fruit", "Red");
        //Water
        SpawnItem(25).AddTags("Water", "Drink", "Liquid");
        //Watermelon
        SpawnItem(26).AddTags("Watermelon", "Fruit", "Green");

        foreach (var type in foodType)
        {
            GameObject button = Instantiate(ButtonPrefab, Selecton.transform);
            button.GetComponentInChildren<Text>().text = type;
            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                UpdateFilter(type);
            });
        }
        
        foreach (var type in color)
        {
            GameObject button = Instantiate(ButtonPrefab, Selecton.transform);
            button.GetComponentInChildren<Text>().text = type;
            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                UpdateFilter(type);
            });
        }
        
        GameObject b1 = Instantiate(ButtonPrefab, Selecton.transform);
        b1.GetComponentInChildren<Text>().text = "Fruit";
        b1.GetComponent<Button>().onClick.AddListener(() =>
        {
            UpdateFilter(fruit);
        });
        
        GameObject b2 = Instantiate(ButtonPrefab, Selecton.transform);
        b2.GetComponentInChildren<Text>().text = "Vegetable";
        b2.GetComponent<Button>().onClick.AddListener(() =>
        {
            UpdateFilter(vegetable);
        });
        
        foreach (var type in fruit)
        {
            GameObject button = Instantiate(ButtonPrefab, Selecton.transform);
            button.GetComponentInChildren<Text>().text = type;
            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                UpdateFilter(type);
            });
        }
        
        foreach (var type in vegetable)
        {
            GameObject button = Instantiate(ButtonPrefab, Selecton.transform);
            button.GetComponentInChildren<Text>().text = type;
            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                UpdateFilter(type);
            });
        }
    }

    private void UpdateFilter(string tag)
    {
        //set alpha to 0.5f in all items
        foreach (Transform item in Grid.transform)
        {
            item.GetComponent<CanvasGroup>().alpha = 0.5f;
        }
        
        //Because we know every item, we could use this solution:
        // foreach (Transform item in Grid.transform)
        // {
        //     if (!item.gameObject.HasTag(tag))
        //     {
        //         item.GetComponent<CanvasGroup>().alpha = 0.5f;
        //     }
        // }
        
        //But we pretend we don't know every item and we use the TaggerSystem to get the items with the tag
        foreach (var item in TaggerSystem.FindGameObjectsWithTag(tag))
        {
            item.GetComponent<CanvasGroup>().alpha = 1f;
        }
    } 
    
    private void UpdateFilter(TaggerGroup group)
    {
        //set alpha to 0.5f in all items
        foreach (Transform item in Grid.transform)
        {
            item.GetComponent<CanvasGroup>().alpha = 0.5f;
        }
        
        //Because we know every item, we could use this solution:
        // foreach (Transform item in Grid.transform)
        // {
        //     if (!item.gameObject.HasAnyTagsOfGroup(tag))
        //     {
        //         item.GetComponent<CanvasGroup>().alpha = 0.5f;
        //     }
        // }
        
        //But we pretend we don't know every item and we use the TaggerSystem to get the items with the tag
        foreach (var item in TaggerSystem.FindGameObjectsWithGroup(group))
        {
            item.GetComponent<CanvasGroup>().alpha = 1f;
        }
    }

    private GameObject SpawnItem(int i)
    {
        GameObject item = Instantiate(ItemPrefab, Grid.transform);
        item.transform.GetChild(0).GetComponent<Image>().sprite = Sprites[i];
        return item;
    }
}

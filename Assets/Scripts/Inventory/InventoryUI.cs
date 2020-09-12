using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Assuming the hierarchy is like this: 
/// InventoryUI (animator)
///     InventoryPanel
///     InventoryButton
/// 
/// </summary>
public class InventoryUI : ClosableUI
{
    [Header("Items")]
    public ItemUI itemUiPrefab;
    public Transform itemsParent;
    [Header("Text When Examining")]
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI description, effects;
    private Dictionary<ItemName, ItemUI> itemsUI = new Dictionary<ItemName, ItemUI>();

    protected override void Awake()
    {
        base.Awake();
        InventoryManager inventoryManager = MainSingleton.Instance.inventoryManager;
        inventoryManager.OnAddItem += AddInventory;
        inventoryManager.OnRemoveItem += (data) => RemoveFromInventory(data.item);
    }

    protected override void Open()
    {
        // set all text to empty
        itemName.text = string.Empty;
        description.text = string.Empty;
        effects.text = string.Empty;

        base.Open();
    }

    /// <summary>
    /// Show item in inventory UI
    /// </summary>
    /// <param name="item"></param>
    private void AddInventory(ItemData data)
    {
        // instantiate gameobject
        ItemUI ui = GameObject.Instantiate(itemUiPrefab.gameObject, itemsParent).GetComponent<ItemUI>();
        ui.Init(data, () => ExamineItem(data));
        // add to dictionary
        itemsUI.Add(data.item, ui);
    }

    /// <summary>
    /// Remove item in inventory UI
    /// </summary>
    /// <param name="item"></param>
    private void RemoveFromInventory(ItemName name)
    {
        ItemUI itemUI;
        if (itemsUI.TryGetValue(name, out itemUI))
        { // destroy gameobject
            Destroy(itemUI.gameObject);
            // remove from dictionary
            itemsUI.Remove(name);
        }
    }

    private void ExamineItem(ItemData data)
    {
        itemName.text = data.item.ToString();
        description.text = data.itemDescription;
        effects.text = data.effectsDescription;
    }
}

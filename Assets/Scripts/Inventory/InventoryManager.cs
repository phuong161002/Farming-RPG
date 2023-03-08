using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : SingletonMonobehavior<InventoryManager>, ISaveable
{
    private Dictionary<int, ItemDetail> itemDetailsDictionary;

    public List<InventoryItem>[] inventoryLists;

    [HideInInspector] public int[] inventoryListCapacityIntArray; // the index of array is the inventory list (from the InventoryLocation enum), and the value is the capacity of that inventory list
    [HideInInspector] public int[] selectedInventoryItem; // the index of array is the inventory list, and the value is the item code;
    [SerializeField] private SO_ItemList itemList = null;

    private string _iSaveableUniqueID;
    public string ISaveableUniqueID { get => _iSaveableUniqueID; set => _iSaveableUniqueID = value; }

    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave { get => _gameObjectSave; set => _gameObjectSave = value; }

    private UIInventoryBar inventoryBar;

    protected override void Awake()
    {
        base.Awake();

        //Create Inventory lists
        CreateInventoryLists();

        //Create item details dictionary
        CreateItemDetailsDictionary();

        // Initalize selected inventory item array
        selectedInventoryItem = new int[(int)InventoryLocation.Count];
        for (int i = 0; i < selectedInventoryItem.Length; i++)
        {
            selectedInventoryItem[i] = -1;
        }

        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;

        GameObjectSave = new GameObjectSave();
    }

    private void OnEnable()
    {
        ISaveableRegister();
    }

    private void OnDisable()
    {
        ISaveableDeregister();
    }

    private void Start()
    {
        inventoryBar = FindObjectOfType<UIInventoryBar>();
    }

    private void CreateInventoryLists()
    {
        int count = (int)InventoryLocation.Count;
        inventoryLists = new List<InventoryItem>[count];

        for(int i = 0; i < count; i++)
        {
            inventoryLists[i] = new List<InventoryItem>();
        }

        inventoryListCapacityIntArray = new int[count];
        inventoryListCapacityIntArray[(int)InventoryLocation.Player] = Settings.playerInitialInventoryCapacity;
    }


    /// <summary>
    /// Populates the itemDetailsDictionary from the scriptable object item list
    /// </summary>
    private void CreateItemDetailsDictionary()
    {
        itemDetailsDictionary = new Dictionary<int, ItemDetail>();

        foreach(var itemDetail in itemList.itemDetails)
        {
            itemDetailsDictionary.Add(itemDetail.itemCode, itemDetail);
        }
    }

    /// <summary>
    /// Add an item to the inventory list for the inventoryLocation and then destroy the gameObjectToDelete
    /// </summary>
    /// <param name="inventoryLocation"></param>
    /// <param name="item"></param>
    /// <param name="gameObjectToDelete"></param>
    public void AddItem(InventoryLocation inventoryLocation, Item item, GameObject gameObjectToDelete)
    {
        AddItem(inventoryLocation, item);

        Destroy(gameObjectToDelete);
    }

    internal void SwapInventoryItems(InventoryLocation inventoryLocation, int fromItem, int toItem)
    {
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];
        if(fromItem < inventoryList.Count && toItem < inventoryList.Count && fromItem != toItem && fromItem >= 0 && toItem >= 0)
        {
            InventoryItem fromInventoryItem = inventoryList[fromItem];
            InventoryItem toInventoryItem = inventoryList[toItem];

            inventoryList[toItem] = fromInventoryItem;
            inventoryList[fromItem] = toInventoryItem;

            EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryList);
        }
    }

    /// <summary>
    /// Add an item to the inventory list for the inventoryLocation
    /// </summary>
    /// <param name="inventoryLocation"></param>
    /// <param name="item"></param>
    public void AddItem(InventoryLocation inventoryLocation, Item item)
    {
        int itemCode = item.ItemCode;
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];

        //Check if inventory already contains the item
        int itemPosition = FindItemInInventory(inventoryLocation, itemCode);

        if(itemPosition != -1)
        {
            AddItemAtPosition(inventoryList, itemCode, itemPosition);
        }
        else
        {
            AddItemAtPosition(inventoryList, itemCode);
        }

        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryList);
    }

    public void AddItem(InventoryLocation inventoryLocation, int itemCode)
    {
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];

        //Check if inventory already contains the item
        int itemPosition = FindItemInInventory(inventoryLocation, itemCode);

        if (itemPosition != -1)
        {
            AddItemAtPosition(inventoryList, itemCode, itemPosition);
        }
        else
        {
            AddItemAtPosition(inventoryList, itemCode);
        }

        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryList);
    }

    internal void RemoveItem(InventoryLocation inventoryLocation, int itemCode)
    {
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];

        int itemPosition = FindItemInInventory(inventoryLocation, itemCode);

        if(itemPosition != -1)
        {
            RemoveItemAtPosition(inventoryList, itemCode, itemPosition);
        }

        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryList);
    }

    private void RemoveItemAtPosition(List<InventoryItem> inventoryList, int itemCode, int itemPosition)
    {
        InventoryItem inventoryItem = new InventoryItem();

        int quantity = inventoryList[itemPosition].itemQuantity - 1;

        if(quantity > 0)
        {
            inventoryItem.itemQuantity = quantity;
            inventoryItem.itemCode = itemCode;
            inventoryList[itemPosition] = inventoryItem;
        }
        else
        {
            inventoryList.RemoveAt(itemPosition);
        }
    }

    /// <summary>
    /// Add item to the end of inventory if itemPosition equals -1, or add item to the specify item position
    /// </summary>
    /// <param name="inventoryList"></param>
    /// <param name="itemCode"></param>
    /// <param name="itemPosition"></param>
    private void AddItemAtPosition(List<InventoryItem> inventoryList, int itemCode, int itemPosition = -1)
    {
        if(itemPosition == -1)
        {
            inventoryList.Add(new InventoryItem() { itemCode = itemCode, itemQuantity = 1 });
        }
        else
        {
            inventoryList[itemPosition].itemQuantity++;
        }
        //DebugPrintInventoryList(inventoryList);
    }

    private void DebugPrintInventoryList(List<InventoryItem> inventoryList)
    {
        Debug.ClearDeveloperConsole();
        foreach(InventoryItem inventoryItem in inventoryList)
        {
            Debug.Log("Item Description: " + InventoryManager.Instance.GetItemDetail(inventoryItem.itemCode).itemDescription + " Item Quantity: " + inventoryItem.itemQuantity);
        }
        Debug.Log("================================");
    }

    /// <summary>
    /// Find if an itemCode is already in the inventory. Returns the item position
    /// in the inventory list, or -1 if the item is not in the inventory
    /// </summary>
    /// <param name="inventoryLocation"></param>
    /// <param name="itemCode"></param>
    /// <returns></returns>
    public int FindItemInInventory(InventoryLocation inventoryLocation, int itemCode)
    {
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];

        for(int i = 0; i < inventoryList.Count; i++)
        {
            if (inventoryList[i].itemCode == itemCode)
            {
                return i;
            }
        }
        return -1;
    }



    /// <summary>
    /// Returns the itemDetail (from the SO_ItemList) for the itemCode, or null if the item code doesn't exist
    /// </summary>
    /// <param name="itemCode"></param>
    /// <returns></returns>
    public ItemDetail GetItemDetail(int itemCode)
    {
        ItemDetail itemDetail;
        if(itemDetailsDictionary.TryGetValue(itemCode, out itemDetail))
        {
            return itemDetail;
        }
        return null;
    }

    public ItemDetail GetSelectedInventoryItemDetail(InventoryLocation inventoryLocation)
    {
        int itemCode = GetSelectedInventoryItem(inventoryLocation);
        
        if(itemCode == -1)
        {
            return null;
        }
        return GetItemDetail(itemCode);
    }

    private int GetSelectedInventoryItem(InventoryLocation inventoryLocation)
    {
        return selectedInventoryItem[(int)inventoryLocation];
    }

    public string GetItemTypeDescription(ItemType type)
    {
        switch(type)
        {
            case ItemType.Breaking_tool:
                return Settings.BreakingTool;
            case ItemType.Chopping_tool:
                return Settings.ChoppingTool;
            case ItemType.Collecting_tool:
                return Settings.CollectingTool;
            case ItemType.Hoeing_tool:
                return Settings.HoeingTool;
            case ItemType.Reaping_tool:
                return Settings.ReapingTool;
            case ItemType.Watering_tool:
                return Settings.WateringTool;
            default:
                return type.ToString();
        }
    }

    public void SetSelectedInventoryItem(InventoryLocation inventoryLocation, int itemCode)
    {
        selectedInventoryItem[(int)inventoryLocation] = itemCode;
    }

    public void ClearSelectedInventoryItem(InventoryLocation inventoryLocation)
    {
        selectedInventoryItem[(int)inventoryLocation] = -1;
    }

    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
    }

    public void ISaveableDeregister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
    }

    public GameObjectSave ISaveableSave()
    {
        SceneSave sceneSave = new SceneSave();

        GameObjectSave.sceneData.Remove(Settings.PersistentScene);

        sceneSave.listInvItemArray = inventoryLists;

        sceneSave.intArrayDictionary = new Dictionary<string, int[]>();
        sceneSave.intArrayDictionary.Add("inventoryListCapacityArray", inventoryListCapacityIntArray);

        GameObjectSave.sceneData.Add(Settings.PersistentScene, sceneSave);

        return GameObjectSave;
    }

    public void ISaveableLoad(GameSave gameSave)
    {
        if(gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out GameObjectSave gameObjectSave))
        {
            GameObjectSave = gameObjectSave;

            if(gameObjectSave.sceneData.TryGetValue(Settings.PersistentScene, out SceneSave sceneSave))
            {

                if(sceneSave.listInvItemArray != null)
                {
                    inventoryLists = sceneSave.listInvItemArray;

                    for(int i = 0; i < (int)InventoryLocation.Count; i++)
                    {
                        EventHandler.CallInventoryUpdatedEvent((InventoryLocation)i, inventoryLists[i]);
                    }

                    Player.Instance.ClearCarriedItem();

                    inventoryBar.ClearHighlightOnInventorySlots();
                }

                if(sceneSave.intArrayDictionary != null && sceneSave.intArrayDictionary.TryGetValue("inventoryListCapacityArray", out int[]
                    inventoryCapacityArray))
                {
                    inventoryListCapacityIntArray = inventoryCapacityArray;
                }
            }
        }
    }

    public void ISaveableStoreScene(string sceneName)
    {
    }

    public void ISaveableRestoreScene(string sceneName)
    {
    }
}

using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class StoreItem
{
    public GameObject itemPrefab; // Item prefab
    public int Purchasecost; // Cost of the item
    public int KillCoast;
    public float intensity = 0.2f;
    public List<Breakpoint> breakpoints = new();
    public KillRange killRange = new();
    public int lifes = 1;
}

public class GoatStore : MonoBehaviour
{
    [SerializeField] public List<StoreItem> storeItems; // List of store items
    private List<string> purchasedItems = new List<string>();
    private int playerCoins;

    private const string CoinsKey = "PlayerCoins";
    private const string PurchasedItemsKey = "PurchasedItems";

    public TextMeshProUGUI coins;

    public ParticleSystem celebration;

    private void Awake()
    {
        LoadData();
        Purchase(0, true);
    }

    public bool CheckItemPurchased(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= storeItems.Count)
        {
            Debug.LogError("Invalid item index.");
            return false;
        }

        string itemName = storeItems[itemIndex].itemPrefab.name;

        if (purchasedItems.Contains(itemName))
        {
            Debug.Log("Item already purchased.");
            return true;
        }
        return false;
    }

    // Purchase an item by its index in the storeItems list
    public bool Purchase(int itemIndex, bool force = false)
    {
        if (itemIndex < 0 || itemIndex >= storeItems.Count)
        {
            Debug.LogError("Invalid item index.");
            return false;
        }

        string itemName = storeItems[itemIndex].itemPrefab.name;

        if (purchasedItems.Contains(itemName))
        {
            Debug.Log("Item already purchased.");
            return false;
        }

        int itemCost = storeItems[itemIndex].Purchasecost;

        if (playerCoins >= itemCost)
        {
            playerCoins -= itemCost;
            coins.text = playerCoins.ToString();
            Finalise();
            return true;
        }
        else if(force)
        {
            Finalise();
            return true;
        }
        coins.GetComponent<Animator>().SetTrigger("Shake");
            Debug.Log("Not enough coins.");
            return false;
        

        void Finalise()
        {
            purchasedItems.Add(itemName);
            SaveData();
            celebration.Play();
            Debug.Log($"Purchased {itemName} for {itemCost} coins.");
        }

    }

    // Get all purchased items
    public List<GameObject> GetPurchasedItems()
    {
        List<GameObject> result = new List<GameObject>();
        foreach (var storeItem in storeItems)
        {
            if (purchasedItems.Contains(storeItem.itemPrefab.name))
            {
                result.Add(storeItem.itemPrefab);
            }
        }
        return result;
    }

    // Get all items that are not purchased
    public List<GameObject> GetUnpurchasedItems()
    {
        List<GameObject> result = new List<GameObject>();
        foreach (var storeItem in storeItems)
        {
            if (!purchasedItems.Contains(storeItem.itemPrefab.name))
            {
                result.Add(storeItem.itemPrefab);
            }
        }
        return result;
    }

    // Get all items in the store
    public List<GameObject> GetAllStoreItems()
    {
        List<GameObject> result = new List<GameObject>();
        foreach (var storeItem in storeItems)
        {
            result.Add(storeItem.itemPrefab);
        }
        return result;
    }

    // Increment the player's coins
    public void AddCoins(int amount)
    {
        playerCoins += amount;
        SaveData();
        Debug.Log($"Added {amount} coins. Total coins: {playerCoins}.");
    }

    // Save data to PlayerPrefs
    private void SaveData()
    {
        PlayerPrefs.SetInt(CoinsKey, playerCoins);
        PlayerPrefs.SetString(PurchasedItemsKey, string.Join(",", purchasedItems));
        PlayerPrefs.Save();
        Debug.Log("Data saved.");
    }

    // Load data from PlayerPrefs
    private void LoadData()
    {
        playerCoins = PlayerPrefs.GetInt(CoinsKey, 0);

        string purchasedData = PlayerPrefs.GetString(PurchasedItemsKey, "");
        if (!string.IsNullOrEmpty(purchasedData))
        {
            purchasedItems = new List<string>(purchasedData.Split(","));
        }
        else
        {
            purchasedItems.Clear();
        }

        Debug.Log("Data loaded.");
    }

    public int GetTotalCoinsAvailable()
    {
        return playerCoins;
    }

    public StoreItem GetStoreItem(string animalName)
    {
        animalName = animalName.Replace("(Clone)", "");
        StoreItem si = storeItems.Find(x => x.itemPrefab.name.Contains(animalName));

        if (si == null)
        {
            Debug.LogError("No prefab found with name " + animalName);
        }

        return si;
    }
}

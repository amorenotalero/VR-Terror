using UnityEngine;
using HFPS.Systems;
using HFPS.UI;

namespace HFPS.Player
{
    public class ItemOnStart : MonoBehaviour
    {
        const string ITEM_TAG = "StartingItem";
        Inventory inventory;

        [System.Serializable]
        public sealed class StartingItem
        {
            [InventorySelector]
            public int itemID;
            public int amount = 1;
        }

        public StartingItem[] itemsToAdd;

        [HideInInspector, SaveableField]
        public bool CanAddStartItems = true;

        private void Awake()
        {
            inventory = Inventory.Instance;
        }

        public void OnItemAtStart()
        {
            if (!CanAddStartItems) return;

            foreach (var startingItem in itemsToAdd)
            {
                // check if item exist
                Item item = inventory.GetItem(startingItem.itemID);

                if (item != null)
                {
                    InventoryItemData itemData = inventory.ItemDataOfItem(startingItem.itemID);

                    // prevents the addition of an item if there is a "StartingItem" tag
                    if (itemData == null || !itemData.data.Exist(ITEM_TAG))
                    {
                        // add an item to inventory with a tag that prevents the item from being added at each start
                        inventory.AddItem(startingItem.itemID, startingItem.amount, new ItemData
                        (
                            (ITEM_TAG, "")
                        ));
                    }
                }
            }

            CanAddStartItems = false;
        }
    }
}
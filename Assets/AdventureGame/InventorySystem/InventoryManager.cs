using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.AdventureGame
{
    public class InventoryManager : MonoBehaviour
    {
        public const int INVENTORY_SLOTS = 8;

        public Image[] itemImages = new Image[INVENTORY_SLOTS];
        public InventoryItem[] items = new InventoryItem[INVENTORY_SLOTS];
        public InventoryItem Selected = null;

        InventoryUI inventoryUI = null;

        public void RegisterInventoryUI(InventoryUI newInventoryUI) {
            inventoryUI = newInventoryUI;
        }

		public void UpdateUI(int index)
		{
			inventoryUI.UpdateSlot(index);
		}

        public bool AddItem(InventoryItem itemToAdd)
        {
            //find the first empty inventory slot
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null)
                {
                    items[i] = itemToAdd;
                    itemImages[i].sprite = itemToAdd.sprite;
                    itemImages[i].enabled = true;
                    UpdateUI(i);
                    return true;
                }
            }
			Debug.Log("Inventory is full!");
			return false;
        }

        public bool RemoveItem(InventoryItem itemToRemove)
        {
            //find the slot containing the item and remove it
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == itemToRemove)
                {
                    items[i] = null;
                    itemImages[i].sprite = null;
                    itemImages[i].enabled = false;
                    UpdateUI(i);
                    return true;
                }
            }
			Debug.Log("That isn't in your inventory!");
			return false;
        }

		public void SelectItem(int index)
		{
			if (items[index] == null)
			{
				Debug.Log("Nothing to select here!");
				return;
			}
			this.Selected = items[index];
		}

		public void ClearSelected()
		{
			this.Selected = null;
		}

		public void SlotClicked(int index)
		{
            if(items[index] == null){
                //nothing to do here
                return;
            }

			InventoryItem itemInSlot = items[index];
            //clicking an item on another item
			if (Selected != null)
			{
                if (Selected.Id == itemInSlot.Id)
				{
					//clicking the item on itself, clear selection
					ClearSelected();
				}
				//combine the selected item with the item in this slot
				else
				{
					CombineItems(Selected, itemInSlot);
				}
			}
            //clicking an item when nothing is currently selected
            else {
                SelectItem(index);
            }
		}

		public void CombineItems(InventoryItem first, InventoryItem second)
		{
			Debug.Log("You've combined two items!");
            //TODO replace one of the items with the new (combined) item if applicable, clear the other
		}
    }
}
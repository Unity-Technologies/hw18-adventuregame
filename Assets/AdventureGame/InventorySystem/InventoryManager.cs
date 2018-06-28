using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.AdventureGame
{
	public class InventoryManager : MonoBehaviour
	{
		public const int INVENTORY_SLOTS = 8;

		public InventoryItem[] items = new InventoryItem[INVENTORY_SLOTS];
		public InventoryItem Selected = null;

		private static InventoryManager instance;

		public static InventoryManager Instance
		{
			get
			{
				if (instance == null)
				{
					InventoryManager existingInventoryManager = FindObjectOfType<InventoryManager>();
					if (existingInventoryManager != null)
					{
						instance = existingInventoryManager;
					}
                    else {
                        throw new System.Exception("No Inventory Manager exists!");
                    }
				}
				return instance;
			}
		}

		public void UpdateUI(int index)
		{
			InventoryUI.Instance.UpdateSlot(index);
		}

		//returns inventory item with the given id if it is in the inventory, 
		//otherwise returns null
		public InventoryItem GetItemWithId(string id)
		{
			for (int i = 0; i < items.Length; i++)
			{
				if (items[i] != null && items[i].Id == id)
				{
					return items[i];
				}
			}
			return null;
		}

		public void DropSelectedItem(Vector2 position)
		{
			if (Selected == null)
			{
				Debug.Log("Called DropSelectedItem with nothing selected!");
				return;
			}

			Selected.transform.position = new Vector3(position.x, 
                                                      position.y, 
                                                      Selected.transform.position.z);
			Debug.Log("Dropping " + Selected.Id + " at " + Selected.transform.position);
			Selected.Dropped();
			ClearSelected();
		}

		public bool AddItem(InventoryItem itemToAdd)
		{
			//find the first empty inventory slot
			for (int i = 0; i < items.Length; i++)
			{
				if (items[i] == null)
				{
					items[i] = itemToAdd;
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
					UpdateUI(i);
                    if(Selected == itemToRemove){
                        ClearSelected();
                    }
					return true;
				}
			}
			return false;
		}

		private void selectItem(int index)
		{
			if (items[index] == null)
			{
				return;
			}
			Debug.Log("Selected " + items[index].Id);
			this.Selected = items[index];
			AdventureGameOverlayManager.Instance.ChangeCursor(items[index].sprite);
		}

		public void ClearSelected()
		{
			Debug.Log("Selected item cleared");
			this.Selected = null;
			AdventureGameOverlayManager.Instance.ChangeCursor(null);
		}

		public void SlotClicked(int index)
		{
			if (items[index] == null)
			{
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
			else
			{
				selectItem(index);
			}
		}

		public void CombineItems(InventoryItem first, InventoryItem second)
		{
			//TODO replace one of the items with the new (combined) item if applicable, clear the other
			Debug.Log("You've combined two items!");
			ClearSelected();
		}

	    public string[] GetInventoryItemIDs()
	    {
	        string[] ids = new string[INVENTORY_SLOTS];

	        for (int i = 0; i < INVENTORY_SLOTS; i++)
	        {
	            ids[i] = items[i] != null ? items[i].Id : string.Empty;
	        }

	        return ids;
	    }

	    public void ReloadInventoryWithIDs(string[] ids)
	    {
	        for (int i = 0; i < INVENTORY_SLOTS && i < ids.Length; i++)
	        {
	            if (string.IsNullOrEmpty(ids[i]))
	            {
	                var inventoryItem = SceneManager.Instance.GetInventoryItem(ids[i]);
                    items[i] = inventoryItem;
	            }
	            else
	            {
	                items[i] = null;
	            }

                UpdateUI(i);
	        }
	    }
	}
}
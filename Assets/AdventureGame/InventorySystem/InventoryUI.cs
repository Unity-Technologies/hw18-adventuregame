using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace UnityEngine.AdventureGame
{
	public class InventoryUI : MonoBehaviour
	{
		private InventorySlot[] inventorySlots;

		private static InventoryUI instance;

		public static InventoryUI Instance
		{
			get
			{
				if (instance == null)
				{
					InventoryUI existingInventoryUI = FindObjectOfType<InventoryUI>();
					if (existingInventoryUI != null)
					{
						instance = existingInventoryUI;
					}
					else
					{
						Debug.Log("Inventory UI was automatically added - it may not be in the correct place!");
						GameObject inventoryUI = new GameObject();
						instance = inventoryUI.AddComponent<InventoryUI>();
					}
				}
				return instance;
			}
		}

		void Start()
		{
			inventorySlots = GetComponentsInChildren<InventorySlot>();
			if (InventoryManager.INVENTORY_SLOTS != inventorySlots.Length)
			{
				Debug.Log("WARNING: Number of Inventory Slots does not match max inventory size! InventoryUI is incorrectly configured.");
			}
			ConfigureSlots();
		}

		void ConfigureSlots()
		{
			for (int i = 0; i < InventoryManager.INVENTORY_SLOTS; i++)
			{
				UpdateSlot(i);
				SetSlotClickHandler(i);
			}
		}

		public void UpdateSlot(int index)
		{
			if (InventoryManager.Instance.items[index] != null)
			{
				inventorySlots[index].GetComponent<Image>().sprite = InventoryManager.Instance.items[index].sprite;
			}
			else
			{
                //go back to the slot background image
                inventorySlots[index].GetComponent<Image>().sprite = inventorySlots[index].backgroundImage;
			}
		}

		private void SetSlotClickHandler(int index)
		{
			inventorySlots[index].GetComponent<Button>().onClick.AddListener(() => { InventoryManager.Instance.SlotClicked(index); });
		}

	}
}

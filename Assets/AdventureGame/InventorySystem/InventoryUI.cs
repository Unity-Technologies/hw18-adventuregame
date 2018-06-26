using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace UnityEngine.AdventureGame
{
    public class InventoryUI : MonoBehaviour
    {
        InventoryManager inventoryManager;
        InventorySlot[] inventorySlots;

        void Start()
        {
            inventoryManager = FindObjectOfType<InventoryManager>();
            inventorySlots = GetComponentsInChildren<InventorySlot>();
            if(InventoryManager.INVENTORY_SLOTS != inventorySlots.Length){
                Debug.Log("WARNING: Number of Inventory Slots does not match max inventory size!");
            }
            //let the Inventory Manager know what UI needs to be updated when inventory changes
            inventoryManager.RegisterInventoryUI(this);
            UpdateSlots();
        }

        void UpdateSlots(){
			for (int i = 0; i < InventoryManager.INVENTORY_SLOTS; i++)
			{
                UpdateSlot(i);
			}
        }

        public void UpdateSlot(int index){
            if(inventoryManager.items[index] != null){
				inventorySlots[index].GetComponent<Image>().sprite = inventoryManager.items[index].sprite;
			}
            else {
                inventorySlots[index].onClick = null;
                inventorySlots[index].GetComponent<Image>().sprite = null;
            }
		}

    }
}

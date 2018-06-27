using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace UnityEngine.AdventureGame
{
    public class InventoryUI : MonoBehaviour
    {
        InventorySlot[] inventorySlots;

        void Start()
        {
            inventorySlots = GetComponentsInChildren<InventorySlot>();
            if(InventoryManager.INVENTORY_SLOTS != inventorySlots.Length){
                Debug.Log("WARNING: Number of Inventory Slots does not match max inventory size!");
            }
            //let the Inventory Manager know what UI needs to be updated when inventory changes
            InventoryManager.Instance.RegisterInventoryUI(this);
            ConfigureSlots();
        }

        void ConfigureSlots(){
			for (int i = 0; i < InventoryManager.INVENTORY_SLOTS; i++)
			{
                UpdateSlot(i);
                SetSlotClickHandler(i);
			}
        }

        public void UpdateSlot(int index){
            if(InventoryManager.Instance.items[index] != null){
				inventorySlots[index].GetComponent<Image>().sprite = InventoryManager.Instance.items[index].sprite;
			}
            else {
                inventorySlots[index].GetComponent<Image>().sprite = null;
            }
		}

        private void SetSlotClickHandler(int index){
			inventorySlots[index].GetComponent<Button>().onClick.AddListener(() => { InventoryManager.Instance.SlotClicked(index); });
        }

    }
}

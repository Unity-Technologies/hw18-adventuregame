using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
            Debug.Log(inventorySlots.Length);

            for (int i = 0; i < InventoryManager.INVENTORY_SLOTS; i++)
            {
                inventorySlots[i].index = i;
                if(inventoryManager.items[i] != null){
                    inventorySlots[i].GetComponent<Image>().sprite = inventoryManager.items[i].sprite;
                }
            }
        }

    }
}

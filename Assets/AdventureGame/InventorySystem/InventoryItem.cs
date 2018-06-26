using UnityEngine;

namespace UnityEngine.AdventureGame
{
    public class InventoryItem : MonoBehaviour
    {
        public string Id;
        public Sprite sprite;
        SpriteRenderer spriteRenderer;
        InventoryManager inventoryManager;

        void Start()
        {
            inventoryManager = FindObjectOfType<InventoryManager>();
            spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
        }

        public void PickedUp(){
            inventoryManager.AddItem(this);
        }

		public void Dropped()
		{
			inventoryManager.RemoveItem(this);
		}
    }
}


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
            spriteRenderer.enabled = false;
            inventoryManager.AddItem(this);
        }

		public void Dropped()
		{
            spriteRenderer.enabled = true;
			inventoryManager.RemoveItem(this);
		}
    }
}


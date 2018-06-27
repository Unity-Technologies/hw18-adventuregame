using UnityEngine;

namespace UnityEngine.AdventureGame
{
    public class InventoryItem : MonoBehaviour
    {
        public string Id;
        public Sprite sprite;
        SpriteRenderer spriteRenderer;

        void Start()
        {
            spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
        }

        public void PickedUp(){
            Debug.Log(Id + " picked up.");
            spriteRenderer.enabled = false;
            InventoryManager.Instance.AddItem(this);
        }

		public void Dropped()
		{
            Debug.Log(Id + " dropped.");
            spriteRenderer.enabled = true;
			InventoryManager.Instance.RemoveItem(this);
		}
    }
}


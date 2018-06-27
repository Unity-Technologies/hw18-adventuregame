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
			//spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            //spriteRenderer.enabled = false;
			Debug.Log(Id + " picked up.");
			InventoryManager.Instance.AddItem(this);
        }

		public void Dropped()
		{
			//spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
			//spriteRenderer.enabled = true;
			Debug.Log(Id + " dropped.");
			InventoryManager.Instance.RemoveItem(this);
		}
    }
}


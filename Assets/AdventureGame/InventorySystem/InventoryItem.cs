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
            gameObject.SetActive(false); //disable the game object while it's in the inventory, so it doesn't appear in the scene
			InventoryManager.Instance.AddItem(this);
        }

		public void Dropped()
		{
			//spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
			//spriteRenderer.enabled = true;
			Debug.Log(Id + " dropped.");
			gameObject.SetActive(true); //enable the game object so it appears in the scene
			InventoryManager.Instance.RemoveItem(this);
		}
    }
}


using UnityEngine;

namespace UnityEngine.AdventureGame
{
	public class InventoryItem : MonoBehaviour
	{
		public string Id;
		public Sprite sprite;

        void Start()
        {
            SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer)
            {
                //if the item has a sprite renderer make sure it's using the correct sprite
                spriteRenderer.sprite = sprite;
            }
        }

		public void PickedUp()
		{
			Debug.Log(Id + " picked up.");
			gameObject.SetActive(false); //disable the game object while it's in the inventory, so it doesn't appear in the scene
			InventoryManager.Instance.AddItem(this);
		}

		public void Dropped()
		{
			Debug.Log(Id + " dropped.");
			gameObject.SetActive(true); //enable the game object so it appears in the scene
			InventoryManager.Instance.RemoveItem(this);
		}
	}
}


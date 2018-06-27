namespace UnityEngine.AdventureGame
{
	public class ScreenClickHandler : MonoBehaviour
	{
		void Update()
		{
			if (Input.GetMouseButtonUp(0))
			{
				Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);

				Collider2D[] hits = Physics2D.OverlapPointAll(ray);

				if (UnityEngine.EventSystems.EventSystem.current == null || !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
				{
					if (hits.Length > 0)
					{
						for (int i = 0; i < hits.Length; i++)
						{
							var hit = hits[i];
							var interactableObject = hit.gameObject.GetComponent<Interactable>();
							if (InventoryManager.Instance.Selected != null)
							{
								if (interactableObject != null)
								{
									interactableObject.OnInteracted(InventoryManager.Instance.Selected);
									break;
								}
								else if (i == hits.Length - 1)
								{
									//if this is the last object we're checking, and it isn't interactable, just drop the item
									InventoryManager.Instance.DropSelectedItem(ray);
								}
							}
							else if (interactableObject != null)
							{
								interactableObject.OnInteracted();
								break;
							}
							else
							{
								Debug.LogFormat("Walk Command Triggered!");
								if (SceneManager.Instance.Character != null && SceneManager.Instance.Character.Controllable)
								{
									SceneManager.Instance.Character.WalkToPosition(ray);
								}
							}
						}
					}
				}
			}
		}
	}
}

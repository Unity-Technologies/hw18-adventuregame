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
                        foreach (var hit in hits)
                        {
                            var interactableObject = hit.gameObject.GetComponent<Interactable>();
                            if (interactableObject != null)
                            {
                                interactableObject.OnInteracted();
                                break;
                            }
                            else if (InventoryManager.Instance.Selected != null)
                            {
                                InventoryManager.Instance.DropSelectedItem(ray);
                            }
                            else {
                                Debug.LogFormat("Walk Command Triggered!");
                                if (SceneManager.Instance.Character != null)
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

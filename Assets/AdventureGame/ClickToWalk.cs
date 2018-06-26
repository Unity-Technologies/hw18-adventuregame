namespace UnityEngine.AdventureGame
{
    public class ClickToWalk : MonoBehaviour
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
                            var interactableAction = hit.gameObject.GetComponent<Interactable>();
                            if (interactableAction != null)
                            {
                                interactableAction.OnInteracted();
                                break;
                            }
                            else
                            {
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

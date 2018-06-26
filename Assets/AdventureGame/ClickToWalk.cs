namespace UnityEngine.AdventureGame
{
    public class ClickToWalk : MonoBehaviour
    {
        void OnMouseUp()
        {
            Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Collider2D[] hits = Physics2D.OverlapPointAll(ray);

            if (UnityEngine.EventSystems.EventSystem.current == null || !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                if (hits.Length > 0)
                {
                    foreach (var hit in hits)
                    {
                        if (hit.gameObject.tag == "ClickableAction")
                        {
                            var clickableAction = gameObject.GetComponent<ClickableAction>();
                            clickableAction.ItemClicked();
                        }
                        else if (hit.gameObject.GetInstanceID() == gameObject.GetInstanceID())
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

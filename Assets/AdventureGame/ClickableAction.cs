using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.AdventureGame
{
    public class ClickableAction : MonoBehaviour
    {
        public void ItemClicked()
        {
            Debug.Log("Item with a clickable action attached was clicked.");
        }
    }
}
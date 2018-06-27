using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.AdventureGame
{
    public class DisplaySystemMenuButton : MonoBehaviour
    {
        #region Private Methods
        void Start()
        {
            Button button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(HandleButtonClick);
            }
        }

        private void HandleButtonClick()
        {
			AdventureGameOverlayManager.Instance.DisplaySystemMenu();
        }
        #endregion
    }
}
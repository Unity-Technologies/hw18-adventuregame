using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.AdventureGame
{
    /// <summary>
    /// The options for System Menu Buttons
    /// </summary>
    public enum SystemMenuButtonOptions
    {
        SAVE,
        QUIT,
        SETTINGS,
		CLOSEMENU,
        OTHER,
    }

    public class SystemMenuButton : MonoBehaviour
    {

        #region Public Variables
        public SystemMenuButtonOptions systemMenuButtonAction;
        #endregion

        #region Private Variables
        #endregion

        #region Public Methods
        #endregion

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
            AdventureGameOverlayManager.Instance.HandleSystemAction(systemMenuButtonAction);
        }
        #endregion
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.AdventureGame
{
    public class CharacterActionButton : MonoBehaviour
    {
        #region Public Variables
        public CharacterActionType actionType;
        #endregion

        #region Private Variables
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
		void Start () {
			Button button = GetComponent<Button>();
			if (button != null) {
				button.onClick.AddListener(HandleButtonClick);
			}
		}

		private void HandleButtonClick () {
			AdventureGameOverlayManager.Instance.HandleActionButtonClick (actionType);
		}
        #endregion
    }
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityEngine.AdventureGame
{
	// TODO(laurenfrazier): UI currently passes through touches. It should swallow them instead.

    /// <summary>
    /// Sets up and manages showing/hiding of menus/overlay UI.
    /// </summary>
    public class AdventureGameOverlayManager : MonoBehaviour
    {
        #region Public Variables
        // Static singleton
        public static AdventureGameOverlayManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject adventureGameOverlayManager = new GameObject();
                    instance = adventureGameOverlayManager.AddComponent<AdventureGameOverlayManager>();
                }
                return instance;
            }
        }

        // UIs for different types of UI. Enable/disable based on game type.
		[Header("Top Level UIs")]
        public GameObject sierraActionUI;
        public GameObject verbCoinActionUI;

        // Prefabs for action menu buttons
		[Header("Button Prefabs")]
        public Button sierraActionButton;
        public Button verbCoinActionButton;
        #endregion

        #region Private Variables
        private static AdventureGameOverlayManager instance;
        #endregion

        #region Public Methods

        #endregion

        #region Private Methods
        private void Start()
        {
            // Set all menus to false and selectively enable
            sierraActionUI.SetActive(false);
            verbCoinActionUI.SetActive(false);

            SetUpGameTypeUI();
        }

        /// <summary>
        /// Initial setup call. Sets up all the other UIs and enables the appropriate ones.
        /// </summary>
        private void SetUpGameTypeUI()
        {
            switch (InputSystemManager.Instance.adventureGameType)
            {
                case AdventureGameType.SIERRA:
                    {
                        SetUpSierraActionUI();   
                        break;
                    }
                case AdventureGameType.VERBCOIN:
                    {
                        //verbCoinActionUI.SetActive(true);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }


        private void SetUpSierraActionUI()
        {
			sierraActionUI.SetActive(true);
            foreach (InputSystemManager.CharacterAction characterAction in InputSystemManager.Instance.characterActions)
            {
                if (sierraActionButton != null)
                {
                    Button characterActionButton = Instantiate(sierraActionButton);
                    characterActionButton.GetComponentInChildren<Text>().text = characterAction.actionName;
					characterActionButton.name = characterAction.actionName;
					
					characterActionButton.transform.SetParent(sierraActionUI.transform, false);
					characterActionButton.onClick.AddListener(delegate { HandleSierraActionButtonClick(characterAction.actionType); });
                }
            }
        }

		private void HandleSierraActionButtonClick (CharacterActionType characterActionType) {
			InputSystemManager.Instance.currentlySelectedActionType = characterActionType;
			Debug.Log(characterActionType);
		}
        #endregion
    }
}
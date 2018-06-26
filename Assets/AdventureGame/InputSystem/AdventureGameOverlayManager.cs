using System.Collections;
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
        /// <summary>
        /// The type of menu
        /// </summary>
        public enum MenuType
        {
        }

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

        // UIs for different adventure game types. Enable/disable based on game type.
        [HideInInspector]
        public GameObject sierraActionUI;
        [HideInInspector]
        public GameObject verbCoinActionUI;

        // Prefabs for action menu buttons
        [Header("Action Button Prefabs")]
        public Button sierraActionButton;
        public Button verbCoinActionButton;

        // Settings for Dialogue Menus
        [Header("Settings for Dialogue Menus")]
        [Tooltip("Use the Sprite Editor to set the slicing on the sprite.")]
        public Sprite borderSprite;

        #endregion

        #region Private Variables
        private static AdventureGameOverlayManager instance;
        private GameObject dialogueBoxPrefab;
        private Canvas canvas;
        private GameObject currentlyDisplayedDialogueBox;
        #endregion

        #region Public Methods
        public void CreateDialogueBox() {
            GameObject dialogueBox = Instantiate(dialogueBoxPrefab);
            if (borderSprite != null) {
                dialogueBox.GetComponent<Image>().sprite = borderSprite;
            }
            dialogueBox.transform.SetParent(canvas.transform, false);
            currentlyDisplayedDialogueBox = dialogueBox;
        }
        #endregion

        #region Private Methods
        private void Start()
        {
            canvas = GetComponentInChildren<Canvas>();

            // Set all menus to false and selectively enable
            if (sierraActionUI != null)
            {
                sierraActionUI.SetActive(false);
            }
            if (verbCoinActionUI != null)
            {
                verbCoinActionUI.SetActive(false);
            }

            SetUpGameTypeUI();

            // Set up Dialogue Box prefab
            dialogueBoxPrefab = (GameObject)Resources.Load("DialogueBox", typeof(GameObject));

            // test
            //CreateDialogueBox();
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
                        if (sierraActionUI != null)
                        {
                            SetUpSierraActionUI();
                        }
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

        private void HandleSierraActionButtonClick(CharacterActionType characterActionType)
        {
            InputSystemManager.Instance.currentlySelectedActionType = characterActionType;
            Debug.Log(characterActionType);
        }

        #endregion
    }
}

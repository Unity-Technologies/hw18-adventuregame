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
        /// The placement of the dialogue menu for selecting options
        /// </summary>
        public enum DialogueOptionMenuPlacement
        {
            BOTTOM,
            LEFT,
            RIGHT,
            TOP,
            CENTER,
        }

        /// <summary>
        /// The placement of the dialogue menu for selecting options
        /// </summary>
        public enum DialogueOptionMenuSize
        {
            SMALL,
            MEDIUM,
            LARGE,
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

        // Prefabs for action menu buttons
        [Header("Action Button Prefabs")]
        public Button naiveActionButton;
        public Button contextualActionButton;

        // Settings for Dialogue Menus
        [Header("Settings for Dialogue Menus")]
        [Tooltip("Use the Sprite Editor to set the slicing on the sprite.")]
        public Sprite borderSprite;
        public DialogueOptionMenuSize dialogueOptionMenuSize = DialogueOptionMenuSize.MEDIUM;
        public DialogueOptionMenuPlacement dialogueOptionMenuPlacement = DialogueOptionMenuPlacement.BOTTOM;
        public Font menuFont;
        // Callback that returns the currently selected action (or the result of a menu selection)
        public delegate void DialogueSelectionDelegate(string result);
        #endregion

        #region Private Variables
        private static AdventureGameOverlayManager instance;
        private GameObject naiveActionUI;
        private GameObject contextualActionUI;
        private GameObject dialogueBoxPrefab;
        private Canvas canvas;
        private GameObject currentlyDisplayedDialogueBox;
        #endregion

        #region Public Methods
        public void CreateDialogueBox(string[] dialogueOptions, string description = null, DialogueSelectionDelegate dialogueSelectionDelegate = null) {
            // If we are only showing a dialogue box, get rid of it before we display the next one.
            if (currentlyDisplayedDialogueBox != null) {
                // TODO(laurenfrazier): Add a transition here, don't just make it disappear!
                Destroy(currentlyDisplayedDialogueBox);
            }

            // Create fresh dialogue box and add to screen
            GameObject dialogueBox = Instantiate(dialogueBoxPrefab);
            if (borderSprite != null) {
                dialogueBox.GetComponent<Image>().sprite = borderSprite;
            }
            dialogueBox.transform.SetParent(canvas.transform, true);

            // Set size of dialogue box
            float boxWidth = 0;
            float boxHeight = 0;
            int boxFontSize = 14;
            switch (dialogueOptionMenuSize) {
                case DialogueOptionMenuSize.SMALL: {
                    boxWidth = (float)Screen.width * 0.3f;
                    boxHeight = (float)Screen.height * 0.3f;
                    boxFontSize = 14;
                    break;
                }
                case DialogueOptionMenuSize.MEDIUM: {
                    boxWidth = (float)Screen.width * 0.5f;
                    boxHeight = (float)Screen.height * 0.5f;
                    boxFontSize = 24;
                    break;
                }
                case DialogueOptionMenuSize.LARGE: {
                    boxWidth = (float)Screen.width * 0.8f;
                    boxHeight = (float)Screen.height * 0.8f;
                    boxFontSize = 34;
                    break;
                }
                default: {
                    // Should never hit this, just bail and pick arbitrary size
                    boxWidth = 100f;
                    boxHeight = 100f;
                    break;
                }
            }

            // Set placement of dialogue box
            float offsetX = Screen.width - boxWidth;
            float offsetY = Screen.height - boxHeight;
            switch (dialogueOptionMenuPlacement) {
                case DialogueOptionMenuPlacement.BOTTOM: {
                    dialogueBox.GetComponent<RectTransform>().offsetMin = new Vector2(offsetX / 2.0f, 0); // left, bottom
                    dialogueBox.GetComponent<RectTransform>().offsetMax = new Vector2(-1.0f * (offsetX / 2.0f), -1.0f * offsetY); // -right, -top
                    break;
                }
                case DialogueOptionMenuPlacement.CENTER: {
                    dialogueBox.GetComponent<RectTransform>().offsetMin = new Vector2(offsetX / 2.0f, offsetY / 2.0f); // left, bottom
                    dialogueBox.GetComponent<RectTransform>().offsetMax = new Vector2(-1.0f * (offsetX / 2.0f), -1.0f * (offsetY / 2.0f)); // -right, -top
                    break;
                }
                case DialogueOptionMenuPlacement.LEFT: {
                    dialogueBox.GetComponent<RectTransform>().offsetMin = new Vector2(0, offsetY / 2.0f); // left, bottom
                    dialogueBox.GetComponent<RectTransform>().offsetMax = new Vector2(-1.0f * offsetX, -1.0f * (offsetY / 2.0f)); // -right, -top
                    break;
                }
                case DialogueOptionMenuPlacement.RIGHT: {
                    dialogueBox.GetComponent<RectTransform>().offsetMin = new Vector2(offsetX, offsetY / 2.0f); // left, bottom
                    dialogueBox.GetComponent<RectTransform>().offsetMax = new Vector2(0, -1.0f * (offsetY / 2.0f)); // -right, -top
                    break;
                }
                case DialogueOptionMenuPlacement.TOP: {
                    dialogueBox.GetComponent<RectTransform>().offsetMin = new Vector2(offsetX / 2.0f, offsetY); // left, bottom
                    dialogueBox.GetComponent<RectTransform>().offsetMax = new Vector2(-1.0f * (offsetX / 2.0f), 0); // -right, -top
                    break;
                }
                default: {
                    break;
                }
            }
            currentlyDisplayedDialogueBox = dialogueBox;

            // Set up optional title text
            if (description != null) {
                GameObject descriptionObject = new GameObject("Description");
                Text descriptionText = descriptionObject.AddComponent<Text>();
                descriptionText.text = description;
                descriptionText.font = menuFont;
                descriptionText.fontSize = boxFontSize;
                descriptionObject.transform.SetParent(dialogueBox.transform, false);
            }

            // Set up dialogue selections
            foreach (string dialogueOption in dialogueOptions) {
                Button dialogueOptionButton = Instantiate(naiveActionButton);
                Text buttonText = dialogueOptionButton.GetComponentInChildren<Text>();
                buttonText.text = dialogueOption;
                buttonText.font = menuFont;
                buttonText.fontSize = boxFontSize;
                dialogueOptionButton.name = dialogueOption;
                dialogueOptionButton.transform.SetParent(dialogueBox.transform, false);
            }
            // Return dialogue selection
        }
        #endregion

        #region Private Methods
        private void Start()
        {
            canvas = GetComponentInChildren<Canvas>();
            naiveActionUI = GameObject.Find("NaiveActionUI");
            contextualActionUI = GameObject.Find("ContextualActionUI");
            // Set all menus to false and selectively enable
            if (naiveActionUI != null)
            {
                naiveActionUI.SetActive(false);
            }
            if (contextualActionUI != null)
            {
                contextualActionUI.SetActive(false);
            }

            SetUpGameTypeUI();

            // Set up Dialogue Box prefab
            dialogueBoxPrefab = (GameObject)Resources.Load("DialogueBox", typeof(GameObject));
            // Set up default menu font
            if (menuFont == null) {
                menuFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            // test
            //CreateDialogueBox(new []{"Option 1", "Option 2", "Option 3"}, "Here is some dialogue. Respond!");
        }

        /// <summary>
        /// Initial setup call. Sets up all the other UIs and enables the appropriate ones.
        /// </summary>
        private void SetUpGameTypeUI()
        {
            switch (InputSystemManager.Instance.adventureGameType)
            {
                case AdventureGameType.NAIVE:
                    {
                        if (naiveActionUI != null)
                        {
                            SetUpNaiveActionUI();
                        }
                        break;
                    }
                case AdventureGameType.CONTEXTUAL:
                    {
                        // TODO(laurenfrazier): Set up Contextual UI
                        //contextualActionUI.SetActive(true);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }


        private void SetUpNaiveActionUI()
        {
            naiveActionUI.SetActive(true);
            foreach (InputSystemManager.CharacterAction characterAction in InputSystemManager.Instance.characterActions)
            {
                if (naiveActionButton != null)
                {
                    Button characterActionButton = Instantiate(naiveActionButton);
                    characterActionButton.GetComponentInChildren<Text>().text = characterAction.actionName;
                    characterActionButton.name = characterAction.actionName;

                    characterActionButton.transform.SetParent(naiveActionUI.transform, false);
                    characterActionButton.onClick.AddListener(delegate { HandleNaiveActionButtonClick(characterAction.actionType); });
                }
            }
        }

        private void HandleNaiveActionButtonClick(CharacterActionType characterActionType)
        {
            InputSystemManager.Instance.currentlySelectedActionType = characterActionType;
            Debug.Log(characterActionType);
        }

        private void HandleDialogueOptionClick() {

        }
        #endregion
    }
}

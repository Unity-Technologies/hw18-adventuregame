using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AdventureGame;
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
        [Header("Action Menu")]
        public bool autoCreateButtons = true;
        [Tooltip("Only set if Auto Create is true.")]
        public Button naiveActionButton;
        [Tooltip("Only set if Auto Create is true.")]
        public Button contextualActionButton;

        // Settings for Dialogue Menus
        [Header("Settings for Dialogue Menus")]
        [Tooltip("Use the Sprite Editor to set the slicing on the sprite.")]
        public Sprite borderSprite;
        public DialogueOptionMenuSize dialogueOptionMenuSize = DialogueOptionMenuSize.MEDIUM;
        public DialogueOptionMenuPlacement dialogueOptionMenuPlacement = DialogueOptionMenuPlacement.BOTTOM;
        public Font menuFont;
        public Color menuTextColor = Color.black;

        // Settings for System Menus
        [Header("Settings for System Menus")]
        public GameObject systemMenuPrefab;

        // Settings for Mouse Cursor
        [Header("Settings for Mouse Cursor")]
        public Sprite defaultMouseCursor;

        // Callback that returns the currently selected action (or the result of a menu selection)
        public delegate void DialogueSelectionDelegate(int result);
        // Callback that returns when the user advances dialogue
        public delegate void DialogueAdvanceDelegate();
        #endregion

        #region Private Variables
        private static AdventureGameOverlayManager instance;
        private GameObject naiveActionUI;
        private GameObject contextualActionUI;
        private GameObject dialogueBoxPrefab;
        private Canvas canvas;
        private GameObject currentlyDisplayedDialogueBox;
        private GameObject currentlyDisplayedSystemMenu;
        private bool awaitingDialogueAdvance;
        private DialogueAdvanceDelegate dialogueAdvanceDelegate;
        private Text currentlyDisplayedDialogue;
        private GameObject screenTouchPanel;
        private float kDialogueVerticalOffset = 2.0f;
        #endregion

        #region Public Methods
        public void Awake()
        {
            instance = this;
        }

        public void Start()
        {
            canvas = GetComponentInChildren<Canvas>();
            naiveActionUI = GameObject.Find("NaiveActionUI");
            contextualActionUI = GameObject.Find("ContextualActionUI");
            screenTouchPanel = GameObject.Find("ScreenTouchPanel");
            // Set all menus to false and selectively enable
            if (naiveActionUI != null)
            {
                naiveActionUI.SetActive(false);
            }
            if (contextualActionUI != null)
            {
                contextualActionUI.SetActive(false);
            }
            if (screenTouchPanel != null) {
                screenTouchPanel.SetActive(false);
            }

            SetUpGameTypeUI();
            SetUpCursor();

            // Set up Dialogue Box prefab
            dialogueBoxPrefab = (GameObject)Resources.Load("DialogueBox", typeof(GameObject));
            // Set up default menu font
            if (menuFont == null)
            {
                menuFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            // testing only!
            //DisplaySystemMenu();
            //DisplayCharacterDialogue("hello world", "Character1");
            //DisplayDialogueOptions(new []{"Option 1", "Option 2", "Option 3"}, "Here is some dialogue. Respond!");
        }

        public void Update()
        {
            if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    // Handle a dialogue advance press
                    if (awaitingDialogueAdvance && screenTouchPanel != null && screenTouchPanel.activeInHierarchy) 
                    {
                        HandleAdvanceDialogue();
                    }
                }
            }
        }

        public void DisplaySystemMenu()
        {
            if (systemMenuPrefab != null)
            {
                GameObject systemMenu = Instantiate(systemMenuPrefab);
                systemMenu.transform.SetParent(canvas.transform, false);
                currentlyDisplayedSystemMenu = systemMenu;
            }
        }

        public void CloseSystemMenu()
        {
            // TODO(laurenfrazier): Add a transition here, don't just make it disappear!
            Destroy(currentlyDisplayedSystemMenu);
        }

        public void DisplayCharacterDialogue(string dialogue, string characterName = null, DialogueAdvanceDelegate dialogueAdvanceDelegate = null)
        {
            Debug.Log(characterName + " says: " + dialogue);
            // Place dialogue
            Vector2 dialoguePoint = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            if (characterName != null) {
                GameObject speaker = GameObject.Find(characterName);
                Renderer renderer = speaker.GetComponent<Renderer>();
                float height = 150;
                if (renderer != null) {
                    height = renderer.bounds.size.y;
                }
                Vector3 offsetPosition = new Vector3(speaker.transform.position.x, speaker.transform.position.y + height + kDialogueVerticalOffset, speaker.transform.position.z) ;
                Camera cam = FindObjectsOfType<Camera>().First();
                if (cam != null && speaker != null) {
                    Vector3 screenPosition = cam.WorldToScreenPoint(offsetPosition);
                    dialoguePoint = new Vector2(screenPosition.x, screenPosition.y);
                }
            }
            GameObject textObject = new GameObject("Current Dialogue");
            Text dialogueText = textObject.AddComponent<Text>();
            ContentSizeFitter sizeFitter = textObject.AddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            dialogueText.text = dialogue;
            // TODO(laurenfrazier): Will each character have their own font/size/color?
            dialogueText.font = menuFont;
            dialogueText.fontSize = 44;
            textObject.transform.SetParent(canvas.transform, true);
            textObject.transform.position = dialoguePoint;
            currentlyDisplayedDialogue = dialogueText;

            // When user dismisses by touching the screen, call callback
            this.dialogueAdvanceDelegate = dialogueAdvanceDelegate;
            if (screenTouchPanel != null) {
                screenTouchPanel.SetActive(true);
            }
            awaitingDialogueAdvance = true;
        }

        public void DisplayDialogueOptions(string[] dialogueOptions, string description = null, DialogueSelectionDelegate dialogueSelectionDelegate = null)
        {
            // If we are only showing a dialogue box, get rid of it before we display the next one.
            if (currentlyDisplayedDialogueBox != null)
            {
                DestroyDialogueBox();
            }

            // Create fresh dialogue box and add to screen
            GameObject dialogueBox = Instantiate(dialogueBoxPrefab);
            if (borderSprite != null)
            {
                dialogueBox.GetComponent<Image>().sprite = borderSprite;
            }
            dialogueBox.transform.SetParent(canvas.transform, true);

            // Set size of dialogue box
            float boxWidth = 0;
            float boxHeight = 0;
            int boxFontSize = 14;
            switch (dialogueOptionMenuSize)
            {
                case DialogueOptionMenuSize.SMALL:
                    {
                        boxWidth = (float)Screen.width * 0.3f;
                        boxHeight = (float)Screen.height * 0.3f;
                        boxFontSize = 14;
                        break;
                    }
                case DialogueOptionMenuSize.MEDIUM:
                    {
                        boxWidth = (float)Screen.width * 0.5f;
                        boxHeight = (float)Screen.height * 0.5f;
                        boxFontSize = 24;
                        break;
                    }
                case DialogueOptionMenuSize.LARGE:
                    {
                        boxWidth = (float)Screen.width * 0.8f;
                        boxHeight = (float)Screen.height * 0.8f;
                        boxFontSize = 34;
                        break;
                    }
                default:
                    {
                        // Should never hit this, just bail and pick arbitrary size
                        boxWidth = 100f;
                        boxHeight = 100f;
                        break;
                    }
            }

            // Set placement of dialogue box
            float offsetX = Screen.width - boxWidth;
            float offsetY = Screen.height - boxHeight;
            switch (dialogueOptionMenuPlacement)
            {
                case DialogueOptionMenuPlacement.BOTTOM:
                    {
                        dialogueBox.GetComponent<RectTransform>().offsetMin = new Vector2(offsetX / 2.0f, 0); // left, bottom
                        dialogueBox.GetComponent<RectTransform>().offsetMax = new Vector2(-1.0f * (offsetX / 2.0f), -1.0f * offsetY); // -right, -top
                        break;
                    }
                case DialogueOptionMenuPlacement.CENTER:
                    {
                        dialogueBox.GetComponent<RectTransform>().offsetMin = new Vector2(offsetX / 2.0f, offsetY / 2.0f); // left, bottom
                        dialogueBox.GetComponent<RectTransform>().offsetMax = new Vector2(-1.0f * (offsetX / 2.0f), -1.0f * (offsetY / 2.0f)); // -right, -top
                        break;
                    }
                case DialogueOptionMenuPlacement.LEFT:
                    {
                        dialogueBox.GetComponent<RectTransform>().offsetMin = new Vector2(0, offsetY / 2.0f); // left, bottom
                        dialogueBox.GetComponent<RectTransform>().offsetMax = new Vector2(-1.0f * offsetX, -1.0f * (offsetY / 2.0f)); // -right, -top
                        break;
                    }
                case DialogueOptionMenuPlacement.RIGHT:
                    {
                        dialogueBox.GetComponent<RectTransform>().offsetMin = new Vector2(offsetX, offsetY / 2.0f); // left, bottom
                        dialogueBox.GetComponent<RectTransform>().offsetMax = new Vector2(0, -1.0f * (offsetY / 2.0f)); // -right, -top
                        break;
                    }
                case DialogueOptionMenuPlacement.TOP:
                    {
                        dialogueBox.GetComponent<RectTransform>().offsetMin = new Vector2(offsetX / 2.0f, offsetY); // left, bottom
                        dialogueBox.GetComponent<RectTransform>().offsetMax = new Vector2(-1.0f * (offsetX / 2.0f), 0); // -right, -top
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            currentlyDisplayedDialogueBox = dialogueBox;

            // Set up optional title text
            if (description != null)
            {
                GameObject descriptionObject = new GameObject("Description");
                Text descriptionText = descriptionObject.AddComponent<Text>();
                descriptionText.text = description;
                descriptionText.font = menuFont;
                descriptionText.fontSize = boxFontSize;
                descriptionText.color = menuTextColor;
                descriptionObject.transform.SetParent(dialogueBox.transform, false);
            }

            // Set up dialogue selections
            for (int i = 0; i < dialogueOptions.Length; i++)
            {
                string dialogueOption = dialogueOptions[i];
                Button dialogueOptionButton = Instantiate(naiveActionButton);
                Text buttonText = dialogueOptionButton.GetComponentInChildren<Text>();
                buttonText.text = dialogueOption;
                buttonText.font = menuFont;
                buttonText.fontSize = boxFontSize;
                dialogueOptionButton.name = dialogueOption;
                dialogueOptionButton.transform.SetParent(dialogueBox.transform, false);
                dialogueOptionButton.onClick.AddListener(delegate { HandleDialogueOptionClick(i, dialogueSelectionDelegate); });
            }
        }

        public void DestroyDialogueBox()
        {
            // TODO(laurenfrazier): Add a transition here, don't just make it disappear!
            Destroy(currentlyDisplayedDialogueBox);
        }
        /// <summary>
        /// Changes the cursor to the given sprite, or back to the default sprite if null.
        /// </summary>
        public void ChangeCursor(Sprite cursorSprite = null)
        {
            //reset the cursor
            if (cursorSprite == null)
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                return;
            }

            var croppedTexture = new Texture2D((int)cursorSprite.rect.width, (int)cursorSprite.rect.height);
			var pixels = cursorSprite.texture.GetPixels((int)cursorSprite.textureRect.x,
                                                    (int)cursorSprite.textureRect.y,
                                                    (int)cursorSprite.textureRect.width,
                                                    (int)cursorSprite.textureRect.height);
            croppedTexture.SetPixels(pixels);
            croppedTexture.Apply();

            Cursor.SetCursor(croppedTexture, Vector2.zero, CursorMode.Auto);
        }

        public void HandleActionButtonClick(CharacterActionType characterActionType)
        {
            InputSystemManager.Instance.currentlySelectedActionType = characterActionType;
            Debug.Log(characterActionType);
        }

        public void HandleSystemAction(SystemMenuButtonOptions buttonAction)
        {
            Debug.Log(buttonAction);
            switch (buttonAction)
            {
                case SystemMenuButtonOptions.SAVE:
                    {
                        PersistentDataManager.Instance.Save();
                        break;
                    }
                case SystemMenuButtonOptions.QUIT:
                    {
                        if (Application.platform != RuntimePlatform.IPhonePlayer)
                        {
                            Application.Quit();
                        }
                        break;
                    }
                case SystemMenuButtonOptions.SETTINGS:
                    {
                        // TODO(laurenfrazier): Show settings screen!
                        break;
                    }
                case SystemMenuButtonOptions.CLOSEMENU:
                    {
                        CloseSystemMenu();
                        break;
                    }
                case SystemMenuButtonOptions.LOAD:
                    {
                        // TODO(laurenfrazier): Show load saved game screen!
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Initial setup call. Sets up all the other UIs and enables the appropriate ones.
        /// </summary>
        private void SetUpGameTypeUI()
        {
            switch (InputSystemManager.Instance.adventureGameType)
            {
                case AdventureGameType.NAIVE:
                    {
                        if (naiveActionUI != null && autoCreateButtons)
                        {
                            SetUpNaiveActionButtonUI();
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

        private void SetUpNaiveActionButtonUI()
        {
            naiveActionUI.SetActive(true);
            foreach (InputSystemManager.CharacterAction characterAction in InputSystemManager.Instance.characterActions)
            {
                if (naiveActionButton != null)
                {
                    Button characterActionButton = Instantiate(naiveActionButton);
                    // TODO(laurenfrazier): Icon?
                    characterActionButton.GetComponentInChildren<Text>().text = characterAction.actionName;
                    characterActionButton.name = characterAction.actionName;

                    characterActionButton.transform.SetParent(naiveActionUI.transform, false);
                    characterActionButton.onClick.AddListener(delegate { HandleActionButtonClick(characterAction.actionType); });
                }
            }
        }

        private void HandleDialogueOptionClick(int dialogueOption, DialogueSelectionDelegate dialogueSelectionDelegate = null)
        {
            if (dialogueSelectionDelegate != null)
            {
                dialogueSelectionDelegate(dialogueOption);
            }
            DestroyDialogueBox();
        }

        private void SetUpCursor()
        {
            if (defaultMouseCursor != null)
            {
                // TODO(laurenfrazier): Set up cursor
            }
        }

        private void HandleAdvanceDialogue () {
            if (dialogueAdvanceDelegate != null) {
                dialogueAdvanceDelegate();
            }
            if (currentlyDisplayedDialogue != null) {
                Destroy(currentlyDisplayedDialogue.gameObject);
            }
            awaitingDialogueAdvance = false;
            screenTouchPanel.SetActive(false);
        }
        #endregion
    }
}

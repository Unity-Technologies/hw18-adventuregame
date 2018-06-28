using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UnityEngine.AdventureGame
{
    public class DialogueMenu : MonoBehaviour
    {

        #region Public Variables
        public Button buttonPrefab;
        public Font titleFont;
        public int titleFontSize = 14;
        public Font descriptionFont;
        public int descriptionFontSize = 14;
        public Font buttonFont;
        public int buttonFontSize = 14;
        #endregion

        #region Private Variables
        private VerticalLayoutGroup layoutGroup;
        #endregion

        #region Public Methods
        public void Awake()
        {
            if (buttonPrefab == null)
            {
                Debug.LogError("Attempted to create dialogue menu without button prefab set!");
            }
            if (titleFont == null)
            {
                titleFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            if (descriptionFont == null)
            {
                descriptionFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            if (buttonFont == null)
            {
                buttonFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            layoutGroup = GetComponentInChildren<VerticalLayoutGroup>();
			if (layoutGroup == null) {
				Debug.Log("Layout group is null");
			}
        }

        public void AddTitle(string title)
        {
            Debug.Log("adding title:" + title);
			AddText(title, titleFont, titleFontSize);
        }

        public void AddDescription(string description)
        {
            Debug.Log("adding description:" + description);
			AddText(description, descriptionFont, descriptionFontSize);
        }

        public void AddButton(string buttonTitle, UnityAction buttonAction)
        {
            Debug.Log("adding button:" + buttonTitle + "\naction: " + buttonAction);
        }
        #endregion

        #region Private Methods
		private void AddText(string text, Font font, int size) {
			GameObject dialogueObject = new GameObject(text);
            Text dialogueText = dialogueObject.AddComponent<Text>();
            ContentSizeFitter sizeFitter = dialogueObject.AddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            dialogueText.text = text;
            dialogueText.font = font;
            dialogueText.fontSize = size;
			if (layoutGroup != null) {
				dialogueObject.transform.SetParent(layoutGroup.transform, false);
			}
		}
        #endregion
    }
}

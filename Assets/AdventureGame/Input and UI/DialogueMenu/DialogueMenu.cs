using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.AdventureGame
{
    public class DialogueMenu : MonoBehaviour
    {
		public void AddTitle(string title) {
			Debug.Log("adding title:" + title);
		}

		public void AddDescription(string description) {
			Debug.Log("adding description:" + description);
		}

		public void AddButton(string buttonTitle) {
			Debug.Log("adding button:" + buttonTitle);
		}
    }
}

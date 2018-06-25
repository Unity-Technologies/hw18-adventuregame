using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace UnityEngine.AdventureGame
{
    /// <summary>
    /// The type of the game. Currently supported options include VerbCoin and Sierra. 
    /// </summary>
    public enum AdventureGameType {
        SIERRA, // User selects an action before selecting the object to perform it on
        VERBCOIN, // User selects an object and then selects an action to perform on it
    }

    /// <summary>
    /// Handles user selections of objects and menu actions
    /// ex. Walk, Speak, Pick Up
    /// </summary>
    public sealed class InputSystemManager : MonoBehaviour
    {
#region Internal Classes
        /// <summary>
        /// An action that a character can take.
        /// ex. Walk, Speak, Pick Up
        /// (Defined inside InputSystemManager to allow easy editing of available options in Editor)
        /// </summary>
        [System.Serializable]
        public class CharacterAction
        {
            public string actionName;
            public string actionDescription;
            public Sprite actionIcon;
            public CharacterActionType actionType;
        }
#endregion

#region Public Variables
        // Static singleton
        public static InputSystemManager Instance
        {
            get
            {
                if (instance == null){
                    instance = new InputSystemManager();
                }
                return instance;
            }
        }

        // The type of adventure game. This will mostly impact the way the user
        // interacts with game object (ex. select object then action vs. select action then object)
        public AdventureGameType adventureGameType = AdventureGameType.SIERRA;

        // The potential actions that a user can take.
        public CharacterAction[] characterActions;

        // Callback that returns the currently selected action (or the result of a menu selection)
        public delegate void ActionSelectionDelegate(CharacterActionType selectedType = CharacterActionType.NONE);
#endregion

#region Private Variables
        private static InputSystemManager instance;
        // The currently selected action (ACTION1, ACTION2, ..., NONE)
        private CharacterActionType currentlySelectedActionType = CharacterActionType.NONE;
#endregion

#region Public Methods
        public void BeginActionSelection(CharacterActionType[] allowedTypes, ActionSelectionDelegate actionSelectionDelegate) {
            switch (adventureGameType) {
                case AdventureGameType.SIERRA: {
                    if (actionSelectionDelegate != null) {
                        actionSelectionDelegate(currentlySelectedActionType);
                    }
                    break;
                }
                case AdventureGameType.VERBCOIN: {
                    // TODO: Display menu here with allowedTypes
                    // Call delegate with result of action selection

                    // Code to unblock Audrey, remove when real menu selection is done!
                    if (actionSelectionDelegate != null) {
                        actionSelectionDelegate(CharacterActionType.LOOKAT); // Pass result of menu selection as param here!
                    }
                    break;
                }
                default: {
                    if (actionSelectionDelegate != null) {
                        actionSelectionDelegate();
                    }
                    break;
                }
            }
        }
#endregion

#region Private Methods
        private InputSystemManager() {}

        private void Awake () {
            DontDestroyOnLoad(this);
        }
#endregion
    }
}
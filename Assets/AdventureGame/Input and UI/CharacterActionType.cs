using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEngine.AdventureGame
{
    /// <summary>
    /// The type of action. The user will define the name, but we will refer
    /// to the opaque action type under the hood.
    /// The max number of 8 actions is an arbitrary number and can be expanded.
    /// </summary>
    public enum CharacterActionType {
        SPEAK,
        PICKUP,
        LOOKAT,
        USE,
        USE_SELECTED_OBJECT,
        ACTION1,
        ACTION2,
        ACTION3,
        ACTION4,
        NONE,
    }
}

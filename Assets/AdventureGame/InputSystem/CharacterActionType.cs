using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEngine.AdventureGame
{
    /// <summary>
    /// The type of action. The user will define the name, but we will refer
    /// to the opaque action type under the hood. Whenever possible, the actions
    /// will be displayed in ascending order.
    /// The max number of 10 actions is an arbitrary number.
    /// </summary>
    public enum CharacterActionType {
        ACTION1,
        ACTION2,
        ACTION3,
        ACTION4,
        ACTION5,
        ACTION6,
        ACTION7,
        ACTION8,
        ACTION9,
        ACTION10,
        NONE,
    }
}

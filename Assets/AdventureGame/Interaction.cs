using System;
using UnityEngine;
using UnityEngine.AdventureGame;
using UnityEngine.Events;

/// <summary>
/// An interaction between the player and an Interactable.
/// TODO Change from UnityEvent to scripting system on ce available.
/// </summary>
[Serializable]
public struct Interaction
{
    [SerializeField]
    CharacterActionType m_Action;
    [SerializeField]
    UnityEvent m_Reaction;

    /// <summary>
    /// The player's action.
    /// </summary>
    public CharacterActionType Action
    {
        get { return m_Action; }
    }

    /// <summary>
    /// The Interectable's reaction.
    /// </summary>
    public UnityEvent Reaction
    {
        get { return m_Reaction; }
    }
}

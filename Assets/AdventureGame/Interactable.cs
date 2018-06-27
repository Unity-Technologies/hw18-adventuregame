using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AdventureGame;

/// <summary>
/// An Interactable actor in the world.
/// Could be an object or an NPC.
/// </summary>
public class Interactable : MonoBehaviour
{
    [SerializeField]
	Interaction[] m_Interactions;

	CharacterActionType[] m_PossibleActions;

	/// <summary>
	/// Caches a list of the allowed action types when enabled since they will not change at runtime.
	/// </summary>
	void OnEnable()
	{
		List<CharacterActionType> actions = new List<CharacterActionType>();

		for (int i = 0; i < m_Interactions.Length; ++i)
		{
			if (!actions.Contains(m_Interactions[i].Action))
			{
				actions.Add(m_Interactions[i].Action);
			}
		}

		m_PossibleActions = actions.ToArray();
	}

	/// <summary>
	/// Event called when the player attempts to interact with the object.
	/// Retrieves the desired action from the InputSystem the performs the interaction.
	/// </summary>
	public void OnInteracted()
	{
		Debug.Log("Clickable Item Clicked");
		InputSystemManager.Instance.SelectAction(m_PossibleActions, PerformInteraction);
	}

    public void OnInteracted(InventoryItem item)
	{
		Debug.Log("Used " + item.Id);
		//TODO use game logic to determine if interaction is valid
		InventoryManager.Instance.RemoveItem(item);
	}

	/// <summary>
	/// Perform all Interactions associated with the given action.
	/// </summary>
	/// <param name="action"></param>
	void PerformInteraction(CharacterActionType action)
	{
		for (int i = 0; i < m_Interactions.Length; ++i)
		{
			if (m_Interactions[i].Action == action && m_Interactions[i].Reaction != null)
			{
                Debug.Log("Interaction " + action);
			    StartCoroutine(m_Interactions[i].Reaction.Execute());
			    return;
			}
		}

        AdventureGameOverlayManager.Instance.DisplayCharacterDialogue("I can't do that.", "Character");
	}
}

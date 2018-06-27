using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateInfo {

	private static HashSet<string> finishedStoryEvents = new HashSet<string>();

	public static bool IsStoryEventFinished(string storyEvent)
	{
		return finishedStoryEvents.Contains(storyEvent);
	}

	public static void AddFinishedStoryEvent(string storyEvent)
	{
		finishedStoryEvents.Add(storyEvent);
	}


}

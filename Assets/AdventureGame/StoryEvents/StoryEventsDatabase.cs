using System.Collections;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

public class StoryEventsDatabase : ScriptableObject
{
	public List<string> events;
	
	[UsedImplicitly]
	[InitializeOnLoadMethod]
	public static void CreateIfMissing()
	{
		EditorApplication.delayCall += () =>
		{
			var storyEventDatabasePath = "Assets/AdventureGame/StoryEvents/StoryEventsDatabase.asset";
			var storyEventDatabase = AssetDatabase.LoadAssetAtPath<StoryEventsDatabase>(storyEventDatabasePath);
			if (storyEventDatabase == null)
			{
				storyEventDatabase = ScriptableObject.CreateInstance<StoryEventsDatabase>();
				AssetDatabase.CreateAsset(storyEventDatabase, storyEventDatabasePath);
				AssetDatabase.SaveAssets();
			}
		};
	}
}

using System.Collections;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

public class StoryEventsDatabase : ScriptableObject
{
	private static StoryEventsDatabase instance;

	public static StoryEventsDatabase StoryEventDatabase
	{
		get
		{
			if (instance == null)
			{
				instance = AssetDatabase.LoadAssetAtPath<StoryEventsDatabase>(storyEventDatabasePath);
			}

			return instance;
		}
	}

	public List<string> events;
	
	public static string storyEventDatabasePath = "Assets/AdventureGame/StoryEvents/StoryEventsDatabase.asset";

	[InitializeOnLoadMethod]
	public static void CreateIfMissing()
	{
		EditorApplication.delayCall += () =>
		{
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

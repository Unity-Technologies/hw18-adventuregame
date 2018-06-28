using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AdventureGame;

public class StoryEventsEditorWindow : EditorWindow
{
	public StoryEventsDatabase storyEvents;

	private int currentlyChangingNameIndex = -1;

	[MenuItem("Adventure Game/Story Events Window &e")]
	public static void OpenWindow()
	{
		GetWindow<StoryEventsEditorWindow>("Story Events", true, typeof(SceneView));
	}

	void OnEnable()
	{
		storyEvents = AssetDatabase.LoadAssetAtPath<StoryEventsDatabase>(StoryEventsDatabase.storyEventDatabasePath);
		titleContent.text = "Story events";
	}

	void OnGUI()
	{
		GUILayout.Space(20);

		if (storyEvents != null && storyEvents.events != null)
		{
			GUILayout.BeginHorizontal();

			if (GUILayout.Button("Add Item", GUILayout.ExpandWidth(false)))
			{
				AddEvent();
			}

			GUILayout.EndHorizontal();
			if (storyEvents.events.Count > 0)
			{
				for (int i = 0; i < storyEvents.events.Count; i++)
				{
					GUILayout.BeginHorizontal();

					if (currentlyChangingNameIndex == i)
					{
						var changedTextInField = EditorGUILayout.TextField(storyEvents.events[i], GUILayout.ExpandWidth(false));
						var storyEventExists = storyEvents.events.Exists((x) => string.Equals(x, changedTextInField));
						var indexOfExistingElement = storyEvents.events.FindIndex((x) => string.Equals(x, changedTextInField));

						if (string.Equals(string.Empty, changedTextInField))
						{
							EditorGUILayout.LabelField(" - you can't leave empty event name", GUILayout.ExpandWidth(true));
						}
						if (!storyEventExists || indexOfExistingElement == i)
						{
							ChangeEvent(i, changedTextInField);
							if (GUILayout.Button("Save Changes", GUILayout.ExpandWidth(false)))
							{
								currentlyChangingNameIndex = -1;
								AssetDatabase.SaveAssets();
							}
						}
						else
						{
							EditorGUILayout.LabelField(" - you can't add same event", GUILayout.ExpandWidth(true));
						}
					}
					else
					{
						EditorGUILayout.LabelField("- " + storyEvents.events[i], "", GUILayout.ExpandWidth(false));

						if (GUILayout.Button("Change Event", GUILayout.ExpandWidth(false)))
						{
							currentlyChangingNameIndex = i;
						}

						if (GUILayout.Button("Delete Event", GUILayout.ExpandWidth(false)))
						{
							DeleteEvent(i);
						}
					}

					GUILayout.EndHorizontal();
				}
			}
			else
			{
				GUILayout.Label("This events list is empty.");
			}

			if (GUI.changed)
			{
				EditorUtility.SetDirty(storyEvents);
			}
		}
	}

	private void AddEvent()
	{
		string newEvent = "New event";
		storyEvents.events.Add(newEvent);
		AssetDatabase.SaveAssets();
	}

	void ChangeEvent(int index, string newEventName)
	{
		storyEvents.events[index] = newEventName;
	}

	void DeleteEvent(int index)
	{
		storyEvents.events.RemoveAt(index);
		AssetDatabase.SaveAssets();
	}
}

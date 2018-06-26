using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AdventureGame;

public class StoryEventsEditorWindow : EditorWindow
{
	public StoryEventsDatabase storyEvents;

	private int currentlyChangingNameIndex = -1;

	[MenuItem("Story events/Open")]
	public static void OpenWindow()
	{
		GetWindow<StoryEventsEditorWindow>();
	}

	void OnEnable()
	{
		storyEvents = AssetDatabase.LoadAssetAtPath<StoryEventsDatabase>(StoryEventsDatabase.storyEventDatabasePath);
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
						var changedTextInField = EditorGUILayout.TextField("- ", storyEvents.events[i], GUILayout.ExpandWidth(false));

						if (!storyEvents.events.Exists((x) => string.Equals(x, changedTextInField)) || storyEvents.events.FindAll((x) => string.Equals(x, changedTextInField)).Count <= 1)
						{
							storyEvents.events[i] = changedTextInField;
							if (GUILayout.Button("Save Changes", GUILayout.ExpandWidth(false)))
							{
								currentlyChangingNameIndex = -1;
							}
						}
						else
						{
							EditorGUILayout.LabelField(" - you can't add event with same name which already exists ", "", GUILayout.ExpandWidth(false));
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
	}

	void DeleteEvent(int index)
	{
		storyEvents.events.RemoveAt(index);
	}
}

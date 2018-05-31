using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ResaveScenePrefabs
{

	[MenuItem("Adventure Game/Reload Scene Prefabs #r")]
	static void MenuItemReloadScenePrefabs()
	{
		SceneManager sceneManager = (SceneManager)Object.FindObjectOfType(typeof(SceneManager));
		if (sceneManager == null)
		{
			Debug.LogWarning("Could not find SceneManager in the scene! Make sure to add a SceneManager component to a game component in your scene.");
		}
		else
		{
			sceneManager.ReloadScenePrefabs();
		}
	}
	
	[MenuItem("Adventure Game/Save Scene Prefabs #s")]
	static void MenuItemSaveScenePrefabs()
	{
		SceneManager sceneManager = (SceneManager)Object.FindObjectOfType(typeof(SceneManager));
		if (sceneManager == null)
		{
			Debug.LogWarning("Could not find SceneManager in the scene! Make sure to add a SceneManager component to a game component in your scene.");
		}
		else
		{
		    sceneManager.SaveScenePrefabs();
		}
	}
	
}

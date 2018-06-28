using System;
using UnityEditor;
using UnityEngine;

public class DialogInputField : EditorWindow {
    Action<string> callback;
    string sceneName = "";

    public void RegisterCallback(Action<string> callback) {
        this.callback = callback;
    }

    void OnGUI() {
        sceneName = EditorGUILayout.TextField("Scene name", sceneName);
 
        if (GUILayout.Button("OK")) {
            callback(sceneName);
            Close();
        }
    }
}
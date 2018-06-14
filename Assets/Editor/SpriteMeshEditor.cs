using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(SpriteMesh))]
public class SpriteMeshEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SpriteMesh spriteMesh = (SpriteMesh)target;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_sprite"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_detail"));

        if (GUILayout.Button("Regenerate Mesh"))
        {
            spriteMesh.RegenerateMesh();
        }

        serializedObject.ApplyModifiedProperties();
    }
}

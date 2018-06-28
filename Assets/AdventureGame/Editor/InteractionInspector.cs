
using System.IO;
using UnityEngine;
using UnityEngine.AdventureGame;

namespace UnityEditor.AdventureGame
{
    [CustomPropertyDrawer(typeof(Interaction))]
    public class InteractableEditor : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + 50;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty actionProperty = property.FindPropertyRelative("m_Action");
            EditorGUI.BeginProperty(position, label, property);

            var actionRect = new Rect(position.x, position.y, position.width, EditorGUI.GetPropertyHeight(actionProperty));
            EditorGUI.PropertyField(actionRect, actionProperty, GUIContent.none);
            EditorGUI.GetPropertyHeight(actionProperty);

            var buttonRect = new Rect(position.x + 15, position.y + EditorGUI.GetPropertyHeight(actionProperty) + EditorGUIUtility.standardVerticalSpacing, position.width - 15.0f, 40.0f);
            SerializedProperty reactionProperty = property.FindPropertyRelative("m_Reaction");
            Color oldColor = GUI.color;
            GUI.color = Color.yellow;
            if (GUI.Button(buttonRect, "Edit Game Logic"))
            {
                string actionType = actionProperty.enumDisplayNames[actionProperty.enumValueIndex];
                string assetPath = AssetDatabase.GetAssetPath(reactionProperty.objectReferenceValue);
                if (reactionProperty.objectReferenceValue == null || !assetPath.Contains(actionType))
                {
                    MonoBehaviour behaviour = property.serializedObject.targetObject as MonoBehaviour;
                    if (behaviour.transform.parent == null)
                    {
                        Debug.LogErrorFormat("Interactable must be inside of a scene prefab!");
                        return;
                    }
                    GameObject root = PrefabUtility.FindValidUploadPrefabInstanceRoot(behaviour.transform.parent.gameObject);
                    if (root == null)
                    {
                        Debug.LogErrorFormat("Interactable must be inside of a scene prefab!");
                        return;
                    }
                    string gameLogicPath = Path.Combine(Path.Combine(SceneManager.Instance.m_outputPath, root.name), "GameLogic");
                    Directory.CreateDirectory(gameLogicPath);
                    string gameLogicAssetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(gameLogicPath, string.Format("{0}-{1}.asset", behaviour.name, actionProperty.enumDisplayNames[actionProperty.enumValueIndex])));

                    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<GameLogicData>(), gameLogicAssetPath);
                    AssetDatabase.SaveAssets();

                    reactionProperty.objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameLogicData>(gameLogicAssetPath);
                }

                GameLogicGraphViewWindow.OpenWindow((GameLogicData)reactionProperty.objectReferenceValue);
            }
            GUI.color = oldColor;
            EditorGUI.EndProperty();
        }
    }
}

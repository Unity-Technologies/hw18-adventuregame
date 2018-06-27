using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace Unity.Adventuregame {
    public class DialogueGraphViewWindow : SampleGraphViewWindow {
        
        [MenuItem("Dialogue/Open Window")]
        public static void OpenWindow()
        {
            GetWindow<DialogueGraphViewWindow>();
        }

        public override bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context) {
            if (entry.name == "Create Dialogue") {
                var node = new DialogueNode();

                var characterGameObject = new ObjectField
                {
                    objectType = typeof(GameObject),
                    allowSceneObjects = true
                };

                node.mainContainer.Insert(1, characterGameObject);        
                m_GraphView.AddElement(node);
                node.SetPosition(new Rect(new Vector2(10, 100), Vector2.zero));

                m_GraphView.AddElement(node);

                Vector2 pointInWindow = context.screenMousePosition - position.position;
                Vector2 pointInGraph = node.parent.WorldToLocal(pointInWindow);

                node.SetPosition(new Rect(pointInGraph, Vector2.zero)); // it's ok to pass zero here because width/height is dynamic

                node.Select(m_GraphView, false);
                return true;
            }
            return base.OnSelectEntry(entry, context);
        }
    }
}
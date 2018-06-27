using System.Collections.Generic;
using System.Linq;
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

                var characterName = new TextField()
                {
                    multiline = false,
                    value = "<CharacerName Here>"
                };

                node.mainContainer.Insert(1, characterName);        
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

        public override void SaveGraphData(List<Node> nodes, string outputPath)
        {
            SerializableDialogData dialogGraphData = ScriptableObject.CreateInstance<SerializableDialogData>();
            dialogGraphData.m_dialogNodes = new List<SerializableDialogData.SerializableDialogNode>();

                            string inputString = string.Empty;
                string outPutString = string.Empty;

            for (int i = 0; i < nodes.Count; ++i)
            {
                SerializableDialogData.SerializableDialogNode dialogGraphNode = new SerializableDialogData.SerializableDialogNode();
                Node currentNode = nodes[i];

                dialogGraphNode.m_title = currentNode.title;
                dialogGraphNode.m_position = currentNode.GetPosition().position;
                dialogGraphNode.m_outputs = new List<SerializableDialogData.SerializableDialogEdge>();

                foreach (VisualElement element in currentNode.mainContainer)
                {
                    if (element is TextField)
                    {
                        dialogGraphNode.m_speakingCharacterName = ((TextField) element).value;
                    }
                }

                foreach (VisualElement element in currentNode.inputContainer)
                {
                    if (element is TextField)
                    {
                        inputString = ((TextField)element).value;
                    }
                }

                foreach (VisualElement element in currentNode.outputContainer)
                {
                    if (element is TextField)
                    {

                        outPutString = ((TextField) element).value;
                    }
                    if (element is Port)
                    {
                        foreach (Edge edge in ((Port)element).connections)
                        {
                            SerializableDialogData.SerializableDialogEdge serializedEdge =
                                new SerializableDialogData.SerializableDialogEdge();
                            if (dialogGraphNode.m_title == "Start")
                            {
                                serializedEdge.m_sourcePort = currentNode.outputContainer.IndexOf(edge.output);
                                serializedEdge.m_targetNode = nodes.IndexOf(edge.input.node);
                            }
                            else
                            {
                                serializedEdge.m_sourcePort = currentNode.outputContainer.IndexOf(edge.output);
                                serializedEdge.m_targetPort = edge.input.node.inputContainer.IndexOf(edge.input);

                                serializedEdge.m_outputDialog = outPutString;
                                if (serializedEdge.m_outputDialog != string.Empty)
                                {
                                    outPutString = string.Empty;
                                }
                                serializedEdge.m_targetNode = nodes.IndexOf(edge.input.node);

                                serializedEdge.m_inputDialog = inputString;
                                if (serializedEdge.m_inputDialog != string.Empty)
                                {
                                    inputString = string.Empty;
                                }                                                       
                            }

                            dialogGraphNode.m_outputs.Add(serializedEdge);
                        }
                    }
                }
                dialogGraphData.m_dialogNodes.Add(dialogGraphNode);
            }

            AssetDatabase.CreateAsset(dialogGraphData, outputPath);
            AssetDatabase.SaveAssets();
        }
    }
}
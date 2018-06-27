using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

public class SampleGraphViewWindow : GraphViewWindow
{
    const string k_TestGraphDataPath = "Assets/AdventureGame/SampleGraphView/Test.asset";

    //[MenuItem("SampleGraphView/Open Window")]
    public static void OpenWindow()
    {
        GetWindow<SampleGraphViewWindow>();
    }

	// Use this for initialization
	void OnEnable()
	{
        var sampleGraphView = new SampleGraphView();
        m_GraphView = sampleGraphView;

        m_GraphView.name = "theView";
        m_GraphView.persistenceKey = "theView";
        m_GraphView.StretchToParentSize();

        this.GetRootVisualContainer().Add(m_GraphView);

        if (!LoadGraphData(k_TestGraphDataPath))
        {
	        Node node = CreateRootNode();
	        m_GraphView.AddElement(node);
        }

        m_GraphView.graphViewChanged += OnGraphViewChanged;
        m_GraphView.nodeCreationRequest += OnRequestNodeCreation;
    }

    Node CreateRootNode()
    {
        Node node = new Node();
        node.title = "Start";
        node.capabilities &= ~(Capabilities.Movable | Capabilities.Deletable);
        node.style.backgroundColor = Color.green;
        node.SetPosition(new Rect(new Vector2(-50, -50), new Vector2(100, 100)));

        Port outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
        outputPort.portName = "start";
        outputPort.userData = null;
        
        node.outputContainer.Add(outputPort);
        return node;
    }

    Node DeserializeNode(SerializableGraphData.SerializableGraphNode graphNode)
    {
        Node node = graphNode.m_title == "Start" ? CreateRootNode() : CreateNode(graphNode.m_title);
        node.SetPosition(new Rect(graphNode.m_position, Vector2.zero));
        return node;
    }

    Node CreateNode(string title)
    {
        Node node = new Node();
        node.title = title;
        
        node.capabilities |= Capabilities.Movable;
        Port inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
        inputPort.portName = "in port";
        inputPort.userData = null;
        node.inputContainer.Add(inputPort);

        Port outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
        outputPort.portName = "out port";
        outputPort.userData = null;
        node.outputContainer.Add(outputPort);

        return node;
    }

    public void SaveGraphData(string outputPath)
    {
        List<Node> nodes = m_GraphView.nodes.ToList();
        SerializableGraphData graphData = ScriptableObject.CreateInstance<SerializableGraphData>();
        graphData.m_graphNodes = new List<SerializableGraphData.SerializableGraphNode>();
        for (int i = 0; i < nodes.Count; ++i)
        {
            SerializableGraphData.SerializableGraphNode graphNode = new SerializableGraphData.SerializableGraphNode();
            Node currentNode = nodes[i];

            graphNode.m_title = currentNode.title;
            graphNode.m_position = currentNode.GetPosition().position;
            graphNode.m_outputs = new List<SerializableGraphData.SerializableGraphEdge>();

            foreach (Port port in currentNode.outputContainer)
            {
                foreach (Edge edge in port.connections)
                {
                    SerializableGraphData.SerializableGraphEdge serializedEdge = new SerializableGraphData.SerializableGraphEdge();
                    serializedEdge.m_sourcePort = currentNode.outputContainer.IndexOf(edge.output);
                    serializedEdge.m_targetNode = nodes.IndexOf(edge.input.node);
                    serializedEdge.m_targetPort = edge.input.node.inputContainer.IndexOf(edge.input);
                    graphNode.m_outputs.Add(serializedEdge);
                }
            }
            graphData.m_graphNodes.Add(graphNode);
        }

        AssetDatabase.CreateAsset(graphData, outputPath);
        AssetDatabase.SaveAssets();
    }
}

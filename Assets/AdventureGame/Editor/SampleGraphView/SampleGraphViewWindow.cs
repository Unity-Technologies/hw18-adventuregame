using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

public class SampleGraphViewWindow : EditorWindow, ISearchWindowProvider
{
    class MyDataPort
    {
        public int data;
    }

    SampleGraphView m_GraphView;

    [MenuItem("SampleGraphView/Open Window")]
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

	    m_GraphView.graphViewChanged = GraphViewChanged;

        this.GetRootVisualContainer().Add(m_GraphView);


	    Node node1 = CreateNode(8);
        m_GraphView.AddElement(node1);
        node1.SetPosition(new Rect(new Vector2(10, 100), Vector2.zero));
        
	    Node node2 = CreateNode(4);
        m_GraphView.AddElement(node2);
        node1.SetPosition(new Rect(new Vector2(100, 20), Vector2.zero));

        m_GraphView.nodeCreationRequest += OnRequestNodeCreation;
    }

    protected void OnRequestNodeCreation(NodeCreationContext context)
    {
        SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), this);
    }

    public GraphViewChange GraphViewChanged(GraphViewChange change)
    {
        if (change.edgesToCreate != null)
        {
            foreach (var edge in change.edgesToCreate)
            {
                Debug.Log("input: " +
                    (edge.input.userData as MyDataPort).data +
                    " output: " +
                    (edge.output.userData as MyDataPort).data);
            }
        }

        return change;
    }

    Node CreateNode(int num)
    {
        Node node = new Node();
        node.title = "hello";
        node.capabilities |= Capabilities.Movable;

        var dataPort = new MyDataPort() { data = num + 4 };
        var dataPort1 = new MyDataPort() { data = num + 8 };

        var inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
        inputPort.portName = "in port";
        inputPort.userData = dataPort;
        
        var outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
        outputPort.portName = "out port";
        outputPort.userData = dataPort1;

        node.inputContainer.Add(inputPort);
        node.outputContainer.Add(outputPort);

        return node;
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        var tree = new List<SearchTreeEntry>();
        tree.Add(new SearchTreeGroupEntry(new GUIContent("Create Node"), 0));

        Texture2D icon = null;
        tree.Add(new SearchTreeGroupEntry(new GUIContent("Test Category"), 1));
        tree.Add(new SearchTreeEntry(new GUIContent("Test Item1", icon)) { level = 2, userData = typeof(SampleGraphView) });
        tree.Add(new SearchTreeEntry(new GUIContent("Test Item2", icon)) { level = 2, userData = typeof(SampleGraphView) });
        tree.Add(new SearchTreeEntry(new GUIContent("Test Item3", icon)) { level = 2, userData = typeof(SampleGraphView) });
        tree.Add(new SearchTreeEntry(new GUIContent("Test Item4", icon)) { level = 2, userData = typeof(SampleGraphView) });
        tree.Add(new SearchTreeEntry(new GUIContent("Test Item5", icon)) { level = 2, userData = typeof(SampleGraphView) });

        return tree;
    }

    public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
    {
        if (!(entry is SearchTreeGroupEntry))
        {
            Node node = CreateNode(8);
            m_GraphView.AddElement(node);
            node.SetPosition(new Rect(new Vector2(10, 100), Vector2.zero));

            m_GraphView.AddElement(node);

            Vector2 pointInWindow = context.screenMousePosition - position.position;
            Vector2 pointInGraph = node.parent.WorldToLocal(pointInWindow);

            node.SetPosition(new Rect(pointInGraph, Vector2.zero)); // it's ok to pass zero here because width/height is dynamic

            node.Select(m_GraphView, false);
            return true;
        }
        return false;
    }
}

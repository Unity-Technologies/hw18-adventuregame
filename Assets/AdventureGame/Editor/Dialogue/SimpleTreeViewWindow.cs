using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UnityEditor;

class SimpleTreeViewWindow : EditorWindow
{
    [SerializeField] TreeViewState m_TreeViewState;

    TreeViewCollection m_SimpleTreeView;

    void OnEnable ()
    {
        if (m_TreeViewState == null)
            m_TreeViewState = new TreeViewState ();

        m_SimpleTreeView = new TreeViewCollection(m_TreeViewState);
    }

    void OnGUI ()
    {
        m_SimpleTreeView.OnGUI(new Rect(0, 0, position.width, position.height));
    }

    [MenuItem ("Dialogue/TreeView")]
    static void ShowWindow ()
    {
        var window = GetWindow<SimpleTreeViewWindow> ();
        window.titleContent = new GUIContent ("My Window");
        window.Show ();
    }
}
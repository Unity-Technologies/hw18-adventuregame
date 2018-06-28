using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UnityEditor;
using System;

namespace Unity.Adventuregame.Dialogue {
    class SimpleTreeViewWindow : EditorWindow
    {
        EditorWindow currentWindow;

        [SerializeField] TreeViewState m_TreeViewState;

        TreeViewCollection m_SimpleTreeView;
        TreeViewItem chosen;
        double clickTime;
        double doubleClickTime = 0.3;

        void OnEnable ()
        {
            if (m_TreeViewState == null)
                m_TreeViewState = new TreeViewState ();

            m_SimpleTreeView = new TreeViewCollection(m_TreeViewState);
        }

        private void OpenWindow(TreeViewItem treeViewItem)
        {
            if (currentWindow != null) {
                currentWindow.Close();
            }
            var titleList = new List<string>();
            while (treeViewItem.parent != null) {
                titleList.Add(treeViewItem.displayName);
                treeViewItem = treeViewItem.parent;
            }
            titleList.Reverse();
            currentWindow = GetWindow<DialogueGraphViewWindow>(String.Join(" ", titleList.ToArray()));
        }

        void OnGUI ()
        {
            var e = Event.current;

            if (e.type == EventType.MouseUp && e.button == 1) {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("new child"), false, Newchild, m_SimpleTreeView.GetRows()[m_SimpleTreeView.GetSelection()[0] - 1]);
                menu.ShowAsContext();
            } else if (e.type == EventType.MouseDown && e.button == 0) {
                if ((EditorApplication.timeSinceStartup - clickTime) < doubleClickTime)
                    OpenWindow(m_SimpleTreeView.GetRows()[m_SimpleTreeView.GetSelection()[0] - 1]);

                clickTime = EditorApplication.timeSinceStartup;
            }

            m_SimpleTreeView.OnGUI(new Rect(0, 0, position.width, position.height));
        }

        void Newchild(object obj) {
            chosen = obj as TreeViewItem;
            if (chosen != null) {
                var inputField = GetWindow<DialogInputField>();
                inputField.RegisterCallback(AddChild);
            }
        }

        void AddChild(string name) {
            if (chosen != null) {
                chosen.AddChild(new TreeViewItem { id = m_SimpleTreeView.GetAndIncrementID(), depth = chosen.depth + 1, displayName = name });
            }
        }

        [MenuItem ("Dialogue/TreeView")]
        static void ShowWindow ()
        {
            var window = GetWindow<SimpleTreeViewWindow> ();
            window.titleContent = new GUIContent ("My Window");
            window.Show ();
        }
    }
}
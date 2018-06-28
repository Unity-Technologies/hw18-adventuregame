using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

class TreeViewCollection : TreeView
{
    int currentID = 10;

    public int GetAndIncrementID() {
        return currentID++;
    }

    public TreeViewCollection(TreeViewState treeViewState)
        : base(treeViewState)
    {
        Reload();
    }
        
    protected override TreeViewItem BuildRoot ()
    {
        // BuildRoot is called every time Reload is called to ensure that TreeViewItems 
        // are created from data. Here we create a fixed set of items. In a real world example,
        // a data model should be passed into the TreeView and the items created from the model.

        // This section illustrates that IDs should be unique. The root item is required to 
        // have a depth of -1, and the rest of the items increment from that.
        var root = new TreeViewItem {id = 0, depth = -1, displayName = "Root"};
        var allItems = new List<TreeViewItem> 
        {
            new TreeViewItem {id = 1, depth = 0, displayName = "Scene 1"},
            new TreeViewItem {id = 2, depth = 1, displayName = "Scene 1.2"},
            new TreeViewItem {id = 3, depth = 2, displayName = "A"},
            new TreeViewItem {id = 4, depth = 2, displayName = "B"},
            new TreeViewItem {id = 5, depth = 2, displayName = "C"},
            new TreeViewItem {id = 6, depth = 2, displayName = "D"},
            new TreeViewItem {id = 7, depth = 1, displayName = "Scene 1.3"},
            new TreeViewItem {id = 8, depth = 2, displayName = "A"},
            new TreeViewItem {id = 9, depth = 2, displayName = "B"},
        };
            
        // Utility method that initializes the TreeViewItem.children and .parent for all items.
        SetupParentsAndChildrenFromDepths (root, allItems);
            
        // Return root of the tree
        return root;
    }
}
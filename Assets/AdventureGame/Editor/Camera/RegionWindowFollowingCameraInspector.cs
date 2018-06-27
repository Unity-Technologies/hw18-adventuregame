using UnityEditor;
using UnityEngine;

namespace UnityEngine.AdventureGame
{
    [CustomEditor(typeof(RegionWindowFollowingCamera))]
    public class RegionWindowFollowingCameraInspector : Editor 
    {
        public void OnSceneGUI()
        {
            var camera = (target as RegionWindowFollowingCamera);
            Handles.BeginGUI();
            GUI.Box(camera.GetRegionWindow(), "Region window");
            Handles.EndGUI();
        }
    }
}
using UnityEditor;

public class SampleGraphViewWindow : GraphViewWindow {
    
    [MenuItem("SampleGraphView/Open Window")]
    public static void OpenWindow()
    {
        GetWindow<SampleGraphViewWindow>();
    }
}
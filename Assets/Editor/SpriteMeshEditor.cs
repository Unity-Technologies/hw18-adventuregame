using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(SpriteMesh))]
public class SpriteMeshEditor : Editor
{
    const float k_SpriteMeshSize = 2.6f;
    readonly Color k_TransparentWhite = new Color(1.0f, 1.0f, 1.0f, 0.0f);

    Material           m_CollisionMeshMaterial;
    GameObject         m_CollisionObject;

    SerializedProperty m_SpriteProp;
    SerializedProperty m_DetailProp;
    SerializedProperty m_ColorProp;
    SpriteMesh         m_SpriteMesh;
    bool               m_Painting = false;

    enum PaintMode
    {
        Painting,
        Erasing
    }

    static float       m_BrushSize = 10.0f;
    static PaintMode   m_PaintMode = PaintMode.Painting;


    void OnEnable()
    {
        m_SpriteMesh = (SpriteMesh)target;

        m_SpriteProp = serializedObject.FindProperty("m_sprite");
        m_DetailProp = serializedObject.FindProperty("m_detail");
        m_ColorProp = serializedObject.FindProperty("m_color");

        Mesh collisionMesh = new Mesh();
        collisionMesh.vertices = new[]
        {
            new Vector3(-k_SpriteMeshSize, -0.1f, -k_SpriteMeshSize),
            new Vector3( k_SpriteMeshSize, -0.1f, -k_SpriteMeshSize),
            new Vector3(-k_SpriteMeshSize, -0.1f,  k_SpriteMeshSize),
            new Vector3( k_SpriteMeshSize, -0.1f,  k_SpriteMeshSize)
        };
        collisionMesh.triangles = new[]
        {
            0, 2, 1,
            2, 3, 1
        };
        collisionMesh.normals = new[]
        {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
        };
        collisionMesh.uv = new[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };

        m_CollisionMeshMaterial = new Material(Shader.Find("UI/Default"));

        m_CollisionObject = new GameObject("__CollisionObject__");
        m_CollisionObject.transform.SetParent(m_SpriteMesh.transform, false);
        m_CollisionObject.hideFlags = HideFlags.HideAndDontSave;

        MeshFilter meshFilter = m_CollisionObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = m_CollisionObject.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = m_CollisionObject.AddComponent<MeshCollider>();

        meshFilter.mesh = collisionMesh;
        meshCollider.sharedMesh = collisionMesh;
        meshRenderer.material = m_CollisionMeshMaterial;
    }

    void OnDisable()
    {
        DestroyImmediate(m_CollisionObject);
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(m_SpriteProp);
        EditorGUILayout.PropertyField(m_DetailProp);
        EditorGUILayout.PropertyField(m_ColorProp);

        if (GUILayout.Button("Regenerate Mesh"))
        {
            m_CollisionObject.SetActive(false);
            m_SpriteMesh.RegenerateMesh();
        }

        serializedObject.ApplyModifiedProperties();
    }

    void OnSceneGUI()
    {
        Handles.BeginGUI();
        Color oldGuiColor = GUI.color;
        switch (m_PaintMode)
        {
            case PaintMode.Painting:
                GUI.color = Color.green;
                if (GUI.Button(new Rect(0, 0, 100, 25), "Paint Mode"))
                {
                    m_PaintMode = PaintMode.Erasing;
                }
                break;
            case PaintMode.Erasing:
                GUI.color = Color.red;
                if (GUI.Button(new Rect(0, 0, 100, 25), "Erase Mode"))
                {
                    m_PaintMode = PaintMode.Painting;
                }
                break;
        }
        GUI.color = Color.black;
        GUI.Label(new Rect(110, 0, 150, 25), string.Format("Brush Size: {0}", m_BrushSize));
        GUI.color = oldGuiColor;

        m_BrushSize = GUI.HorizontalSlider(new Rect(110, 10, 150, 25), m_BrushSize, 1.0f, 20.0f);
        
        Handles.EndGUI();

        Color oldColor = Handles.color;

        m_CollisionMeshMaterial.mainTexture = m_SpriteMesh.m_sprite.texture;
        m_CollisionMeshMaterial.color = m_SpriteMesh.m_color;
        Handles.DrawAAPolyLine(3, 5, new []
        {
            m_SpriteMesh.transform.TransformPoint(new Vector3(-k_SpriteMeshSize, -0.1f, -k_SpriteMeshSize)),
            m_SpriteMesh.transform.TransformPoint(new Vector3( k_SpriteMeshSize, -0.1f, -k_SpriteMeshSize)),
            m_SpriteMesh.transform.TransformPoint(new Vector3( k_SpriteMeshSize, -0.1f,  k_SpriteMeshSize)),
            m_SpriteMesh.transform.TransformPoint(new Vector3(-k_SpriteMeshSize, -0.1f,  k_SpriteMeshSize)),
            m_SpriteMesh.transform.TransformPoint(new Vector3(-k_SpriteMeshSize, -0.1f, -k_SpriteMeshSize))
        });

        // for some reason we get a zero pixel height or pixel width during editing so ignore it
        if (Camera.current.pixelHeight == 0 || Camera.current.pixelWidth == 0)
        {
            return;
        }

        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit;
        bool isHit = Physics.Raycast(ray, out hit);
        if (isHit)
        {
            Handles.color = new Color(m_SpriteMesh.m_color.r, m_SpriteMesh.m_color.g, m_SpriteMesh.m_color.b);
            Handles.DrawWireDisc(hit.point, hit.normal, m_BrushSize);
            Handles.DrawSolidDisc(hit.point, hit.normal, 0.5f);
        }
        
        switch (Event.current.type)
        {
            case EventType.MouseDown:
                if (Event.current.button == 0 && !Event.current.alt && !Event.current.shift && !Event.current.control)
                {
                    if (isHit)
                    {
                        GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
                        DrawBrush(hit.textureCoord);
                        m_Painting = true;

                        Event.current.Use();
                    }
                }
                break;
            case EventType.MouseUp:
                if (m_Painting)
                {
                    GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
                    if (isHit)
                    {
                        DrawBrush(hit.textureCoord);
                        Event.current.Use();
                    }

                    string outputPath = AssetDatabase.GetAssetPath(m_SpriteMesh.m_sprite);
                    byte[] bytes = m_SpriteMesh.m_sprite.texture.EncodeToPNG();
                    File.WriteAllBytes(outputPath, bytes);
                    AssetDatabase.ImportAsset(outputPath);

                    m_CollisionObject.SetActive(false);
                    m_SpriteMesh.RegenerateMesh();

                    m_Painting = false;
                }
                break;
            case EventType.MouseDrag:
                if (m_Painting)
                {
                    if (isHit)
                    {
                        GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
                        DrawBrush(hit.textureCoord);
                        Event.current.Use();
                    }
                }
                break;
        }
        
        SceneView.RepaintAll();
        Handles.color = oldColor;
    }

    void DrawBrush(Vector2 texCoord)
    {
        // convert brush size to pixel size in texture
        int pixelWidth = (int)(m_SpriteMesh.m_sprite.texture.width * m_BrushSize / m_SpriteMesh.transform.localScale.x / k_SpriteMeshSize / 2.0f);
        int pixelHeight = (int)(m_SpriteMesh.m_sprite.texture.height * m_BrushSize / m_SpriteMesh.transform.localScale.z / k_SpriteMeshSize / 2.0f);

        int pixelHitX = (int)(texCoord.x * m_SpriteMesh.m_sprite.texture.width);
        int pixelHitY = (int)(texCoord.y * m_SpriteMesh.m_sprite.texture.height);

        for (int x = pixelHitX - pixelWidth; x <= pixelHitX + pixelWidth; ++x)
        {
            if (x >= 0 && x < m_SpriteMesh.m_sprite.texture.width)
            {
                for (int y = pixelHitY - pixelHeight; y <= pixelHitY + pixelHeight; ++y)
                {
                    if (y >= 0 && y < m_SpriteMesh.m_sprite.texture.height)
                    {
                        float distX = x - pixelHitX;
                        float distY = y - pixelHitY;
                        if ((distX * distX) / (pixelWidth * pixelWidth) + (distY * distY) / (pixelHeight * pixelHeight) <= 1.0f)
                        {
                            m_SpriteMesh.m_sprite.texture.SetPixel(x, y, m_PaintMode == PaintMode.Painting ? Color.white : k_TransparentWhite);
                        }
                    }
                }
            }
        }

        m_SpriteMesh.m_sprite.texture.Apply();
    }
}

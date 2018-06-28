using System.IO;
using UnityEditor.AI;
using UnityEngine;
using UnityEngine.AdventureGame;
using UnityEngine.AI;

namespace UnityEditor.AdventureGame
{
    [CustomEditor(typeof(WalkableArea))]
    public class WalkableAreaEditor : Editor
    {
        readonly Color k_TransparentWhite = new Color(1.0f, 1.0f, 1.0f, 0.0f);

        Material m_CollisionMeshMaterial;
        GameObject m_CollisionObject;

        SerializedProperty m_Sprite;
        SerializedProperty m_NavMeshData;
        SerializedProperty m_Detail;
        SerializedProperty m_Color;

        SerializedProperty m_AgentTypeID;
        SerializedProperty m_BuildHeightMesh;
        SerializedProperty m_DefaultArea;
        SerializedProperty m_LayerMask;
        SerializedProperty m_OverrideTileSize;
        SerializedProperty m_OverrideVoxelSize;
        SerializedProperty m_TileSize;
        SerializedProperty m_VoxelSize;

        WalkableArea m_WalkableArea;
        bool m_Modified = false;
        bool m_Painting = false;
        Texture2D m_PaintTexture;

        enum PaintMode
        {
            Painting,
            Erasing
        }

        static float s_BrushSize = 5.0f;
        static PaintMode s_PaintMode = PaintMode.Painting;

        void OnEnable()
        {
            m_WalkableArea = (WalkableArea)target;

            m_Sprite = serializedObject.FindProperty("m_sprite");
            m_NavMeshData = serializedObject.FindProperty("m_NavMeshData");
            m_Detail = serializedObject.FindProperty("m_detail");
            m_Color = serializedObject.FindProperty("m_color");
            m_AgentTypeID = serializedObject.FindProperty("m_AgentTypeID");
            m_BuildHeightMesh = serializedObject.FindProperty("m_BuildHeightMesh");
            m_DefaultArea = serializedObject.FindProperty("m_DefaultArea");
            m_LayerMask = serializedObject.FindProperty("m_LayerMask");
            m_OverrideTileSize = serializedObject.FindProperty("m_OverrideTileSize");
            m_OverrideVoxelSize = serializedObject.FindProperty("m_OverrideVoxelSize");
            m_TileSize = serializedObject.FindProperty("m_TileSize");
            m_VoxelSize = serializedObject.FindProperty("m_VoxelSize");

            Mesh collisionMesh = new Mesh();
            collisionMesh.vertices = new[]
            {
                new Vector3(-1.0f, -0.1f, -1.0f),
                new Vector3(1.0f, -0.1f, -1.0f),
                new Vector3(-1.0f, -0.1f, 1.0f),
                new Vector3(1.0f, -0.1f, 1.0f)
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
            m_CollisionObject.transform.SetParent(m_WalkableArea.transform, false);
            m_CollisionObject.transform.localPosition = new Vector3(0.0f, 10.0f, 0.0f);
            m_CollisionObject.hideFlags = HideFlags.HideAndDontSave;

            MeshFilter meshFilter = m_CollisionObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = m_CollisionObject.AddComponent<MeshRenderer>();
            MeshCollider meshCollider = m_CollisionObject.AddComponent<MeshCollider>();

            meshFilter.mesh = collisionMesh;
            meshCollider.sharedMesh = collisionMesh;
            meshRenderer.material = m_CollisionMeshMaterial;

            if (SceneManager.Instance == null)
            {
                Debug.LogError("Could not find SceneManager as parent of SpriteMesh!");
                return;
            }

            if (m_WalkableArea.m_sprite == null)
            {
                GameObject root = PrefabUtility.FindValidUploadPrefabInstanceRoot(m_WalkableArea.transform.parent.gameObject);
                string spritePath = Path.Combine(Path.Combine(SceneManager.Instance.m_outputPath, root.name), "Editor");
                Directory.CreateDirectory(spritePath);
                string spriteAssetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(spritePath, string.Format("{0}.png", m_WalkableArea.name)));

                // load the sprite or create it if it doesn't exist
                m_WalkableArea.m_sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spriteAssetPath);
                if (m_WalkableArea.m_sprite == null)
                {
                    // create the texture if it doesn't exist
                    Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(spriteAssetPath);
                    if (texture == null)
                    {
                        Directory.CreateDirectory(spritePath);

                        WalkableAreaGroup group = m_WalkableArea.transform.parent.gameObject.GetComponent<WalkableAreaGroup>();

                        m_PaintTexture = new Texture2D(group.m_textureWidth, group.m_textureHeight, TextureFormat.RGBA32, false);
                        Color[] colors = m_PaintTexture.GetPixels();
                        for (int i = 0; i < colors.Length; ++i)
                        {
                            colors[i] = k_TransparentWhite;
                        }
                        m_PaintTexture.SetPixels(colors);
                        byte[] bytes = m_PaintTexture.EncodeToPNG();
                        File.WriteAllBytes(spriteAssetPath, bytes);
                        AssetDatabase.Refresh();
                    }
                    ResetTextureImporterSettings(spriteAssetPath);

                    Sprite sprite = Sprite.Create(m_PaintTexture, new Rect(0.0f, 0.0f, m_PaintTexture.width, m_PaintTexture.height), new Vector2(0.5f, 0.5f));
                    AssetDatabase.AddObjectToAsset(sprite, spriteAssetPath);
                    AssetDatabase.SaveAssets();
                    
                    m_WalkableArea.m_sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spriteAssetPath);
                }
            }

            m_PaintTexture = new Texture2D(m_WalkableArea.m_sprite.texture.width, m_WalkableArea.m_sprite.texture.height, TextureFormat.RGBA32, false);

            string texturePath = AssetDatabase.GetAssetPath(m_WalkableArea.m_sprite);
            TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer != null)
            {
                if (importer.DoesSourceTextureHaveAlpha())
                {
                    Graphics.CopyTexture(m_WalkableArea.m_sprite.texture, m_PaintTexture);
                }
                else
                {
                    Graphics.ConvertTexture(m_WalkableArea.m_sprite.texture, m_PaintTexture);
                }
            }

            Tools.hidden = true;
        }

        void OnDisable()
        {
            if (m_Modified)
            {
                RegenerateMesh();
            }

            DestroyImmediate(m_CollisionObject);
            Tools.hidden = false;
        }

        public static void EnsureProjectConsistency(SceneManager manager, WalkableArea walkableArea)
        {
            if (walkableArea.m_sprite != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(walkableArea.m_sprite);
                string assetFilename = Path.GetFileName(assetPath);
                string targetFilename = string.Format("{0}.png", walkableArea.name);
                if (assetFilename != targetFilename)
                {
                    EditorUtility.DisplayProgressBar("Renaming WalkableArea Sprite",
                        string.Format("Renaming WalkableArea sprite from {0} to {1}", assetFilename, targetFilename), 0.5f);
                    string targetPath = Path.Combine(Path.GetDirectoryName(assetPath), targetFilename);
                    if (File.Exists(targetPath))
                    {
                        targetPath = AssetDatabase.GenerateUniqueAssetPath(targetPath);
                    }

                    AssetDatabase.MoveAsset(assetPath, targetPath);
                    AssetDatabase.ImportAsset(targetPath);
                    Debug.LogFormat("Moved WalkableArea sprite from {0} to {1}.", assetPath, targetPath);
                    EditorUtility.ClearProgressBar();
                }
            }

            if (walkableArea.navMeshData != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(walkableArea.navMeshData);
                string assetFilename = Path.GetFileName(assetPath);
                string targetFilename = string.Format("{0}.asset", walkableArea.name);
                if (assetFilename != targetFilename)
                {
                    EditorUtility.DisplayProgressBar("Renaming WalkableArea NavMeshData",
                        string.Format("Renaming WalkableArea NavMeshData from {0} to {1}", assetFilename, targetFilename), 0.5f);
                    string targetPath = Path.Combine(Path.GetDirectoryName(assetPath), targetFilename);
                    if (File.Exists(targetPath))
                    {
                        targetPath = AssetDatabase.GenerateUniqueAssetPath(targetPath);
                    }

                    AssetDatabase.MoveAsset(assetPath, targetPath);
                    AssetDatabase.ImportAsset(targetPath);
                    Debug.LogFormat("Moved WalkableArea navMeshData from {0} to {1}.", assetPath, targetPath);
                    EditorUtility.ClearProgressBar();
                }
            }
        }

        void ResetTextureImporterSettings(string texturePath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer != null)
            {
                // set the texture importer settings
                if (importer.textureType != TextureImporterType.Sprite ||
                    importer.mipmapEnabled ||
                    !importer.isReadable ||
                    importer.textureCompression != TextureImporterCompression.Uncompressed)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.mipmapEnabled = false;
                    importer.isReadable = true;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var bs = NavMesh.GetSettingsByID(m_AgentTypeID.intValue);

            if (bs.agentTypeID != -1)
            {
                // Draw image
                const float diagramHeight = 80.0f;
                Rect agentDiagramRect = EditorGUILayout.GetControlRect(false, diagramHeight);
                NavMeshEditorHelpers.DrawAgentDiagram(agentDiagramRect, bs.agentRadius, bs.agentHeight, bs.agentClimb, bs.agentSlope);
            }
            AgentTypePopup("Agent Type", m_AgentTypeID);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_LayerMask);

            EditorGUILayout.Space();

            EditorGUILayout.Space();

            m_OverrideVoxelSize.isExpanded = EditorGUILayout.Foldout(m_OverrideVoxelSize.isExpanded, "Advanced");
            if (m_OverrideVoxelSize.isExpanded)
            {
                EditorGUI.indentLevel++;

                AreaPopup("Default Area", m_DefaultArea);

                // Override voxel size.
                EditorGUILayout.PropertyField(m_OverrideVoxelSize);

                using (new EditorGUI.DisabledScope(!m_OverrideVoxelSize.boolValue || m_OverrideVoxelSize.hasMultipleDifferentValues))
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.PropertyField(m_VoxelSize);

                    if (!m_OverrideVoxelSize.hasMultipleDifferentValues)
                    {
                        if (!m_AgentTypeID.hasMultipleDifferentValues)
                        {
                            float voxelsPerRadius = m_VoxelSize.floatValue > 0.0f ? (bs.agentRadius / m_VoxelSize.floatValue) : 0.0f;
                            EditorGUILayout.LabelField(" ", voxelsPerRadius.ToString("0.00") + " voxels per agent radius", EditorStyles.miniLabel);
                        }
                        if (m_OverrideVoxelSize.boolValue)
                            EditorGUILayout.HelpBox("Voxel size controls how accurately the navigation mesh is generated from the level geometry. A good voxel size is 2-4 voxels per agent radius. Making voxel size smaller will increase build time.", MessageType.None);
                    }
                    EditorGUI.indentLevel--;
                }

                // Override tile size
                EditorGUILayout.PropertyField(m_OverrideTileSize);

                using (new EditorGUI.DisabledScope(!m_OverrideTileSize.boolValue || m_OverrideTileSize.hasMultipleDifferentValues))
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.PropertyField(m_TileSize);

                    if (!m_TileSize.hasMultipleDifferentValues && !m_VoxelSize.hasMultipleDifferentValues)
                    {
                        float tileWorldSize = m_TileSize.intValue * m_VoxelSize.floatValue;
                        EditorGUILayout.LabelField(" ", tileWorldSize.ToString("0.00") + " world units", EditorStyles.miniLabel);
                    }

                    if (!m_OverrideTileSize.hasMultipleDifferentValues)
                    {
                        if (m_OverrideTileSize.boolValue)
                            EditorGUILayout.HelpBox("Tile size controls the how local the changes to the world are (rebuild or carve). Small tile size allows more local changes, while potentially generating more data in overal.", MessageType.None);
                    }
                    EditorGUI.indentLevel--;
                }


                // Height mesh
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(m_BuildHeightMesh);
                }

                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_Sprite);
            EditorGUILayout.PropertyField(m_NavMeshData);
            EditorGUILayout.PropertyField(m_Detail);
            EditorGUILayout.PropertyField(m_Color);

            if (GUILayout.Button("Regenerate Collision", GUILayout.Height(50)))
            {
                RegenerateMesh();
            }

            serializedObject.ApplyModifiedProperties();
        }

        void OnSceneGUI()
        {
            Color oldColor = Handles.color;

            m_CollisionMeshMaterial.mainTexture = m_PaintTexture;
            m_CollisionMeshMaterial.color = m_WalkableArea.m_color;
            Handles.DrawAAPolyLine(3, 5, new[]
            {
                m_WalkableArea.transform.TransformPoint(new Vector3(-1.0f, -0.1f, -1.0f)),
                m_WalkableArea.transform.TransformPoint(new Vector3(1.0f, -0.1f, -1.0f)),
                m_WalkableArea.transform.TransformPoint(new Vector3(1.0f, -0.1f, 1.0f)),
                m_WalkableArea.transform.TransformPoint(new Vector3(-1.0f, -0.1f, 1.0f)),
                m_WalkableArea.transform.TransformPoint(new Vector3(-1.0f, -0.1f, -1.0f))
            });

            Handles.BeginGUI();
            Color oldGuiColor = GUI.color;
            switch (s_PaintMode)
            {
                case PaintMode.Painting:
                    GUI.color = Color.green;
                    if (GUI.Button(new Rect(0, 0, 100, 25), "Paint Mode"))
                    {
                        s_PaintMode = PaintMode.Erasing;
                    }

                    break;
                case PaintMode.Erasing:
                    GUI.color = Color.red;
                    if (GUI.Button(new Rect(0, 0, 100, 25), "Erase Mode"))
                    {
                        s_PaintMode = PaintMode.Painting;
                    }

                    break;
            }

            if (m_Modified)
            {
                GUI.color = Color.cyan;
                if (GUI.Button(new Rect(0, 25, 100, 25), "Bake"))
                {
                    RegenerateMesh();
                }
            }

            GUI.color = Color.black;
            GUI.Label(new Rect(110, 0, 150, 25), string.Format("Brush Size: {0}", s_BrushSize));
            GUI.color = oldGuiColor;

            s_BrushSize = GUI.HorizontalSlider(new Rect(110, 10, 150, 25), s_BrushSize, 1.0f, 20.0f);

            Handles.EndGUI();

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
                Handles.color = new Color(m_WalkableArea.m_color.r, m_WalkableArea.m_color.g, m_WalkableArea.m_color.b);
                Handles.DrawWireDisc(hit.point, hit.normal, s_BrushSize);
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

                            m_Painting = true;

                            DrawBrush(hit.textureCoord);

                            Event.current.Use();
                        }
                    }

                    break;
                case EventType.MouseUp:
                    if (m_Painting)
                    {
                        GUIUtility.hotControl = 0;
                        if (isHit)
                        {
                            DrawBrush(hit.textureCoord);
                            Event.current.Use();
                        }

                        m_Modified = true;
                        m_Painting = false;
                    }

                    break;
                case EventType.MouseDrag:
                    if (m_Painting)
                    {
                        if (isHit)
                        {
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
            int pixelWidth = (int)(m_PaintTexture.width * s_BrushSize / m_WalkableArea.transform.lossyScale.x / 2.0f);
            int pixelHeight = (int)(m_PaintTexture.height * s_BrushSize / m_WalkableArea.transform.lossyScale.z / 2.0f);

            int pixelHitX = (int)(texCoord.x * m_PaintTexture.width);
            int pixelHitY = (int)(texCoord.y * m_PaintTexture.height);

            for (int x = pixelHitX - pixelWidth; x <= pixelHitX + pixelWidth; ++x)
            {
                if (x >= 0 && x < m_PaintTexture.width)
                {
                    for (int y = pixelHitY - pixelHeight; y <= pixelHitY + pixelHeight; ++y)
                    {
                        if (y >= 0 && y < m_PaintTexture.height)
                        {
                            float distX = x - pixelHitX;
                            float distY = y - pixelHitY;
                            if ((distX * distX) / (pixelWidth * pixelWidth) + (distY * distY) / (pixelHeight * pixelHeight) <= 1.0f)
                            {
                                m_PaintTexture.SetPixel(x, y, s_PaintMode == PaintMode.Painting ? Color.white : k_TransparentWhite);
                            }
                        }
                    }
                }
            }

            m_PaintTexture.Apply();
        }

        static void AgentTypePopup(string labelName, SerializedProperty agentTypeID)
        {
            var index = -1;
            var count = NavMesh.GetSettingsCount();
            var agentTypeNames = new string[count + 2];
            for (var i = 0; i < count; i++)
            {
                var id = NavMesh.GetSettingsByIndex(i).agentTypeID;
                var name = NavMesh.GetSettingsNameFromID(id);
                agentTypeNames[i] = name;
                if (id == agentTypeID.intValue)
                    index = i;
            }
            agentTypeNames[count] = "";
            agentTypeNames[count + 1] = "Open Agent Settings...";

            bool validAgentType = index != -1;
            if (!validAgentType)
            {
                EditorGUILayout.HelpBox("Agent Type invalid.", MessageType.Warning);
            }

            var rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
            EditorGUI.BeginProperty(rect, GUIContent.none, agentTypeID);

            EditorGUI.BeginChangeCheck();
            index = EditorGUI.Popup(rect, labelName, index, agentTypeNames);
            if (EditorGUI.EndChangeCheck())
            {
                if (index >= 0 && index < count)
                {
                    var id = NavMesh.GetSettingsByIndex(index).agentTypeID;
                    agentTypeID.intValue = id;
                }
                else if (index == count + 1)
                {
                    NavMeshEditorHelpers.OpenAgentSettings(-1);
                }
            }

            EditorGUI.EndProperty();
        }

        static void AreaPopup(string labelName, SerializedProperty areaProperty)
        {
            var areaIndex = -1;
            var areaNames = GameObjectUtility.GetNavMeshAreaNames();
            for (var i = 0; i < areaNames.Length; i++)
            {
                var areaValue = GameObjectUtility.GetNavMeshAreaFromName(areaNames[i]);
                if (areaValue == areaProperty.intValue)
                    areaIndex = i;
            }
            ArrayUtility.Add(ref areaNames, "");
            ArrayUtility.Add(ref areaNames, "Open Area Settings...");

            var rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
            EditorGUI.BeginProperty(rect, GUIContent.none, areaProperty);

            EditorGUI.BeginChangeCheck();
            areaIndex = EditorGUI.Popup(rect, labelName, areaIndex, areaNames);

            if (EditorGUI.EndChangeCheck())
            {
                if (areaIndex >= 0 && areaIndex < areaNames.Length - 2)
                    areaProperty.intValue = GameObjectUtility.GetNavMeshAreaFromName(areaNames[areaIndex]);
                else if (areaIndex == areaNames.Length - 1)
                    NavMeshEditorHelpers.OpenAreaSettings();
            }

            EditorGUI.EndProperty();
        }

        void RegenerateMesh()
        {
            if (m_WalkableArea == null)
            {
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(m_WalkableArea.m_sprite);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogFormat("Failed to load TextureImporter at path: {0}", assetPath);
                return;
            }

            EditorUtility.DisplayProgressBar("Rebuilding Navmesh",
                string.Format("Applying Changes to WalkableArea {0}", m_WalkableArea.name), 0.5f);
            string outputPath = AssetDatabase.GetAssetPath(m_WalkableArea.m_sprite);
            byte[] bytes = m_PaintTexture.EncodeToPNG();
            File.WriteAllBytes(outputPath, bytes);
            AssetDatabase.ImportAsset(outputPath);
            m_CollisionObject.SetActive(false);

            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.Tight;
            settings.spriteTessellationDetail = m_WalkableArea.m_detail;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();

            EditorUtility.SetDirty(importer);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            Vector3[] vertices = new Vector3[m_WalkableArea.m_sprite.vertices.Length];
            Vector3[] normals = new Vector3[m_WalkableArea.m_sprite.vertices.Length];
            Vector2[] uv = new Vector2[m_WalkableArea.m_sprite.vertices.Length];

            // normalize the vertex data from -1.0f to 1.0f in x and z coordinates
            for (int i = 0; i < m_WalkableArea.m_sprite.vertices.Length; ++i)
            {
                vertices[i] = new Vector3(
                    m_WalkableArea.m_sprite.vertices[i].x * 2.0f * m_WalkableArea.m_sprite.pixelsPerUnit / m_WalkableArea.m_sprite.texture.width,
                    0.0f,
                    m_WalkableArea.m_sprite.vertices[i].y * 2.0f * m_WalkableArea.m_sprite.pixelsPerUnit / m_WalkableArea.m_sprite.texture.height);
                normals[i] = -Vector3.forward;
                uv[i] = Vector2.zero;
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uv;

            int[] triangles = new int[m_WalkableArea.m_sprite.triangles.Length];
            for (int i = 0; i < m_WalkableArea.m_sprite.triangles.Length; ++i)
            {
                triangles[i] = m_WalkableArea.m_sprite.triangles[i];
            }

            mesh.triangles = triangles;

            MeshFilter meshFilter = m_WalkableArea.gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            MeshRenderer meshRenderer = m_WalkableArea.gameObject.AddComponent<MeshRenderer>();

            // set modified to false here so that OnDisable of the editor doesn't retrigger RegenerateMesh
            m_Modified = false;

            string navmeshPath = Path.Combine(SceneManager.Instance.m_outputPath, PrefabUtility.FindPrefabRoot(m_WalkableArea.transform.parent.gameObject).name);
            string navmeshAssetPath = Path.Combine(navmeshPath, string.Format("{0}.asset", m_WalkableArea.name));

            if (m_WalkableArea.navMeshData != null)
            {
                string walkableAreaNavMeshAssetPath = AssetDatabase.GetAssetPath(m_WalkableArea.navMeshData);
                if (!string.IsNullOrEmpty(walkableAreaNavMeshAssetPath))
                {
                    navmeshAssetPath = walkableAreaNavMeshAssetPath;
                }
            }
            else
            {
                Directory.CreateDirectory(navmeshPath);
                navmeshAssetPath = AssetDatabase.GenerateUniqueAssetPath(navmeshAssetPath);
            }
            
            m_WalkableArea.BuildNavMesh();
            
            AssetDatabase.CreateAsset(m_WalkableArea.navMeshData, navmeshAssetPath);

            DestroyImmediate(meshRenderer);
            DestroyImmediate(meshFilter);

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                WalkableArea prefab = PrefabUtility.GetCorrespondingObjectFromSource(m_WalkableArea) as WalkableArea;
                if (prefab != null)
                {
                    prefab.navMeshData = m_WalkableArea.navMeshData;
                }
            }

            EditorUtility.ClearProgressBar();
        }
    }
}
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AdventureGame;

namespace UnityEditor.AdventureGame
{
    public class BaseAreaEditor : Editor
    {
        readonly Color k_TransparentWhite = new Color(1.0f, 1.0f, 1.0f, 0.0f);

        Material m_CollisionMeshMaterial;
        GameObject m_CollisionObject;

        SerializedProperty m_Sprite;
        SerializedProperty m_Detail;
        SerializedProperty m_Color;

        IBaseArea m_BaseArea;
        MonoBehaviour m_Behavior;
        bool m_Painting = false;
        Texture2D m_PaintTexture;

        enum PaintMode
        {
            Painting,
            Erasing
        }

        static float s_BrushSize = 5.0f;
        static PaintMode s_PaintMode = PaintMode.Painting;

        public virtual void OnEnable()
        {
            SceneView sceneViewWindow = EditorWindow.GetWindow<SceneView>();
            if (sceneViewWindow != null)
            {
                sceneViewWindow.Focus();
            }

            m_BaseArea = (IBaseArea)target;
            m_Behavior = (MonoBehaviour)target;

            m_Sprite = serializedObject.FindProperty("m_sprite");
            m_Detail = serializedObject.FindProperty("m_detail");
            m_Color = serializedObject.FindProperty("m_color");

            Mesh collisionMesh = new Mesh();
            collisionMesh.vertices = new[]
            {
                new Vector3(-1.0f, -1.0f, -0.1f),
                new Vector3( 1.0f, -1.0f, -0.1f),
                new Vector3(-1.0f,  1.0f, -0.1f),
                new Vector3( 1.0f,  1.0f, -0.1f)
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
            m_CollisionObject.transform.SetParent(m_Behavior.transform, false);
            m_CollisionObject.transform.localPosition = new Vector3(0.0f, 0.0f, -10.0f);
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

            if (m_BaseArea.Sprite == null)
            {
                GameObject root = PrefabUtility.FindValidUploadPrefabInstanceRoot(m_Behavior.transform.parent.gameObject);
                string spritePath = Path.Combine(Path.Combine(SceneManager.Instance.m_outputPath, root.name), "Editor");
                Directory.CreateDirectory(spritePath);
                string spriteAssetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(spritePath, string.Format("{0}.png", m_Behavior.name)));

                // load the sprite or create it if it doesn't exist
                m_BaseArea.Sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spriteAssetPath);
                if (m_BaseArea.Sprite == null)
                {
                    // create the texture if it doesn't exist
                    Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(spriteAssetPath);
                    if (texture == null)
                    {
                        Directory.CreateDirectory(spritePath);

                        BaseAreaGroup group = m_Behavior.transform.parent.gameObject.GetComponent<BaseAreaGroup>();

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
                    
                    m_BaseArea.Sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spriteAssetPath);
                }
            }

            m_PaintTexture = new Texture2D(m_BaseArea.Sprite.texture.width, m_BaseArea.Sprite.texture.height, TextureFormat.RGBA32, false);

            string texturePath = AssetDatabase.GetAssetPath(m_BaseArea.Sprite);
            TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer != null)
            {
                if (importer.DoesSourceTextureHaveAlpha())
                {
                    Graphics.CopyTexture(m_BaseArea.Sprite.texture, m_PaintTexture);
                }
                else
                {
                    Graphics.ConvertTexture(m_BaseArea.Sprite.texture, m_PaintTexture);
                }
            }

            Tools.hidden = true;
        }

        public virtual void OnDisable()
        {
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
            EditorGUILayout.PropertyField(m_Sprite);
            EditorGUILayout.PropertyField(m_Detail);
            EditorGUILayout.PropertyField(m_Color);

            if (GUILayout.Button("Regenerate NavMesh"))
            {
                RegenerateMesh();
            }

            serializedObject.ApplyModifiedProperties();
        }

        public void OnSceneGUI()
        {
            Color oldColor = Handles.color;

            m_CollisionMeshMaterial.mainTexture = m_PaintTexture;
            m_CollisionMeshMaterial.color = m_BaseArea.Color;
            Handles.DrawAAPolyLine(3, 5, new[]
            {
                m_Behavior.transform.TransformPoint(new Vector3(-1.0f, -1.0f, -0.1f)),
                m_Behavior.transform.TransformPoint(new Vector3( 1.0f, -1.0f, -0.1f)),
                m_Behavior.transform.TransformPoint(new Vector3( 1.0f,  1.0f, -0.1f)),
                m_Behavior.transform.TransformPoint(new Vector3(-1.0f,  1.0f, -0.1f)),
                m_Behavior.transform.TransformPoint(new Vector3(-1.0f, -1.0f, -0.1f))
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
                Handles.color = m_BaseArea.Color;
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

                        RegenerateMesh();
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
            int pixelWidth = (int)(m_PaintTexture.width * s_BrushSize / m_Behavior.transform.lossyScale.x / 2.0f);
            int pixelHeight = (int)(m_PaintTexture.height * s_BrushSize / m_Behavior.transform.lossyScale.y / 2.0f);

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

        void IncrementIndex(int idx1, int idx2, Dictionary<int, Dictionary<int, int>> foundEdges)
        {
            if (!foundEdges.ContainsKey(idx1))
            {
                foundEdges[idx1] = new Dictionary<int, int>();
                foundEdges[idx1][idx2] = 1;
            }
            else if (foundEdges[idx1].ContainsKey(idx2))
            {
                ++foundEdges[idx1][idx2];
            }
            else
            {
                foundEdges[idx1][idx2] = 1;
            }

            if (!foundEdges.ContainsKey(idx2))
            {
                foundEdges[idx2] = new Dictionary<int, int>();
                foundEdges[idx2][idx1] = 1;
            }
            else if (foundEdges[idx2].ContainsKey(idx1))
            {
                ++foundEdges[idx2][idx1];
            }
            else
            {
                foundEdges[idx2][idx1] = 1;
            }
        }

        void RegenerateMesh()
        {
            if (m_BaseArea == null)
            {
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(m_BaseArea.Sprite);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogFormat("Failed to load TextureImporter at path: {0}", assetPath);
                return;
            }

            EditorUtility.DisplayProgressBar("Rebuilding Collision Mesh",
                string.Format("Applying Changes to HotSpot {0}", m_Behavior.name), 0.5f);
            string outputPath = AssetDatabase.GetAssetPath(m_BaseArea.Sprite);
            byte[] bytes = m_PaintTexture.EncodeToPNG();
            File.WriteAllBytes(outputPath, bytes);
            AssetDatabase.ImportAsset(outputPath);

            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.Tight;
            settings.spriteTessellationDetail = m_BaseArea.Detail;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();

            EditorUtility.SetDirty(importer);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            Vector2[] vertices = new Vector2[m_BaseArea.Sprite.vertices.Length];

            // normalize the vertex data from -1.0f to 1.0f in x and z coordinates
            for (int i = 0; i < m_BaseArea.Sprite.vertices.Length; ++i)
            {
                vertices[i] = new Vector3(
                    m_BaseArea.Sprite.vertices[i].x * 2.0f * m_BaseArea.Sprite.pixelsPerUnit / m_BaseArea.Sprite.texture.width,
                    m_BaseArea.Sprite.vertices[i].y * 2.0f * m_BaseArea.Sprite.pixelsPerUnit / m_BaseArea.Sprite.texture.height);
            }
            
            Dictionary<int, Dictionary<int, int>> foundEdges = new Dictionary<int, Dictionary<int, int>>();
            for (int i = 0; i < m_BaseArea.Sprite.triangles.Length / 3; ++i)
            {
                int idx1 = m_BaseArea.Sprite.triangles[i * 3];
                int idx2 = m_BaseArea.Sprite.triangles[i * 3 + 1];
                int idx3 = m_BaseArea.Sprite.triangles[i * 3 + 2];

                IncrementIndex(idx1, idx2, foundEdges);
                IncrementIndex(idx2, idx3, foundEdges);
                IncrementIndex(idx3, idx1, foundEdges);
            }

            foreach (KeyValuePair<int, Dictionary<int, int>> pair in foundEdges)
            {
                List<int> eraseList = new List<int>();
                foreach (KeyValuePair<int, int> pair2 in pair.Value)
                {
                    if (pair2.Value > 1)
                    {
                        eraseList.Add(pair2.Key);
                    }
                }

                // erase edges that are duplicated
                foreach (int erase in eraseList)
                {
                    pair.Value.Remove(erase);
                }
            }

            List<List<int>> edgeList = new List<List<int>>();
            foreach (KeyValuePair<int, Dictionary<int, int>> pair in foundEdges)
            {
                if (pair.Value.Count > 0)
                {
                    List<int> newList = new List<int>();
                    edgeList.Add(newList);
                    int vert1 = pair.Key;
                    int vert2 = pair.Value.Keys.First();
                    newList.Add(vert1);
                    newList.Add(vert2);

                    foundEdges[vert1].Remove(vert2);
                    foundEdges[vert2].Remove(vert1);

                    int vertBegin = vert1;
                    while (!foundEdges[vert2].Keys.Contains(vertBegin) && foundEdges[vert2].Keys.Count > 0)
                    {
                        vert1 = foundEdges[vert2].Keys.First();
                        newList.Add(vert1);
                        foundEdges[vert1].Remove(vert2);
                        foundEdges[vert2].Remove(vert1);
                        vert2 = vert1;
                    }
                    foundEdges[vert2].Remove(vertBegin);
                }
            }

            PolygonCollider2D collider = m_Behavior.GetComponent<PolygonCollider2D>();
            collider.pathCount = edgeList.Count;
            for (int i = 0; i < edgeList.Count; ++i)
            {
                Vector2[] edgeVerts = new Vector2[edgeList[i].Count];
                for (int iEdge = 0; iEdge < edgeList[i].Count; ++iEdge)
                {
                    edgeVerts[iEdge] = vertices[edgeList[i][iEdge]];
                }
                collider.SetPath(i, edgeVerts);
            }
            EditorSceneManager.MarkSceneDirty(collider.gameObject.scene);

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                MonoBehaviour prefab = PrefabUtility.GetCorrespondingObjectFromSource(m_Behavior) as MonoBehaviour;
                if (prefab != null)
                {
                    PolygonCollider2D colliderPrefab = prefab.GetComponent<PolygonCollider2D>();
                    colliderPrefab.pathCount = collider.pathCount;
                    for (int i = 0; i < edgeList.Count; ++i)
                    {
                        colliderPrefab.SetPath(i, collider.GetPath(i));
                    }
                }
            }

            EditorUtility.ClearProgressBar();
        }
    }
}
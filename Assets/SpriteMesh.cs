using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(MeshFilter), typeof(NavMeshSurface), typeof(MeshRenderer))]
public class SpriteMesh : MonoBehaviour
{
    public Sprite m_sprite;

    [Range(0.0f, 1.0f)]
    public float m_detail = 0.5f;

    void Awake()
    {
    }

    public void RegenerateMesh()
    {
        Mesh mesh = new Mesh();

        string assetPath = AssetDatabase.GetAssetPath(m_sprite);
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogFormat("Failed to load TextureImporter at path: {0}", assetPath);
            return;
        }

        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.Tight;
        settings.spriteTessellationDetail = m_detail;
        importer.SetTextureSettings(settings);
        importer.SaveAndReimport();

        EditorUtility.SetDirty(importer);
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

        //Sprite testSprite = Sprite.Create(m_texture, new Rect(0, 0, m_texture.width, m_texture.height), new Vector2(0.5f, 0.5f));
        
        Vector3[] vertices = new Vector3[m_sprite.vertices.Length];
        Vector3[] normals = new Vector3[m_sprite.vertices.Length];
        Vector2[] uv = new Vector2[m_sprite.vertices.Length];
        for (int i = 0; i < m_sprite.vertices.Length; ++i)
        {
            vertices[i] = new Vector3(m_sprite.vertices[i].x, 0.0f, m_sprite.vertices[i].y);
            normals[i] = -Vector3.forward;
            uv[i] = Vector2.zero;
        }
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;

        int[] triangles = new int[m_sprite.triangles.Length];
        for (int i = 0; i < m_sprite.triangles.Length; ++i)
        {
            triangles[i] = m_sprite.triangles[i];
        }
        mesh.triangles = triangles;

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        NavMeshSurface surface = GetComponent<NavMeshSurface>();
        surface.BuildNavMesh();

        SceneManager sceneManager = null;
        Transform parent = transform.parent;
        while (parent != null)
        {
            sceneManager = parent.GetComponent<SceneManager>();
            if (sceneManager != null)
            {
                break;
            }
            parent = parent.parent;
        }

        if (sceneManager == null)
        {
            Debug.LogError("Could not find SceneManager as parent of SpriteMesh!");
            return;
        }

        string navmeshPath = Path.Combine(sceneManager.m_outputPath, PrefabUtility.FindPrefabRoot(gameObject).name);
        string navmeshAssetPath = Path.Combine(navmeshPath, string.Format("{0}.asset", name));
        Directory.CreateDirectory(navmeshPath);

        NavMeshData outputNavmesh = AssetDatabase.LoadMainAssetAtPath(navmeshAssetPath) as NavMeshData;
        if (outputNavmesh != null)
        {
            EditorUtility.CopySerialized(surface.navMeshData, outputNavmesh);
            AssetDatabase.SaveAssets();
            surface.navMeshData = outputNavmesh;
        }
        else
        {
            AssetDatabase.CreateAsset(surface.navMeshData, navmeshAssetPath);
        }

        if (Application.isPlaying)
        {
            meshFilter.mesh = null;
        }
    }
}

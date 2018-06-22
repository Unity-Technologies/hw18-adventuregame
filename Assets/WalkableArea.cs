using System.Collections.Generic;
using UnityEngine.AI;

namespace UnityEngine.AdventureGame
{
    [ExecuteInEditMode]
    [DefaultExecutionOrder(-102)]
    public class WalkableArea : MonoBehaviour
    {
#if UNITY_EDITOR
        public Sprite m_sprite;
        public Color m_color = new Color(0.0f, 0.0f, 1.0f, 0.25f);

        [Range(0.0f, 1.0f)]
        public float m_detail = 0.5f;
#endif

        [SerializeField]
        int m_AgentTypeID;

        public int agentTypeID
        {
            get { return m_AgentTypeID; }
            set { m_AgentTypeID = value; }
        }

        [SerializeField]
        Vector3 m_Size = new Vector3(10.0f, 10.0f, 10.0f);

        public Vector3 size
        {
            get { return m_Size; }
            set { m_Size = value; }
        }

        [SerializeField]
        Vector3 m_Center = new Vector3(0, 2.0f, 0);

        public Vector3 center
        {
            get { return m_Center; }
            set { m_Center = value; }
        }

        [SerializeField]
        LayerMask m_LayerMask = ~0;

        public LayerMask layerMask
        {
            get { return m_LayerMask; }
            set { m_LayerMask = value; }
        }

        [SerializeField]
        int m_DefaultArea;

        public int defaultArea
        {
            get { return m_DefaultArea; }
            set { m_DefaultArea = value; }
        }

        [SerializeField]
        bool m_IgnoreNavMeshAgent = true;

        public bool ignoreNavMeshAgent
        {
            get { return m_IgnoreNavMeshAgent; }
            set { m_IgnoreNavMeshAgent = value; }
        }

        [SerializeField]
        bool m_IgnoreNavMeshObstacle = true;

        public bool ignoreNavMeshObstacle
        {
            get { return m_IgnoreNavMeshObstacle; }
            set { m_IgnoreNavMeshObstacle = value; }
        }

        [SerializeField]
        bool m_OverrideTileSize;

        public bool overrideTileSize
        {
            get { return m_OverrideTileSize; }
            set { m_OverrideTileSize = value; }
        }

        [SerializeField]
        int m_TileSize = 256;

        public int tileSize
        {
            get { return m_TileSize; }
            set { m_TileSize = value; }
        }

        [SerializeField]
        bool m_OverrideVoxelSize;

        public bool overrideVoxelSize
        {
            get { return m_OverrideVoxelSize; }
            set { m_OverrideVoxelSize = value; }
        }

        [SerializeField]
        float m_VoxelSize;

        public float voxelSize
        {
            get { return m_VoxelSize; }
            set { m_VoxelSize = value; }
        }

        // Currently not supported advanced options
        [SerializeField]
        bool m_BuildHeightMesh;

        public bool buildHeightMesh
        {
            get { return m_BuildHeightMesh; }
            set { m_BuildHeightMesh = value; }
        }

        // Reference to whole scene navmesh data asset.
        [SerializeField]
        NavMeshData m_NavMeshData;

        public NavMeshData navMeshData
        {
            get { return m_NavMeshData; }
            set { m_NavMeshData = value; }
        }

        // Do not serialize - runtime only state.
        NavMeshDataInstance m_NavMeshDataInstance;
        Vector3 m_LastPosition = Vector3.zero;
        Quaternion m_LastRotation = Quaternion.identity;

        static readonly List<WalkableArea> s_NavMeshSurfaces = new List<WalkableArea>();

        public static List<WalkableArea> activeSurfaces
        {
            get { return s_NavMeshSurfaces; }
        }

        void OnEnable()
        {
            Register(this);
            AddData();
        }

        void OnDisable()
        {
            RemoveData();
            Unregister(this);
        }

        public void AddData()
        {
            if (m_NavMeshDataInstance.valid)
                return;

            if (m_NavMeshData != null)
            {
                m_NavMeshDataInstance = NavMesh.AddNavMeshData(m_NavMeshData, transform.position, transform.rotation);
                m_NavMeshDataInstance.owner = this;
            }

            m_LastPosition = transform.position;
            m_LastRotation = transform.rotation;
        }

        public void RemoveData()
        {
            m_NavMeshDataInstance.Remove();
            m_NavMeshDataInstance = new NavMeshDataInstance();
        }

        public NavMeshBuildSettings GetBuildSettings()
        {
            var buildSettings = NavMesh.GetSettingsByID(m_AgentTypeID);
            if (buildSettings.agentTypeID == -1)
            {
                Debug.LogWarning("No build settings for agent type ID " + agentTypeID, this);
                buildSettings.agentTypeID = m_AgentTypeID;
            }

            if (overrideTileSize)
            {
                buildSettings.overrideTileSize = true;
                buildSettings.tileSize = tileSize;
            }

            if (overrideVoxelSize)
            {
                buildSettings.overrideVoxelSize = true;
                buildSettings.voxelSize = voxelSize;
            }

            return buildSettings;
        }

        public void BuildNavMesh()
        {
            var sources = CollectSources();

            // Use unscaled bounds - this differs in behaviour from e.g. collider components.
            // But is similar to reflection probe - and since navmesh data has no scaling support - it is the right choice here.
            var sourcesBounds = new Bounds(m_Center, Abs(m_Size));
            sourcesBounds = CalculateWorldBounds(sources);

            var data = NavMeshBuilder.BuildNavMeshData(GetBuildSettings(),
                sources, sourcesBounds, transform.position, transform.rotation);

            if (data != null)
            {
                data.name = gameObject.name;
                RemoveData();
                m_NavMeshData = data;
                if (isActiveAndEnabled)
                    AddData();
            }
        }

        public AsyncOperation UpdateNavMesh(NavMeshData data)
        {
            var sources = CollectSources();

            // Use unscaled bounds - this differs in behaviour from e.g. collider components.
            // But is similar to reflection probe - and since navmesh data has no scaling support - it is the right choice here.
            var sourcesBounds = new Bounds(m_Center, Abs(m_Size));
            sourcesBounds = CalculateWorldBounds(sources);

            return NavMeshBuilder.UpdateNavMeshDataAsync(data, GetBuildSettings(), sources, sourcesBounds);
        }

        static void Register(WalkableArea surface)
        {
            if (s_NavMeshSurfaces.Count == 0)
                NavMesh.onPreUpdate += UpdateActive;

            if (!s_NavMeshSurfaces.Contains(surface))
                s_NavMeshSurfaces.Add(surface);
        }

        static void Unregister(WalkableArea surface)
        {
            s_NavMeshSurfaces.Remove(surface);

            if (s_NavMeshSurfaces.Count == 0)
                NavMesh.onPreUpdate -= UpdateActive;
        }

        static void UpdateActive()
        {
            for (var i = 0; i < s_NavMeshSurfaces.Count; ++i)
                s_NavMeshSurfaces[i].UpdateDataIfTransformChanged();
        }

        List<NavMeshBuildSource> CollectSources()
        {
            var sources = new List<NavMeshBuildSource>();
            var markups = new List<NavMeshBuildMarkup>();

            NavMeshBuilder.CollectSources(transform, m_LayerMask, NavMeshCollectGeometry.RenderMeshes, m_DefaultArea, markups, sources);

            if (m_IgnoreNavMeshAgent)
                sources.RemoveAll((x) => (x.component != null && x.component.gameObject.GetComponent<NavMeshAgent>() != null));

            if (m_IgnoreNavMeshObstacle)
                sources.RemoveAll((x) => (x.component != null && x.component.gameObject.GetComponent<NavMeshObstacle>() != null));

            return sources;
        }

        static Vector3 Abs(Vector3 v)
        {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }

        static Bounds GetWorldBounds(Matrix4x4 mat, Bounds bounds)
        {
            var absAxisX = Abs(mat.MultiplyVector(Vector3.right));
            var absAxisY = Abs(mat.MultiplyVector(Vector3.up));
            var absAxisZ = Abs(mat.MultiplyVector(Vector3.forward));
            var worldPosition = mat.MultiplyPoint(bounds.center);
            var worldSize = absAxisX * bounds.size.x + absAxisY * bounds.size.y + absAxisZ * bounds.size.z;
            return new Bounds(worldPosition, worldSize);
        }

        Bounds CalculateWorldBounds(List<NavMeshBuildSource> sources)
        {
            // Use the unscaled matrix for the NavMeshSurface
            Matrix4x4 worldToLocal = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            worldToLocal = worldToLocal.inverse;

            var result = new Bounds();
            foreach (var src in sources)
            {
                switch (src.shape)
                {
                    case NavMeshBuildSourceShape.Mesh:
                    {
                        var m = src.sourceObject as Mesh;
                        result.Encapsulate(GetWorldBounds(worldToLocal * src.transform, m.bounds));
                        break;
                    }
                    case NavMeshBuildSourceShape.Terrain:
                    {
                        // Terrain pivot is lower/left corner - shift bounds accordingly
                        var t = src.sourceObject as TerrainData;
                        result.Encapsulate(GetWorldBounds(worldToLocal * src.transform, new Bounds(0.5f * t.size, t.size)));
                        break;
                    }
                    case NavMeshBuildSourceShape.Box:
                    case NavMeshBuildSourceShape.Sphere:
                    case NavMeshBuildSourceShape.Capsule:
                    case NavMeshBuildSourceShape.ModifierBox:
                        result.Encapsulate(GetWorldBounds(worldToLocal * src.transform, new Bounds(Vector3.zero, src.size)));
                        break;
                }
            }

            // Inflate the bounds a bit to avoid clipping co-planar sources
            result.Expand(0.1f);
            return result;
        }

        bool HasTransformChanged()
        {
            if (m_LastPosition != transform.position) return true;
            if (m_LastRotation != transform.rotation) return true;
            return false;
        }

        void UpdateDataIfTransformChanged()
        {
            if (HasTransformChanged())
            {
                RemoveData();
                AddData();
            }
        }

#if UNITY_EDITOR
        bool UnshareNavMeshAsset()
        {
            // Nothing to unshare
            if (m_NavMeshData == null)
                return false;

            // Prefab parent owns the asset reference
            var prefabType = UnityEditor.PrefabUtility.GetPrefabType(this);
            if (prefabType == UnityEditor.PrefabType.Prefab)
                return false;

            // An instance can share asset reference only with its prefab parent
            var prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(this) as WalkableArea;
            if (prefab != null && prefab.navMeshData == navMeshData)
                return false;

            // Don't allow referencing an asset that's assigned to another surface
            for (var i = 0; i < s_NavMeshSurfaces.Count; ++i)
            {
                var surface = s_NavMeshSurfaces[i];
                if (surface != this && surface.m_NavMeshData == m_NavMeshData)
                    return true;
            }

            // Asset is not referenced by known surfaces
            return false;
        }

        void OnValidate()
        {
            if (UnshareNavMeshAsset())
            {
                Debug.LogWarning("Duplicating NavMeshSurface does not duplicate the referenced navmesh data", this);
                m_NavMeshData = null;
            }

            var settings = NavMesh.GetSettingsByID(m_AgentTypeID);
            if (settings.agentTypeID != -1)
            {
                // When unchecking the override control, revert to automatic value.
                const float kMinVoxelSize = 0.01f;
                if (!m_OverrideVoxelSize)
                    m_VoxelSize = settings.agentRadius / 3.0f;
                if (m_VoxelSize < kMinVoxelSize)
                    m_VoxelSize = kMinVoxelSize;

                // When unchecking the override control, revert to default value.
                const int kMinTileSize = 16;
                const int kMaxTileSize = 1024;
                const int kDefaultTileSize = 256;

                if (!m_OverrideTileSize)
                    m_TileSize = kDefaultTileSize;

                // Make sure tilesize is in sane range.
                if (m_TileSize < kMinTileSize)
                    m_TileSize = kMinTileSize;
                if (m_TileSize > kMaxTileSize)
                    m_TileSize = kMaxTileSize;
            }
        }
#endif
    }
}
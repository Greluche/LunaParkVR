using UnityEngine;

namespace LowPolyWater
{
    public class LowPolyWater : MonoBehaviour
    {
        [Header("Wave Settings")]
        public float waveHeight = 0.5f;
        public float waveFrequency = 0.5f;
        public float waveLength = 0.75f;

        [Header("Wave Origin")]
        public Vector3 waveOriginPosition = Vector3.zero;

        [Header("Target Object")]
        [SerializeField] private MeshFilter targetMeshFilter;

        private Mesh mesh;
        private Vector3[] vertices;

        private void Awake()
        {
            // If not set manually, use the attached MeshFilter
            if (targetMeshFilter == null)
                targetMeshFilter = GetComponent<MeshFilter>();

            // Duplicate the mesh to avoid modifying shared asset
            mesh = Instantiate(targetMeshFilter.sharedMesh);
            targetMeshFilter.mesh = mesh;
        }

        void Start()
        {
            CreateMeshLowPoly();
        }

        void CreateMeshLowPoly()
        {
            // Get original vertices and triangles
            Vector3[] originalVertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            // Create new vertices for each triangle to break shared verts
            Vector3[] newVertices = new Vector3[triangles.Length];
            for (int i = 0; i < triangles.Length; i++)
            {
                newVertices[i] = originalVertices[triangles[i]];
                triangles[i] = i;
            }

            mesh.vertices = newVertices;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            vertices = mesh.vertices;
        }

        void Update()
        {
            GenerateWaves();
        }

        void GenerateWaves()
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 v = vertices[i];

                float distance = Vector3.Distance(v, waveOriginPosition);
                distance = (distance % waveLength) / waveLength;

                v.y = waveHeight * Mathf.Sin(Time.time * Mathf.PI * 2f * waveFrequency + (Mathf.PI * 2f * distance));
                vertices[i] = v;
            }

            mesh.vertices = vertices;
            mesh.RecalculateNormals();
        }
    }
}
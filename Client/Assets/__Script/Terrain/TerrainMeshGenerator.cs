using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 
    /// </summary>
    public class TerrainMeshGenerator : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        public bool isLocalTerrainMesh;

        /// <summary>
        /// 
        /// </summary>
        public HeightMapFuncTypes heightMapFuncTypes = HeightMapFuncTypes.Global;

        /// <summary>
        /// Zone의 인덱싱 x축 넘버
        /// </summary>
        public int zoneNumX;

        /// <summary>
        /// Zone의 인덱싱 y축 (3차원에선 z축) 넘버
        /// </summary>
        public int zoneNumY;

        /// <summary>
        /// 
        /// </summary>
        public float width = 64;

        /// <summary>
        /// 
        /// </summary>
        public float height = 64;

        /// <summary>
        /// 
        /// </summary>
        public float tileSize = 4;

        /// <summary>
        /// 
        /// </summary>
        public int patchNum = 32;

        /// <summary>
        /// 
        /// </summary>
        public float patchSuffleStrength = 0.1f;

        /// <summary>
        /// 
        /// </summary>
        public Vector3[] HeightMap { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int HeightMapSize => (patchNum + 1) * (patchNum + 1);

        /// <summary>
        /// 
        /// </summary>
        public int VertexNum => patchNum * patchNum * 6;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Vector3 GetRandomPoint(float rangeX, float rangeY)
        {
            var ret = transform.position;

            ret += Vector3.right * UnityEngine.Random.Range(-width * rangeX * 0.5f, width * rangeX * 0.5f);
            ret += Vector3.forward * UnityEngine.Random.Range(-height * rangeY * 0.5f, height * rangeY * 0.5f);

            return TerrainManager.GetTerrainPoint(ret);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        Vector3 GetHeightMapValue(int x, int y)
        {
            return HeightMap[(patchNum + 1) * y + x];
        }

        MeshFilter __meshFilter;
        Vector3[] __meshVertices;
        Vector3[] __meshNormals;
        Vector2[] __meshUVs;
        Color[] __meshColors;
        int[] __meshTriangles;


        /// <summary>
        /// 
        /// </summary>
        public void Init()
        {
            __meshFilter = GetComponent<MeshFilter>();
            __meshVertices = new Vector3[VertexNum];
            __meshNormals = new Vector3[VertexNum];
            __meshUVs = new Vector2[VertexNum];
            __meshColors = new Color[VertexNum];
            __meshTriangles = new int[VertexNum];

            HeightMap = new Vector3[HeightMapSize];

            var stepX = width / patchNum;
            var stepY = height / patchNum;
            var halfSizeVec = new Vector3(width * 0.5f, 0, height * 0.5f);
            var terrainManager = transform.parent.GetComponent<TerrainManager>();

            for (int y = 0; y <= patchNum; y++)
            {
                for (int x = 0; x <= patchNum; x++)
                {
                    var randX = UnityEngine.Random.Range(-stepX * patchSuffleStrength, stepX * patchSuffleStrength);
                    var randY = UnityEngine.Random.Range(-stepY * patchSuffleStrength, stepY * patchSuffleStrength);

                    HeightMap[(patchNum + 1) * y + x] = new Vector3(stepX * x + randX, 0f, stepY * y + randY) - halfSizeVec;
                    HeightMap[(patchNum + 1) * y + x] = HeightMap[(patchNum + 1) * y + x].AdjustY(0);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void FixTearing()
        {
            var adjacencyX = transform.parent.GetComponent<TerrainManager>().GetAdjacency(this, -1, 0);
            var adjacencyY = transform.parent.GetComponent<TerrainManager>().GetAdjacency(this, 0, -1);
            var sizeVec = new Vector3(width, 0, height);

            if (adjacencyX != null)
            {
                for (int y = 0; y <= patchNum; y++)
                    HeightMap[(patchNum + 1) * y] = adjacencyX.HeightMap[(patchNum + 1) * y + patchNum] - Vector3.right * sizeVec.x;
            }

            if (adjacencyY != null)
            {
                for (int x = 0; x <= patchNum; x++)
                    HeightMap[x] = adjacencyY.HeightMap[(patchNum + 1) * patchNum + x] - Vector3.forward * sizeVec.z;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sideVec"></param>
        public void AddBoundary(Vector3 sideVec)
        {
            if (sideVec == Vector3.right)
            {
                var collider = new GameObject("Boundary - Right").AddComponent<BoxCollider>();

                collider.gameObject.tag = "TerrainBoundary";
                collider.gameObject.layer = LayerMask.NameToLayer("Obstacle");
                collider.gameObject.AddComponent<Rigidbody>().isKinematic = true;

                collider.size = new Vector3(1, 10, height + 2);
                collider.transform.SetParent(transform, false);
                // collider.transform.localPosition = new Vector3(width * 0.5f - 0.5f, 0, 0);
                collider.transform.localPosition = Vector3.left * 0.5f;
            }
            else if (sideVec == Vector3.left)
            {
                var collider = new GameObject("Boundary - Left").AddComponent<BoxCollider>();

                collider.gameObject.tag = "TerrainBoundary";
                collider.gameObject.layer = LayerMask.NameToLayer("Obstacle");
                collider.gameObject.AddComponent<Rigidbody>().isKinematic = true;

                collider.size = new Vector3(1, 10, height + 2);
                collider.transform.SetParent(transform, false);
                // collider.transform.localPosition = new Vector3(width * -0.5f + 0.5f, 0, 0)
                collider.transform.localPosition = Vector3.right * 0.5f;

            }
            else if (sideVec == Vector3.forward)
            {
                var collider = new GameObject("Boundary - Forward").AddComponent<BoxCollider>();

                collider.gameObject.tag = "TerrainBoundary";
                collider.gameObject.layer = LayerMask.NameToLayer("Obstacle");
                collider.gameObject.AddComponent<Rigidbody>().isKinematic = true;

                collider.size = new Vector3(width + 2, 10, 1);
                collider.transform.SetParent(transform, false);
                // collider.transform.localPosition = new Vector3(0, 0, height * 0.5f - 0.5f);
                collider.transform.localPosition = Vector3.back * 0.5f;
            }
            else if (sideVec == Vector3.back)
            {
                var collider = new GameObject("Boundary - Back").AddComponent<BoxCollider>();

                collider.gameObject.tag = "TerrainBoundary";
                collider.gameObject.layer = LayerMask.NameToLayer("Obstacle");
                collider.gameObject.AddComponent<Rigidbody>().isKinematic = true;

                collider.size = new Vector3(width + 2, 10, 1);
                collider.transform.SetParent(transform, false);
                // collider.transform.localPosition = new Vector3(0, 0, height * -0.5f + 0.5f);
                collider.transform.localPosition = Vector3.forward * 0.5f;
            }
            else
            {
                Debug.LogAssertion($"2?? sideVec is invalid => {sideVec}");
            }
        }

        public void ResetMesh()
        {
            var stepX = width / patchNum;
            var stepY = height / patchNum;

            int vertexIndex = 0;

            for (int y = 0; y < patchNum; y++)
            {
                for (int x = 0; x < patchNum; x++)
                {
                    __meshVertices[vertexIndex] = HeightMap[(patchNum + 1) * y + x];
                    __meshVertices[vertexIndex + 1] = HeightMap[(patchNum + 1) * (y + 1) + x];
                    __meshVertices[vertexIndex + 2] = HeightMap[(patchNum + 1) * y + x + 1];
                    __meshVertices[vertexIndex + 3] = HeightMap[(patchNum + 1) * y + x + 1];
                    __meshVertices[vertexIndex + 4] = HeightMap[(patchNum + 1) * (y + 1) + x];
                    __meshVertices[vertexIndex + 5] = HeightMap[(patchNum + 1) * (y + 1) + x + 1];

                    __meshNormals[vertexIndex] = Vector3.up;
                    __meshNormals[vertexIndex + 1] = Vector3.up;
                    __meshNormals[vertexIndex + 2] = Vector3.up;
                    __meshNormals[vertexIndex + 3] = Vector3.up;
                    __meshNormals[vertexIndex + 4] = Vector3.up;
                    __meshNormals[vertexIndex + 5] = Vector3.up;

                    __meshUVs[vertexIndex] = new Vector2(stepX * x / tileSize, stepY * y / tileSize);
                    __meshUVs[vertexIndex + 1] = new Vector2(stepX * x / tileSize, stepY * (y + 1) / tileSize);
                    __meshUVs[vertexIndex + 2] = new Vector2(stepX * (x + 1) / tileSize, stepY * y / tileSize);
                    __meshUVs[vertexIndex + 3] = new Vector2(stepX * (x + 1) / tileSize, stepY * y / tileSize);
                    __meshUVs[vertexIndex + 4] = new Vector2(stepX * x / tileSize, stepY * (y + 1) / tileSize);
                    __meshUVs[vertexIndex + 5] = new Vector2(stepX * (x + 1) / tileSize, stepY * (y + 1) / tileSize);

                    __meshColors[vertexIndex] = Color.clear;
                    __meshColors[vertexIndex + 1] = Color.clear;
                    __meshColors[vertexIndex + 2] = Color.clear;
                    __meshColors[vertexIndex + 3] = Color.clear;
                    __meshColors[vertexIndex + 4] = Color.clear;
                    __meshColors[vertexIndex + 5] = Color.clear;

                    vertexIndex += 6;
                }
            }

            for (int i = 0; i < VertexNum; ++i)
            {
                __meshNormals[i] = Vector3.up;
                __meshTriangles[i] = i;
            }

            RefreshMesh();
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateMesh()
        {
            var terrainManager = transform.parent.GetComponent<TerrainManager>();

            Debug.Assert(terrainManager != null);

            for (int y = 0; y <= patchNum; y++)
            {
                for (int x = 0; x <= patchNum; x++)
                {
                    var index = (patchNum + 1) * y + x;

                    // if (heightMapFuncTypes == HeightMapFuncTypes.Global)
                    //     HeightMap[index] = HeightMap[index].AdjustY(HeightMapFunc.GetGlobalHeight(HeightMap[index] + (isLocalTerrainMesh ? Vector3.zero : transform.position), terrainManager.worldSize));
                    // else if (heightMapFuncTypes == HeightMapFuncTypes.Bubble)
                    //     HeightMap[index] = HeightMap[index].AdjustY(HeightMapFunc.GetBubbleHeight(HeightMap[index] + (isLocalTerrainMesh ? Vector3.zero : transform.position), width, 1, 1, 1));
                    // else if (heightMapFuncTypes == HeightMapFuncTypes.Slope)
                    //     HeightMap[index] = HeightMap[index].AdjustY(0);
                    // else
                    //     Debug.Assert(false);

                    HeightMap[index] = HeightMap[index].AdjustY(terrainManager.HeightMapFunc(HeightMap[index] + (isLocalTerrainMesh ? Vector3.zero : transform.position)));
                }
            }

            int vertexIndex = 0;

            for (int y = 0; y < patchNum; y++)
            {
                for (int x = 0; x < patchNum; x++)
                {
                    __meshVertices[vertexIndex] = HeightMap[(patchNum + 1) * y + x];
                    __meshVertices[vertexIndex + 1] = HeightMap[(patchNum + 1) * (y + 1) + x];
                    __meshVertices[vertexIndex + 2] = HeightMap[(patchNum + 1) * y + x + 1];
                    __meshVertices[vertexIndex + 3] = HeightMap[(patchNum + 1) * y + x + 1];
                    __meshVertices[vertexIndex + 4] = HeightMap[(patchNum + 1) * (y + 1) + x];
                    __meshVertices[vertexIndex + 5] = HeightMap[(patchNum + 1) * (y + 1) + x + 1];

                    vertexIndex += 6;
                }
            }

            for (int i = 0; i < VertexNum; i += 3)
            {
                var vecA = __meshVertices[i + 1] - __meshVertices[i];
                var vecB = __meshVertices[i + 2] - __meshVertices[i];
                var normal = Vector3.Cross(vecA, vecB).normalized;

                __meshNormals[i] = __meshNormals[i + 1] = __meshNormals[i + 2] = normal;

                var t = Mathf.Clamp(Vector3.Angle(normal, Vector3.up), 0, 30) / 30;

                __meshColors[i] = __meshColors[i + 1] = __meshColors[i + 2] = Color.Lerp(Color.white, Color.black, t).AdjustAlpha(Mathf.Lerp(0, 0.2f, t));
            }

            RefreshMesh();
        }

        void RefreshMesh()
        {
            var meshFilter = GetComponent<MeshFilter>();

            meshFilter.sharedMesh = new Mesh();
            meshFilter.sharedMesh.vertices = __meshVertices;
            meshFilter.sharedMesh.normals = __meshNormals;
            meshFilter.sharedMesh.uv = __meshUVs;
            meshFilter.sharedMesh.triangles = __meshTriangles;
            meshFilter.sharedMesh.colors = __meshColors;

            meshFilter.sharedMesh.RecalculateBounds();
            meshFilter.sharedMesh.Optimize();

            GetComponent<MeshCollider>().sharedMesh = null;
            GetComponent<MeshCollider>().sharedMesh = meshFilter.sharedMesh;
        }
    }
}
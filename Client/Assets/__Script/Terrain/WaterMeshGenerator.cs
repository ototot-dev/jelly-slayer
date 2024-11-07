using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 
    /// </summary>
    public class WaterMeshGenerator : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        public float width = 16;

        /// <summary>
        /// 
        /// </summary>
        public float height = 16;
        
        /// <summary>
        /// 
        /// </summary>
        public float tileSize = 4; 

        /// <summary>
        /// 
        /// </summary>
        public int patchNum = 4;

        /// <summary>
        /// 
        /// </summary>
        public int VertexNum => patchNum * patchNum * 6;

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

            for (int y = 0; y <= patchNum; y++)
            {
                for (int x = 0; x <= patchNum; x++)
                {
                    HeightMap[(patchNum + 1) * y + x] = new Vector3(stepX * x, 0f, stepY * y) - halfSizeVec;
                    HeightMap[(patchNum + 1) * y + x] = HeightMap[(patchNum + 1) * y + x].AdjustY(0);
                }
            }
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
            for (int y = 0; y <= patchNum; y++)
            {
                for (int x = 0; x <= patchNum; x++)
                {
                    var index = (patchNum + 1) * y + x;

                    // if (heightMapFuncTypes == HeightMapFuncTypes.Global)
                    //     HeightMap[index] = HeightMap[index].AdjustY(HeightMapFunc.GetGlobalHeight(HeightMap[index] + (isLocalTerrainMesh ? Vector3.zero : transform.position)));
                    // else if (heightMapFuncTypes == HeightMapFuncTypes.Bubble)
                    //     HeightMap[index] = HeightMap[index].AdjustY(HeightMapFunc.GetBubbleHeight(HeightMap[index] + (isLocalTerrainMesh ? Vector3.zero : transform.position), width, 1, 1, 1));
                    // else if (heightMapFuncTypes == HeightMapFuncTypes.Slope)
                    //     HeightMap[index] = HeightMap[index].AdjustY(0);
                    // else
                    //     Debug.Assert(false);

                    HeightMap[index] = HeightMap[index].AdjustY(0);
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

            meshFilter.mesh.Clear(false);

            meshFilter.mesh.vertices = __meshVertices;
            meshFilter.mesh.normals = __meshNormals;
            meshFilter.mesh.uv = __meshUVs;
            meshFilter.mesh.triangles = __meshTriangles;
            meshFilter.mesh.colors = __meshColors;

            meshFilter.mesh.RecalculateBounds();
            meshFilter.mesh.Optimize();

            GetComponent<MeshCollider>().sharedMesh = null;
            GetComponent<MeshCollider>().sharedMesh = meshFilter.mesh;
        }
    }
}
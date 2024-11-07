using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{

    /// <summary>
    /// 
    /// </summary>
    public static class SharedMeshBuilder
    {
        
        /// <summary>
        /// 
        /// </summary>
        public static class Sphere
        {

            /// <summary>
            /// 
            /// </summary>
            public static Mesh sharedMeshCached;

            /// <summary>
            /// 원 반경
            /// </summary>
            public const float SHARED_MESH_RADIUS = 0.5f;

            /// <summary>
            /// 가로 둘레를 구성하는 선분 갯수
            /// </summary>
            public const int SHARED_MESH_PATCH_NUM = 32;

            /// <summary>
            /// 세로 둘레를 구성하는 선분 갯수
            /// </summary>
            public const int SHARED_MESH_SEGMENT_NUM = 32;

            /// <summary>
            /// 
            /// </summary>
            public static int VertextNumPerSegment => SHARED_MESH_SEGMENT_NUM - 1;

            /// <summary>
            /// 
            /// </summary>
            public static int TotalVertextNum => VertextNumPerSegment * SHARED_MESH_PATCH_NUM + 2;

            /// <summary>
            /// 
            /// </summary>>
            public static int FaceNumPerPatch => ((SHARED_MESH_SEGMENT_NUM - 2) * 2 + 2);

            /// <summary>
            /// 
            /// </summary>
            public static int TotalFaceNum => FaceNumPerPatch * SHARED_MESH_PATCH_NUM;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="sharedMesh"></param>
            public static Mesh BuildMesh()
            {
                if (sharedMeshCached == null)
                {
                    var vertices = new Vector3[TotalVertextNum];
                    var uvs = new Vector2[TotalVertextNum];
                    var triangles = new int[TotalFaceNum * 3];

                    FillVertexBuffer(vertices, uvs);
                    FillIndexBuffer(triangles);

                    sharedMeshCached = new Mesh();

                    sharedMeshCached.vertices = vertices;
                    sharedMeshCached.uv = uvs;
                    sharedMeshCached.triangles = triangles;

                    sharedMeshCached.RecalculateNormals();
                    sharedMeshCached.RecalculateBounds();
                    // mesh.Optimize();
                }

                return sharedMeshCached;
            }

            /// <summary>
            /// 버택스 값 셋팅
            /// </summary>
            /// <param name="vertices"></param>
            static void FillVertexBuffer(Vector3[] vertices, Vector2[] uv)
            {
                // 최하단과 최상단 꼭지점
                vertices[0] = Vector3.down * SHARED_MESH_RADIUS;
                vertices[1] = Vector3.up * SHARED_MESH_RADIUS;

                uv[0] = new Vector2(0.5f, 0);
                uv[1] = new Vector2(0.5f, 1);

                // Debug.Log($"@@ vertex => top: {vertices[0]}, bottom: {vertices[1]}");

                // 측면
                for (int x = 0; x < SHARED_MESH_PATCH_NUM; x++)
                {
                    var baseIndex = VertextNumPerSegment * x + 2;
                    var baseX = Quaternion.Euler(0, 360f / SHARED_MESH_PATCH_NUM * x, 0) * Vector3.right;

                    for (int y = 0; y < VertextNumPerSegment; y++)
                    {
                        var baseY = Quaternion.Euler(0, 0, 180f / SHARED_MESH_SEGMENT_NUM * (y + 1)) * Vector3.down;
                        var radius = Mathf.Sqrt(1 - baseY.y * baseY.y) * SHARED_MESH_RADIUS;

                        vertices[baseIndex + y] = radius * baseX + baseY.y * Vector3.up * SHARED_MESH_RADIUS;
                        uv[baseIndex + y] = new Vector2(x / (float)SHARED_MESH_PATCH_NUM, (y + 1) / (float)SHARED_MESH_SEGMENT_NUM);

                        // Debug.Log($"@@ vertex => side: {vertices[baseIndex + y]}");
                    }
                }
            }

            /// <summary>
            /// 인덱스 (삼각형) 값 셋팅
            /// </summary>
            /// <param name="triangles"></param>
            static void FillIndexBuffer(int[] triangles)
            {
                for (int x = 0; x < SHARED_MESH_PATCH_NUM; x++)
                {
                    var currIndex = FaceNumPerPatch * 3 * x;

                    // 최하단 꼭지점과 연결
                    triangles[currIndex] = 0;
                    triangles[currIndex + 1] = GetVertexStartIndex(x + 1);
                    triangles[currIndex + 2] = GetVertexStartIndex(x);

                    // Debug.Log($"@@bottom => {currIndex}:{triangles[currIndex]}, {currIndex + 1}:{triangles[currIndex + 1]}, {currIndex + 2}:{triangles[currIndex + 2]}");

                    currIndex += 3;

                    // 최상단 꼭지점과 연결
                    triangles[currIndex] = 1;
                    triangles[currIndex + 1] = GetVertexEndIndex(x);
                    triangles[currIndex + 2] = GetVertexEndIndex(x + 1);

                    // Debug.Log($"@@top => {currIndex}:{triangles[currIndex]}, {currIndex + 1}:{triangles[currIndex + 1]}, {currIndex + 2}:{triangles[currIndex + 2]}");

                    currIndex += 3;

                    // 왼쪽 라인과 연결된 삼각형
                    for (int y = 0; y < VertextNumPerSegment - 1; y++)
                    {
                        triangles[currIndex] = GetVertexStartIndex(x) + y;
                        triangles[currIndex + 1] = GetVertexStartIndex(x + 1) + y + 1;
                        triangles[currIndex + 2] = GetVertexStartIndex(x) + y + 1;

                        // Debug.Log($"@@left => {currIndex}:{triangles[currIndex]}, {currIndex + 1}:{triangles[currIndex + 1]}, {currIndex + 2}:{triangles[currIndex + 2]}");

                        currIndex += 3;
                    }

                    // 오른쪽 라인과 연결된 상각형
                    for (int y = 0; y < VertextNumPerSegment - 1; y++)
                    {
                        triangles[currIndex] = GetVertexStartIndex(x + 1) + y;
                        triangles[currIndex + 1] = GetVertexStartIndex(x + 1) + y + 1;
                        triangles[currIndex + 2] = GetVertexStartIndex(x) + y;

                        // Debug.Log($"@@right => {currIndex}:{triangles[currIndex]}, {currIndex + 1}:{triangles[currIndex + 1]}, {currIndex + 2}:{triangles[currIndex + 2]}");

                        currIndex += 3;
                    }
                }
            }

            /// <summary>
            /// 패치 인덱스 값에 해당하는 버텍스의 시작 인덱스 값을 리턴
            /// </summary>
            /// <param name="patchIndex"></param>
            /// <returns></returns>
            static int GetVertexStartIndex(int patchIndex)
            {
                var ret = VertextNumPerSegment * patchIndex + 2;

                if (ret >= TotalVertextNum)
                    ret -= TotalVertextNum - 2;

                return ret;
            }

            /// <summary>
            /// 패치 인덱스 값에 해당하는 버텍스의 마지막 인덱스 값을 리턴
            /// </summary>
            /// <param name="patchIndex"></param>
            /// <returns></returns>
            static int GetVertexEndIndex(int patchIndex)
            {
                var ret = VertextNumPerSegment * (patchIndex + 1) + 1;

                if (ret >= TotalVertextNum)
                    ret -= TotalVertextNum - 2;

                return ret;
            }

        }

    }

}

using UnityEngine;
using System.Collections.Generic;
using Unity.Linq;
using System.Linq;
using UniRx;

namespace Game
{
    public class JellyMeshBuilder : MonoBehaviour
    {
        public float radius;
        public Mesh sourceMesh;
        public SkinnedMeshRenderer meshRenderer;

        void Start()
        {
            if (meshRenderer != null && meshRenderer.sharedMesh == null)
                BuildMesh();
        }

        public void BuildMesh()
        {
            Debug.Assert(meshRenderer != null);

            var vertices = new List<Vector3>(); sourceMesh.GetVertices(vertices);
            var normals = new List<Vector3>(); sourceMesh.GetNormals(normals);
            var uv = new List<Vector2>(); sourceMesh.GetUVs(0, uv);

            var targetMesh = new Mesh();

            targetMesh.vertices = vertices.Select(v => v.normalized * radius).ToArray();
            targetMesh.normals = normals.ToArray();
            // targetMesh.uv = uv.ToArray();
            targetMesh.uv = vertices.Select(v => CartesianToSpherical(v.normalized)).ToArray();
            targetMesh.triangles = sourceMesh.GetTriangles(0);

            var bones = new Transform[] { meshRenderer.rootBone }.Concat(meshRenderer.rootBone.gameObject.Children().Select(c => c.transform)).ToArray();

            FillBindPoseAndBoneWeight(targetMesh, bones);

            targetMesh.RecalculateBounds();

            meshRenderer.sharedMesh = targetMesh;
            meshRenderer.bones = bones;
        }

        /// <summary>
        /// 초기화 (구현 없음)
        /// </summary>
        public void Reset()
        {
            ;
        }

        public Vector2 CartesianToSpherical(Vector3 position)
        {
            var theta = (Mathf.Atan2(position.z,  position.x) * Mathf.Rad2Deg + 180f) / 360f;
            var phi = Mathf.Acos(position.y) * Mathf.Rad2Deg / 180f;
            return new Vector2(theta, phi);
        }

        /// <summary>
        /// 스킨 애니메이션 본웨이트 셋팅
        /// </summary>
        /// <param name="boneWeights"></param>
        /// <param name="vertices"></param>
        void FillBindPoseAndBoneWeight(Mesh mesh, Transform[] bones)
        {
            var bindPoses = new Matrix4x4[bones.Length];

            for (int i = 0; i < bindPoses.Length; i++)
                bindPoses[i] = bones[i].worldToLocalMatrix * meshRenderer.rootBone.transform.localToWorldMatrix;

            var boneWeights = new BoneWeight[mesh.vertexCount];

            for (int i = 0; i < mesh.vertexCount; i++)
            {
                var t4 = bones.Skip(1).OrderBy(b => (transform.worldToLocalMatrix.MultiplyPoint(b.position) - mesh.vertices[i]).sqrMagnitude).Take(4).ToArray();

                var p0 = meshRenderer.rootBone.worldToLocalMatrix.MultiplyPoint(t4[0].position);
                var p1 = meshRenderer.rootBone.worldToLocalMatrix.MultiplyPoint(t4[1].position);
                var p2 = meshRenderer.rootBone.worldToLocalMatrix.MultiplyPoint(t4[2].position);
                var p3 = meshRenderer.rootBone.worldToLocalMatrix.MultiplyPoint(t4[3].position);

                var d0 = 1f / (p0 - mesh.vertices[i]).magnitude;
                var d1 = 1f / (p1 - mesh.vertices[i]).magnitude;
                var d2 = 1f / (p2 - mesh.vertices[i]).magnitude;
                var d3 = 1f / (p3 - mesh.vertices[i]).magnitude;
                var sum = d0 + d1 + d2 + d3;

                boneWeights[i].weight0 = d0 / sum;
                boneWeights[i].weight1 = d1 / sum;
                boneWeights[i].weight2 = d2 / sum;
                boneWeights[i].weight3 = d3 / sum;
                
                for (int j = 0; j < bones.Length; j++)
                {
                    if (bones[j] == t4[0]) boneWeights[i].boneIndex0 = j;
                    else if (bones[j] == t4[1]) boneWeights[i].boneIndex1 = j;
                    else if (bones[j] == t4[2]) boneWeights[i].boneIndex2 = j;
                    else if (bones[j] == t4[3]) boneWeights[i].boneIndex3 = j;
                }
            }

            mesh.bindposes = bindPoses;
            mesh.boneWeights = boneWeights;
        }
    }
}
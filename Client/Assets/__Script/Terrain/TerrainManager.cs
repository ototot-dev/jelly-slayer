using UnityEngine;

namespace Game
{
    /// <summary>
    /// 
    /// </summary>
    public class TerrainManager : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Vector3 GetTerrainPoint(Vector3 position)
        {
            var hit = GetTerrainHitPoint(position);

            if (hit.collider != null)
                return hit.point;
            else
                return position;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="up"></param>
        /// <returns></returns>
        public static RaycastHit GetTerrainHitPoint(Vector3 position)
        {
            if (__nonAllocHits == null)
                __nonAllocHits = new RaycastHit[1];

            if (Physics.RaycastNonAlloc(new Ray(position + 9999f * Vector3.up, Vector3.down), __nonAllocHits, Mathf.Infinity, LayerMask.GetMask(TerrainManager.LayerName)) > 0)
                return __nonAllocHits[0];
            else
                return new RaycastHit();
        }

        static RaycastHit[] __nonAllocHits;

        /// <summary>
        /// 기본 Zone 사이즈
        /// </summary>
        public const float DEFAULT_ZONE_SIZE = 32;

        /// <summary>
        /// 월드의 x축 길이
        /// </summary>
        public Vector2 worldSize = new Vector2(DEFAULT_ZONE_SIZE * 3, DEFAULT_ZONE_SIZE * 3);

        /// <summary>
        /// 
        /// </summary>
        public AnimationCurve globalHeightCurve;

        public int zoneNumX = 3;
        public int zoneNumY = 3;
        public int ZoneNumWithPaddingX => zoneNumX + 2;
        public int ZoneNumWithPaddingY => zoneNumY + 2;
        public float ZoneLength => Mathf.Max(worldSize.x / zoneNumX, worldSize.y / zoneNumY);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public float HeightMapFunc(Vector3 position)
        {
            var point = new Vector2(position.x, position.z);
            
            const float a = 0.03f, b = 3.2f, c = 0.2f, d = 0.4f;
            
            return Perlin.Noise01(point * a) * b + Perlin.Noise(point * c) * d;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Generate()
        {
            //* Zone의 갯수는 홀수여야 함
            Debug.Assert(zoneNumX % 2 == 1);
            Debug.Assert(zoneNumY % 2 == 1);

            var minWithPaddingX = -ZoneNumWithPaddingX / 2;
            var maxWithPaddingX = ZoneNumWithPaddingX / 2;

            var minWithPaddingY = -ZoneNumWithPaddingY / 2;
            var maxWithPaddingY =  ZoneNumWithPaddingY / 2;

            zoneMeshGenerators = new TerrainMeshGenerator[ZoneNumWithPaddingX * ZoneNumWithPaddingY];

            for (int x = minWithPaddingX; x <= maxWithPaddingX; x++)
            {
                for (int y = minWithPaddingY; y <= maxWithPaddingY; y++)
                {
                    var zone = new GameObject($"Zone[{x},{y}]");

                    zone.layer = LayerMask.NameToLayer("Terrain");
                    zone.transform.position = new Vector3(ZoneLength * x, 0, ZoneLength * y);

                    zone.AddComponent<MeshFilter>();
                    zone.AddComponent<MeshCollider>();
                    zone.AddComponent<MeshRenderer>().sharedMaterial = Resources.Load<Material>("Material/Common/Terrain");

                    var meshGenerator = zone.AddComponent<TerrainMeshGenerator>();

                    meshGenerator.zoneNumX = x;
                    meshGenerator.zoneNumY = y;
                    meshGenerator.width = ZoneLength;
                    meshGenerator.height = ZoneLength;
                    meshGenerator.tileSize = 4;
                    meshGenerator.patchNum = (int)(ZoneLength * 0.5f);
                    meshGenerator.patchSuffleStrength = 0.2f;
                    meshGenerator.heightMapFuncTypes = HeightMapFuncTypes.Global;

                    zoneMeshGenerators[(y + maxWithPaddingY) * ZoneNumWithPaddingX + (x + maxWithPaddingX)] = meshGenerator;

                    meshGenerator.transform.SetParent(transform);
                    meshGenerator.Init();

                    // Debug.Log($"1?? zone index => {x}, {y} / {x + maxX}, {y + maxY}");
                }
            }

            foreach (var g in zoneMeshGenerators)
            {
                g.FixTearing();
                g.ResetMesh();
                g.UpdateMesh();

                if (g.zoneNumX == minWithPaddingX)
                    g.AddBoundary(Vector3.left);
                if (g.zoneNumX == maxWithPaddingX)
                    g.AddBoundary(Vector3.right);
                if (g.zoneNumY == minWithPaddingY)
                    g.AddBoundary(Vector3.back);
                if (g.zoneNumY == maxWithPaddingY)
                    g.AddBoundary(Vector3.forward);
            }
        }

        /// <summary>
        /// 생성 확인
        /// </summary>
        public bool IsTerrainGenerated => transform.childCount > 1;
        
        /// <summary>
        /// 
        /// </summary>
        public TerrainMeshGenerator[] zoneMeshGenerators;
        
        /// <summary>
        /// 
        /// </summary>
        public static string LayerName => "Terrain";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        /// <returns></returns>
        public TerrainMeshGenerator GetAdjacency(TerrainMeshGenerator self, int offsetX, int offsetY)
        {
            var x = (self.zoneNumX + ZoneNumWithPaddingX / 2) + offsetX;
            var y = (self.zoneNumY + ZoneNumWithPaddingY / 2) + offsetY;

            // Debug.Log($"1?? zone index => {self.zoneNumX} - {offsetX}, {self.zoneNumY} - {offsetY} = {x}, {y}");

            var index = y * ZoneNumWithPaddingX + x;

            if (index >= 0 && index < zoneMeshGenerators.Length)
                return zoneMeshGenerators[index];
            else
                return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public GameObject GetTerrainZone(Vector3 position)
        {
            var x = (int)(Mathf.Clamp(position.x - worldSize.x, 0, ZoneNumWithPaddingX - 1) / ZoneLength);
            var y = (int)(Mathf.Clamp(position.z - worldSize.y, 0, ZoneNumWithPaddingY - 1) / ZoneLength);

            var maxX = ZoneNumWithPaddingX / 2;
            var maxY = ZoneNumWithPaddingY / 2;

            return zoneMeshGenerators[(y + maxY) * zoneNumX + (x + maxX)].gameObject;
        }
    }
}
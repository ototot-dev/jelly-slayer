using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;


namespace Game
{

    /// <summary>
    /// 플레이어 조작 커서 컨트롤러
    /// </summary>
    public class KeyboardController : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        //public Vector3 CursorPosition => __cursorObj.transform.position;

        //GameObject __cursorObj;

        public Vector3 _vDir;
        public Vector3 _vCamDir;

        public Vector3 MoveVec => _vDir.Vector2D();

        public Vector3 GetTargetPosition(Vector3 playerPosition)
        {
            return playerPosition + (1 * _vDir);
            // return playerPosition + Vector3.right;
        }

        void Awake()
        {
            //__cursorObj = Instantiate(Resources.Load<GameObject>("Player/Cursor"));
        }

        void Update()
        {
            //__cursorObj.transform.localRotation *= Quaternion.Euler(0f, 180f * Time.deltaTime, 0f);
        }
        
        /// <summary>
        /// 커서를 지면 위 Picking Point로 이동
        /// </summary>
        /// <param name="mousePoint"></param>
        /// <returns></returns>
        public bool TryMoveCursorPositionOnTerrain(Vector3 mousePoint)
        {
            var camera = GameContext.Instance.cameraCtrler.gameCamera;

            if (camera == null)
                return false;

            var ray = camera.ScreenPointToRay(mousePoint);
            var hitPoints = Physics.RaycastAll(ray, 9999f, LayerMask.GetMask(TerrainManager.LayerName));

            if (hitPoints.Length > 0)
            {
                //__cursorObj.transform.position = hitPoints.OrderBy(h => h.distance).ToArray().First().point + Vector3.up * 0.25f;
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool TryInputKey(Vector3 mousePoint)
        {
            bool isInput = false;
            Vector3 v = Vector3.zero;
            if (Input.GetKey(KeyCode.W))
            {
                v += Vector3.forward;
                isInput = true;
            }
            if (Input.GetKey(KeyCode.A))
            { 
                v += Vector3.left;
                isInput = true;
            }
            if (Input.GetKey(KeyCode.S))
            {
                v += Vector3.back;
                isInput = true;
            }
            if (Input.GetKey(KeyCode.D))
            {
                v += Vector3.right;
                isInput = true;
            }
            if (isInput == true)
            {
                var vCam = Camera.main.transform.eulerAngles;
                _vDir = Quaternion.AngleAxis(vCam.y, Vector3.up) * v.normalized;
            }
            else
            {
                //_vDir = Vector3.zero;
            }
            return isInput;
        }
        public bool TryInputStick(float horz, float vert)
        {
            Vector3 v = new Vector3(horz, 0, vert);
            if (v.magnitude > 0)
            {
                v.Normalize();

                var vCam = Camera.main.transform.eulerAngles;
                _vDir = Quaternion.AngleAxis(vCam.y, Vector3.up) * v;
                return true;
            }
            return false;
        }
    }
}
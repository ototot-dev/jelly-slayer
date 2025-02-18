using UnityEngine;
using PixelCamera;

namespace Game
{
    public class AutoZommController : MonoBehaviour
    {
        public PlayerController _playerCtrler;
        public CameraController _cameraCtrler;
        public PixelCameraManager _pixelCamera;

        float _curZomm = 1;
        [SerializeField] float _targetZoom = 1;
        [SerializeField] float _dist = 0;
        public float _min = 0.7f;
        public float _max = 1.2f;
        public float _limit = 4.0f;
        public float _rate = 0.2f;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }
        void SetTargetZoom(Vector3 vDist)
        {
            _dist = vDist.magnitude;
            var zoom = _rate * ((_dist / _limit) - 1.0f) + 1.0f;
            _targetZoom = Mathf.Clamp(zoom, _min, _max);
        }
        // Update is called once per frame
        void Update()
        {
            if(_playerCtrler == null || _playerCtrler.possessedBrain == null)
                return;

            var brain = _playerCtrler.possessedBrain;
            var target = brain.BB.TargetBrain;
            var heroPos = brain.coreColliderHelper.transform.position;
            if (target != null)
            {
                var vDist = target.coreColliderHelper.transform.position - heroPos;
                SetTargetZoom(vDist);
            }
            else 
            {
                float distMax = 0;
                PawnBrainController findBrain = null;
                PawnBrainController[] brains = FindObjectsByType<PawnBrainController>(FindObjectsSortMode.None);
                for (int ia = 0; ia < brains.Length; ia++)
                {
                    var br = brains[ia];
                    if(br != null &&  br != brain) 
                    {
                        var vDist = br.coreColliderHelper.transform.position - heroPos;
                        if (vDist.magnitude > distMax) 
                        {
                            findBrain = br;
                        }
                    }
                }
                if (findBrain != null)
                {
                    SetTargetZoom(findBrain.coreColliderHelper.transform.position - heroPos);
                }
                else _targetZoom = 1;
            }
            _pixelCamera.GameCameraZoom = _targetZoom * 4.2f;
            //_cameraCtrler.zoom = _targetZoom;
        }
    }
}
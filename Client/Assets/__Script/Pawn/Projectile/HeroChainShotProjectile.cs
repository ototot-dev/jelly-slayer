using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Game
{
    public class HeroChainShotProjectile : HeroProjectile
    {
        [SerializeField]
        float _speed = 1;

        bool _isBind = false;
        bool _isPulling = false;

        Vector3 _bindVec;
        Quaternion _bindRot;
        PawnBrainController _targetBrain;

        protected override void StartInternal()
        {
            base.StartInternal();

            //* Terrain 디텍션 코드
            /*
            rigidBodyCollider.OnCollisionEnterAsObservable().Subscribe(c =>
            {
                if ((c.gameObject.layer & LayerMask.NameToLayer("Terrain")) > 0)
                    __isGrounded = true;
            }).AddTo(this);
            */
            sensorCollider.OnTriggerEnterAsObservable().Subscribe(t =>
            {
                Debug.Log("sensor OnTriggerEnterAsObservable");
                if (_isBind == false && t.gameObject != rigidBody.gameObject) 
                {
                    var helper = t.GetComponent<PawnColliderHelper>();
                    if(helper != null && helper.pawnBrain != null && helper.pawnBrain != emitterBrain)
                    {
                        _isBind = true;
                        _targetBrain = helper.pawnBrain;
                        emitterBrain.Bind(_targetBrain);

                        transform.SetParent(_targetBrain.CoreTransform);

                        // 묶인 거리 설정
                        var posTarget = transform.position;
                        var posSource = emitterBrain.CoreTransform.position;
                        _bindVec = posTarget - posSource;

                        _bindRot = transform.rotation;
                    }
                }

            }).AddTo(this);
        }

        protected override void OnUpdateHandler() 
        {
            base.OnUpdateHandler();

            if (_isBind == true)
            {
                var posTarget = _targetBrain.CoreTransform.position;
                var posSource = emitterBrain.CoreTransform.position;

                var vDist = (posTarget - posSource);

                if (vDist.magnitude > _bindVec.magnitude)
                {
                    var newPos = posSource + (_bindVec.magnitude * vDist.normalized);
                    _targetBrain.CoreTransform.position = newPos;

                    var rot = Quaternion.FromToRotation(_bindVec, vDist.normalized);
                    _targetBrain.CoreTransform.rotation = rot;
                }
            }
            else if (_isPulling == true) 
            { 
            }
            else
            {
                transform.position += _speed * Time.deltaTime * velocity;
            }
        }
    }
}
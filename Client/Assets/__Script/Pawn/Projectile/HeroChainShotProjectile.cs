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

            //* Terrain ���ؼ� �ڵ�
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
                if (_isBind == false && t.gameObject != __rigidBody.gameObject) 
                {
                    var helper = t.GetComponent<PawnColliderHelper>();
                    if(helper != null && helper.pawnBrain != null && helper.pawnBrain != heroBrain)
                    {
                        _isBind = true;
                        _targetBrain = helper.pawnBrain;
                        // emitterBrain.Bind(_targetBrain);

                        transform.SetParent(_targetBrain.GetWorldTransform());

                        // ���� �Ÿ� ����
                        var posTarget = transform.position;
                        var posSource = heroBrain.GetWorldPosition();
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
                var posTarget = _targetBrain.GetWorldPosition();
                var posSource = heroBrain.GetWorldPosition();

                var vDist = (posTarget - posSource);

                if (vDist.magnitude > _bindVec.magnitude)
                {
                    var newPos = posSource + (_bindVec.magnitude * vDist.normalized);
                    _targetBrain.GetWorldTransform().position = newPos;

                    var rot = Quaternion.FromToRotation(_bindVec, vDist.normalized);
                    _targetBrain.GetWorldTransform().rotation = rot;
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
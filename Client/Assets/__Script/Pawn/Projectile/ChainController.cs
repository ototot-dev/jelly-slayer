using UniRx;
using UniRx.Triggers;
using Game;
using UnityEngine;

public class ChainController : MonoBehaviour
{
    public enum ChainStatus
    {
        None,
        Rolling,
        Shoot,
        Bind,
    }

    public HeroBrain _heroBrain;
    public PawnBrainController _bindBrain;

    public GameObject _cableObj;
    public BoxCollider _boxCollider;

    [Header("Chain")]
    public Transform _trRightHand;
    public Transform _trHandle; // Handle
    public Transform _trDagger; // Dagger

    readonly float _cableLength = 80.0f;

    [SerializeField]
    ChainStatus _status = ChainStatus.None;

    float _angle = 0;
    float _rollSpeed = 100;
    float _shotSpeed = 30;
    Vector3 _vShoot;
    Vector3 _shootPos;

    float _bindDist = 5;
    Vector3 _bindLocalPos;


    public bool IsRolling
    {
        get { return (_status == ChainStatus.Rolling); }
    }
    public bool IsBind
    {
        get { return (_status == ChainStatus.Bind); }
    }

    // Start is called before the first frame update
    void Start()
    {
        _cableObj.SetActive(false);
        _trDagger.gameObject.SetActive(false);

        _boxCollider.OnTriggerEnterAsObservable().Subscribe(t =>
        {
            Hit(t);

        }).AddTo(this);
    }
    public void Rolling(bool isRolling) 
    {
        _status = ChainStatus.Rolling;
        _rollSpeed = 900;

        _cableObj.SetActive(isRolling);
        _trDagger.gameObject.SetActive(isRolling);
    }

    public void Shoot(PawnBrainController targetPawn) 
    {
        _status = ChainStatus.Shoot;
        _boxCollider.enabled = true;

        _shotSpeed = 30;

        _shootPos = _heroBrain.CoreTransform.position;
        _shootPos.y += 1.0f;
        _trDagger.position = _shootPos;

        _trHandle.position = _trRightHand.position;

        if (targetPawn != null)
        {
            var targetPos = targetPawn.CoreTransform.position;
            targetPos.y = _shootPos.y;
            _vShoot = (targetPos - _shootPos).normalized;
        }
        else 
        { 
            _vShoot = _heroBrain.CoreTransform.forward;
        }
        _trDagger.LookAt(_shootPos + (10.0f * _vShoot));
    }
    void Hit(Collider collider) 
    {
        if (IsBind == true)
            return;
        if (collider == _boxCollider)
            return;
        Debug.Log("sensor OnTriggerEnterAsObservable");

        var helper = collider.GetComponent<PawnColliderHelper>();
        if (helper != null && helper.pawnBrain != null && helper.pawnBrain != _heroBrain)
        {
            _status = ChainStatus.Bind;
            _bindBrain = helper.pawnBrain;
            _heroBrain.Bind(_bindBrain, true);

            _trDagger.SetParent(null);

            // Ÿ�ٰ� �ܰ��� ��� ��ġ ����
            _bindLocalPos = _bindBrain.CoreTransform.position - _trDagger.position;

            // ���� ���
            SoundManager.Instance.Play(SoundID.HIT_FLESH);
        }
    }

    public void ResetChain() 
    {
        _status = ChainStatus.None;
        _trDagger.SetParent(transform);

        _cableObj.SetActive(false);
        _trDagger.gameObject.SetActive(false);

        // ���� �ִٸ� ���´�
        _heroBrain.Bind(_bindBrain, false);
        _bindBrain = null;
    }

    // Update is called once per frame
    void Update()
    {
        var handlePos = _trRightHand.position;
        _trHandle.position = handlePos;

        switch (_status) 
        {
            case ChainStatus.Rolling:
                {
                    // ȸ��
                    Quaternion rot = Quaternion.AngleAxis(_angle, _heroBrain.CoreTransform.right);
                    _trDagger.position = handlePos + (rot * Vector3.up);

                    // �ܰ� ����
                    var vLook = _trDagger.position - handlePos;
                    var vLook2 = handlePos + _cableLength * vLook.normalized;
                    _trDagger.LookAt(vLook2);

                    _angle += _rollSpeed * Time.deltaTime;
                    _rollSpeed += 200.0f * Time.deltaTime;
                    if (_rollSpeed > 900.0f)
                        _rollSpeed = 900.0f;
                }
                break;
            case ChainStatus.Shoot:
                {
                    _trDagger.position += _shotSpeed * Time.deltaTime * _vShoot;

                    var vec = _trDagger.position - _shootPos;
                    if (vec.magnitude >= 1.5f * _bindDist)
                    {
                        ResetChain();
                    }
                }
                break;
            case ChainStatus.Bind:
                {
                    //_bindDist
                    var vDist = _trDagger.position - handlePos;
                    if (vDist.magnitude > _bindDist) 
                    {
                        _trDagger.position = handlePos + (_bindDist * vDist.normalized);
                    }
                    // ���� �ܰ� ��ġ�� ����
                    _bindBrain.CoreTransform.position = _trDagger.position + _bindLocalPos;
                }
                break;
        }
    }
    private void LateUpdate()
    {
        _trHandle.position = _trRightHand.position;
    }
    private void OnDrawGizmos()
    {
        var vec = _heroBrain.CoreTransform.right;
        var start = _trRightHand.position;
        Gizmos.color = Color.red;
        //Gizmos.DrawLine(start, start + 100.0f * vec);
    }
}

using UnityEngine;

public enum EMOTION 
{
    NONE,
    ANGRY,
    FUN,
    JOY,
    SORROW,
    SUPRISE,
}

public class SocialAvatar : MonoBehaviour
{
    [SerializeField] SkinnedMeshRenderer _skinnedMeshRenderer;
    [SerializeField] Mesh _skinnedMesh;

    public Animator _anim;
    public Transform _lookTarget;

    public EMOTION _emotion = EMOTION.NONE;

    public int _currentID = 1;
    public int _currentIDTemp = 1;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _skinnedMesh = _skinnedMeshRenderer.sharedMesh;

        SetEmotion(EMOTION.ANGRY);
    }
    void OnAnimatorIK(int layerIndex)
    {
        if (_anim)
        {
            _anim.SetLookAtWeight(1.0f); // 1.0은 완전히 바라봄
            _anim.SetLookAtPosition(_lookTarget.position);
            //_anim.SetIKPositionWeight
            /*
            // 오른손 IK 설정
            if (rightHandTarget != null)
            {
                _anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);
                _anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1.0f);
                _anim.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
                _anim.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
            }

            // 왼손 IK 설정
            if (leftHandTarget != null)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
            }
            */
        }
    }
    void SetEmotion(EMOTION emotion) 
    {
        if (_emotion == emotion)
            return;

        _emotion = emotion;
        switch(_emotion)
        {
            case EMOTION.NONE:
                {
                    for (int ia = 0; ia < 5; ia++)
                    {
                        _skinnedMeshRenderer.SetBlendShapeWeight(27 + ia, 0);
                    }
                }
                break;
            case EMOTION.ANGRY:
                _skinnedMeshRenderer.SetBlendShapeWeight(27, 100);
                break;
            case EMOTION.FUN:
                _skinnedMeshRenderer.SetBlendShapeWeight(28, 100);
                break;
            case EMOTION.JOY:
                _skinnedMeshRenderer.SetBlendShapeWeight(29, 100);
                break;
            case EMOTION.SORROW:
                _skinnedMeshRenderer.SetBlendShapeWeight(30, 100);
                break;
            case EMOTION.SUPRISE:
                _skinnedMeshRenderer.SetBlendShapeWeight(31, 100);
                break;
        }
    }
}

using UnityEngine;

public enum EMOTION 
{
    None,
    Angry,
    Fun,
    Joy,
    Sorrow,
    Surprise,
}

public enum MOUTH 
{
    None,
    A,
    I,
    U,
    E,
    O,
}

public class SocialAvatar : MonoBehaviour
{
    [SerializeField] SkinnedMeshRenderer _skinnedMeshRenderer;
    [SerializeField] Mesh _skinnedMesh;

    public Animator _anim;
    public Transform _lookTarget;

    public EMOTION _emotion = EMOTION.None;
    public MOUTH _mouth = MOUTH.None;

    public int _emotionStart = 0;
    public int _mouthStart = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _skinnedMesh = _skinnedMeshRenderer.sharedMesh;

        //SetEmotion(EMOTION.ANGRY);
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
    public void CloseEye(float value) 
    {
        _skinnedMeshRenderer.SetBlendShapeWeight(14, value);
    }
    public void SetMouth(MOUTH mouth) 
    {
        if (_mouth == mouth)
            return;

        _mouth = mouth;
        float value = Random.Range(40.0f, 70.0f);
        switch(mouth) 
        {
            case MOUTH.None:
                {
                    for (int ia = 0; ia < 5; ia++)
                    {
                        _skinnedMeshRenderer.SetBlendShapeWeight(_mouthStart + ia, 0);
                    }
                }
                break;
            case MOUTH.A:
                _skinnedMeshRenderer.SetBlendShapeWeight(_mouthStart, value);
                break;
            case MOUTH.I:
                _skinnedMeshRenderer.SetBlendShapeWeight(_mouthStart+1, value);
                break;
            case MOUTH.U:
                _skinnedMeshRenderer.SetBlendShapeWeight(_mouthStart+2, value);
                break;
            case MOUTH.E:
                _skinnedMeshRenderer.SetBlendShapeWeight(_mouthStart+3, value);
                break;
            case MOUTH.O:
                _skinnedMeshRenderer.SetBlendShapeWeight(_mouthStart+4, value);
                break;
        }
    }
    public void SetEmotion(EMOTION emotion) 
    {
        if (_emotion == emotion)
            return;

        _emotion = emotion;
        switch(_emotion)
        {
            case EMOTION.None:
                {
                    for (int ia = 0; ia < 5; ia++)
                    {
                        _skinnedMeshRenderer.SetBlendShapeWeight(_emotionStart + ia, 0);
                    }
                }
                break;
            case EMOTION.Angry:
                _skinnedMeshRenderer.SetBlendShapeWeight(_emotionStart, 100);
                break;
            case EMOTION.Fun:
                _skinnedMeshRenderer.SetBlendShapeWeight(_emotionStart+1, 100);
                break;
            case EMOTION.Joy:
                _skinnedMeshRenderer.SetBlendShapeWeight(_emotionStart+2, 100);
                break;
            case EMOTION.Sorrow:
                _skinnedMeshRenderer.SetBlendShapeWeight(_emotionStart+3, 100);
                break;
            case EMOTION.Surprise:
                _skinnedMeshRenderer.SetBlendShapeWeight(_emotionStart+4, 100);
                break;
        }
    }
}

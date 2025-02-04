using DG.Tweening;
using FlowCanvas.Nodes;
using System.Collections;
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
    public int _blinkIndex = 14;

    public bool _isEnableBlink = false;
    public float _blinkValue = 0;
    public bool _isBlinking = false;

    public float _closeEyeValue = 0;

    //float[] _emotionValue = { 0, 0, 0, 0, 0 };
    //float[] _emotionTarget = { 0, 0, 0, 0, 0 };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_skinnedMeshRenderer != null)
        {
            _skinnedMesh = _skinnedMeshRenderer.sharedMesh;
        }
        DoBlink();
        //SetEmotion(EMOTION.ANGRY);
    }
    void OnAnimatorIK(int layerIndex)
    {
        if (_anim)
        {
            _anim.SetLookAtWeight(0.5f, 0.5f); // 1.0은 완전히 바라봄
            _anim.SetLookAtPosition(_lookTarget.position);
        }
    }
    public void SetTrigger(string trigger) 
    {
        if (trigger == null || trigger == "")
            return;

        Debug.Log("settrigger ; " + trigger);
        _anim.SetTrigger(trigger);
    }
    public void CloseEye(float value) 
    {
        if (_skinnedMeshRenderer == null)
            return;

        _closeEyeValue = value;

        _skinnedMeshRenderer.SetBlendShapeWeight(_blinkIndex, value);
    }
    public void SetMouth(MOUTH mouth) 
    {
        if (_skinnedMeshRenderer == null)
            return;
        if (_mouth == mouth)
            return;

        _mouth = mouth;
        float value = UnityEngine.Random.Range(40.0f, 70.0f);
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
    void SetEmotionWeight(int setIndex, float setValue) 
    {
        for (int ia = 0; ia < 5; ia++)
        {
            int index = _emotionStart + ia;
            float value = (setIndex == index) ? setValue : 0;
            _skinnedMeshRenderer.SetBlendShapeWeight(index, value);
        }
    }
    public void SetEmotion(EMOTION emotion) 
    {
        if (_skinnedMeshRenderer == null)
            return;
        if (_emotion == emotion)
            return;

        _emotion = emotion;
        switch(_emotion)
        {
            case EMOTION.None:
                SetEmotionWeight(-1, 0);
                break;
            case EMOTION.Angry:
                SetEmotionWeight(_emotionStart, 100);
                break;
            case EMOTION.Fun:
                SetEmotionWeight(_emotionStart+1, 100);
                break;
            case EMOTION.Joy:
                SetEmotionWeight(_emotionStart+2, 100);
                break;
            case EMOTION.Sorrow:
                SetEmotionWeight(_emotionStart+3, 100);
                break;
            case EMOTION.Surprise:
                SetEmotionWeight(_emotionStart+4, 100);
                //_skinnedMeshRenderer.SetBlendShapeWeight(_emotionStart+4, value);
                break;
        }
    }
    void DoBlink() 
    {
        _isBlinking = true;
        DG.Tweening.Sequence sequence = DOTween.Sequence();
        sequence.Append(DOTween.To(() => _blinkValue, x => _blinkValue = x, 100, 0.15f)) // 0 → 1 (1초 동안)
                .Append(DOTween.To(() => _blinkValue, x => _blinkValue = x, _closeEyeValue, 0.3f)) // 1 → 0 (1초 동안)
                .OnUpdate(() => _skinnedMeshRenderer.SetBlendShapeWeight(_blinkIndex, _blinkValue)) // 값 확인
                .OnComplete(() => _isBlinking = false);
    }
    private void Update()
    {
        if (_isEnableBlink == true && _isBlinking == false)
        {
            var rand = UnityEngine.Random.Range(0, 1000);
            if(rand <= 0)
                DoBlink();
        }
    }
    /*
    IEnumerator TimerCoroutine(int emotionIndex, int blendIndex)
    {
        float time = 0;
        float duration = 1.0f;
        _emotionTarget[emotionIndex] = 100;
        float curValue = _emotionValue[emotionIndex];

        while (time < duration)
        {
            time += Time.deltaTime;

            // 비율 계산
            float t = time / duration;

            // 값 서서히 변화
            curValue = Mathf.Lerp(curValue, 100, t);
            _skinnedMeshRenderer.SetBlendShapeWeight(blendIndex, curValue);
            //Debug.Log($"Current Value: {curValue}");

            yield return null; // 다음 프레임 대기
        }
        // 최종 값 설정
        _emotionValue[emotionIndex] = _emotionTarget[emotionIndex];
        _skinnedMeshRenderer.SetBlendShapeWeight(blendIndex, _emotionValue[emotionIndex]);
        //Debug.Log($"Final Value: {currentValue}");
    }
    */
}

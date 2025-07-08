using UnityEngine;

public class AnimationObj : MonoBehaviour
{
    [SerializeField] Animator _anim;
 
    [Button("Play")]
    public int dummyButton;

    public void Play() 
    {
        if(_anim != null) 
        {
            PlayAnimation("open");
        }
    }

    /// <summary>
    /// 특정 애니메이션을 이름으로 플레이
    /// </summary>
    /// <param name="animationName">플레이할 애니메이션 이름</param>
    public void PlayAnimation(string animationName)
    {
        if (_anim == null)
        {
            Debug.LogWarning("Animator가 할당되지 않았습니다.");
            return;
        }

        if (string.IsNullOrEmpty(animationName))
        {
            Debug.LogWarning("애니메이션 이름이 비어있습니다.");
            return;
        }

        _anim.Play(animationName);
    }

    /// <summary>
    /// 특정 애니메이션을 이름과 레이어로 플레이
    /// </summary>
    /// <param name="animationName">플레이할 애니메이션 이름</param>
    /// <param name="layer">애니메이션 레이어</param>
    public void PlayAnimation(string animationName, int layer)
    {
        if (_anim == null)
        {
            Debug.LogWarning("Animator가 할당되지 않았습니다.");
            return;
        }

        if (string.IsNullOrEmpty(animationName))
        {
            Debug.LogWarning("애니메이션 이름이 비어있습니다.");
            return;
        }

        if (layer < 0 || layer >= _anim.layerCount)
        {
            Debug.LogWarning($"유효하지 않은 레이어 번호: {layer}");
            return;
        }

        _anim.Play(animationName, layer);
    }

    /// <summary>
    /// 특정 애니메이션을 이름, 레이어, 정규화된 시간으로 플레이
    /// </summary>
    /// <param name="animationName">플레이할 애니메이션 이름</param>
    /// <param name="layer">애니메이션 레이어</param>
    /// <param name="normalizedTime">정규화된 시작 시간 (0.0f ~ 1.0f)</param>
    public void PlayAnimation(string animationName, int layer, float normalizedTime)
    {
        if (_anim == null)
        {
            Debug.LogWarning("Animator가 할당되지 않았습니다.");
            return;
        }

        if (string.IsNullOrEmpty(animationName))
        {
            Debug.LogWarning("애니메이션 이름이 비어있습니다.");
            return;
        }

        if (layer < 0 || layer >= _anim.layerCount)
        {
            Debug.LogWarning($"유효하지 않은 레이어 번호: {layer}");
            return;
        }

        if (normalizedTime < 0f || normalizedTime > 1f)
        {
            Debug.LogWarning($"정규화된 시간은 0.0f ~ 1.0f 범위여야 합니다: {normalizedTime}");
            return;
        }

        _anim.Play(animationName, layer, normalizedTime);
    }

    /// <summary>
    /// 트리거 파라미터를 설정하여 애니메이션 플레이
    /// </summary>
    /// <param name="triggerName">트리거 파라미터 이름</param>
    public void SetTrigger(string triggerName)
    {
        if (_anim == null)
        {
            Debug.LogWarning("Animator가 할당되지 않았습니다.");
            return;
        }

        if (string.IsNullOrEmpty(triggerName))
        {
            Debug.LogWarning("트리거 이름이 비어있습니다.");
            return;
        }

        _anim.SetTrigger(triggerName);
    }

    /// <summary>
    /// 불 파라미터를 설정하여 애니메이션 플레이
    /// </summary>
    /// <param name="boolName">불 파라미터 이름</param>
    /// <param name="value">설정할 값</param>
    public void SetBool(string boolName, bool value)
    {
        if (_anim == null)
        {
            Debug.LogWarning("Animator가 할당되지 않았습니다.");
            return;
        }

        if (string.IsNullOrEmpty(boolName))
        {
            Debug.LogWarning("불 파라미터 이름이 비어있습니다.");
            return;
        }

        _anim.SetBool(boolName, value);
    }

    /// <summary>
    /// 정수 파라미터를 설정하여 애니메이션 플레이
    /// </summary>
    /// <param name="intName">정수 파라미터 이름</param>
    /// <param name="value">설정할 값</param>
    public void SetInteger(string intName, int value)
    {
        if (_anim == null)
        {
            Debug.LogWarning("Animator가 할당되지 않았습니다.");
            return;
        }

        if (string.IsNullOrEmpty(intName))
        {
            Debug.LogWarning("정수 파라미터 이름이 비어있습니다.");
            return;
        }

        _anim.SetInteger(intName, value);
    }

    /// <summary>
    /// 실수 파라미터를 설정하여 애니메이션 플레이
    /// </summary>
    /// <param name="floatName">실수 파라미터 이름</param>
    /// <param name="value">설정할 값</param>
    public void SetFloat(string floatName, float value)
    {
        if (_anim == null)
        {
            Debug.LogWarning("Animator가 할당되지 않았습니다.");
            return;
        }

        if (string.IsNullOrEmpty(floatName))
        {
            Debug.LogWarning("실수 파라미터 이름이 비어있습니다.");
            return;
        }

        _anim.SetFloat(floatName, value);
    }

    /// <summary>
    /// 현재 재생 중인 애니메이션 정보 출력
    /// </summary>
    public void PrintCurrentAnimationInfo()
    {
        if (_anim == null)
        {
            Debug.LogWarning("Animator가 할당되지 않았습니다.");
            return;
        }

        AnimatorStateInfo stateInfo = _anim.GetCurrentAnimatorStateInfo(0);
        Debug.Log($"현재 애니메이션: {stateInfo.fullPathHash}, 이름: {stateInfo.IsName("")}");
    }
}

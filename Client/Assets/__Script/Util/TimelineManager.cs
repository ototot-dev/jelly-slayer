using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UniRx;
using FinalFactory;

public class TimelineManager : MonoSingleton<TimelineManager>
{
    private PlayableDirector _director;
    
    public PlayableDirector Director
    {
        get
        {
            if (_director == null)
            {
                _director = GetComponent<PlayableDirector>();
                if (_director == null)
                {
                    _director = gameObject.AddComponent<PlayableDirector>();
                }
            }
            return _director;
        }
    }

    protected TimelineManager() { }


    public void LoadTimeline(string timelineName)
    {
        TimelineAsset timelineAsset = Resources.Load<TimelineAsset>(timelineName);
        if (timelineAsset == null)
        {
            Debug.LogError($"Timeline asset '{timelineName}' not found in Resources!");
            return;
        }

        Director.playableAsset = timelineAsset;
        Debug.Log($"Timeline '{timelineName}' loaded successfully.");
    }

    public void Play()
    {
        if (Director.playableAsset == null)
        {
            Debug.LogError("No TimelineAsset is loaded! Please load a timeline first.");
            return;
        }

        Director.Play();
        Debug.Log("Timeline playback started.");
    }

    public void PlayTimeline(TimelineAsset timelineAsset, Action onComplete = null)
    {
        if (timelineAsset == null)
        {
            Debug.LogError("TimelineAsset is null!");
            return;
        }

        Director.playableAsset = timelineAsset;
        Director.Play();

        if (onComplete != null)
        {
            StartCoroutine(WaitForTimelineComplete(onComplete));
        }
    }

    public void PlayTimelineByName(string timelineName, Action onComplete = null)
    {
        TimelineAsset timelineAsset = Resources.Load<TimelineAsset>(timelineName);
        if (timelineAsset == null)
        {
            Debug.LogError($"Timeline asset '{timelineName}' not found in Resources!");
            return;
        }
        PlayTimeline(timelineAsset, onComplete);
    }

    public void StopTimeline()
    {
        Director.Stop();
    }

    public void PauseTimeline()
    {
        Director.Pause();
    }

    public void ResumeTimeline()
    {
        Director.Resume();
    }

    public bool IsPlaying()
    {
        return Director.state == PlayState.Playing;
    }

    public double GetCurrentTime()
    {
        return Director.time;
    }

    public double GetDuration()
    {
        return Director.duration;
    }

    public void SetTime(double time)
    {
        Director.time = time;
    }

    private System.Collections.IEnumerator WaitForTimelineComplete(Action onComplete)
    {
        while (Director.state == PlayState.Playing)
        {
            yield return null;
        }
        onComplete?.Invoke();
    }

    public IObservable<Unit> PlayTimelineAsObservable(TimelineAsset timelineAsset)
    {
        return Observable.Create<Unit>(observer =>
        {
            PlayTimeline(timelineAsset, () =>
            {
                observer.OnNext(Unit.Default);
                observer.OnCompleted();
            });

            return Disposable.Create(() =>
            {
                if (IsPlaying())
                {
                    StopTimeline();
                }
            });
        });
    }

    public IObservable<Unit> PlayTimelineByNameAsObservable(string timelineName)
    {
        return Observable.Create<Unit>(observer =>
        {
            PlayTimelineByName(timelineName, () =>
            {
                observer.OnNext(Unit.Default);
                observer.OnCompleted();
            });

            return Disposable.Create(() =>
            {
                if (IsPlaying())
                {
                    StopTimeline();
                }
            });
        });
    }

    public void BindAnimatorToTrack(string tagName, int trackIndex)
    {
        if (Director.playableAsset == null)
        {
            Debug.LogError("No TimelineAsset is currently loaded!");
            return;
        }

        // TaggerSystem으로 객체 찾기
        GameObject targetObject = TaggerSystem.FindGameObjectWithTag(tagName);
        if (targetObject == null)
        {
            Debug.LogError($"GameObject with tag '{tagName}' not found!");
            return;
        }

        // Animator 컴포넌트 찾기
        Animator animator = targetObject.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError($"Animator component not found on GameObject '{targetObject.name}'!");
            return;
        }

        TimelineAsset timelineAsset = Director.playableAsset as TimelineAsset;
        var tracks = timelineAsset.GetOutputTracks();
        
        // 트랙 인덱스 유효성 검사
        if (trackIndex < 0 || trackIndex >= tracks.Count())
        {
            Debug.LogError($"Track index {trackIndex} is out of range! Available tracks: {tracks.Count()}");
            return;
        }

        // 지정된 인덱스의 트랙 가져오기
        var targetTrack = tracks.ElementAt(trackIndex);
        
        // AnimationTrack인지 확인
        if (targetTrack is AnimationTrack)
        {
            Director.SetGenericBinding(targetTrack, animator);
            Debug.Log($"Successfully bound Animator from '{targetObject.name}' to track {trackIndex}");
        }
        else
        {
            Debug.LogError($"Track at index {trackIndex} is not an AnimationTrack! Track type: {targetTrack.GetType().Name}");
        }
    }

    public void BindAnimatorToTrackByName(string tagName, string trackName)
    {
        if (Director.playableAsset == null)
        {
            Debug.LogError("No TimelineAsset is currently loaded!");
            return;
        }

        // TaggerSystem으로 객체 찾기
        GameObject targetObject = TaggerSystem.FindGameObjectWithTag(tagName);
        if (targetObject == null)
        {
            Debug.LogError($"GameObject with tag '{tagName}' not found!");
            return;
        }

        // Animator 컴포넌트 찾기
        Animator animator = targetObject.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError($"Animator component not found on GameObject '{targetObject.name}'!");
            return;
        }

        TimelineAsset timelineAsset = Director.playableAsset as TimelineAsset;
        var tracks = timelineAsset.GetOutputTracks();
        
        // 트랙 이름으로 찾기
        var targetTrack = tracks.FirstOrDefault(track => track.name == trackName);
        if (targetTrack == null)
        {
            Debug.LogError($"Track with name '{trackName}' not found!");
            return;
        }

        // AnimationTrack인지 확인
        if (targetTrack is AnimationTrack)
        {
            Director.SetGenericBinding(targetTrack, animator);
            Debug.Log($"Successfully bound Animator from '{targetObject.name}' to track '{trackName}'");
        }
        else
        {
            Debug.LogError($"Track '{trackName}' is not an AnimationTrack! Track type: {targetTrack.GetType().Name}");
        }
    }
}

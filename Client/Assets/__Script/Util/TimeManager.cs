using UnityEngine;
using UniRx;
using System;

public class TimeManager : MonoSingleton<TimeManager>
{
    IDisposable _timer;

    public void SlomoTime(MonoBehaviour obj, float slomoRate = 0.5f, float slomoTime = 0.3f)
    {
        Time.timeScale = slomoRate;
        _timer?.Dispose();
        _timer = Observable.Timer(TimeSpan.FromSeconds(slomoTime))
            .Subscribe(_ =>
            {
                Time.timeScale = 1f;
            }).AddTo(obj);
    }
}
